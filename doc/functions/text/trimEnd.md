# TrimEnd Function Documentation

## Overview

The `TrimEnd` function removes whitespace characters or specified characters from the end (right side) of a string. It's essential for data cleaning, user input processing, and text normalization workflows where trailing characters need to be removed.

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
// Remove whitespace from end
"=trimEnd('Hello World  ')"

// Remove specific characters from end
"=trimEnd('Hello World---', '-')"

// JSONPath source
"=trimEnd($.userInput)"
```

### Programmatic Usage
```csharp
// Default whitespace trimming
var trimEndFunction = new TrimEnd("text");

// Custom character trimming
var trimEndFunction = new TrimEnd("text", "characters");

// Empty constructor for dynamic arguments
var trimEndFunction = new TrimEnd();
```

### Builder Pattern
```csharp
var trimEndFunction = TrimEndBuilders.TrimEnd("$.text");
var customTrim = TrimEndBuilders.TrimEnd("$.text", ".,!?");
```

## Parameters

- **text** (required): The source string to trim
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **trimChars** (optional): Characters to remove from the end
  - **Type**: String containing characters to trim
  - **Default**: Whitespace characters (spaces, tabs, newlines, etc.)
  - **Usage**: Each character in the string is used as a trim character

## Return Value

- **Type**: String (JValue)
- **Value**: String with specified characters removed from the end

## Examples

### Basic Whitespace Trimming
```json
{
  "path": "$.cleaned",
  "value": "=trimEnd('   Hello World   ')",
  "command": "add"
}
```

**Result**:
```json
{
  "cleaned": "   Hello World"
}
```

### Custom Character Trimming
```json
{
  "path": "$.trimmed",
  "value": "=trimEnd('---Hello World---', '-')",
  "command": "add"
}
```

**Result**:
```json
{
  "trimmed": "---Hello World"
}
```

### Multiple Character Trimming
```json
{
  "path": "$.cleaned",
  "value": "=trimEnd('.,!Hello World!,.', '.!,')",
  "command": "add"
}
```

**Result**:
```json
{
  "cleaned": ".,!Hello World"
}
```

### File Extension Removal
```json
{
  "path": "$.baseFilename",
  "value": "=trimEnd($.filename, '.pdf.txt.doc')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filename": "document.pdf"
}
```

**Result**:
```json
{
  "filename": "document.pdf",
  "baseFilename": "document"
}
```

### Array Processing for Data Cleanup
```json
{
  "path": "$.products[*].cleanName",
  "value": "=trimEnd(@.name, ' \t\n')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "products": [
    {"name": "Laptop \t"},
    {"name": "Mouse\n"},
    {"name": "Keyboard   "}
  ]
}
```

**Result**:
```json
{
  "products": [
    {"name": "Laptop \t", "cleanName": "Laptop"},
    {"name": "Mouse\n", "cleanName": "Mouse"},
    {"name": "Keyboard   ", "cleanName": "Keyboard"}
  ]
}
```

### Punctuation Removal
```json
{
  "path": "$.cleanSentence",
  "value": "=trimEnd($.sentence, '.!?,')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "sentence": "This is a great example!"
}
```

**Result**:
```json
{
  "sentence": "This is a great example!",
  "cleanSentence": "This is a great example"
}
```

### SQL Statement Cleanup
```json
{
  "path": "$.cleanSQL",
  "value": "=trimEnd($.sqlStatement, '; \t\n')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "sqlStatement": "SELECT * FROM users; \n"
}
```

**Result**:
```json
{
  "sqlStatement": "SELECT * FROM users; \n",
  "cleanSQL": "SELECT * FROM users"
}
```

### Trailing Zero Removal from Decimals
```json
{
  "path": "$.cleanDecimal",
  "value": "=trimEnd($.decimalString, '0')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "decimalString": "12.34500"
}
```

**Result**:
```json
{
  "decimalString": "12.34500",
  "cleanDecimal": "12.345"
}
```

### Data Validation Pipeline
```json
[
  {
    "path": "$.step1",
    "value": "=trimEnd($.rawInput)",
    "command": "add"
  },
  {
    "path": "$.step2",
    "value": "=trimEnd($.step1, '.,;')",
    "command": "add"
  },
  {
    "path": "$.isValid",
    "value": "=length($.step2) > 0",
    "command": "add"
  }
]
```

## Advanced Usage

### Multi-step Text Cleaning
```csharp
var script = new JLioScript()
    .Add(TrimEndBuilders.TrimEnd("$.userInput"))
    .OnPath("$.step1")
    .Add(TrimEndBuilders.TrimEnd("$.step1", ".!?"))
    .OnPath("$.step2")
    .Add(TrimEndBuilders.TrimEnd("$.step2", ",;"))
    .OnPath("$.cleaned");
