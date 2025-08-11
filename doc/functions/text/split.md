# Split Function Documentation

## Overview

The `Split` function divides a string into an array of substrings based on a specified delimiter. It supports advanced options like maximum splits and empty string removal, making it essential for parsing structured data, CSV processing, and text analysis workflows.

## Installation

### Extension Pack Registration
```csharp
// Register text functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterText();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Syntax

### Function Expression Formats
```json
// Basic split by delimiter
"=split('apple,banana,cherry', ',')"

// Split with maximum number of splits
"=split('a-b-c-d-e', '-', 2)"

// Split with empty string removal
"=split('a,,b,,c', ',', -1, true)"

// Split by whitespace (empty separator)
"=split('hello world test', '')"
```

### Programmatic Usage
```csharp
// Basic split
var splitFunction = new Split("text", "separator");

// With max splits
var splitFunction = new Split("text", "separator", "maxSplits");

// With all options
var splitFunction = new Split("text", "separator", "maxSplits", "removeEmpty");

// Empty constructor for dynamic arguments
var splitFunction = new Split();
```

### Builder Pattern
```csharp
var splitFunction = SplitBuilders.Split("$.csvData", ",");
var limitedSplit = SplitBuilders.Split("$.text", " ", "3");
var cleanSplit = SplitBuilders.Split("$.text", ",", "-1", "true");
```

## Parameters

- **text** (required): The source string to split
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **separator** (required): The delimiter string to split on
  - **Type**: String, JSONPath expression, or any value
  - **Empty String**: Splits by whitespace characters
- **maxSplits** (optional): Maximum number of splits to perform
  - **Type**: Integer or string representation of integer
  - **Default**: Unlimited (-1 or int.MaxValue)
  - **Behavior**: Results in maxSplits + 1 elements maximum
- **removeEmpty** (optional): Whether to remove empty strings from result
  - **Type**: Boolean, string representation, or integer (0/1)
  - **Default**: false (keep empty strings)

## Return Value

- **Type**: Array (JArray)
- **Value**: Array of string elements from the split operation

## Examples

### Basic CSV Parsing
```json
{
  "path": "$.fields",
  "value": "=split('name,age,city', ',')",
  "command": "add"
}
```

**Result**:
```json
{
  "fields": ["name", "age", "city"]
}
```

### Limited Splits
```json
{
  "path": "$.parts",
  "value": "=split('a-b-c-d-e', '-', 2)",
  "command": "add"
}
```

**Result**:
```json
{
  "parts": ["a", "b", "c-d-e"]
}
```

### Remove Empty Strings
```json
{
  "path": "$.cleanParts",
  "value": "=split('a,,b,,c', ',', -1, true)",
  "command": "add"
}
```

**Result**:
```json
{
  "cleanParts": ["a", "b", "c"]
}
```

### Whitespace Splitting
```json
{
  "path": "$.words",
  "value": "=split('hello world test', '')",
  "command": "add"
}
```

**Result**:
```json
{
  "words": ["hello", "world", "test"]
}
```

### JSONPath Data Processing
```json
{
  "path": "$.tags",
  "value": "=split($.tagString, ',')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "tagString": "javascript,web,frontend,react"
}
```

**Result**:
```json
{
  "tagString": "javascript,web,frontend,react",
  "tags": ["javascript", "web", "frontend", "react"]
}
```

### Array Processing for Multiple Records
```json
{
  "path": "$.records[*].skills",
  "value": "=split(@.skillsString, ',')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "records": [
    {"name": "Alice", "skillsString": "java,python,sql"},
    {"name": "Bob", "skillsString": "javascript,react,node"}
  ]
}
```

**Result**:
```json
{
  "records": [
    {"name": "Alice", "skillsString": "java,python,sql", "skills": ["java", "python", "sql"]},
    {"name": "Bob", "skillsString": "javascript,react,node", "skills": ["javascript", "react", "node"]}
  ]
}
```

### Path Parsing
```json
{
  "path": "$.pathComponents",
  "value": "=split($.filePath, '/')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filePath": "/home/user/documents/file.txt"
}
```

**Result**:
```json
{
  "filePath": "/home/user/documents/file.txt",
  "pathComponents": ["", "home", "user", "documents", "file.txt"]
}
```

### Email Processing
```json
[
  {
    "path": "$.emailParts",
    "value": "=split($.email, '@')",
    "command": "add"
  },
  {
    "path": "$.username",
    "value": "=$.emailParts[0]",
    "command": "add"
  },
  {
    "path": "$.domain",
    "value": "=$.emailParts[1]",
    "command": "add"
  }
]
```

## Advanced Usage

### CSV Processing Pipeline
```csharp
var script = new JLioScript()
    .Add(SplitBuilders.Split("@.csvRow", ","))
    .OnPath("$.csvData[*].fields")
    .Add(TrimBuilders.Trim("@[0]"))
    .OnPath("$.csvData[*].name")
    .Add(TrimBuilders.Trim("@[1]"))
    .OnPath("$.csvData[*].age");
