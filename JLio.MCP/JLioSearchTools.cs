using System.ComponentModel;
using System.Linq;
using ModelContextProtocol.Server;
using Newtonsoft.Json;

namespace JLio.MCP;

[McpServerToolType]
public sealed class JLioSearchTools
{
    // All capability metadata lives in JLioCapabilityRegistry — no duplication here.
    private static JLioCapabilityRegistry.CapabilityEntry[] Reg => JLioCapabilityRegistry.AllCapabilities;

    // ─────────────────────────────────────────────────────────────────────────
    // Tool 1 – search_jlio
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "search_jlio")]
    [Description(
        "Searches all JLio commands and functions by intent, keyword, or natural-language description. " +
        "Returns the best-matching capabilities with their 'whenToUse' guidance. " +
        "Examples: 'rename a field', 'conditional sum', 'timestamp', 'wrap in array'. " +
        "Filter further with optional 'category' ('core', 'math', 'text', 'etl', 'timedate', 'advanced') " +
        "and 'type' ('command' or 'function').")]
    public string SearchJlio(
        [Description("Intent or keyword to search for, e.g. 'rename', 'aggregate', 'date', 'conditional'.")] string query,
        [Description("Optional category filter: 'core', 'math', 'text', 'etl', 'timedate', 'advanced'.")] string? category = null,
        [Description("Optional type filter: 'command' or 'function'.")] string? type = null)
    {
        var q = query.ToLowerInvariant();
        var results = Reg
            .Where(e => (category == null || e.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                     && (type == null || e.Type.Equals(type, StringComparison.OrdinalIgnoreCase)))
            .Select(e => new { e, score = Score(e, q) })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .Take(10)
            .Select(x => new
            {
                name = x.e.Name,
                type = x.e.Type,
                category = x.e.Category,
                summary = x.e.Summary,
                whenToUse = x.e.WhenToUse,
                relevance = x.score
            })
            .ToList();

        if (results.Count == 0)
            return JsonConvert.SerializeObject(new
            {
                query,
                note = "No matching capabilities found. Try a different keyword, or call list_jlio_capabilities to browse all.",
                results = System.Array.Empty<object>()
            }, Formatting.Indented);

        return JsonConvert.SerializeObject(new
        {
            query,
            matchCount = results.Count,
            note = "Call get_jlio_item_details(name) for full schema and examples.",
            results
        }, Formatting.Indented);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tool 2 – compare_jlio_commands
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "compare_jlio_commands")]
    [Description(
        "Side-by-side comparison of two or more JLio commands or functions that have overlapping behaviour. " +
        "Returns the key differences and a decision guide. " +
        "Most useful pairings: ['add','set','put'], ['copy','move'], ['compare','merge'], ['ifElse','decisionTable'], ['flatten','toCsv'].")]
    public string CompareCommands(
        [Description("Array of 2–5 command or function names to compare, e.g. [\"add\",\"set\",\"put\"].")] string[] names)
    {
        var entries = names
            .Select(n => Reg.FirstOrDefault(e => e.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
            .Where(e => e != null)
            .Cast<JLioCapabilityRegistry.CapabilityEntry>()
            .ToList();

        var notFound = names
            .Where(n => !Reg.Any(e => e.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var comparison = entries.Select(e => new
        {
            name = e.Name,
            type = e.Type,
            category = e.Category,
            summary = e.Summary,
            whenToUse = e.WhenToUse
        }).ToArray();

        return JsonConvert.SerializeObject(new
        {
            compared = names,
            notFound = notFound.Count > 0 ? notFound : null,
            writerCommandMatrix = names.Any(n => new[] { "add", "set", "put" }.Contains(n, StringComparer.OrdinalIgnoreCase))
                ? JLioCapabilityRegistry.WriterCommandMatrix
                : null,
            comparison,
            decisionGuide = BuildDecisionGuide(names),
            tip = "Call get_jlio_item_details(name) for full schemas and code examples."
        }, Formatting.Indented);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tool 3 – list_jlio_recipes
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "list_jlio_recipes")]
    [Description(
        "Lists a curated library of canonical JLio script recipes — multi-command patterns for common real-world tasks. " +
        "Each recipe shows name, description, commands used, and tags. " +
        "Call get_jlio_recipe(name) to retrieve the full runnable script and sample data.")]
    public string ListRecipes()
    {
        return JsonConvert.SerializeObject(new
        {
            count = Recipes.Length,
            tip = "Call get_jlio_recipe(name) to get the full script + sample input/output.",
            recipes = Recipes.Select(r => new { r.Name, r.Description, r.CommandsUsed, r.Tags }).ToArray()
        }, Formatting.Indented);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Tool 4 – get_jlio_recipe
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "get_jlio_recipe")]
    [Description(
        "Returns the full runnable JLio script, sample input, expected output, and step-by-step explanation " +
        "for a canonical recipe. Call list_jlio_recipes to see available recipe names.")]
    public string GetRecipe(
        [Description("The recipe name, exactly as returned by list_jlio_recipes.")] string name)
    {
        var recipe = Recipes.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (recipe == null)
            return JsonConvert.SerializeObject(new
            {
                error = $"Recipe '{name}' not found. Call list_jlio_recipes to see all available recipe names."
            }, Formatting.Indented);

        return JsonConvert.SerializeObject(recipe, Formatting.Indented);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Scoring
    // ─────────────────────────────────────────────────────────────────────────

    private static int Score(JLioCapabilityRegistry.CapabilityEntry entry, string q)
    {
        int score = 0;
        if (entry.Name.Equals(q, StringComparison.OrdinalIgnoreCase)) score += 100;
        else if (entry.Name.Contains(q, StringComparison.OrdinalIgnoreCase)) score += 40;
        if (entry.Summary.Contains(q, StringComparison.OrdinalIgnoreCase)) score += 20;
        if (entry.WhenToUse.Contains(q, StringComparison.OrdinalIgnoreCase)) score += 15;
        foreach (var kw in entry.Keywords)
        {
            if (kw.Equals(q, StringComparison.OrdinalIgnoreCase)) score += 30;
            else if (kw.Contains(q, StringComparison.OrdinalIgnoreCase)) score += 10;
            else if (q.Contains(kw, StringComparison.OrdinalIgnoreCase)) score += 8;
        }
        return score;
    }

    private static string[] BuildDecisionGuide(string[] names)
    {
        var key = string.Join(",", names.Select(n => n.ToLowerInvariant()).OrderBy(n => n));
        return key switch
        {
            "add,put,set" or "add,set,put" => JLioCapabilityRegistry.WriterCommandMatrix,
            "copy,move" => new[]
            {
                "copy → keep the source value in place AND write a clone to the destination.",
                "move → remove the source AND write to the destination (rename/relocate pattern).",
                "Rule: if you want the old path gone afterwards, use move."
            },
            "compare,merge" => new[]
            {
                "compare → read-only analysis: produces a diff array, does not modify source or target.",
                "merge   → write operation: copies properties from source into target.",
                "Rule: compare to audit differences; merge to apply them."
            },
            "decisiontable,ifelse" or "ifelse,decisiontable" => new[]
            {
                "ifElse       → binary (true/false or equality). ONE condition, two branches. Schema: condition OR first+second.",
                "decisionTable → multi-dimensional rule table. N conditions, M outcomes, firstMatch/allMatches strategies.",
                "Rule: use ifElse for simple guards; use decisionTable when you'd otherwise chain many ifElse commands.",
                "IMPORTANT: ifElse.condition must be a quoted '=function()' string, not an object."
            },
            "flatten,tocsv" or "tocsv,flatten" => new[]
            {
                "flatten → produces a flat JSON object (keys joined with separator). Still JSON.",
                "toCsv   → produces a CSV string. Output is no longer JSON.",
                "Rule: use flatten if the downstream system still parses JSON; use toCsv for text/file exports."
            },
            _ => names.Select(n =>
            {
                var e = Reg.FirstOrDefault(x => x.Name.Equals(n, StringComparison.OrdinalIgnoreCase));
                return e == null ? $"{n}: unknown" : $"{n}: {e.WhenToUse}";
            }).ToArray()
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Recipe catalogue
    // ─────────────────────────────────────────────────────────────────────────

    private static readonly RecipeEntry[] Recipes = new[]
    {
        new RecipeEntry(
            "rename-a-field",
            "Rename a top-level field without losing its value.",
            new[] { "move" },
            new[] { "rename", "restructure", "afd" },
            "[{\"command\":\"move\",\"fromPath\":\"$.oldName\",\"toPath\":\"$.newName\"}]",
            "{\"oldName\":\"Alice\"}",
            "{\"newName\":\"Alice\"}",
            new[]
            {
                "move copies $.oldName → $.newName then removes $.oldName.",
                "JLio models field renames as moves, not as add+remove pairs."
            }
        ),

        new RecipeEntry(
            "rename-field-in-all-array-elements",
            "Rename a field inside every element of an array — e.g. rename 'cust_name' → 'customerName' in every $.orders[*].",
            new[] { "move" },
            new[] { "rename", "array", "batch rename", "afd" },
            "[{\"command\":\"move\",\"fromPath\":\"$.orders[*].cust_name\",\"toPath\":\"$.orders[*].customerName\"}]",
            "{\"orders\":[{\"id\":1,\"cust_name\":\"Alice\"},{\"id\":2,\"cust_name\":\"Bob\"}]}",
            "{\"orders\":[{\"id\":1,\"customerName\":\"Alice\"},{\"id\":2,\"customerName\":\"Bob\"}]}",
            new[]
            {
                "Wildcard [*] makes move operate on every matched element in one command.",
                "fromPath and toPath share the same [*] segment — JLio pairs them positionally."
            }
        ),

        new RecipeEntry(
            "bulk-rename-restructure",
            "Rename and restructure many fields in one script — typical AFD 1.0 → 2.0 migration pattern: rename flat fields, group into sub-objects, convert J/N flags to booleans.",
            new[] { "move", "put", "remove", "ifElse", "set" },
            new[] { "afd", "migration", "bulk rename", "restructure", "mapping", "flatten to nested" },
            "[" +
            "{\"command\":\"move\",\"fromPath\":\"$.PP_INGDAT\",\"toPath\":\"$.policy.startDate\"}," +
            "{\"command\":\"move\",\"fromPath\":\"$.PP_EINDAT\",\"toPath\":\"$.policy.endDate\"}," +
            "{\"command\":\"move\",\"fromPath\":\"$.PP_PREMIE\",\"toPath\":\"$.policy.premium\"}," +
            "{\"command\":\"put\",\"path\":\"$.policy\",\"property\":\"entityType\",\"value\":\"commonFunctional\"}," +
            "{\"command\":\"ifElse\",\"condition\":\"=contains(fetch('$.PP_PRINBOK'),'J')\",\"ifScript\":[{\"command\":\"put\",\"path\":\"$.policy\",\"property\":\"principalBookkeeping\",\"value\":true}],\"elseScript\":[{\"command\":\"put\",\"path\":\"$.policy\",\"property\":\"principalBookkeeping\",\"value\":false}]}" +
            "]",
            "{\"PP_INGDAT\":\"20240101\",\"PP_EINDAT\":\"20241231\",\"PP_PREMIE\":1200.00,\"PP_PRINBOK\":\"J\"}",
            "{\"policy\":{\"startDate\":\"20240101\",\"endDate\":\"20241231\",\"premium\":1200.00,\"entityType\":\"commonFunctional\",\"principalBookkeeping\":true}}",
            new[]
            {
                "Each 'move' renames a flat field into a sub-object property in one step.",
                "Use 'put' (not 'set') to add 'entityType' — 'set' would silently no-op if the property doesn't exist yet.",
                "ifElse with fetch() reads the J/N flag and writes a boolean. fetch() is required here to resolve the path inside the condition.",
                "For many J/N conversions, use decisionTable instead of chained ifElse commands.",
                "If the policy sub-object doesn't exist yet, add a 'put path:$ property:policy value:{}' as the first command."
            }
        ),

        new RecipeEntry(
            "wrap-scalar-in-array",
            "Wrap a scalar value (or object) in an array — the 'arrayify' pattern required when AFD 2.0 entities must be arrays even with one element.",
            new[] { "set", "fetch" },
            new[] { "arrayify", "wrap in array", "afd", "entity array", "promote to array" },
            "[{\"command\":\"set\",\"path\":\"$\",\"property\":\"policies\",\"value\":[\"=fetch('$.policy')\"]}]",
            "{\"policy\":{\"id\":1,\"type\":\"home\"}}",
            "{\"policy\":{\"id\":1,\"type\":\"home\"},\"policies\":[{\"id\":1,\"type\":\"home\"}]}",
            new[]
            {
                "JLio has no built-in 'arrayify' command. The pattern is: set a new property with value: [\"=fetch('$.sourcePath')\"].",
                "The array literal [...] containing a fetch() call produces a single-element array containing the resolved value.",
                "To replace the original with the array (instead of adding a new property), use fromPath=$.policy toPath=$.policies with move, then set policies to [fetch('$.policies')].",
                "If 'promote' goes the wrong direction for you (scalar → named object), this recipe is the complement."
            }
        ),

        new RecipeEntry(
            "add-computed-field-to-array",
            "Add a computed field to every element of an array — e.g. totalPrice = qty * unitPrice.",
            new[] { "put", "calculate" },
            new[] { "compute", "derived field", "formula", "array enrich" },
            "[{\"command\":\"put\",\"path\":\"$.items[*]\",\"property\":\"totalPrice\",\"value\":\"=calculate('{{$.items[*].qty}} * {{$.items[*].unitPrice}}')\"  }]",
            "{\"items\":[{\"qty\":2,\"unitPrice\":15.0},{\"qty\":5,\"unitPrice\":8.0}]}",
            "{\"items\":[{\"qty\":2,\"unitPrice\":15.0,\"totalPrice\":30.0},{\"qty\":5,\"unitPrice\":8.0,\"totalPrice\":40.0}]}",
            new[]
            {
                "put (not set) is used so the field is created if it doesn't exist yet.",
                "calculate uses {{$.path}} placeholders resolved against the root context.",
                "Wildcard path [*] runs the command once per matched element."
            }
        ),

        new RecipeEntry(
            "stamp-timestamp",
            "Add a 'processedAt' UTC timestamp to every document that doesn't already have one.",
            new[] { "add", "datetime" },
            new[] { "timestamp", "audit", "created at" },
            "[{\"command\":\"add\",\"path\":\"$\",\"property\":\"processedAt\",\"value\":\"=datetime('UTC')\"}]",
            "{\"id\":1,\"name\":\"Order A\"}",
            "{\"id\":1,\"name\":\"Order A\",\"processedAt\":\"2024-06-01T12:00:00Z\"}",
            new[]
            {
                "add is used (not set) so that existing processedAt values are not overwritten.",
                "datetime('UTC') returns the current UTC time in ISO-8601 format."
            }
        ),

        new RecipeEntry(
            "conditional-field-enrichment",
            "Set a field based on whether another field meets a condition — e.g. 'priority':'high' if tags contains 'urgent'.",
            new[] { "ifElse", "put", "contains", "fetch" },
            new[] { "conditional", "classify", "branch", "flag", "j/n boolean" },
            "[{\"command\":\"ifElse\",\"condition\":\"=contains(fetch('$.tags[0]'),'urgent')\",\"ifScript\":[{\"command\":\"put\",\"path\":\"$\",\"property\":\"priority\",\"value\":\"high\"}],\"elseScript\":[{\"command\":\"put\",\"path\":\"$\",\"property\":\"priority\",\"value\":\"normal\"}]}]",
            "{\"id\":1,\"tags\":[\"urgent\",\"billing\"]}",
            "{\"id\":1,\"tags\":[\"urgent\",\"billing\"],\"priority\":\"high\"}",
            new[]
            {
                "ifElse condition must be a quoted string starting with '='. Never an object.",
                "fetch() resolves the path inside the condition — bare $.path without fetch() may not resolve in all contexts.",
                "put is used in ifScript/elseScript (not set) so the field is created if absent."
            }
        ),

        new RecipeEntry(
            "lookup-join",
            "Enrich each order with the product name from a products lookup array (SQL JOIN pattern).",
            new[] { "resolve" },
            new[] { "join", "enrich", "lookup", "foreign key" },
            "[{\"command\":\"resolve\",\"path\":\"$.orders[*]\",\"resolveSettings\":[{\"referencesCollectionPath\":\"$.products\",\"resolveKeys\":[{\"sourcePath\":\"$.productId\",\"referencePath\":\"$.id\"}],\"values\":[{\"sourcePath\":\"$.name\",\"targetPath\":\"$.productName\"}]}]}]",
            "{\"orders\":[{\"id\":1,\"productId\":10},{\"id\":2,\"productId\":20}],\"products\":[{\"id\":10,\"name\":\"Widget\"},{\"id\":20,\"name\":\"Gadget\"}]}",
            "{\"orders\":[{\"id\":1,\"productId\":10,\"productName\":\"Widget\"},{\"id\":2,\"productId\":20,\"productName\":\"Gadget\"}],\"products\":[{\"id\":10,\"name\":\"Widget\"},{\"id\":20,\"name\":\"Gadget\"}]}",
            new[]
            {
                "resolve is the JLio equivalent of a SQL LEFT JOIN.",
                "resolveKeys defines the join condition (sourcePath on the token, referencePath on the lookup row).",
                "values defines what to copy from the lookup row onto the matched token."
            }
        ),

        new RecipeEntry(
            "build-subobject-from-siblings",
            "Reshape siblings into a sub-object — e.g. move 'street', 'city', 'zip' into an 'address' sub-object.",
            new[] { "put", "move" },
            new[] { "restructure", "group fields", "nest", "sub-object" },
            "[{\"command\":\"put\",\"path\":\"$\",\"property\":\"address\",\"value\":{}},{\"command\":\"move\",\"fromPath\":\"$.street\",\"toPath\":\"$.address.street\"},{\"command\":\"move\",\"fromPath\":\"$.city\",\"toPath\":\"$.address.city\"},{\"command\":\"move\",\"fromPath\":\"$.zip\",\"toPath\":\"$.address.zip\"}]",
            "{\"name\":\"Alice\",\"street\":\"Main St\",\"city\":\"Springfield\",\"zip\":\"12345\"}",
            "{\"name\":\"Alice\",\"address\":{\"street\":\"Main St\",\"city\":\"Springfield\",\"zip\":\"12345\"}}",
            new[]
            {
                "put creates the empty address container first — use put not set so it works whether the property exists or not.",
                "Each move relocates the sibling into the new sub-object and removes it from root.",
                "Order matters: create the container before moving into it."
            }
        ),

        new RecipeEntry(
            "remove-internal-fields",
            "Strip a set of internal/audit fields from every record in an array.",
            new[] { "remove" },
            new[] { "strip", "clean", "redact", "prune" },
            "[{\"command\":\"remove\",\"path\":\"$.items[*].internalId\"},{\"command\":\"remove\",\"path\":\"$.items[*].auditLog\"},{\"command\":\"remove\",\"path\":\"$.items[*]._etag\"}]",
            "{\"items\":[{\"id\":1,\"name\":\"Widget\",\"internalId\":\"X1\",\"auditLog\":[],\"_etag\":\"abc\"},{\"id\":2,\"name\":\"Gadget\",\"internalId\":\"X2\",\"auditLog\":[],\"_etag\":\"def\"}]}",
            "{\"items\":[{\"id\":1,\"name\":\"Widget\"},{\"id\":2,\"name\":\"Gadget\"}]}",
            new[]
            {
                "One remove command per field to strip.",
                "Wildcard [*] applies removal to every element in the array.",
                "remove silently ignores elements where the property is absent."
            }
        ),

        new RecipeEntry(
            "aggregate-summary",
            "Build a summary object with count, total, and average from an array.",
            new[] { "put", "count", "sum", "avg" },
            new[] { "aggregate", "summarise", "totals", "report" },
            "[{\"command\":\"put\",\"path\":\"$\",\"property\":\"summary\",\"value\":{}},{\"command\":\"put\",\"path\":\"$.summary\",\"property\":\"count\",\"value\":\"=count('$.orders[*]')\"},{\"command\":\"put\",\"path\":\"$.summary\",\"property\":\"total\",\"value\":\"=sum('$.orders[*].amount')\"},{\"command\":\"put\",\"path\":\"$.summary\",\"property\":\"average\",\"value\":\"=avg('$.orders[*].amount')\"}]",
            "{\"orders\":[{\"id\":1,\"amount\":100},{\"id\":2,\"amount\":200},{\"id\":3,\"amount\":150}]}",
            "{\"orders\":[{\"id\":1,\"amount\":100},{\"id\":2,\"amount\":200},{\"id\":3,\"amount\":150}],\"summary\":{\"count\":3,\"total\":450,\"average\":150.0}}",
            new[]
            {
                "put (not set) is used throughout so fields are created even if absent.",
                "count, sum, avg functions accept a quoted JSONPath string referencing the source array.",
                "Each put adds one aggregate field to the summary."
            }
        ),
    };

    private sealed record RecipeEntry(
        string Name,
        string Description,
        string[] CommandsUsed,
        string[] Tags,
        string Script,
        string SampleInput,
        string SampleOutput,
        string[] StepExplanation);
}