```

### CSV Field Processing
```csharp
var script = new JLioScript()
    .Add(SplitBuilders.Split("$.csvRow", ","))
    .OnPath("$.fields")
    .Add(TrimEndBuilders.TrimEnd("@", " \""))
    .OnPath("$.fields[*]");
```

### Log Entry Cleanup
```json
{
  "path": "$.cleanLogEntries",
  "value": "$.logEntries.map(x => trimEnd(x, ' \t\n\r'))",
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=trimEnd('hello  ')"  // Result: "hello"
```

### Number Conversion
```json
"=trimEnd(12300, '0')"  // Result: "123" (converted from "12300")
```

### Empty String
```json
"=trimEnd('')"  // Result: "" (empty string)
"=trimEnd('   ')"  // Result: "" (only whitespace)
```

### Null Values
```json
"=trimEnd($.missing)"  // Result: "" (null treated as empty)
```

## Fluent API Usage

### Basic Right Trimming
```csharp
var script = new JLioScript()
    .Add(TrimEndBuilders.TrimEnd("$.input"))
    .OnPath("$.cleaned");
```

### Custom Character Trimming
```csharp
var script = new JLioScript()
    .Add(TrimEndBuilders.TrimEnd("$.filename", ".pdf.doc.txt"))
    .OnPath("$.basename");
```

### Bulk Data Cleaning
```csharp
var script = new JLioScript()
    .Add(TrimEndBuilders.TrimEnd("@.description"))
    .OnPath("$.records[*].description")
    .Add(TrimEndBuilders.TrimEnd("@.notes", "."))
    .OnPath("$.records[*].notes");
```

## Related Functions

### Trim (Both Sides)
```json
"=trim('  hello  ')"  // Result: "hello" (both sides trimmed)
```

### TrimStart (Left Side)
```json
"=trimStart('  hello  ')"  // Result: "hello  " (only left side trimmed)
```

### Combined Usage
```json
{
  "path": "$.fullyProcessed",
  "value": "=trimStart(trimEnd($.input, '.'), '#')",
  "command": "add"
}
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=trimEnd()"  // Logs error: "TrimEnd requires 1 or 2 arguments"

// Too many arguments
"=trimEnd('text', 'chars', 'extra')"  // Logs error: "TrimEnd requires 1 or 2 arguments"
```

## Performance Considerations

- **String Length**: Performance scales with source string length
- **Character Set**: Larger trim character sets may impact performance slightly
- **Memory Usage**: Creates new string instances for results
- **Whitespace Detection**: Default whitespace trimming is optimized

## Best Practices

1. **Input Validation**: Use trimEnd for cleaning user inputs and data
2. **Chain Operations**: Combine with other string functions for complete processing
3. **File Processing**: Useful for removing file extensions and suffixes
4. **Data Format**: Clean up formatted data by removing trailing delimiters
5. **Performance**: Use default whitespace trimming when possible
6. **Testing**: Test with various input scenarios including edge cases

## Common Patterns

### User Input Sanitization Pattern
```json
"=trimEnd($.userInput)"  // Basic input cleaning
```

### File Extension Removal Pattern
```json
"=trimEnd($.filename, '.pdf.doc.txt')"  // Remove file extensions
```

### Punctuation Cleanup Pattern
```json
"=trimEnd($.sentence, '.!?,')"  // Remove trailing punctuation
```

### SQL Statement Cleanup Pattern
```json
"=trimEnd($.sql, '; \t\n')"  // Clean SQL statements
```

### Decimal Formatting Pattern
```json
"=trimEnd($.number, '0')"  // Remove trailing zeros
```

## Integration Examples

### With String Functions
```json
"=toUpper(trimEnd($.text, '.'))"  // Trim then uppercase
```

### With Conditional Logic
```json
"=if(endsWith($.text, '?'), trimEnd($.text, '?'), $.text)"
```

### With Replace Function
```json
"=replace(trimEnd($.text, '.'), 'old', 'new')"
```

### With Validation
```json
"=length(trimEnd($.field)) > 0 && !endsWith(trimEnd($.field), '.')"
```

### With File Operations
```json
"=concat(trimEnd($.filename, '.pdf'), '_processed.pdf')"
```