```

### Log Analysis
```csharp
var script = new JLioScript()
    .Add(SplitBuilders.Split("@.logLine", " ", "4"))
    .OnPath("$.logs[*].parts")
    .Add("@.parts[0]")
    .OnPath("$.logs[*].timestamp")
    .Add("@.parts[1]")
    .OnPath("$.logs[*].level");
```

### Multi-level Parsing
```json
[
  {
    "path": "$.categories",
    "value": "=split($.categoryPath, '/')",
    "command": "add"
  },
  {
    "path": "$.subcategories",
    "value": "=split($.categories[-1], ',')",
    "command": "add"
  }
]
```

## Data Type Handling

### String Values
```json
"=split('a,b,c', ',')"  // Result: ["a", "b", "c"]
```

### Number Conversion
```json
"=split(12345, '3')"  // Result: ["12", "45"] (from "12345")
```

### Empty String Handling
```json
"=split('', ',')"  // Result: [""] (array with one empty string)
"=split('', ',', -1, true)"  // Result: [] (empty array when removing empties)
```

### Special Separators
```json
"=split('a\nb\nc', '\n')"  // Result: ["a", "b", "c"] (newline separator)
"=split('a\tb\tc', '\t')"  // Result: ["a", "b", "c"] (tab separator)
```

## Fluent API Usage

### Basic Split
```csharp
var script = new JLioScript()
    .Add(SplitBuilders.Split("$.csvRow", ","))
    .OnPath("$.fields");
```

### Multiple Splits
```csharp
var script = new JLioScript()
    .Add(SplitBuilders.Split("@.tags", ","))
    .OnPath("$.posts[*].tagArray")
    .Add(SplitBuilders.Split("@.categories", "/"))
    .OnPath("$.posts[*].categoryArray");
```

### Conditional Split
```csharp
var script = new JLioScript()
    .Add(SplitBuilders.Split("$.data", 
        "if(contains($.data, ','), ',', ';')"))
    .OnPath("$.parsedData");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=split('text')"  // Logs error: "Split requires 2 to 4 arguments"

// Too many arguments
"=split('text', ',', 2, true, 'extra')"  // Logs error: "Split requires 2 to 4 arguments"
```

### Invalid Numeric Parameters
```json
// Invalid maxSplits
"=split('text', ',', 'abc')"  // Logs error: "maxSplits must be a non-negative integer"

// Invalid removeEmpty
"=split('text', ',', 2, 'invalid')"  // Logs error: "removeEmpty must be a valid boolean"
```

## Performance Considerations

- **String Length**: Performance scales with source string length
- **Split Count**: More splits require more processing
- **Memory Usage**: Creates array and string objects for results
- **Separator Complexity**: Simple single-character separators are fastest

## Best Practices

1. **Separator Choice**: Use appropriate separators for your data format
2. **Empty Handling**: Consider whether to keep or remove empty strings
3. **Limit Splits**: Use maxSplits when you only need the first few parts
4. **Memory Management**: Be aware of memory usage with large strings and many splits
5. **Validation**: Validate input format before splitting
6. **Combine with Trim**: Often useful to trim results after splitting

## Common Patterns

### CSV Field Extraction Pattern
```json
"=split(trim($.csvRow), ',')"
```

### Path Component Pattern
```json
"=split($.path, '/')"
```

### Tag Processing Pattern
```json
"=split(replace($.tags, ' ', ''), ',')"
```

### Limited Split Pattern
```json
"=split($.fullName, ' ', 1)"  // Split into first name and rest
```

### Clean Split Pattern
```json
"=split($.list, ',', -1, true)"  // Remove any empty entries
```

## Integration Examples

### With Array Functions
```json
"=length(split($.csvData, ','))"  // Count fields
```

### With Join Function
```json
"=join('-', split($.spacedText, ' '))"  // Convert spaces to dashes
```

### With Filtering
```json
"=split($.tags, ',')[?length(@) > 0]"  // Filter out empty tags
```