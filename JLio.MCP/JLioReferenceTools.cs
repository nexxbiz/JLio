using System.ComponentModel;
using ModelContextProtocol.Server;
using Newtonsoft.Json;

namespace JLio.MCP;

[McpServerToolType]
public sealed class JLioReferenceTools
{
    // -------------------------------------------------------------------------
    // Tool 1 – lightweight index
    // -------------------------------------------------------------------------

    [McpServerTool(Name = "list_jlio_capabilities")]
    [Description(
        "Returns a structured index of every JLio command and function that is available in the V3 engine. " +
        "Use this first to discover what is available, then call get_jlio_item_details for full schema, " +
        "parameters, and examples of a specific item.")]
    public string ListCapabilities()
    {
        var index = new
        {
            note = "All commands go in the top-level JSON array of a script. Functions are used as values inside commands.",
            scriptSyntax = "A JLio script is a JSON array: [ { \"command\": \"<name>\", ... }, ... ]",
            functionSyntax = "A function value looks like: \"=functionName(arg1, arg2)\" or a nested object representation.",
            commands = new[]
            {
                new { name = "add",           category = "core",     summary = "Adds a property/element only if it does not already exist." },
                new { name = "set",           category = "core",     summary = "Creates or replaces a property with a new value." },
                new { name = "put",           category = "core",     summary = "Creates a property if missing, or replaces its value if present." },
                new { name = "remove",        category = "core",     summary = "Removes all tokens matched by a JSONPath." },
                new { name = "copy",          category = "core",     summary = "Copies values from one JSONPath to another." },
                new { name = "move",          category = "core",     summary = "Moves values from one JSONPath to another (copy + remove)." },
                new { name = "compare",       category = "advanced", summary = "Deep-compares two subtrees and writes a diff result to a path." },
                new { name = "merge",         category = "advanced", summary = "Merges a source subtree into a target subtree." },
                new { name = "decisionTable", category = "advanced", summary = "Evaluates a rule table against each matched token and writes outputs." },
                new { name = "ifElse",        category = "advanced", summary = "Conditionally runs an ifScript or elseScript based on a condition or value equality." },
                new { name = "flatten",       category = "etl",      summary = "Flattens a nested JSON object into a single-level key/value structure." },
                new { name = "restore",       category = "etl",      summary = "Restores a previously flattened structure back to its nested form." },
                new { name = "resolve",       category = "etl",      summary = "Looks up values from a reference collection and writes them onto matched tokens." },
                new { name = "toCsv",         category = "etl",      summary = "Converts an array of objects (or a single object) to a CSV string." },
            },
            functions = new[]
            {
                // Core
                new { name = "datetime",     category = "core",     summary = "Returns the current date/time, optionally formatted." },
                new { name = "fetch",        category = "core",     summary = "Reads a value from a JSONPath; returns a default when path is absent." },
                new { name = "partial",      category = "core",     summary = "Returns a deep clone of a token with selected properties removed." },
                new { name = "promote",      category = "core",     summary = "Wraps a value inside a new object with a named property." },
                new { name = "indirect",     category = "core",     summary = "Reads a JSONPath string from the data and uses it as a dynamic path." },
                new { name = "path",         category = "core",     summary = "Returns the JSONPath string of the current or a relative token." },
                // Math extension
                new { name = "sum",          category = "math",     summary = "Sums all numeric arguments or values from a JSONPath array." },
                new { name = "avg",          category = "math",     summary = "Averages all numeric arguments or values from a JSONPath array." },
                new { name = "count",        category = "math",     summary = "Counts elements in a JSONPath array." },
                new { name = "min",          category = "math",     summary = "Returns the minimum numeric value from arguments." },
                new { name = "max",          category = "math",     summary = "Returns the maximum numeric value from arguments." },
                new { name = "subtract",     category = "math",     summary = "Subtracts subsequent numeric arguments from the first." },
                new { name = "calculate",    category = "math",     summary = "Evaluates a math expression string, supports {{$.path}} token substitution." },
                new { name = "abs",          category = "math",     summary = "Returns the absolute value of a number." },
                new { name = "round",        category = "math",     summary = "Rounds a number to a given number of decimals." },
                new { name = "floor",        category = "math",     summary = "Rounds a number down to the nearest integer." },
                new { name = "ceiling",      category = "math",     summary = "Rounds a number up to the nearest integer." },
                new { name = "pow",          category = "math",     summary = "Raises a base number to an exponent." },
                new { name = "sqrt",         category = "math",     summary = "Returns the square root of a number." },
                new { name = "median",       category = "math",     summary = "Returns the median of a set of numbers." },
                new { name = "modulo",       category = "math",     summary = "Returns the remainder of a division (dividend % divisor)." },
                new { name = "sumif",        category = "math",     summary = "Sums values in a range that meet a criteria (like Excel SUMIF)." },
                new { name = "sumifs",       category = "math",     summary = "Sums values meeting multiple criteria (like Excel SUMIFS)." },
                new { name = "countif",      category = "math",     summary = "Counts elements in a range that meet a criteria." },
                new { name = "countifs",     category = "math",     summary = "Counts elements meeting multiple criteria." },
                new { name = "averageif",    category = "math",     summary = "Averages values in a range that meet a criteria." },
                new { name = "averageifs",   category = "math",     summary = "Averages values meeting multiple criteria." },
                new { name = "minifs",       category = "math",     summary = "Returns the minimum value from a range that meets criteria." },
                new { name = "maxifs",       category = "math",     summary = "Returns the maximum value from a range that meets criteria." },
                // Text extension
                new { name = "concat",       category = "text",     summary = "Concatenates all string arguments into one string." },
                new { name = "format",       category = "text",     summary = "Formats a value using a .NET composite format string." },
                new { name = "newGuid",      category = "text",     summary = "Generates a new random GUID string." },
                new { name = "parse",        category = "text",     summary = "Parses a JSON string into a JToken." },
                new { name = "toString",     category = "text",     summary = "Converts any value to its string representation." },
                new { name = "length",       category = "text",     summary = "Returns the length of a string or array." },
                new { name = "substring",    category = "text",     summary = "Extracts a substring by start index and optional length." },
                new { name = "replace",      category = "text",     summary = "Replaces occurrences of a pattern inside a string." },
                new { name = "indexOf",      category = "text",     summary = "Returns the index of the first occurrence of a substring." },
                new { name = "trim",         category = "text",     summary = "Removes leading and trailing whitespace from a string." },
                new { name = "trimStart",    category = "text",     summary = "Removes leading whitespace from a string." },
                new { name = "trimEnd",      category = "text",     summary = "Removes trailing whitespace from a string." },
                new { name = "toUpper",      category = "text",     summary = "Converts a string to upper-case." },
                new { name = "toLower",      category = "text",     summary = "Converts a string to lower-case." },
                new { name = "contains",     category = "text",     summary = "Returns true if a string contains a given substring." },
                new { name = "startsWith",   category = "text",     summary = "Returns true if a string starts with a given prefix." },
                new { name = "endsWith",     category = "text",     summary = "Returns true if a string ends with a given suffix." },
                new { name = "isEmpty",      category = "text",     summary = "Returns true if a string is null, empty, or whitespace." },
                new { name = "split",        category = "text",     summary = "Splits a string on a delimiter and returns a JSON array." },
                new { name = "join",         category = "text",     summary = "Joins a JSON array of strings with a separator." },
                new { name = "padLeft",      category = "text",     summary = "Left-pads a string to a given total width." },
                new { name = "padRight",     category = "text",     summary = "Right-pads a string to a given total width." },
                // TimeDate extension
                new { name = "maxDate",      category = "timedate", summary = "Returns the latest date from a set of date values or an array path." },
                new { name = "minDate",      category = "timedate", summary = "Returns the earliest date from a set of date values or an array path." },
                new { name = "avgDate",      category = "timedate", summary = "Returns the average (midpoint) date from a set of dates." },
                new { name = "dateCompare",  category = "timedate", summary = "Compares two dates; returns -1, 0, or 1." },
                new { name = "isDateBetween",category = "timedate", summary = "Returns true if a date falls between a start and end date (inclusive)." },
            }
        };

        return JsonConvert.SerializeObject(index, Formatting.Indented);
    }

