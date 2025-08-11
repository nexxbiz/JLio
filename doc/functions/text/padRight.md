# PadRight Function Documentation

## Overview

The `PadRight` function pads a string with specified characters on the right side to reach a desired total width. It's essential for creating fixed-width output, formatting tables, aligning text, and generating structured data displays.

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
// Pad with spaces to specified width
"=padRight('Hello', 10)"

// Pad with custom character
"=padRight('Hello', 10, '-')"

// JSONPath source
"=padRight($.text, $.width, $.padChar)"
```

### Programmatic Usage
```csharp
// Pad with spaces (default)
var padRightFunction = new PadRight("text", "totalWidth");

// Pad with custom character
var padRightFunction = new PadRight("text", "totalWidth", "padChar");

// Empty constructor for dynamic arguments
var padRightFunction = new PadRight();
```

### Builder Pattern
```csharp
var padRightFunction = PadRightBuilders.PadRight("$.text", "20");
var customPad = PadRightBuilders.PadRight("$.code", "12", "0");
```

## Parameters

- **text** (required): The source string to pad
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **totalWidth** (required): The desired total width of the result string
  - **Type**: Integer or string representation of integer
  - **Validation**: Must be non-negative
  - **Behavior**: If text is longer than totalWidth, original text is returned
- **padChar** (optional): The character to use for padding
  - **Type**: String (first character is used)
  - **Default**: Space character (' ')
  - **Empty String**: Uses space character if empty

## Return Value

- **Type**: String (JValue)
- **Value**: String padded on the right to reach totalWidth

## Examples

### Basic Right Padding with Spaces
```json
{
  "path": "$.paddedText",
  "value": "=padRight('Hello', 10)",
  "command": "add"
}
```

**Result**:
```json
{
  "paddedText": "Hello     "
}
```

### Custom Character Padding
```json
{
  "path": "$.paddedCode",
  "value": "=padRight('ABC', 8, '0')",
  "command": "add"
}
```

**Result**:
```json
{
  "paddedCode": "ABC00000"
}
```

### Table Column Formatting
```json
{
  "path": "$.formattedRow",
  "value": "=concat(padRight($.name, 20), padRight($.department, 15), padRight(toString($.salary), 10))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "name": "John Doe",
  "department": "Engineering",
  "salary": 75000
}
```

**Result**:
```json
{
  "name": "John Doe",
  "department": "Engineering", 
  "salary": 75000,
  "formattedRow": "John Doe            Engineering    75000     "
}
```

### Array Processing for Reports
```json
{
  "path": "$.employees[*].formattedName",
  "value": "=padRight(@.firstName, 15)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "employees": [
    {"firstName": "Alice", "lastName": "Smith"},
    {"firstName": "Bob", "lastName": "Johnson"},
    {"firstName": "Charlie", "lastName": "Brown"}
  ]
}
```

**Result**:
```json
{
  "employees": [
    {"firstName": "Alice", "lastName": "Smith", "formattedName": "Alice          "},
    {"firstName": "Bob", "lastName": "Johnson", "formattedName": "Bob            "},
    {"firstName": "Charlie", "lastName": "Brown", "formattedName": "Charlie        "}
  ]
}
```

### Fixed-Width Data Generation
```json
{
  "path": "$.fixedWidthRecord",
  "value": "=concat(padRight($.id, 8, '0'), padRight($.name, 25), padRight($.status, 10))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "id": "123",
  "name": "Sample Product",
  "status": "Active"
}
```

**Result**:
```json
{
  "id": "123",
  "name": "Sample Product",
  "status": "Active",
  "fixedWidthRecord": "12300000Sample Product        Active    "
}
```

### CSV Column Alignment
```json
{
  "path": "$.csvRow",
  "value": "=join(',', padRight($.field1, 12), padRight($.field2, 20), padRight($.field3, 8))",
  "command": "add"
}
```

### Progress Bar Generation
```json
{
  "path": "$.progressBar",
  "value": "=concat('[', padRight(repeat('=', $.completed), $.total, ' '), ']')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "completed": 7,
  "total": 10
}
```

**Result**:
```json
{
  "completed": 7,
  "total": 10,
  "progressBar": "[=======   ]"
}
```

## Advanced Usage

### Multi-Column Report Generation
```csharp
var script = new JLioScript()
    .Add(PadRightBuilders.PadRight("@.name", "20"))
    .OnPath("$.reports[*].col1")
    .Add(PadRightBuilders.PadRight("@.department", "15"))
    .OnPath("$.reports[*].col2")
    .Add(PadRightBuilders.PadRight("toString(@.salary)", "10"))
    .OnPath("$.reports[*].col3");
