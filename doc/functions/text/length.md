# Length Function Documentation

## Overview

The `Length` function returns the number of characters in a string value. It handles various data types gracefully and provides consistent string length calculation for text processing workflows.

## Installation

### Extension Pack Registration
```csharp
// Register text functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterText();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Syntax

### Function Expression Format
```json
// Get length of a literal string
"=length('hello world')"

// Get length from JSONPath expression
"=length($.userInput)"

// Get length of current token
"=length(@.name)"
```

### Programmatic Usage
```csharp
// With literal argument
var lengthFunction = new Length("hello");

// Empty constructor for dynamic arguments
var lengthFunction = new Length();
```

### Builder Pattern
```csharp
var lengthFunction = LengthBuilders.Length("$.text");
var pathLength = LengthBuilders.Length("@.description");
```

## Parameters

- **text** (required): The string value to measure
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings before measurement

## Return Value

- **Type**: Integer (JValue)
- **Value**: Number of characters in the string

## Examples

### Basic Length Calculation
```json
{
  "path": "$.textLength",
  "value": "=length('Hello World')",
  "command": "add"
}
```

**Result**:
```json
{
  "textLength": 11
}
```

### JSONPath Length Calculation
```json
{
  "path": "$.nameLength",
  "value": "=length($.user.name)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "user": {
    "name": "John Doe"
  }
}
```

**Result**:
```json
{
  "user": {
    "name": "John Doe"
  },
  "nameLength": 8
}
```

### Array Processing
```json
{
  "path": "$.users[*].nameLength",
  "value": "=length(@.name)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"name": "Alice"},
    {"name": "Bob"},
    {"name": "Charlie"}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"name": "Alice", "nameLength": 5},
    {"name": "Bob", "nameLength": 3},
    {"name": "Charlie", "nameLength": 7}
  ]
}
```

### Empty String Handling
```json
{
  "path": "$.emptyLength",
  "value": "=length('')",
  "command": "add"
}
```

**Result**:
```json
{
  "emptyLength": 0
}
```

### Missing Property Handling
```json
{
  "path": "$.missingLength",
  "value": "=length($.nonExistent)",
  "command": "add"
}
```

**Result**:
```json
{
  "missingLength": 0
}
```

### Validation Use Case
```json
{
  "path": "$.isValidPassword",
  "value": "=length($.password) >= 8",
  "command": "add"
}
```

**Input Data**:
```json
{
  "password": "mypassword123"
}
```

**Result**:
```json
{
  "password": "mypassword123",
  "isValidPassword": true
}
```

## Data Type Handling

### String Values
```json
"=length('Hello')"  // Result: 5
```

### Null Values
```json
"=length($.missing)"  // Result: 0 (null treated as empty string)
```

### Number Conversion
```json
"=length(12345)"  // Result: 5 (converted to "12345")
```

### Boolean Conversion
```json
"=length(true)"   // Result: 4 (converted to "true")
"=length(false)"  // Result: 5 (converted to "false")
```

## Advanced Usage

### Text Validation Pipeline
```csharp
var script = new JLioScript()
    .Add(LengthBuilders.Length("$.username"))
    .OnPath("$.usernameLength")
    .Add(LengthBuilders.Length("$.email"))
    .OnPath("$.emailLength")
    .Add(LengthBuilders.Length("$.description"))
    .OnPath("$.descriptionLength");
```

### Conditional Processing Based on Length
```json
[
  {
    "path": "$.textLength",
    "value": "=length($.content)",
    "command": "add"
  },
  {
    "path": "$.contentType",
    "value": "=if(length($.content) > 100, 'long', 'short')",
    "command": "add"
  }
]
```

### Data Quality Assessment
```csharp
var script = new JLioScript()
    .Add(LengthBuilders.Length("@.firstName"))
    .OnPath("$.records[*].firstNameLength")
    .Add(LengthBuilders.Length("@.lastName"))
    .OnPath("$.records[*].lastNameLength")
    .Add(LengthBuilders.Length("@.email"))
    .OnPath("$.records[*].emailLength");
```

## Fluent API Usage

### Basic Length Check
```csharp
var script = new JLioScript()
    .Add(LengthBuilders.Length("$.text"))
    .OnPath("$.textLength");
```

### Multiple Length Calculations
```csharp
var script = new JLioScript()
    .Add(LengthBuilders.Length("$.title"))
    .OnPath("$.titleLength")
    .Add(LengthBuilders.Length("$.description"))
    .OnPath("$.descriptionLength")
    .Add(LengthBuilders.Length("$.tags[*]"))
    .OnPath("$.tagLengths[*]");
```

## Error Handling

### Invalid Arguments
```json
// Too many arguments
"=length('text', 'extra')"  // Logs error: "Length requires exactly one argument"
```

### No Arguments
```json
// No arguments provided
"=length()"  // Logs error: "Length requires exactly one argument"
```

## Performance Considerations

- **String Conversion**: Non-string types require conversion overhead
- **Large Strings**: Memory usage scales with string size
- **Bulk Processing**: Consider performance impact when processing large datasets
- **JSONPath Resolution**: Complex paths may require additional processing time

## Best Practices

1. **Input Validation**: Ensure expected data types when possible
2. **Null Handling**: Remember that null values return 0
3. **Performance**: Use direct string values when path resolution isn't needed
4. **Error Checking**: Monitor execution logs for processing errors
5. **Testing**: Test with various input types including edge cases

## Common Patterns

### Validation Pattern
```json
"=length($.field) > 0"  // Check if field has content
```

### Size Classification Pattern
```json
"=if(length($.text) > 50, 'long', 'short')"  // Classify by length
```

### Data Quality Pattern
```json
"=length($.requiredField) == 0"  // Check for missing data
```

### Formatting Decision Pattern
```json
"=if(length($.title) > 20, substring($.title, 0, 20) + '...', $.title)"
```