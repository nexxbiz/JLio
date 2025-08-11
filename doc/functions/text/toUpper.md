# ToUpper Function Documentation

## Overview

The `ToUpper` function converts all alphabetic characters in a string to uppercase. It supports culture-specific conversion rules and is essential for text normalization, comparison operations, and display formatting workflows.

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
// Basic uppercase conversion
"=toUpper('Hello World')"

// Culture-specific conversion
"=toUpper('türkiye', 'tr-TR')"

// JSONPath source
"=toUpper($.text)"
```

### Programmatic Usage
```csharp
// Default culture (invariant)
var toUpperFunction = new ToUpper("text");

// Specific culture
var toUpperFunction = new ToUpper("text", "culture");

// Empty constructor for dynamic arguments
var toUpperFunction = new ToUpper();
```

### Builder Pattern
```csharp
var toUpperFunction = ToUpperBuilders.ToUpper("$.text");
var withCulture = ToUpperBuilders.ToUpper("$.text", "en-US");
```

## Parameters

- **text** (required): The source string to convert
  - **Type**: String, JSONPath expression, or any value
  - **Conversion**: Non-string values are converted to strings
- **culture** (optional): Culture code for culture-specific conversion
  - **Type**: String representing culture code (e.g., "en-US", "tr-TR")
  - **Default**: InvariantCulture for consistent behavior
  - **Validation**: Must be a valid culture identifier

## Return Value

- **Type**: String (JValue)
- **Value**: String with all alphabetic characters converted to uppercase

## Examples

### Basic Uppercase Conversion
```json
{
  "path": "$.uppercaseText",
  "value": "=toUpper('Hello World')",
  "command": "add"
}
```

**Result**:
```json
{
  "uppercaseText": "HELLO WORLD"
}
```

### Culture-Specific Conversion
```json
{
  "path": "$.turkishUpper",
  "value": "=toUpper('istanbul', 'tr-TR')",
  "command": "add"
}
```

**Result**:
```json
{
  "turkishUpper": "?STANBUL"
}
```

### JSONPath Data Conversion
```json
{
  "path": "$.normalizedName",
  "value": "=toUpper($.userName)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "userName": "john.doe"
}
```

**Result**:
```json
{
  "userName": "john.doe",
  "normalizedName": "JOHN.DOE"
}
```

### Array Processing for Standardization
```json
{
  "path": "$.users[*].displayName",
  "value": "=toUpper(@.firstName)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {"firstName": "alice", "lastName": "smith"},
    {"firstName": "bob", "lastName": "johnson"},
    {"firstName": "charlie", "lastName": "brown"}
  ]
}
```

**Result**:
```json
{
  "users": [
    {"firstName": "alice", "lastName": "smith", "displayName": "ALICE"},
    {"firstName": "bob", "lastName": "johnson", "displayName": "BOB"},
    {"firstName": "charlie", "lastName": "brown", "displayName": "CHARLIE"}
  ]
}
```

### Code Generation
```json
{
  "path": "$.constantName",
  "value": "=toUpper(replace($.variableName, ' ', '_'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "variableName": "user name"
}
```

**Result**:
```json
{
  "variableName": "user name",
  "constantName": "USER_NAME"
}
```

### Header Processing
```json
{
  "path": "$.httpHeaders",
  "value": {
    "ContentType": "=toUpper($.headers.contentType)",
    "Authorization": "=toUpper($.headers.auth)",
    "UserAgent": "=toUpper($.headers.userAgent)"
  },
  "command": "add"
}
```

**Input Data**:
```json
{
  "headers": {
    "contentType": "application/json",
    "auth": "bearer token123",
    "userAgent": "mozilla/5.0"
  }
}
```

**Result**:
```json
{
  "headers": {
    "contentType": "application/json",
    "auth": "bearer token123",
    "userAgent": "mozilla/5.0"
  },
  "httpHeaders": {
    "ContentType": "APPLICATION/JSON",
    "Authorization": "BEARER TOKEN123",
    "UserAgent": "MOZILLA/5.0"
  }
}
```

### Status Code Normalization
```json
{
  "path": "$.normalizedStatus",
  "value": "=toUpper(trim($.status))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "status": "  active  "
}
```

**Result**:
```json
{
  "status": "  active  ",
  "normalizedStatus": "ACTIVE"
}
```

### Multi-language Data Processing
```json
[
  {
    "path": "$.englishUpper",
    "value": "=toUpper($.englishText, 'en-US')",
    "command": "add"
  },
  {
    "path": "$.turkishUpper", 
    "value": "=toUpper($.turkishText, 'tr-TR')",
    "command": "add"
  },
  {
    "path": "$.germanUpper",
    "value": "=toUpper($.germanText, 'de-DE')",
    "command": "add"
  }
]
```

## Advanced Usage

### Data Normalization Pipeline
```csharp
var script = new JLioScript()
    .Add(TrimBuilders.Trim("$.userInput"))
    .OnPath("$.step1")
    .Add(ToUpperBuilders.ToUpper("$.step1"))
    .OnPath("$.step2")
    .Add(ReplaceBuilders.Replace("$.step2", " ", "_"))
    .OnPath("$.normalized");