```

### Fixed-Width File Export
```csharp
var script = new JLioScript()
    .Add(PadRightBuilders.PadRight("@.id", "8", "0"))
    .OnPath("$.records[*].idField")
    .Add(PadRightBuilders.PadRight("@.name", "30"))
    .OnPath("$.records[*].nameField")
    .Add(PadRightBuilders.PadRight("@.code", "10", " "))
    .OnPath("$.records[*].codeField");
```

### Dynamic Width Calculation
```json
{
  "path": "$.dynamicPadding",
  "value": "=padRight($.text, max(length($.text) + 5, 20))",
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=padRight('hello', 10)"  // Result: "hello     "
```

### Number Conversion
```json
"=padRight(123, 8, '0')"  // Result: "12300000" (from "123")
```

### Empty String
```json
"=padRight('', 5, '-')"  // Result: "-----"
```

### Text Longer Than Width
```json
"=padRight('Hello World', 5)"  // Result: "Hello World" (unchanged)
```

## Fluent API Usage

### Basic Right Padding
```csharp
var script = new JLioScript()
    .Add(PadRightBuilders.PadRight("$.code", "10"))
    .OnPath("$.paddedCode");
```

### Custom Character Padding
```csharp
var script = new JLioScript()
    .Add(PadRightBuilders.PadRight("$.number", "8", "0"))
    .OnPath("$.paddedNumber");
```

### Report Column Formatting
```csharp
var script = new JLioScript()
    .Add(PadRightBuilders.PadRight("@.firstName", "15"))
    .OnPath("$.employees[*].col1")
    .Add(PadRightBuilders.PadRight("@.lastName", "15"))
    .OnPath("$.employees[*].col2")
    .Add(PadRightBuilders.PadRight("@.department", "20"))
    .OnPath("$.employees[*].col3");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=padRight('text')"  // Logs error: "PadRight requires 2 or 3 arguments"

// Too many arguments
"=padRight('text', 10, '-', 'extra')"  // Logs error: "PadRight requires 2 or 3 arguments"
```

### Invalid Width Parameter
```json
// Non-numeric width
"=padRight('text', 'abc')"  // Logs error: "totalWidth must be a non-negative integer"

// Negative width
"=padRight('text', -5)"  // Logs error: "totalWidth must be a non-negative integer"
```

## Performance Considerations

- **String Length**: Performance scales with target width
- **Character Count**: More padding characters require more memory
- **Memory Usage**: Creates new string instances for results
- **Large Widths**: Very large widths may impact memory usage

## Best Practices

1. **Width Validation**: Ensure totalWidth is appropriate for your use case
2. **Character Selection**: Choose padding characters that won't interfere with data processing
3. **Performance**: Consider memory usage with very large widths
4. **Data Integrity**: Ensure original text isn't longer than expected width
5. **Consistency**: Use consistent widths across related data
6. **Testing**: Test with various text lengths and edge cases

## Common Patterns

### Fixed-Width Record Pattern
```json
"=padRight($.field, 20)"
```

### Zero-Padding Pattern
```json
"=padRight(toString($.number), 8, '0')"
```

### Table Column Pattern
```json
"=concat(padRight($.col1, 15), padRight($.col2, 20), padRight($.col3, 10))"
```

### Progress Indicator Pattern
```json
"=padRight(repeat('=', $.progress), $.maxWidth, ' ')"
```

### Code Formatting Pattern
```json
"=padRight($.identifier, 12, '_')"
```

## Integration Examples

### With String Functions
```json
"=padRight(toUpper($.text), 15)"  // Uppercase and pad
```

### With Math Functions
```json
"=padRight($.name, max(20, length($.name) + 5))"  // Dynamic width
```

### With Conditional Logic
```json
"=padRight($.text, if(length($.text) < 10, 15, length($.text) + 5))"
```

### Report Generation
```json
[
  {
    "path": "$.header",
    "value": "=concat(padRight('Name', 20), padRight('Department', 15), padRight('Salary', 10))",
    "command": "add"
  },
  {
    "path": "$.employees[*].row", 
    "value": "=concat(padRight(@.name, 20), padRight(@.department, 15), padRight(toString(@.salary), 10))",
    "command": "add"
  }
]
```

## Comparison with PadLeft

| Function | Padding Side | Use Case |
|----------|-------------|----------|
| **PadRight** | Right side | Text alignment, table columns, fixed-width records |
| **PadLeft** | Left side | Number formatting, ID padding, right-aligned text |

### Usage Decision
- Use **PadRight** for left-aligned text and data fields
- Use **PadLeft** for right-aligned numbers and codes
- Combine both for complex formatting scenarios