namespace JLio.MCP;

/// <summary>
/// Single source of truth for all JLio capability metadata.
/// Both list_jlio_capabilities and search_jlio / compare_jlio_commands read from here —
/// no duplication, no sync problem.
/// </summary>
public static class JLioCapabilityRegistry
{
    // ─────────────────────────────────────────────────────────────────────────
    // Entry type
    // ─────────────────────────────────────────────────────────────────────────

    public sealed record CapabilityEntry(
        string Name,
        string Type,       // "command" | "function"
        string Category,   // "core" | "math" | "text" | "etl" | "timedate" | "advanced"
        string Summary,
        string WhenToUse,
        string[] Keywords);

    // ─────────────────────────────────────────────────────────────────────────
    // Writer-command decision matrix — referenced by list_jlio_capabilities
    // and compare_jlio_commands so neither duplicates it.
    // ─────────────────────────────────────────────────────────────────────────

    public static readonly string[] WriterCommandMatrix = new[]
    {
        "┌──────────┬──────────────────────────┬────────────────────────────┐",
        "│ command  │ property already EXISTS  │ property is MISSING        │",
        "├──────────┼──────────────────────────┼────────────────────────────┤",
        "│ add      │ skips (no-op, no error)  │ creates the property       │",
        "│ set      │ overwrites the value     │ NO-OP (silent, no error)   │",
        "│ put      │ overwrites the value     │ creates the property       │",
        "└──────────┴──────────────────────────┴────────────────────────────┘",
        "",
        "CRITICAL: 'set' does NOT create. If the path/property does not exist yet, use 'add' (create-only) or 'put' (upsert).",
        "Rule of thumb: when in doubt, use 'put'."
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Function-argument path-resolution conventions — single place to maintain
    // ─────────────────────────────────────────────────────────────────────────

    public static readonly string[] FunctionArgumentConventions = new[]
    {
        "JLio function arguments have three distinct calling conventions. Mixing them is the most common source of silent failures.",
        "",
        "1. TOP-LEVEL value expression  (e.g. command.value = \"=functionName(...)\")",
        "   The string starting with '=' is evaluated as a function expression.",
        "   JSONPath arguments MUST be quoted strings: =concat('$.firstName', ' ', '$.lastName')",
        "   The engine resolves each quoted '$.…' argument against the root data context.",
        "   Bare path without quotes is NOT resolved — it is treated as a literal string.",
        "",
        "2. calculate() function  (math extension)",
        "   Use {{$.path}} placeholders inside the expression string.",
        "   Example: =calculate('{{$.price}} * {{$.qty}}')",
        "   Standard quoted-path syntax ('$.path') does NOT work inside calculate.",
        "",
        "3. ifElse.condition / ifElse.first / ifElse.second",
        "   Bare path syntax works here: condition: \"=contains($.flags, 'urgent')\"",
        "   No extra quoting of the path is needed.",
        "",
        "COMMON MISTAKES:",
        "  ✗ set value: \"=substring('$.field', 0, 4)\"   — substring receives the literal string '$.field', NOT its value.",
        "  ✓ set value: \"=substring(fetch('$.field'), 0, 4)\"  — fetch() resolves the path first.",
        "  ✓ set value: \"=calculate('{{$.field}}'.Substring(0, 4))\"  — not valid; substring is not in calculate.",
        "  ✓ Use fetch() to resolve a path inside any function: fetch('$.myField') returns the actual value.",
        "",
        "SIGNAL: if your output contains a literal '$.something' string, the path was never resolved.",
        "FIX: wrap with fetch() — e.g. =concat(fetch('$.firstName'), ' ', fetch('$.lastName'))"
    };

    // ─────────────────────────────────────────────────────────────────────────
    // All capabilities
    // ─────────────────────────────────────────────────────────────────────────

    public static readonly CapabilityEntry[] AllCapabilities = new[]
    {
        // ── Commands ─────────────────────────────────────────────────────────

        E("add", "command", "core",
          "Adds a property/element only if it does not already exist. Silently skips if already present.",
          "Use to inject default values without risk of overwriting existing data. " +
          "DOES NOT overwrite — pick 'put' for upsert, or 'set' to replace an existing value.",
          new[] { "default", "initialise", "create if missing", "guard", "missing property" }),

        E("set", "command", "core",
          "Replaces the value of an EXISTING property. Has NO effect if the path or property does not exist yet.",
          "Use ONLY when you are certain the property already exists and you want to overwrite its value. " +
          "If the property might be absent, use 'put' (upsert) instead. " +
          "WARNING: 'set' on a missing path is a silent no-op — no error, no change.",
          new[] { "overwrite", "replace", "update existing" }),

        E("put", "command", "core",
          "Upsert — creates the property if absent OR replaces its value if present.",
          "Use when you want 'the property must end up with this value' regardless of current state. " +
          "The safest general-purpose writer. Pick 'add' only if you must never overwrite, pick 'set' only if you know the path already exists.",
          new[] { "upsert", "create or replace", "safe write", "ensure value" }),

        E("remove", "command", "core",
          "Removes every token matched by a JSONPath.",
          "The only deletion command. Use to strip fields from objects or elements from arrays. " +
          "Logs a warning when path matches zero tokens.",
          new[] { "delete", "strip", "clean", "drop field", "prune" }),

        E("copy", "command", "core",
          "Copies values from 'fromPath' to 'toPath'; source is preserved.",
          "Use to duplicate a value while keeping the original. Pick 'move' if you also want to remove the source (rename pattern).",
          new[] { "duplicate", "clone", "replicate" }),

        E("move", "command", "core",
          "Moves values from 'fromPath' to 'toPath' (copy then remove source).",
          "The idiomatic JLio way to RENAME a field. Use fromPath=old name, toPath=new name on the same object. " +
          "Pick 'copy' when you want to keep the source.",
          new[] { "rename", "restructure", "relocate", "rename field", "rename property" }),

        E("compare", "command", "advanced",
          "Deep-compares two subtrees and writes a diff result array to 'resultPath'.",
          "Use to produce an audit diff between two document versions. Read-only on source/target. " +
          "Pick 'merge' to actually apply changes.",
          new[] { "diff", "audit", "changelog", "version", "delta" }),

        E("merge", "command", "advanced",
          "Merges the subtree at 'path' into the subtree at 'targetPath'. Strategy controls what is copied.",
          "Use to apply defaults or overlays from a template. " +
          "Strategies: fullMerge (copy everything), onlyStructure (add missing keys only — does not overwrite), onlyValues (update existing keys only — does not add). " +
          "Pick 'compare' to inspect differences first.",
          new[] { "combine", "apply defaults", "overlay", "union", "patch", "template" }),

        E("decisionTable", "command", "advanced",
          "Evaluates a rules table against each matched token and writes output values.",
          "Use for multi-condition classification or scoring — like an Excel decision table. " +
          "Requires path to match ≥1 token; silent no-op if path matches nothing. " +
          "Pick 'ifElse' for a single binary condition. " +
          "NOTE: path:'$' on a non-array root is unsupported — wrap in an array or use ifElse.",
          new[] { "classify", "rules", "lookup table", "conditional mapping", "score", "categorise" }),

        E("ifElse", "command", "advanced",
          "Conditionally runs 'ifScript' or 'elseScript'. Condition is either a boolean function expression OR 'first'+'second' equality check.",
          "Use for a single binary decision. " +
          "Schema: use EITHER condition:\"=booleanFunction(...)\" OR first+second for equality. Never mix both forms. " +
          "Pick 'decisionTable' when you have many condition combinations.",
          new[] { "conditional", "if", "branch", "toggle", "switch", "boolean" }),

        E("flatten", "command", "etl",
          "Flattens a nested JSON object into a single-level key/value structure.",
          "Use before CSV export or for flat-storage systems. Pair with 'restore' to reverse. " +
          "Use 'toCsv' when you need a CSV string rather than a flat JSON object.",
          new[] { "flat", "denormalise", "spreadsheet", "csv prep", "one level" }),

        E("restore", "command", "etl",
          "Restores a previously flattened structure back to its nested form.",
          "Use to reverse 'flatten'. Required when ingesting flat documents from external systems.",
          new[] { "unflatten", "nest", "rebuild", "normalise" }),

        E("resolve", "command", "etl",
          "For each token matched by 'path', looks up values from a reference collection and writes them onto the token.",
          "Use for JOIN/lookup enrichment — equivalent to SQL LEFT JOIN. The reference array must be in the same document. " +
          "Pick 'decisionTable' when the lookup is rule-based rather than key-based.",
          new[] { "lookup", "join", "enrich", "reference", "foreign key", "hydrate" }),

        E("toCsv", "command", "etl",
          "Converts an array of objects (or a single object) to a CSV string.",
          "Use when the output must be a CSV string. Pick 'flatten' when you want a flat JSON object instead.",
          new[] { "csv", "export", "spreadsheet", "tabular" }),

        // ── Functions ─────────────────────────────────────────────────────────

        E("datetime", "function", "core",
          "Returns the current date/time, optionally formatted.",
          "Use to stamp records with the current timestamp. Supply a format string for custom patterns. " +
          "Always pass a quoted string argument: =datetime('UTC') or =datetime('yyyy-MM-dd').",
          new[] { "timestamp", "now", "date", "time", "current date" }),

        E("fetch", "function", "core",
          "Reads a value from a JSONPath; returns a default when the path is absent.",
          "Use inside any function when you need to resolve a JSONPath to its actual value. " +
          "This is the correct way to pass dynamic data into functions like concat, substring, replace. " +
          "Example: =concat(fetch('$.firstName'), ' ', fetch('$.lastName')).",
          new[] { "read", "get value", "reference other field", "default", "fallback", "resolve path" }),

        E("partial", "function", "core",
          "Returns a deep clone of a token with selected properties removed.",
          "Use to build projections (all fields except a few).",
          new[] { "exclude", "omit", "projection", "strip fields" }),

        E("promote", "function", "core",
          "Wraps a value inside a new object with a named property.",
          "Use to turn a scalar into an object — e.g. '42' → { sku: 42 }. " +
          "To wrap a value in an array (arrayify), use set with value:[fetch('$.field')] instead.",
          new[] { "wrap", "box", "nest scalar" }),

        E("indirect", "function", "core",
          "Reads a JSONPath string stored in the data and uses it as a dynamic lookup path.",
          "Use when the path to read is itself stored in the data (Excel INDIRECT pattern).",
          new[] { "dynamic path", "computed path", "indirect reference" }),

        E("path", "function", "core",
          "Returns the JSONPath string of the current or a relative token.",
          "Use to capture the path of the current token as a string value.",
          new[] { "current path", "self path" }),

        E("sum", "function", "math",
          "Sums all numeric arguments or values from a JSONPath array.",
          "Use for an unconditional total. Pass a quoted path string: =sum('$.lines[*].amount'). " +
          "Pick 'sumif'/'sumifs' for conditional totals.",
          new[] { "total", "aggregate", "add up", "sum" }),

        E("avg", "function", "math",
          "Averages numeric arguments or values from a JSONPath array.",
          "Use for a simple unconditional average. Pick 'averageif'/'averageifs' for conditional averages.",
          new[] { "mean", "average" }),

        E("count", "function", "math",
          "Counts elements returned by a JSONPath array selector.",
          "Use for a simple element count: =count('$.items[*]'). Pick 'countif'/'countifs' for filtered counts.",
          new[] { "count", "size", "number of" }),

        E("min", "function", "math",
          "Returns the minimum numeric value from arguments or a JSONPath array.",
          "Use for a simple unconditional minimum. Pick 'minifs' for filtered minimum.",
          new[] { "minimum", "lowest" }),

        E("max", "function", "math",
          "Returns the maximum numeric value from arguments or a JSONPath array.",
          "Use for a simple unconditional maximum. Pick 'maxifs' for filtered maximum.",
          new[] { "maximum", "highest" }),

        E("subtract", "function", "math",
          "Subtracts the second and subsequent arguments from the first.",
          "Use for simple a - b. Use 'calculate' for complex expressions.",
          new[] { "minus", "difference", "subtract" }),

        E("calculate", "function", "math",
          "Evaluates a math expression string. Use {{$.path}} placeholders to inject data values.",
          "Use for complex arithmetic: =calculate('{{$.price}} * {{$.qty}} * (1 - {{$.discount}})'). " +
          "NOTE: use {{$.path}} syntax here — quoted '$.path' does NOT resolve inside calculate.",
          new[] { "formula", "expression", "arithmetic", "compute", "calculate" }),

        E("abs",     "function", "math", "Returns the absolute value of a number.", "Use to strip the sign from a number.",          new[] { "absolute", "positive" }),
        E("round",   "function", "math", "Rounds a number to a given number of decimal places.", "Use after division or float arithmetic to control precision.", new[] { "round", "decimal places" }),
        E("floor",   "function", "math", "Rounds down to the nearest integer.", "Use for floor division or bin assignment.",          new[] { "floor", "round down" }),
        E("ceiling", "function", "math", "Rounds up to the nearest integer.",   "Use for ceiling/capacity calculations.",             new[] { "ceiling", "ceil", "round up" }),
        E("pow",     "function", "math", "Raises a base number to an exponent.", "Use for exponential calculations.",                 new[] { "power", "exponent", "squared" }),
        E("sqrt",    "function", "math", "Returns the square root of a number.", "Use for geometric or statistical calculations.",    new[] { "square root" }),
        E("median",  "function", "math", "Returns the median of a set of numbers.", "Use when you need the middle value rather than the mean.", new[] { "median", "middle value" }),
        E("modulo",  "function", "math", "Returns the remainder of a division (dividend % divisor).", "Use for cycle detection, even/odd checks, or banding.", new[] { "remainder", "mod", "even", "odd" }),

        E("sumif", "function", "math",
          "Sums values in a range where the range value matches a criteria (like Excel SUMIF).",
          "Use for a conditional total with one condition. Pick 'sumifs' for multiple conditions. " +
          "criteria syntax: '=5', '>10', '<>value', or an exact match.",
          new[] { "conditional sum", "filtered total" }),

        E("sumifs",     "function", "math", "Sums values meeting multiple criteria (like Excel SUMIFS).",  "Use when the sum filter involves multiple simultaneous conditions.", new[] { "multi-condition sum" }),
        E("countif",    "function", "math", "Counts elements in a range that match a criteria.",          "Use to count rows matching one condition. Pick 'countifs' for multiple.",  new[] { "conditional count", "filter count" }),
        E("countifs",   "function", "math", "Counts elements meeting multiple criteria.",                  "Use when the count filter involves multiple conditions.",                  new[] { "multi-condition count" }),
        E("averageif",  "function", "math", "Averages values where criteria matches.",                     "Use for a conditional average (one condition). Pick 'averageifs' for multiple.", new[] { "conditional average" }),
        E("averageifs", "function", "math", "Averages values meeting multiple criteria.",                  "Use for multi-condition conditional averages.",                             new[] { "multi-condition average" }),
        E("minifs",     "function", "math", "Returns the minimum value from a range that meets criteria.", "Use when you need the minimum of a filtered subset.",                      new[] { "conditional minimum", "filtered min" }),
        E("maxifs",     "function", "math", "Returns the maximum value from a range that meets criteria.", "Use when you need the maximum of a filtered subset.",                      new[] { "conditional maximum", "filtered max" }),

        E("concat", "function", "text",
          "Concatenates all arguments as strings.",
          "Use to build strings from parts: =concat(fetch('$.firstName'), ' ', fetch('$.lastName')). " +
          "IMPORTANT: arguments that are JSONPaths must be wrapped with fetch() — bare '$.path' is treated as a literal string.",
          new[] { "join strings", "combine", "build string", "concatenate", "full name" }),

        E("format", "function", "text",
          "Formats a value using a .NET composite format string ({0}, {1}, etc.).",
          "Use for template-style strings: =format('Order {0} – {1}', fetch('$.id'), fetch('$.status')).",
          new[] { "template", "format string", "interpolate" }),

        E("toString",  "function", "text", "Converts any value to its string representation.", "Use when a downstream field must be a string but the source is a number or boolean.", new[] { "stringify", "convert to string", "coerce" }),
        E("length",    "function", "text", "Returns the character count of a string, or element count of an array.", "Use to validate field lengths or count items.", new[] { "size", "count chars", "string length" }),
        E("substring", "function", "text", "Extracts a portion of a string.", "Use for fixed-format parsing: =substring(fetch('$.dateStr'), 0, 4). MUST wrap path arg with fetch().", new[] { "slice", "extract", "substr" }),
        E("replace",   "function", "text", "Replaces occurrences of a pattern inside a string.", "Use to sanitise or normalise strings. Wrap path args with fetch().", new[] { "substitute", "find and replace" }),
        E("indexOf",   "function", "text", "Returns the index of the first occurrence of a substring, or -1.", "Use to locate a delimiter before splitting.", new[] { "find", "position", "index of" }),
        E("trim",      "function", "text", "Removes leading and trailing whitespace.", "Use to clean user-entered strings.", new[] { "trim", "strip whitespace" }),
        E("trimStart", "function", "text", "Removes leading whitespace.", "Use when only leading whitespace is problematic.", new[] { "ltrim" }),
        E("trimEnd",   "function", "text", "Removes trailing whitespace.", "Use when only trailing whitespace is problematic.", new[] { "rtrim" }),
        E("toUpper",   "function", "text", "Converts a string to upper-case.", "Use to normalise text for comparison or display.", new[] { "uppercase", "caps" }),
        E("toLower",   "function", "text", "Converts a string to lower-case.", "Use to normalise text for comparison or display.", new[] { "lowercase" }),
        E("contains",  "function", "text", "Returns true if a string contains a given substring.", "Use inside ifElse condition expressions.", new[] { "includes", "has substring" }),
        E("startsWith","function", "text", "Returns true if a string starts with a given prefix.", "Use for prefix-based classification.", new[] { "prefix", "begins with" }),
        E("endsWith",  "function", "text", "Returns true if a string ends with a given suffix.", "Use for extension or suffix matching.", new[] { "suffix" }),
        E("isEmpty",   "function", "text", "Returns true if a string is null, empty, or whitespace.", "Use to guard against blank values.", new[] { "blank", "null check", "empty string" }),
        E("split",     "function", "text", "Splits a string on a delimiter and returns a JSON array.", "Use to turn a delimited string into an array.", new[] { "tokenise", "parse delimited" }),
        E("join",      "function", "text", "Joins a JSON array of strings with a separator.", "Use to turn an array back into a delimited string.", new[] { "implode", "array to string" }),
        E("padLeft",   "function", "text", "Left-pads a string to a given total width.", "Use for zero-padding numeric codes.", new[] { "zero pad", "left pad" }),
        E("padRight",  "function", "text", "Right-pads a string to a given total width.", "Use for fixed-width field alignment.", new[] { "right pad" }),
        E("newGuid",   "function", "text", "Generates a new random GUID string.", "Use to assign unique identifiers to new records.", new[] { "uuid", "unique id", "guid" }),
        E("parse",     "function", "text", "Parses a JSON string into a JToken.", "Use when a string field actually contains serialised JSON that should become an object.", new[] { "parse json", "deserialise" }),

        E("maxDate",      "function", "timedate", "Returns the latest date from a set of date values or a JSONPath array.",  "Use to find the most recent date in an array — e.g. last activity date.",  new[] { "latest date", "most recent", "last date" }),
        E("minDate",      "function", "timedate", "Returns the earliest date from a set of date values or a JSONPath array.", "Use to find the oldest date in an array — e.g. first order date.",         new[] { "earliest date", "oldest date", "first date" }),
        E("avgDate",      "function", "timedate", "Returns the average (midpoint) date from a set of dates.",               "Use to compute a midpoint between two dates.",                               new[] { "average date", "midpoint date" }),
        E("dateCompare",  "function", "timedate", "Compares two dates; returns -1, 0, or 1.",                               "Use inside ifElse condition expressions to compare dates.",                  new[] { "date comparison", "compare dates", "before after" }),
        E("isDateBetween","function", "timedate", "Returns true if a date falls between a start and end date (inclusive).", "Use in ifElse or decisionTable conditions to test date ranges.",             new[] { "date range", "within period", "between dates" }),
    };

    private static CapabilityEntry E(
        string name, string type, string category,
        string summary, string whenToUse, string[] keywords) =>
        new(name, type, category, summary, whenToUse, keywords);
}