```

### Multi-field Processing
```csharp
var script = new JLioScript()
    .Add(ToUpperBuilders.ToUpper("@.country"))
    .OnPath("$.records[*].countryCode")
    .Add(ToUpperBuilders.ToUpper("@.status"))
    .OnPath("$.records[*].statusCode")
    .Add(ToUpperBuilders.ToUpper("@.category"))
    .OnPath("$.records[*].categoryCode");
```

### Conditional Uppercase
```json
{
  "path": "$.processedValue",
  "value": "=if($.shouldUppercase, toUpper($.value), $.value)",
  "command": "add"
}
```

## Data Type Handling

### String Values
```json
"=toUpper('hello world')"  // Result: "HELLO WORLD"
"=toUpper('MiXeD cAsE')"   // Result: "MIXED CASE"
```

### Number Conversion
```json
"=toUpper(12345)"  // Result: "12345" (numbers unchanged)
```

### Special Characters
```json
"=toUpper('hello@world.com')"  // Result: "HELLO@WORLD.COM"
"=toUpper('café')"             // Result: "CAFÉ" (accented chars handled)
```

### Culture-Specific Examples
```json
"=toUpper('straße', 'de-DE')"  // Result: "STRASSE" (German ß handling)
"=toUpper('istanbul', 'tr-TR')" // Result: "?STANBUL" (Turkish i handling)
```

### Empty/Null Values
```json
"=toUpper('')"         // Result: ""
"=toUpper($.missing)"  // Result: "" (null treated as empty)
```

## Fluent API Usage

### Basic Uppercase
```csharp
var script = new JLioScript()
    .Add(ToUpperBuilders.ToUpper("$.text"))
    .OnPath("$.uppercaseText");
```

### With Culture Specification
```csharp
var script = new JLioScript()
    .Add(ToUpperBuilders.ToUpper("$.turkishText", "tr-TR"))
    .OnPath("$.uppercaseTurkish");
```

### Bulk Processing
```csharp
var script = new JLioScript()
    .Add(ToUpperBuilders.ToUpper("@.name"))
    .OnPath("$.products[*].name")
    .Add(ToUpperBuilders.ToUpper("@.category"))
    .OnPath("$.products[*].category");
```

## Error Handling

### Invalid Argument Count
```json
// Too few arguments
"=toUpper()"  // Logs error: "ToUpper requires 1 or 2 arguments"

// Too many arguments
"=toUpper('text', 'en-US', 'extra')"  // Logs error: "ToUpper requires 1 or 2 arguments"
```

### Invalid Culture Code
```json
// Invalid culture
"=toUpper('text', 'invalid-culture')"  // Logs error: "Invalid culture code: invalid-culture"
```

## Performance Considerations

- **String Length**: Performance scales linearly with string length
- **Culture Processing**: Culture-specific conversion may be slightly slower
- **Memory Usage**: Creates new string instances for results
- **Invariant Culture**: Default invariant culture provides consistent, fast performance

## Best Practices

1. **Consistency**: Use InvariantCulture (default) for data processing consistency
2. **Localization**: Use specific cultures only when needed for user-facing text
3. **Performance**: Default culture is optimized for most scenarios
4. **Validation**: Validate culture codes when accepting user input
5. **Chaining**: Combine with trim() and replace() for comprehensive text processing
6. **Testing**: Test with international characters and various cultures

## Common Patterns

### Data Normalization Pattern
```json
"=toUpper(trim($.input))"
```

### Constant Generation Pattern
```json
"=toUpper(replace($.name, ' ', '_'))"
```

### Case-Insensitive Comparison Pattern
```json
"=toUpper($.value1) == toUpper($.value2)"
```

### Code Generation Pattern
```json
"=concat('CONST_', toUpper(replace($.name, ' ', '_')))"
```

### Header Standardization Pattern
```json
"=toUpper(replace($.headerName, '-', '_'))"
```

## Integration Examples

### With String Functions
```json
"=toUpper(substring($.text, 0, 1))"  // Capitalize first letter
```

### With Conditional Logic
```json
"=if($.isImportant, toUpper($.message), $.message)"
```

### With Replace Function
```json
"=toUpper(replace($.text, ' ', '_'))"  // Create constants
```

### With Validation
```json
"=contains(toUpper($.status), 'ACTIVE')"  // Case-insensitive validation
```

### With Array Processing
```json
"$.items[?toUpper(@.category) == 'ELECTRONICS']"  // Case-insensitive filtering
```

## Culture Examples

### Common Culture Codes
- **en-US**: English (United States)
- **en-GB**: English (United Kingdom)
- **de-DE**: German (Germany)
- **fr-FR**: French (France)
- **tr-TR**: Turkish (Turkey)
- **ru-RU**: Russian (Russia)
- **ja-JP**: Japanese (Japan)

### Culture-Specific Behaviors
```json
"=toUpper('i', 'tr-TR')"    // Result: "?" (Turkish dotted I)
"=toUpper('i', 'en-US')"    // Result: "I" (English dotless I)
"=toUpper('ß', 'de-DE')"    // Result: "SS" (German sharp s)
```

## Comparison with ToLower

| Function | Use Case | Example |
|----------|----------|---------|
| **ToUpper** | Constants, headers, emphasis | `"HELLO WORLD"` |
| **ToLower** | URLs, filenames, normalization | `"hello world"` |

### Usage Decision
- Use **ToUpper** for constants, headers, and visual emphasis
- Use **ToLower** for URLs, identifiers, and general normalization
- Consider your specific use case and target system requirements