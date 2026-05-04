using System.ComponentModel;
using System.Text.RegularExpressions;
using JLio.Client;
using ModelContextProtocol.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.MCP;

[McpServerToolType]
public sealed class JLioScriptProposerTools
{
    private readonly JLioEngineFactory _factory;

    public JLioScriptProposerTools(JLioEngineFactory factory)
    {
        _factory = factory;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tool 1 – propose_jlio_script
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "propose_jlio_script")]
    [Description(
        "Analyses the structural difference between 'input' and 'desiredOutput' JSON and proposes one or more " +
        "verified JLio scripts that transform the input into the desired output. " +
        "Each candidate is executed against the input and the actual output is compared to desiredOutput before being returned. " +
        "Use this as your first step when you know what the data should look like but not which JLio commands to use. " +
        "It also teaches command selection: the 'steps' array explains why each command was chosen.")]
    public string ProposeScript(
        [Description("The source JSON (object, array, or scalar).")] string input,
        [Description("The desired output JSON after transformation.")] string desiredOutput)
    {
        JToken inputToken, outputToken;
        try { inputToken = JToken.Parse(input); }
        catch (JsonException ex) { return Err($"Invalid input JSON: {ex.Message}"); }
        try { outputToken = JToken.Parse(desiredOutput); }
        catch (JsonException ex) { return Err($"Invalid desiredOutput JSON: {ex.Message}"); }

        if (JToken.DeepEquals(inputToken, outputToken))
            return JsonConvert.SerializeObject(new
            {
                note = "Input and desiredOutput are already identical — no script needed.",
                candidates = Array.Empty<object>()
            }, Formatting.Indented);

        var candidates = new List<object>();

        // Candidate 1: primary diff-derived script
        var primary = BuildScript(inputToken, outputToken);
        AddCandidate(candidates, 1, "Primary script derived from structural diff", primary, inputToken, outputToken);

        // Candidate 2: replace full arrays instead of wildcarding inside them (simpler for small arrays)
        var flatArray = BuildScript(inputToken, outputToken, flattenArrays: true);
        if (!ScriptsEqual(primary, flatArray))
            AddCandidate(candidates, 2, "Alternative: replaces whole arrays instead of diffing element-by-element (fewer commands, useful for small arrays)", flatArray, inputToken, outputToken);

        return JsonConvert.SerializeObject(new
        {
            note = "Each candidate was executed and verified. 'verified: true' means the actual output matches desiredOutput exactly. " +
                   "Use execute_jlio_script to test further variants and explain_jlio_script for a step-by-step trace.",
            diff = SummarizeDiff(inputToken, outputToken),
            candidates
        }, Formatting.Indented);
    }

    private void AddCandidate(List<object> list, int idx, string description, JArray script, JToken input, JToken expected)
    {
        var scriptJson = script.ToString(Formatting.Indented);
        var verified = TryExecute(scriptJson, input, expected, out var actual, out var execError);
        list.Add(new
        {
            candidateIndex = idx,
            description,
            verified,
            verificationNote = verified
                ? "Executed successfully — actual output matches desiredOutput."
                : $"Output did not match desiredOutput after execution. {execError}",
            script,
            stepCount = script.Count,
            steps = script.Select((cmd, i) => new
            {
                step = i + 1,
                command = cmd["command"]?.ToString(),
                why = ExplainWhyCommand((JObject)cmd)
            }).ToArray(),
            actualOutput = actual
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tool 2 – explain_jlio_script
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "explain_jlio_script")]
    [Description(
        "Executes a JLio script one command at a time and returns a full step-by-step trace showing what changed " +
        "after each command. Use this to debug a script, understand exactly what each command does to the data, " +
        "or to show a user the input → transformation → output journey. " +
        "Each step lists: the command, what properties changed (added/removed/modified), and the full data state after that command.")]
    public string ExplainScript(
        [Description("The JLio script to trace, expressed as a JSON array of command objects.")] string script,
        [Description("The JSON input to run the script against.")] string input)
    {
        JToken inputToken;
        JArray scriptArray;

        try { inputToken = JToken.Parse(input); }
        catch (JsonException ex) { return Err($"Invalid input JSON: {ex.Message}"); }

        try
        {
            var parsed = JToken.Parse(script);
            if (parsed is not JArray arr) return Err("Script must be a JSON array of command objects.");
            scriptArray = arr;
        }
        catch (JsonException ex) { return Err($"Invalid script JSON: {ex.Message}"); }

        var steps = new List<object>();
        var current = inputToken.DeepClone();

        for (int i = 0; i < scriptArray.Count; i++)
        {
            var singleScript = new JArray { scriptArray[i].DeepClone() }.ToString(Formatting.None);
            var before = current.DeepClone();
            string? stepError = null;

            try
            {
                var engine = _factory.Create();
                var result = engine.ParseAndExecute(singleScript, current.DeepClone());
                current = result.Data ?? current;
                if (!result.Success) stepError = "Command reported failure — check command syntax and paths.";
            }
            catch (Exception ex)
            {
                stepError = $"{ex.Message}. Verify the command schema with get_jlio_item_details.";
            }

            var changes = CollectChanges(before, current, "$");
            steps.Add(new
            {
                step = i + 1,
                command = scriptArray[i]["command"]?.ToString(),
                commandDetail = scriptArray[i],
                success = stepError == null,
                error = stepError,
                changeSummary = changes.Count == 0 ? "No visible changes." : $"{changes.Count} change(s)",
                changes,
                stateAfter = current.DeepClone()
            });
        }

        return JsonConvert.SerializeObject(new
        {
            totalSteps = scriptArray.Count,
            initialInput = inputToken,
            finalOutput = current,
            trace = steps
        }, Formatting.Indented);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tool 3 – validate_jsonpath
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "validate_jsonpath")]
    [Description(
        "Validates a JSONPath expression against a sample JSON value and returns the matched tokens. " +
        "Use this to confirm that a path expression selects exactly the tokens you intend before embedding it in a script. " +
        "Also provides idiomatic notes and alternative path suggestions to eliminate guesswork about JLio path conventions. " +
        "Enforces a 8-second timeout to prevent hangs.")]
    public string ValidateJsonPath(
        [Description("The JSONPath expression to validate, e.g. '$.items[*].qty'.")] string path,
        [Description("A sample JSON value (object, array, or scalar) to evaluate the path against.")] string sampleJson)
    {
        JToken sample;
        try { sample = JToken.Parse(sampleJson); }
        catch (JsonException ex) { return Err($"Invalid sampleJson: {ex.Message}"); }

        using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(8));
        try
        {
            var task = Task.Run(() =>
            {
                var tokens = sample.SelectTokens(path).ToList();
                return JsonConvert.SerializeObject(new
                {
                    path,
                    matchCount = tokens.Count,
                    matched = tokens.Select(t => new { value = t, type = t.Type.ToString() }).ToArray(),
                    hint = tokens.Count == 0
                        ? "Path matched zero tokens. Double-check property names (case-sensitive) and array indexing. " +
                          "If your write command used this path and returned success with no changes, this is the silent no-op cause."
                        : $"Path matched {tokens.Count} token(s) — safe to use in a script.",
                    alternatives = BuildPathAlternatives(path),
                    filterExpressionWarning = path.Contains("[?(")
                        ? "This path contains a filter expression [?(...)]. Filters work for reading/selecting but cannot CREATE new tokens in write commands (add/set/put). Use ifElse for conditional writes."
                        : null,
                    idiomaticConventions = JLioCapabilityRegistry.FunctionArgumentConventions
                }, Formatting.Indented);
            }, cts.Token);

            if (!task.Wait(TimeSpan.FromSeconds(8)))
                return Err("validate_jsonpath timed out after 8 seconds. The JSONPath expression may contain a construct that causes infinite evaluation. Simplify the expression and try again.");

            return task.Result;
        }
        catch (OperationCanceledException)
        {
            return Err("validate_jsonpath timed out after 8 seconds. Simplify the JSONPath expression and try again.");
        }
        catch (Exception ex)
        {
            return Err($"JSONPath syntax error: {ex.Message}. Use $ as root, . for properties, [*] for all array elements, [0] for first element.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Diff engine
    // ─────────────────────────────────────────────────────────────────────────

    private static JArray BuildScript(JToken input, JToken output, bool flattenArrays = false)
    {
        var ops = new List<JObject>();
        DiffTokens(input, output, "$", ops, flattenArrays);
        return new JArray(ops.Cast<JToken>());
    }

    private static void DiffTokens(JToken input, JToken output, string path, List<JObject> ops, bool flattenArrays)
    {
        if (input.Type == JTokenType.Object && output.Type == JTokenType.Object)
            DiffObjects((JObject)input, (JObject)output, path, ops, flattenArrays);
        else if (input.Type == JTokenType.Array && output.Type == JTokenType.Array && !flattenArrays)
            DiffArrays((JArray)input, (JArray)output, path, ops);
        else if (!JToken.DeepEquals(input, output))
            ops.Add(MakeSet(path, null, output));
    }

    private static void DiffObjects(JObject input, JObject output, string path, List<JObject> ops, bool flattenArrays)
    {
        var inputKeys = input.Properties().Select(p => p.Name).ToList();
        var outputKeys = output.Properties().Select(p => p.Name).ToList();

        var removed = inputKeys.Except(outputKeys).ToList();
        var added = outputKeys.Except(inputKeys).ToList();
        var common = inputKeys.Intersect(outputKeys).ToList();

        // Match renames: value in added equals value in removed → move
        var usedRemoved = new HashSet<string>();
        var usedAdded = new HashSet<string>();
        foreach (var addedKey in added)
        {
            var addedVal = output[addedKey]!;
            // Only rename-match scalars and small objects (avoid false positives on identical empty objects)
            if (addedVal.Type == JTokenType.Object && !((JObject)addedVal).Properties().Any()) continue;
            var match = removed.FirstOrDefault(k =>
                !usedRemoved.Contains(k) && JToken.DeepEquals(input[k], addedVal));
            if (match != null)
            {
                ops.Add(MakeMove(JoinPath(path, match), JoinPath(path, addedKey)));
                usedRemoved.Add(match);
                usedAdded.Add(addedKey);
            }
        }

        foreach (var key in removed.Where(k => !usedRemoved.Contains(k)))
            ops.Add(MakeRemove(JoinPath(path, key)));

        foreach (var key in added.Where(k => !usedAdded.Contains(k)))
            ops.Add(MakeSet(path, key, output[key]!));

        foreach (var key in common)
            DiffTokens(input[key]!, output[key]!, JoinPath(path, key), ops, flattenArrays);
    }

    private static void DiffArrays(JArray input, JArray output, string path, List<JObject> ops)
    {
        bool sameLength = input.Count == output.Count;
        bool allObjects = input.Count > 0
            && input.All(e => e.Type == JTokenType.Object)
            && output.All(e => e.Type == JTokenType.Object);

        if (sameLength && allObjects)
        {
            // Collect per-element diffs
            var perElem = Enumerable.Range(0, input.Count)
                .Select(i =>
                {
                    var elemOps = new List<JObject>();
                    DiffObjects((JObject)input[i], (JObject)output[i], $"{path}[{i}]", elemOps, false);
                    return elemOps;
                })
                .ToList();

            var wildcarded = GeneralizeToWildcards(perElem, path);
            ops.AddRange(wildcarded);
        }
        else if (!JToken.DeepEquals(input, output))
        {
            // Replace the entire array
            ops.Add(MakeSet(path, null, output));
        }
    }

    private static List<JObject> GeneralizeToWildcards(List<List<JObject>> perElem, string arrayPath)
    {
        var result = new List<JObject>();
        int n = perElem.Count;

        // Key = "command:relativePathWithWildcard"
        var groups = perElem
            .SelectMany((ops, ei) => ops.Select(op => new { ei, op, key = WildcardKey(op, arrayPath) }))
            .GroupBy(x => x.key)
            .ToList();

        var handledKeys = new HashSet<string>();

        foreach (var group in groups)
        {
            if (handledKeys.Contains(group.Key)) continue;
            handledKeys.Add(group.Key);

            var elemIndices = group.Select(x => x.ei).Distinct().ToList();
            bool coversAll = elemIndices.Count == n;

            var representative = group.First().op;
            var command = representative["command"]?.ToString();

            if (coversAll)
            {
                bool canWildcard = command is "move" or "remove";
                if (!canWildcard && command is "set" or "add")
                {
                    var values = group.Select(x => x.op["value"]).ToList();
                    canWildcard = values.All(v => JToken.DeepEquals(v, values[0]));
                }

                if (canWildcard)
                {
                    var wo = WildcardOp(representative, arrayPath);
                    if (wo != null) { result.Add(wo); continue; }
                }
            }

            // Fall back to indexed ops (in element order)
            foreach (var item in group.OrderBy(x => x.ei))
                result.Add(item.op);
        }

        return result;
    }

    private static string WildcardKey(JObject op, string arrayPath)
    {
        var command = op["command"]?.ToString() ?? "";
        var path = op["path"]?.ToString() ?? op["fromPath"]?.ToString() ?? "";
        return $"{command}:{WildcardPath(path, arrayPath)}";
    }

    private static string WildcardPath(string path, string arrayPath)
    {
        if (!path.StartsWith(arrayPath + "[")) return path;
        var after = path[(arrayPath.Length + 1)..];
        var close = after.IndexOf(']');
        return close < 0 ? path : $"{arrayPath}[*]{after[(close + 1)..]}";
    }

    private static JObject? WildcardOp(JObject indexed, string arrayPath)
    {
        var command = indexed["command"]?.ToString();
        return command switch
        {
            "move" => MakeMove(
                WildcardPath(indexed["fromPath"]!.ToString(), arrayPath),
                WildcardPath(indexed["toPath"]!.ToString(), arrayPath)),
            "remove" => MakeRemove(WildcardPath(indexed["path"]!.ToString(), arrayPath)),
            "set" or "add" => indexed["property"] != null
                ? MakeSet(WildcardPath(indexed["path"]!.ToString(), arrayPath), indexed["property"]!.ToString(), indexed["value"]!.DeepClone())
                : new JObject { ["command"] = command, ["path"] = WildcardPath(indexed["path"]!.ToString(), arrayPath), ["value"] = indexed["value"]?.DeepClone() },
            _ => null
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Command builders
    // ─────────────────────────────────────────────────────────────────────────

    private static JObject MakeSet(string path, string? property, JToken value)
    {
        var obj = new JObject { ["command"] = "set", ["path"] = path };
        if (property != null) obj["property"] = property;
        obj["value"] = value.DeepClone();
        return obj;
    }

    private static JObject MakeRemove(string path) =>
        new() { ["command"] = "remove", ["path"] = path };

    private static JObject MakeMove(string from, string to) =>
        new() { ["command"] = "move", ["fromPath"] = from, ["toPath"] = to };

    private static string JoinPath(string parent, string key)
    {
        if (key.Contains(' ') || key.Contains('.') || key.Contains('['))
            return $"{parent}['{key}']";
        return $"{parent}.{key}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Execution & verification
    // ─────────────────────────────────────────────────────────────────────────

    private bool TryExecute(string script, JToken input, JToken expected, out JToken? actual, out string? error)
    {
        actual = null;
        error = null;
        try
        {
            var engine = _factory.Create();
            var result = engine.ParseAndExecute(script, input.DeepClone());
            actual = result.Data;
            if (!result.Success)
            {
                error = "Engine reported failure — the generated script may need manual adjustment.";
                return false;
            }
            if (JToken.DeepEquals(actual, expected)) return true;
            error = "Script ran successfully but actual output differs from desiredOutput — the diff may involve dynamic values (timestamps, GUIDs) or conditional logic that requires manual script authoring.";
            return false;
        }
        catch (Exception ex)
        {
            error = $"Execution exception: {ex.Message}";
            return false;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Description helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string ExplainWhyCommand(JObject cmd)
    {
        return cmd["command"]?.ToString() switch
        {
            "move" => $"Renames/moves '{cmd["fromPath"]}' → '{cmd["toPath"]}'. " +
                      "Chosen because the value at the source path is identical to the value at the destination in desiredOutput (rename pattern).",
            "remove" => $"Removes '{cmd["path"]}'. " +
                        "Chosen because this path exists in input but not in desiredOutput.",
            "set" => cmd["property"] != null
                ? $"Creates or overwrites '{cmd["path"]}.{cmd["property"]}' with value {Snip(cmd["value"])}. " +
                  "Chosen because this key is new (not in input) or has a different value in desiredOutput."
                : $"Overwrites '{cmd["path"]}' with value {Snip(cmd["value"])}. " +
                  "Chosen because this path has a different value in desiredOutput.",
            "add" => $"Adds '{cmd["property"]}' to '{cmd["path"]}' only if absent. Value: {Snip(cmd["value"])}.",
            _ => $"{cmd["command"]} on '{cmd["path"]}'"
        };
    }

    private static List<object> CollectChanges(JToken before, JToken after, string path)
    {
        var changes = new List<object>();
        GatherChanges(before, after, path, changes, depth: 0);
        return changes;
    }

    private static void GatherChanges(JToken before, JToken after, string path, List<object> changes, int depth)
    {
        if (depth > 8) return; // guard against deep recursion
        if (before.Type == JTokenType.Object && after.Type == JTokenType.Object)
        {
            var b = (JObject)before;
            var a = (JObject)after;
            foreach (var key in b.Properties().Select(p => p.Name).Union(a.Properties().Select(p => p.Name)))
            {
                var bv = b[key];
                var av = a[key];
                var childPath = JoinPath(path, key);
                if (bv == null)
                    changes.Add(new { path = childPath, change = "added", newValue = av });
                else if (av == null)
                    changes.Add(new { path = childPath, change = "removed", oldValue = bv });
                else
                    GatherChanges(bv, av, childPath, changes, depth + 1);
            }
        }
        else if (!JToken.DeepEquals(before, after))
        {
            changes.Add(new { path, change = "modified", oldValue = before, newValue = after });
        }
    }

    private static object SummarizeDiff(JToken input, JToken output)
    {
        var added = new List<string>();
        var removed = new List<string>();
        var changed = new List<string>();
        var moved = new List<string>();
        ScanDiff(input, output, "$", added, removed, changed, moved);
        return new { addedPaths = added, removedPaths = removed, changedPaths = changed, movedPaths = moved };
    }

    private static void ScanDiff(JToken input, JToken output, string path,
        List<string> added, List<string> removed, List<string> changed, List<string> moved)
    {
        if (input.Type == JTokenType.Object && output.Type == JTokenType.Object)
        {
            var b = (JObject)input; var a = (JObject)output;
            var removedKeys = b.Properties().Select(p => p.Name).Except(a.Properties().Select(p => p.Name)).ToList();
            var addedKeys = a.Properties().Select(p => p.Name).Except(b.Properties().Select(p => p.Name)).ToList();

            foreach (var addedKey in addedKeys)
            {
                var addedVal = a[addedKey]!;
                var matchKey = removedKeys.FirstOrDefault(k => JToken.DeepEquals(b[k], addedVal));
                if (matchKey != null)
                { moved.Add($"{JoinPath(path, matchKey)} → {JoinPath(path, addedKey)}"); removedKeys.Remove(matchKey); }
                else added.Add(JoinPath(path, addedKey));
            }
            foreach (var k in removedKeys) removed.Add(JoinPath(path, k));
            foreach (var k in b.Properties().Select(p => p.Name).Intersect(a.Properties().Select(p => p.Name)))
                ScanDiff(b[k]!, a[k]!, JoinPath(path, k), added, removed, changed, moved);
        }
        else if (!JToken.DeepEquals(input, output))
            changed.Add(path);
    }

    private static string[] BuildPathAlternatives(string path)
    {
        var alts = new List<string>();
        if (path.Contains("[*]"))
            alts.Add(path.Replace("[*]", "[0]") + "  — target only the first element");
        else if (Regex.IsMatch(path, @"\[\d+\]"))
            alts.Add(Regex.Replace(path, @"\[\d+\]", "[*]") + "  — target all elements");
        if (path.Contains('.') && !path.Contains("['"))
        {
            var bracket = Regex.Replace(path, @"\.(\w+)", m => $"['{m.Groups[1].Value}']");
            alts.Add(bracket + "  — bracket notation equivalent");
        }
        return alts.ToArray();
    }

    private static bool ScriptsEqual(JArray a, JArray b) =>
        JToken.DeepEquals(a, b);

    private static string Snip(JToken? v, int max = 50)
    {
        if (v == null) return "null";
        var s = v.ToString(Formatting.None);
        return s.Length > max ? s[..max] + "…" : s;
    }

    private static string Err(string msg) =>
        JsonConvert.SerializeObject(new { success = false, error = msg }, Formatting.Indented);
}