    // -------------------------------------------------------------------------
    // Tool 2 – per-item detail
    // -------------------------------------------------------------------------

    [McpServerTool(Name = "get_jlio_item_details")]
    [Description(
        "Returns the full schema, all parameters, behavioural notes, and at least one complete JSON script example " +
        "for a specific JLio command or function. " +
        "Pass the exact name returned by list_jlio_capabilities (e.g. 'add', 'sumif', 'calculate').")]
    public string GetItemDetails(
        [Description("The exact name of the command or function to look up (case-insensitive).")] string name)
    {
        var key = name?.Trim().ToLowerInvariant() ?? string.Empty;
        var detail = BuildDetail(key);
        if (detail == null)
            return JsonConvert.SerializeObject(new { error = $"Unknown item '{name}'. Call list_jlio_capabilities to see all available names." }, Formatting.Indented);

        return JsonConvert.SerializeObject(detail, Formatting.Indented);
    }

    // -------------------------------------------------------------------------
    // Detail catalogue
    // -------------------------------------------------------------------------

    private static object? BuildDetail(string key) => key switch
    {
        // ── Core commands ────────────────────────────────────────────────────

        "add" => new
        {
            type = "command",
            name = "add",
            category = "core",
            description = "Adds a property or array element only when it does not already exist. Silently skips if the property is present.",
            schema = new
            {
                command = "add",
                path = "<JSONPath – target object(s) or array>",
                property = "<string – property name to add (optional when path already targets the leaf)>",
                value = "<any JSON value or function expression>"
            },
            parameters = new[]
            {
                new { name = "path",     required = true,  description = "JSONPath selecting the parent objects/arrays to operate on." },
                new { name = "property", required = false, description = "Name of the new property. Required when path targets objects." },
                new { name = "value",    required = true,  description = "The value to assign. Can be a literal or a function expression." },
            },
            notes = new[]
            {
                "If the property already exists on a matched object, the command logs a warning and skips that object.",
                "When targeting an array directly, the value is appended as a new element.",
                "Use 'set' if you want to overwrite an existing value, or 'put' for upsert behaviour."
            },
            examples = new[]
            {
                new
                {
                    description = "Add a 'status' property set to 'new' on every item that lacks it",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""add"",""path"":""$.items[*]"",""property"":""status"",""value"":""new""}]"),
                    input  = JsonConvert.DeserializeObject(@"{""items"":[{""id"":1},{""id"":2,""status"":""existing""}]}"),
                    output = JsonConvert.DeserializeObject(@"{""items"":[{""id"":1,""status"":""new""},{""id"":2,""status"":""existing""}]}")
                },
                new
                {
                    description = "Append a value to a root-level array",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""add"",""path"":""$.tags"",""value"":""urgent""}]"),
                    input  = JsonConvert.DeserializeObject(@"{""tags"":[""info""]}"),
                    output = JsonConvert.DeserializeObject(@"{""tags"":[""info"",""urgent""]}")
                }
            }
        },

        "set" => new
        {
            type = "command",
            name = "set",
            category = "core",
            description = "Creates a property if it is absent, or overwrites its value when it already exists.",
            schema = new
            {
                command = "set",
                path = "<JSONPath – target token(s)>",
                property = "<string – optional when path already targets the leaf>",
                value = "<any JSON value or function expression>"
            },
            parameters = new[]
            {
                new { name = "path",     required = true,  description = "JSONPath selecting the tokens to set." },
                new { name = "property", required = false, description = "Property name for new-syntax usage (path points to parent)." },
                new { name = "value",    required = true,  description = "The value to assign." },
            },
            notes = new[]
            {
                "Unlike 'add', set always writes, even when the property already exists.",
                "When path selects an existing leaf node, it is replaced in-place.",
            },
            examples = new[]
            {
                new
                {
                    description = "Set or overwrite the 'processed' flag on every order",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""set"",""path"":""$.orders[*]"",""property"":""processed"",""value"":true}]"),
                    input  = JsonConvert.DeserializeObject(@"{""orders"":[{""id"":1,""processed"":false},{""id"":2}]}"),
                    output = JsonConvert.DeserializeObject(@"{""orders"":[{""id"":1,""processed"":true},{""id"":2,""processed"":true}]}")
                }
            }
        },

        "put" => new
        {
            type = "command",
            name = "put",
            category = "core",
            description = "Upsert – adds the property when absent, replaces its value when present. Combines 'add' and 'set' behaviour.",
            schema = new
            {
                command = "put",
                path = "<JSONPath>",
                property = "<string – optional>",
                value = "<any JSON value or function expression>"
            },
            parameters = new[]
            {
                new { name = "path",     required = true,  description = "JSONPath selecting the target object(s) or array(s)." },
                new { name = "property", required = false, description = "Property name for new-syntax usage." },
                new { name = "value",    required = true,  description = "The value to write." },
            },
            notes = new[]
            {
                "For arrays: replaces the entire array content with the new value.",
                "Use 'add' when you need to guarantee not overwriting and 'set' when you always want to overwrite."
            },
            examples = new[]
            {
                new
                {
                    description = "Upsert a 'version' field",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""put"",""path"":""$"",""property"":""version"",""value"":2}]"),
                    input  = JsonConvert.DeserializeObject(@"{""name"":""doc""}"),
                    output = JsonConvert.DeserializeObject(@"{""name"":""doc"",""version"":2}")
                }
            }
        },

        "remove" => new
        {
            type = "command",
            name = "remove",
            category = "core",
            description = "Removes every token matched by the given JSONPath.",
            schema = new { command = "remove", path = "<JSONPath>" },
            parameters = new[]
            {
                new { name = "path", required = true, description = "JSONPath of the token(s) to delete." }
            },
            notes = new[]
            {
                "Logs a warning when the path matches no tokens.",
                "Supports wildcard paths to remove multiple tokens in one command."
            },
            examples = new[]
            {
                new
                {
                    description = "Remove all 'internalNote' properties from every item",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""remove"",""path"":""$.items[*].internalNote""}]"),
                    input  = JsonConvert.DeserializeObject(@"{""items"":[{""id"":1,""internalNote"":""x""},{""id"":2}]}"),
                    output = JsonConvert.DeserializeObject(@"{""items"":[{""id"":1},{""id"":2}]}")
                }
            }
        },

        "copy" => new
        {
            type = "command",
            name = "copy",
            category = "core",
            description = "Copies a value from 'fromPath' and writes a deep-clone to 'toPath'.",
            schema = new { command = "copy", fromPath = "<JSONPath – source>", toPath = "<JSONPath – destination>", destinationAsArray = false },
            parameters = new[]
            {
                new { name = "fromPath",          required = true,  description = "JSONPath of the value(s) to copy." },
                new { name = "toPath",            required = true,  description = "JSONPath of the destination." },
                new { name = "destinationAsArray", required = false, description = "When true, copied values are pushed into an array at the destination. Default: false." },
            },
            notes = new[] { "The source token is not modified. Use 'move' to also remove the source." },
            examples = new[]
            {
                new
                {
                    description = "Copy the root 'id' to a nested audit.originalId",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""copy"",""fromPath"":""$.id"",""toPath"":""$.audit.originalId""}]"),
                    input  = JsonConvert.DeserializeObject(@"{""id"":42}"),
                    output = JsonConvert.DeserializeObject(@"{""id"":42,""audit"":{""originalId"":42}}")
                }
            }
        },

        "move" => new
        {
            type = "command",
            name = "move",
            category = "core",
            description = "Moves a value from 'fromPath' to 'toPath' (copy then remove source).",
            schema = new { command = "move", fromPath = "<JSONPath – source>", toPath = "<JSONPath – destination>", destinationAsArray = false },
            parameters = new[]
            {
                new { name = "fromPath",          required = true,  description = "JSONPath of the value to move." },
                new { name = "toPath",            required = true,  description = "JSONPath of the destination." },
                new { name = "destinationAsArray", required = false, description = "Push into array at destination. Default: false." },
            },
            notes = new[] { "Source token is removed after the copy." },
            examples = new[]
            {
                new
                {
                    description = "Rename a property by moving it",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""move"",""fromPath"":""$.firstName"",""toPath"":""$.givenName""}]"),
                    input  = JsonConvert.DeserializeObject(@"{""firstName"":""Alice""}"),
                    output = JsonConvert.DeserializeObject(@"{""givenName"":""Alice""}")
                }
            }
        },

        // ── Advanced commands ────────────────────────────────────────────────

        "compare" => new
        {
            type = "command",
            name = "compare",
            category = "advanced",
            description = "Deep-compares two subtrees identified by 'firstPath' and 'secondPath' and writes a list of differences to 'resultPath'.",
            schema = new
            {
                command = "compare",
                firstPath  = "<JSONPath – left side>",
                secondPath = "<JSONPath – right side>",
                resultPath = "<JSONPath – where to write the diff result>",
                settings   = new
                {
                    resultTypes  = new[] { "added", "removed", "changed", "equal" },
                    arraySettings = new[] { new { path = "<array path>", matchKey = "<property to use as array item identity>" } }
                }
            },
            parameters = new[]
            {
                new { name = "firstPath",  required = true,  description = "JSONPath of the left operand." },
                new { name = "secondPath", required = true,  description = "JSONPath of the right operand." },
                new { name = "resultPath", required = true,  description = "JSONPath where the comparison array is written." },
                new { name = "settings",   required = true,  description = "CompareSettings object. resultTypes filters which difference types are returned." },
            },
            notes = new[]
            {
                "Result is an array of objects with 'differenceType', 'path', 'firstValue', 'secondValue'.",
                "resultTypes can contain: 'added', 'removed', 'changed', 'equal'.",
                "arraySettings lets you specify a match key so array elements are compared by identity rather than position."
            },
            examples = new[]
            {
                new
                {
                    description = "Compare two customer records and store all differences",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""compare"",""firstPath"":""$.v1"",""secondPath"":""$.v2"",""resultPath"":""$.diff"",""settings"":{""resultTypes"":[""changed"",""added"",""removed""]}}]"),
                    input  = JsonConvert.DeserializeObject(@"{""v1"":{""name"":""Alice"",""age"":30},""v2"":{""name"":""Alice"",""age"":31}}"),
                }
            }
        },

        "merge" => new
        {
            type = "command",
            name = "merge",
            category = "advanced",
            description = "Merges the subtree at 'path' into the subtree at 'targetPath'.",
            schema = new
            {
                command = "merge",
                path       = "<JSONPath – source>",
                targetPath = "<JSONPath – target>",
                settings   = new { strategy = "fullMerge | onlyStructure | onlyValues", arraySettings = new object[0], matchSettings = new object() }
            },
            parameters = new[]
            {
                new { name = "path",       required = true,  description = "Source subtree path." },
                new { name = "targetPath", required = true,  description = "Destination subtree path. Must differ from path." },
                new { name = "settings",   required = false, description = "MergeSettings. strategy: 'fullMerge' (default), 'onlyStructure', 'onlyValues'." },
            },
            notes = new[]
            {
                "fullMerge copies both structure and values from source to target.",
                "onlyStructure adds missing properties but does not overwrite existing values.",
                "onlyValues updates values for properties that already exist in target."
            },
            examples = new[]
            {
                new
                {
                    description = "Merge defaults into a user record",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""merge"",""path"":""$.defaults"",""targetPath"":""$.user"",""settings"":{""strategy"":""onlyStructure""}}]"),
                    input  = JsonConvert.DeserializeObject(@"{""defaults"":{""role"":""viewer"",""active"":true},""user"":{""name"":""Bob""}}"),
                    output = JsonConvert.DeserializeObject(@"{""defaults"":{""role"":""viewer"",""active"":true},""user"":{""name"":""Bob"",""role"":""viewer"",""active"":true}}")
                }
            }
        },

        "decisiontable" => new
        {
            type = "command",
            name = "decisionTable",
            category = "advanced",
            description = "Evaluates a rules table against each token matched by 'path'. For each matching rule, writes output values onto the token.",
            schema = new
            {
                command = "decisionTable",
                path  = "<JSONPath>",
                decisionTable = new
                {
                    inputs  = new[] { new { path = "<relative JSONPath>", label = "<string>" } },
                    outputs = new[] { new { path = "<relative JSONPath>", label = "<string>" } },
                    rules   = new[] { new { inputValues = new[] { "<value or wildcard '*'>" }, outputValues = new[] { "<value or function>" }, priority = 1 } },
                    defaultResults = new { },
                    executionStrategy = new { mode = "firstMatch | allMatches", conflictResolution = "priority | merge", stopOnError = false }
                }
            },
            parameters = new[]
            {
                new { name = "path",          required = true,  description = "JSONPath selecting the tokens that are processed row by row." },
                new { name = "decisionTable", required = true,  description = "Configuration object with inputs, outputs, rules, optional defaultResults and executionStrategy." },
            },
            notes = new[]
            {
                "Input values support wildcard '*' to match any value.",
                "executionStrategy.mode 'firstMatch' stops at the first matching rule; 'allMatches' applies all.",
                "defaultResults are applied when no rule matches.",
                "Output paths are relative to the matched token."
            },
            examples = new[]
            {
                new
                {
                    description = "Classify orders by amount",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""decisionTable"",""path"":""$.orders[*]"",""decisionTable"":{""inputs"":[{""path"":""$.category"",""label"":""cat""}],""outputs"":[{""path"":""$.discount"",""label"":""disc""}],""rules"":[{""inputValues"":[""premium""],""outputValues"":[0.2]},{""inputValues"":[""*""],""outputValues"":[0.0]}]}}]"),
                    input  = JsonConvert.DeserializeObject(@"{""orders"":[{""id"":1,""category"":""premium""},{""id"":2,""category"":""standard""}]}"),
                    output = JsonConvert.DeserializeObject(@"{""orders"":[{""id"":1,""category"":""premium"",""discount"":0.2},{""id"":2,""category"":""standard"",""discount"":0.0}]}")
                }
            }
        },

        "ifelse" => new
        {
            type = "command",
            name = "ifElse",
            category = "advanced",
            description = "Conditionally executes 'ifScript' or 'elseScript'. The condition can be a boolean-returning function, or a deep-equality check between 'first' and 'second'.",
            schema = new
            {
                command    = "ifElse",
                condition  = "<function expression returning boolean – OR use first+second instead>",
                first      = "<value or function expression>",
                second     = "<value or function expression>",
                ifScript   = new[] { new { command = "..." } },
                elseScript = new[] { new { command = "..." } }
            },
            parameters = new[]
            {
                new { name = "condition",  required = false, description = "A function expression that must evaluate to a boolean. Mutually exclusive with first/second." },
                new { name = "first",      required = false, description = "Left value for equality comparison. Used when condition is absent." },
                new { name = "second",     required = false, description = "Right value for equality comparison." },
                new { name = "ifScript",   required = true,  description = "JLio script executed when condition is true (or first == second)." },
                new { name = "elseScript", required = false, description = "JLio script executed when condition is false (or first != second)." },
            },
            notes = new[]
            {
                "Either 'condition' OR 'first'+'second' must be provided.",
                "Scripts are full JLio script arrays and share the same data context.",
                "elseScript is optional; omitting it results in a no-op on the false branch."
            },
            examples = new[]
            {
                new
                {
                    description = "Set 'priority' to 'high' if 'score' > 80, otherwise 'normal'",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""ifElse"",""condition"":""=contains($.flags, 'urgent')"",""ifScript"":[{""command"":""set"",""path"":""$.priority"",""value"":""high""}],""elseScript"":[{""command"":""set"",""path"":""$.priority"",""value"":""normal""}]}]")
                }
            }
        },

        // ── ETL commands ─────────────────────────────────────────────────────

        "flatten" => new
        {
            type = "command",
            name = "flatten",
            category = "etl",
            description = "Flattens a nested JSON object at 'path' into a single-level key/value object, using a separator for nested key names.",
            schema = new
            {
                command = "flatten",
                path    = "<JSONPath – defaults to '$'>",
                flattenSettings = new { separator = ".", includeJsonPath = false, jsonPathColumn = "__path__" }
            },
            parameters = new[]
            {
                new { name = "path",            required = false, description = "JSONPath of the object(s) to flatten. Defaults to '$'." },
                new { name = "flattenSettings", required = false, description = "separator: key separator (default '.'); includeJsonPath: add original JSONPath column." },
            },
            notes = new[] { "Arrays inside the object are indexed as 'parent.0.field', 'parent.1.field', etc.", "Use 'restore' to reverse the operation." },
            examples = new[]
            {
                new
                {
                    description = "Flatten a nested address object",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""flatten"",""path"":""$.address""}]"),
                    input  = JsonConvert.DeserializeObject(@"{""address"":{""street"":""Main St"",""city"":{""name"":""Springfield""}}}"),
                    output = JsonConvert.DeserializeObject(@"{""address"":{""street"":""Main St"",""city.name"":""Springfield""}}")
                }
            }
        },

        "restore" => new
        {
            type = "command",
            name = "restore",
            category = "etl",
            description = "Restores a previously flattened object back to its nested structure.",
            schema = new
            {
                command = "restore",
                path    = "<JSONPath – defaults to '$'>",
                restoreSettings = new { separator = "." }
            },
            parameters = new[]
            {
                new { name = "path",            required = false, description = "JSONPath of the flattened object(s)." },
                new { name = "restoreSettings", required = false, description = "separator: the separator used during flatten (default '.')." },
            },
            examples = new[]
            {
                new
                {
                    description = "Restore a flat object to nested form",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""restore"",""path"":""$""}]"),
                    input  = JsonConvert.DeserializeObject(@"{""city.name"":""Springfield"",""city.zip"":""12345""}"),
                    output = JsonConvert.DeserializeObject(@"{""city"":{""name"":""Springfield"",""zip"":""12345""}}")
                }
            }
        },

        "resolve" => new
        {
            type = "command",
            name = "resolve",
            category = "etl",
            description = "For each token matched by 'path', looks up values from a reference collection using match keys and writes resolved values onto the token.",
            schema = new
            {
                command = "resolve",
                path    = "<JSONPath – tokens to enrich>",
                resolveSettings = new[]
                {
                    new
                    {
                        referencesCollectionPath = "<JSONPath – lookup table array>",
                        resolveKeys = new[] { new { sourcePath = "<path on token>", referencePath = "<path on lookup row>" } },
                        values      = new[] { new { sourcePath = "<path on lookup row>", targetPath = "<path on token to write>" } }
                    }
                }
            },
            parameters = new[]
            {
                new { name = "path",            required = true, description = "Tokens to enrich." },
                new { name = "resolveSettings", required = true, description = "Array of lookup configurations. Each has resolveKeys (join condition) and values (what to copy)." },
            },
            examples = new[]
            {
                new
                {
                    description = "Enrich orders with product names from a products lookup",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""resolve"",""path"":""$.orders[*]"",""resolveSettings"":[{""referencesCollectionPath"":""$.products"",""resolveKeys"":[{""sourcePath"":""$.productId"",""referencePath"":""$.id""}],""values"":[{""sourcePath"":""$.name"",""targetPath"":""$.productName""}]}]}]"),
                    input  = JsonConvert.DeserializeObject(@"{""orders"":[{""id"":1,""productId"":10}],""products"":[{""id"":10,""name"":""Widget""}]}")
                }
            }
        },

        "tocsv" => new
        {
            type = "command",
            name = "toCsv",
            category = "etl",
            description = "Converts an array of objects (or a single object) matched by 'path' into a CSV string and replaces the matched token with that string.",
            schema = new
            {
                command = "toCsv",
                path    = "<JSONPath – defaults to '$'>",
                csvSettings = new { delimiter = ",", includeHeaders = true, dateFormat = "yyyy-MM-dd" }
            },
            parameters = new[]
            {
                new { name = "path",        required = false, description = "JSONPath of the array/object to convert." },
                new { name = "csvSettings", required = false, description = "delimiter, includeHeaders (default true), dateFormat." },
            },
            examples = new[]
            {
                new
                {
                    description = "Convert a list of records to CSV",
                    script = JsonConvert.DeserializeObject(@"[{""command"":""toCsv"",""path"":""$.data""}]"),
                    input  = JsonConvert.DeserializeObject(@"{""data"":[{""name"":""Alice"",""age"":30},{""name"":""Bob"",""age"":25}]}")
                }
            }
        },

        // ── Core functions ───────────────────────────────────────────────────

        "datetime" => new
        {
            type = "function",
            name = "datetime",
            category = "core",
            description = "Returns the current date/time as a string. Supports time selection and custom .NET format strings.",
            syntax = "datetime() | datetime(selection) | datetime(format) | datetime(selection, format)",
            arguments = new[]
            {
                new { name = "selection", required = false, description = "Time selection: (none)=local now, UTC, now, startOfDay, startOfDayUTC" },
                new { name = "format",    required = false, description = "Any .NET date format string, e.g. 'yyyy-MM-dd'. Default: ISO-8601." },
            },
            examples = new[]
            {
                new { expression = "=datetime()",                    result = "current local time in ISO-8601" },
                new { expression = "=datetime('UTC')",               result = "UTC time in ISO-8601" },
                new { expression = "=datetime('dd-MM-yyyy')",        result = "local date formatted as day-month-year" },
                new { expression = "=datetime('UTC','yyyy-MM-dd')",  result = "UTC date formatted as year-month-day" },
            },
            usageInScript = JsonConvert.DeserializeObject(@"[{""command"":""set"",""path"":""$.createdAt"",""value"":""=datetime('UTC')""}]")
        },

        "fetch" => new
        {
            type = "function",
            name = "fetch",
            category = "core",
            description = "Reads a value by JSONPath from the data context. Returns null (or a provided default) when the path does not exist.",
            syntax = "fetch(path) | fetch(path, default)",
            arguments = new[]
            {
                new { name = "path",    required = true,  description = "JSONPath string to read from the root data context." },
                new { name = "default", required = false, description = "Fallback value returned when the path does not match any token." },
            },
            examples = new[]
            {
                new { expression = "=fetch('$.config.timeout')",               result = "value at $.config.timeout or null" },
                new { expression = "=fetch('$.config.timeout', 30)",           result = "value or 30 if absent" },
            },
            usageInScript = JsonConvert.DeserializeObject(@"[{""command"":""set"",""path"":""$.items[*].region"",""value"":""=fetch('$.defaultRegion','EU')""}]")
        },

        "partial" => new
        {
            type = "function",
            name = "partial",
            category = "core",
            description = "Returns a deep clone of the current token with the specified properties stripped out. Useful for projections.",
            syntax = "partial(property1, property2, ...) | partial(sourcePath, property1, property2, ...)",
            arguments = new[]
            {
                new { name = "properties", required = true, description = "Property names (or JSONPath selectors) to REMOVE from the clone. All other properties are kept." },
            },
            examples = new[]
            {
                new { expression = "=partial('internalId','audit')", result = "clone of current token without 'internalId' and 'audit' properties" },
            },
            usageInScript = JsonConvert.DeserializeObject(@"[{""command"":""set"",""path"":""$.summary"",""value"":""=partial('$.raw', 'internalNote')""}]")
        },

        "promote" => new
        {
            type = "function",
            name = "promote",
            category = "core",
            description = "Wraps a value inside a new object with a named property, effectively promoting a scalar to an object.",
            syntax = "promote(newPropertyName) | promote(sourcePath, newPropertyName)",
            arguments = new[]
            {
                new { name = "sourcePath",      required = false, description = "JSONPath of the value to promote. Defaults to current token." },
                new { name = "newPropertyName", required = true,  description = "Name of the wrapping property in the resulting object." },
            },
            examples = new[]
            {
                new { expression = "=promote('id')", result = "{ \"id\": <currentTokenValue> }" },
            },
            usageInScript = JsonConvert.DeserializeObject(@"[{""command"":""set"",""path"":""$.items[*].code"",""value"":""=promote('sku')""}]")
        },

        "indirect" => new
        {
            type = "function",
            name = "indirect",
            category = "core",
            description = "Reads a JSONPath string stored in the data and uses it as a dynamic lookup path (like Excel INDIRECT).",
            syntax = "indirect(pathToPath)",
            arguments = new[]
            {
                new { name = "pathToPath", required = true, description = "JSONPath pointing to a string property whose value is itself a JSONPath." },
            },
            examples = new[]
            {
                new { expression = "=indirect('$.config.sourcePath')", result = "value at the path stored in $.config.sourcePath" },
            },
            usageInScript = JsonConvert.DeserializeObject(@"[{""command"":""set"",""path"":""$.result"",""value"":""=indirect('$.mapping.field')""}]")
        },

        "path" => new
        {
            type = "function",
            name = "path",
            category = "core",
            description = "Returns the JSONPath string of the current token or a token relative to it.",
            syntax = "path() | path(relativePath)",
            arguments = new[]
            {
                new { name = "relativePath", required = false, description = "A relative path expression. Omit to get the path of the current token." },
            },
            examples = new[]
            {
                new { expression = "=path()",       result = "e.g. '$.items[2]'" },
                new { expression = "=path('../id')", result = "JSONPath of the sibling 'id' property" },
            }
        },

        // ── Math functions ───────────────────────────────────────────────────

        "sum" => new
        {
            type = "function", name = "sum", category = "math",
            description = "Sums all numeric arguments. Arguments can be literals, JSONPath arrays, or function expressions.",
            syntax = "sum(arg1, arg2, ...) | sum('$.array[*].amount')",
            notes  = new[] { "Null values on found paths are treated as 0.", "Path not found is an error." },
            examples = new[]
            {
                new { expression = "=sum(1, 2, 3)",                 result = "6" },
                new { expression = "=sum('$.lines[*].amount')",     result = "total of all amount values in the lines array" },
            }
        },

        "avg" => new
        {
            type = "function", name = "avg", category = "math",
            description = "Averages all numeric arguments or the numeric values of a JSONPath array.",
            syntax = "avg(arg1, arg2, ...) | avg('$.array[*].value')",
            examples = new[] { new { expression = "=avg('$.scores[*]')", result = "mean of all scores" } }
        },

        "count" => new
        {
            type = "function", name = "count", category = "math",
            description = "Counts the elements returned by a JSONPath array selector.",
            syntax = "count('$.array[*]')",
            examples = new[] { new { expression = "=count('$.items[*]')", result = "number of items" } }
        },

        "min" => new
        {
            type = "function", name = "min", category = "math",
            description = "Returns the minimum numeric value among the arguments.",
            syntax = "min(arg1, arg2, ...) | min('$.array[*].value')",
            examples = new[] { new { expression = "=min('$.prices[*]')", result = "lowest price" } }
        },

        "max" => new
        {
            type = "function", name = "max", category = "math",
            description = "Returns the maximum numeric value among the arguments.",
            syntax = "max(arg1, arg2, ...) | max('$.array[*].value')",
            examples = new[] { new { expression = "=max('$.prices[*]')", result = "highest price" } }
        },

        "subtract" => new
        {
            type = "function", name = "subtract", category = "math",
            description = "Subtracts the second and subsequent arguments from the first.",
            syntax = "subtract(minuend, subtrahend1, subtrahend2, ...)",
            examples = new[] { new { expression = "=subtract(100, 20, 5)", result = "75" } }
        },

        "calculate" => new
        {
            type = "function", name = "calculate", category = "math",
            description = "Evaluates a math expression string. Use {{$.path}} placeholders to inject data values into the expression.",
            syntax = "calculate('expression with optional {{$.path}} tokens')",
            notes  = new[] { "Powered by DataTable.Compute under the hood.", "Division by zero returns null." },
            examples = new[]
            {
                new { expression = "=calculate('2 + 3 * 4')",                         result = "14" },
                new { expression = "=calculate('{{$.price}} * {{$.quantity}}')",       result = "price × quantity from the data context" },
                new { expression = "=calculate('{{$.total}} / {{$.count}}')",         result = "average if count > 0" },
            }
        },

        "abs"     => SimpleFunction("abs",     "math", "Returns the absolute value of a number.", "abs(number)", "=abs(-5)", "5"),
        "round"   => SimpleFunction("round",   "math", "Rounds to a given decimal places. Second arg is decimals (default 0).", "round(number, decimals)", "=round(3.14159, 2)", "3.14"),
        "floor"   => SimpleFunction("floor",   "math", "Rounds down to the nearest integer.", "floor(number)", "=floor(4.9)", "4"),
        "ceiling" => SimpleFunction("ceiling", "math", "Rounds up to the nearest integer.", "ceiling(number)", "=ceiling(4.1)", "5"),
        "pow"     => SimpleFunction("pow",     "math", "Raises base to the power of exponent.", "pow(base, exponent)", "=pow(2, 10)", "1024"),
        "sqrt"    => SimpleFunction("sqrt",    "math", "Returns the square root.", "sqrt(number)", "=sqrt(16)", "4"),
        "median"  => SimpleFunction("median",  "math", "Returns the median of a set of numbers.", "median(arg1, arg2, ...) | median('$.array[*]')", "=median(1,3,5)", "3"),
        "modulo"  => SimpleFunction("modulo",  "math", "Returns the remainder of a division.", "modulo(dividend, divisor)", "=modulo(10, 3)", "1"),

        "sumif" => new
        {
            type = "function", name = "sumif", category = "math",
            description = "Sums values in a range where the range value matches a criteria. Like Excel SUMIF.",
            syntax = "sumif(range, criteria) | sumif(range, criteria, sum_range)",
            notes  = new[] { "criteria supports: '=5', '>10', '<10', '>=10', '<=10', '<>10', or an exact value." },
            examples = new[]
            {
                new { expression = "=sumif('$.orders[*].status', '=shipped', '$.orders[*].amount')", result = "total amount of shipped orders" }
            }
        },

        "sumifs"     => ConditionalAggFunction("sumifs",     "math", "Sums a sum_range where ALL criteria-range/criteria pairs match. Like Excel SUMIFS.", "sumifs(sum_range, range1, criteria1, range2, criteria2, ...)"),
        "countif"    => ConditionalAggFunction("countif",    "math", "Counts elements in a range that match a criteria.", "countif(range, criteria)"),
        "countifs"   => ConditionalAggFunction("countifs",   "math", "Counts elements where ALL criteria match.", "countifs(range1, criteria1, range2, criteria2, ...)"),
        "averageif"  => ConditionalAggFunction("averageif",  "math", "Averages values where criteria matches.", "averageif(range, criteria, average_range)"),
        "averageifs" => ConditionalAggFunction("averageifs", "math", "Averages where ALL criteria match.", "averageifs(average_range, range1, criteria1, range2, criteria2, ...)"),
        "minifs"     => ConditionalAggFunction("minifs",     "math", "Minimum value where ALL criteria match.", "minifs(min_range, range1, criteria1, ...)"),
        "maxifs"     => ConditionalAggFunction("maxifs",     "math", "Maximum value where ALL criteria match.", "maxifs(max_range, range1, criteria1, ...)"),

        // ── Text functions ───────────────────────────────────────────────────

        "concat" => new
        {
            type = "function", name = "concat", category = "text",
            description = "Concatenates all arguments as strings.",
            syntax = "concat(arg1, arg2, ...)",
            examples = new[]
            {
                new { expression = "=concat('Hello', ' ', '$.name')", result = "Hello <name>" }
            },
            usageInScript = JsonConvert.DeserializeObject(@"[{""command"":""set"",""path"":""$.fullName"",""value"":""=concat('$.firstName', ' ', '$.lastName')""}]")
        },

        "format" => new
        {
            type = "function", name = "format", category = "text",
            description = "Formats a value using a .NET composite format string ({0}, {1}, etc.).",
            syntax = "format(formatString, arg0, arg1, ...)",
            examples = new[] { new { expression = "=format('Order {0} – {1}', '$.id', '$.status')", result = "Order 42 – shipped" } }
        },

        "newguid"   => SimpleFunction("newGuid",    "text", "Generates a new random GUID string (lowercase, with dashes).", "newGuid()", "=newGuid()", "e.g. '3f2504e0-4f89-11d3-9a0c-0305e82c3301'"),
        "parse"     => SimpleFunction("parse",      "text", "Parses a JSON string into a typed JToken.", "parse(jsonString)", "=parse('{\"a\":1}')", "{ a: 1 } (object)"),
        "tostring"  => SimpleFunction("toString",   "text", "Converts any value to its JSON string representation.", "toString(value)", "=toString(42)", "\"42\""),
        "length"    => SimpleFunction("length",     "text", "Returns the character count of a string, or element count of an array.", "length(value)", "=length('hello')", "5"),
        "substring" => new
        {
            type = "function", name = "substring", category = "text",
            description = "Extracts a portion of a string.",
            syntax = "substring(string, startIndex) | substring(string, startIndex, length)",
            examples = new[] { new { expression = "=substring('Hello World', 6, 5)", result = "World" } }
        },
        "replace"   => new
        {
            type = "function", name = "replace", category = "text",
            description = "Replaces occurrences of 'oldValue' with 'newValue' in a string.",
            syntax = "replace(string, oldValue, newValue)",
            examples = new[] { new { expression = "=replace('Hello World', 'World', 'JLio')", result = "Hello JLio" } }
        },
        "indexof"   => new
        {
            type = "function", name = "indexOf", category = "text",
            description = "Returns the zero-based index of the first occurrence of a substring, or -1 if not found.",
            syntax = "indexOf(string, substring)",
            examples = new[] { new { expression = "=indexOf('Hello World', 'World')", result = "6" } }
        },
        "trim"      => SimpleFunction("trim",      "text", "Removes leading and trailing whitespace.", "trim(string)", "=trim('  hi  ')", "hi"),
        "trimstart" => SimpleFunction("trimStart", "text", "Removes leading whitespace.", "trimStart(string)", "=trimStart('  hi')", "hi"),
        "trimend"   => SimpleFunction("trimEnd",   "text", "Removes trailing whitespace.", "trimEnd(string)", "=trimEnd('hi  ')", "hi"),
        "toupper"   => SimpleFunction("toUpper",   "text", "Converts to upper-case.", "toUpper(string)", "=toUpper('hello')", "HELLO"),
        "tolower"   => SimpleFunction("toLower",   "text", "Converts to lower-case.", "toLower(string)", "=toLower('HELLO')", "hello"),
        "contains"  => new
        {
            type = "function", name = "contains", category = "text",
            description = "Returns true if the string contains the given substring.",
            syntax = "contains(string, substring)",
            examples = new[] { new { expression = "=contains('Hello World', 'World')", result = "true" } }
        },
        "startswith" => new
        {
            type = "function", name = "startsWith", category = "text",
            description = "Returns true if the string starts with the given prefix.",
            syntax = "startsWith(string, prefix)",
            examples = new[] { new { expression = "=startsWith('Hello', 'He')", result = "true" } }
        },
        "endswith" => new
        {
            type = "function", name = "endsWith", category = "text",
            description = "Returns true if the string ends with the given suffix.",
            syntax = "endsWith(string, suffix)",
            examples = new[] { new { expression = "=endsWith('Hello', 'lo')", result = "true" } }
        },
        "isempty" => SimpleFunction("isEmpty", "text", "Returns true if the string is null, empty, or consists only of whitespace.", "isEmpty(string)", "=isEmpty('')", "true"),
        "split" => new
        {
            type = "function", name = "split", category = "text",
            description = "Splits a string by a delimiter and returns a JSON array of strings.",
            syntax = "split(string, delimiter)",
            examples = new[] { new { expression = "=split('a,b,c', ',')", result = "[\"a\",\"b\",\"c\"]" } }
        },
        "join" => new
        {
            type = "function", name = "join", category = "text",
            description = "Joins a JSON array of strings with a separator.",
            syntax = "join(arrayPath, separator)",
            examples = new[] { new { expression = "=join('$.tags[*]', ', ')", result = "tag1, tag2, tag3" } }
        },
        "padleft"  => new
        {
            type = "function", name = "padLeft",  category = "text",
            description = "Left-pads a string to a total width using an optional fill character.",
            syntax = "padLeft(string, totalWidth) | padLeft(string, totalWidth, fillChar)",
            examples = new[] { new { expression = "=padLeft('5', 4, '0')", result = "0005" } }
        },
        "padright" => new
        {
            type = "function", name = "padRight", category = "text",
            description = "Right-pads a string to a total width using an optional fill character.",
            syntax = "padRight(string, totalWidth) | padRight(string, totalWidth, fillChar)",
            examples = new[] { new { expression = "=padRight('hi', 5, '-')", result = "hi---" } }
        },

        // ── TimeDate functions ───────────────────────────────────────────────

        "maxdate" => new
        {
            type = "function", name = "maxDate", category = "timedate",
            description = "Returns the latest (maximum) date from a set of date arguments or a JSONPath array.",
            syntax = "maxDate(date1, date2, ...) | maxDate('$.dates[*]')",
            notes  = new[] { "Accepts ISO-8601 strings, or any format parseable by DateTime.Parse." },
            examples = new[] { new { expression = "=maxDate('$.events[*].date')", result = "the latest event date" } }
        },
        "mindate" => new
        {
            type = "function", name = "minDate", category = "timedate",
            description = "Returns the earliest (minimum) date from a set of date arguments or a JSONPath array.",
            syntax = "minDate(date1, date2, ...) | minDate('$.dates[*]')",
            examples = new[] { new { expression = "=minDate('$.events[*].date')", result = "the earliest event date" } }
        },
        "avgdate" => new
        {
            type = "function", name = "avgDate", category = "timedate",
            description = "Returns the average (arithmetic midpoint) date from a set of dates.",
            syntax = "avgDate(date1, date2, ...) | avgDate('$.dates[*]')",
            examples = new[] { new { expression = "=avgDate('2024-01-01', '2024-01-31')", result = "2024-01-16T00:00:00" } }
        },
        "datecompare" => new
        {
            type = "function", name = "dateCompare", category = "timedate",
            description = "Compares two dates. Returns -1 if date1 < date2, 0 if equal, 1 if date1 > date2.",
            syntax = "dateCompare(date1, date2)",
            examples = new[] { new { expression = "=dateCompare('2024-01-01', '2024-06-01')", result = "-1" } }
        },
        "isdatebetween" => new
        {
            type = "function", name = "isDateBetween", category = "timedate",
            description = "Returns true if the check date falls between start and end dates (inclusive). Start/end order is normalised automatically.",
            syntax = "isDateBetween(checkDate, startDate, endDate)",
            examples = new[] { new { expression = "=isDateBetween('2024-03-15', '2024-01-01', '2024-12-31')", result = "true" } }
        },

        _ => null
    };

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static object SimpleFunction(string name, string category, string description, string syntax, string exampleExpr, string exampleResult) =>
        new
        {
            type = "function",
            name,
            category,
            description,
            syntax,
            examples = new[] { new { expression = exampleExpr, result = exampleResult } }
        };

    private static object ConditionalAggFunction(string name, string category, string description, string syntax) =>
        new
        {
            type = "function",
            name,
            category,
            description,
            syntax,
            notes = new[] { "criteria syntax: '=value', '>value', '<value', '>=value', '<=value', '<>value', or an exact match value." }
        };
}
