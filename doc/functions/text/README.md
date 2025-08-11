# Text Functions Documentation

This directory contains comprehensive documentation for all JLio text manipulation functions.

## Available Functions

### Basic String Operations
- [length](length.md) - Get string length
- [substring](substring.md) - Extract substring with support for negative indices
- [indexOf](indexOf.md) - Find position of substring with optional case sensitivity
- [replace](replace.md) - Replace text with case-sensitive/insensitive options

### String Trimming
- [trim](trim.md) - Remove whitespace or specific characters from both ends
- [trimStart](trimStart.md) - Remove whitespace or specific characters from the beginning
- [trimEnd](trimEnd.md) - Remove whitespace or specific characters from the end

### Case Conversion
- [toUpper](toUpper.md) - Convert strings to uppercase with optional culture support
- [toLower](toLower.md) - Convert strings to lowercase with optional culture support

### String Testing
- [contains](contains.md) - Check if text contains a substring
- [startsWith](startsWith.md) - Check if text starts with a prefix
- [endsWith](endsWith.md) - Check if text ends with a suffix
- [isEmpty](isEmpty.md) - Check if value is empty (string, array, object, or null)

### String Formatting
- [padLeft](padLeft.md) - Pad strings with characters on the left side
- [padRight](padRight.md) - Pad strings with characters on the right side

### Array/String Conversion
- [split](split.md) - Split strings into arrays with separator and options
- [join](join.md) - Join array elements or multiple values with a separator

### Existing Functions
- [concat](../concat.md) - Concatenate strings (already documented)
- [format](../format.md) - String formatting (already documented)
- [parse](../parse.md) - Parse JSON strings (already documented)
- [toString](../toString.md) - Convert values to strings (already documented)

## Installation

### Extension Pack Registration
```csharp
// Register text functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterText();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Usage Patterns

### Basic Text Manipulation
```json
{
  "path": "$.processedText",
  "value": "=toUpper(trim($.userInput))",
  "command": "add"
}
```

### String Validation and Cleanup
```json
{
  "path": "$.isValid",
  "value": "=startsWith(trim($.email), '@') == false",
  "command": "add"
}
```

### Text Processing Pipeline
```csharp
var script = new JLioScript()
    .Add(TrimBuilders.Trim("$.rawText"))
    .OnPath("$.cleanText")
    .Add(ToLowerBuilders.ToLower("$.cleanText"))
    .OnPath("$.normalizedText")
    .Add(ReplaceBuilders.Replace("$.normalizedText", " ", "_"))
    .OnPath("$.slugText");
```

## Common Use Cases

### Data Cleanup
- Trimming whitespace from user inputs
- Normalizing case for comparisons
- Removing unwanted characters

### Text Processing
- Splitting CSV-like strings
- Joining data into formatted strings
- Extracting substrings from structured data

### Validation
- Checking string formats
- Validating prefixes/suffixes
- Testing for empty values

### Formatting
- Padding for fixed-width output
- Converting data to display formats
- Creating identifiers and codes

## Error Handling

All text functions include comprehensive error handling:
- Invalid argument counts
- Type validation for parameters
- Culture validation for case conversion
- Index bounds checking for substring operations
- Graceful handling of null/empty inputs

## Performance Considerations

- String functions use InvariantCulture for consistent behavior
- Large string operations may require memory considerations
- Complex regular expressions (future functions) may impact performance
- Consider caching results for repeated operations

## Best Practices

1. **Input Validation**: Always validate input data before processing
2. **Culture Awareness**: Use culture-specific functions when needed
3. **Error Handling**: Check execution logs for processing errors
4. **Performance**: Consider data size for bulk string operations
5. **Testing**: Test with various input scenarios including edge cases