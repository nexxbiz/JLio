# Parse Function Documentation

## Overview

The `Parse` function converts string values into JSON tokens by parsing their content. It's the opposite of ToString, taking JSON string representations and converting them back into proper JSON objects, arrays, or primitive values.

## Syntax

### Function Expression Format
```json
// Parse current token (must be string)
"=parse()"

// Parse specific path value
"=parse($.jsonString)"

// Parse fetched value
"=parse(fetch($.serializedData))"
```

### Programmatic Usage
```csharp
// Parse current token
var parseFunction = new Parse();

// Parse specific path
var parseFunction = new Parse("$.jsonString");
```

### Builder Pattern
```csharp
var parseFunction = ParseBuilders.Parse();
var pathFunction = ParseBuilders.Parse("$.jsonString");
```

## Parameters

- **Path (Optional)**: JSONPath expression to string value to parse
- **No Arguments**: Parses current token (must be string type)
- **Return Type**: Parsed JSON token (object, array, or primitive)

## Examples

### Basic JSON Object Parsing
```json
{
  "path": "$.parsedConfig",
  "value": "=parse($.configString)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "configString": "{\"theme\":\"dark\",\"language\":\"en\"}"
}
```

**Result**:
```json
{
  "configString": "{\"theme\":\"dark\",\"language\":\"en\"}",
  "parsedConfig": {
    "theme": "dark",
    "language": "en"
  }
}
```

### Array Parsing
```json
{
  "path": "$.parsedItems",
  "value": "=parse($.itemsString)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "itemsString": "[1,2,3,\"four\",true]"
}
```

**Result**:
```json
{
  "itemsString": "[1,2,3,\"four\",true]",
  "parsedItems": [1, 2, 3, "four", true]
}
```

### Primitive Value Parsing
```json
{
  "path": "$.parsedNumber",
  "value": "=parse($.numberString)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "numberString": "42"
}
```

**Result**:
```json
{
  "numberString": "42",
  "parsedNumber": 42
}
```

### Boolean Parsing
```json
{
  "path": "$.parsedFlag",
  "value": "=parse($.flagString)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "flagString": "true"
}
```

**Result**:
```json
{
  "flagString": "true",
  "parsedFlag": true
}
```

### Null Value Parsing
```json
{
  "path": "$.parsedNull",
  "value": "=parse($.nullString)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "nullString": "null"
}
```

**Result**:
```json
{
  "nullString": "null",
  "parsedNull": null
}
```

### Using Without Arguments
```json
// Applied to a string token containing JSON
{
  "path": "@.parsedValue",
  "value": "=parse()",
  "command": "add"
}
```

When applied to string token `"{\"key\":\"value\"}"`:
```json
{
  "originalString": "{\"key\":\"value\"}",
  "parsedValue": {"key": "value"}
}
```

## Advanced Usage

### Nested JSON Parsing
```json
{
  "path": "$.parsedData",
  "value": "=parse($.nestedJsonString)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "nestedJsonString": "{\"user\":{\"id\":123,\"profile\":{\"name\":\"John\",\"settings\":{\"theme\":\"dark\"}}}}"
}
```

**Result**:
```json
{
  "nestedJsonString": "{\"user\":{\"id\":123,\"profile\":{\"name\":\"John\",\"settings\":{\"theme\":\"dark\"}}}}",
  "parsedData": {
    "user": {
      "id": 123,
      "profile": {
        "name": "John",
        "settings": {
          "theme": "dark"
        }
      }
    }
  }
}
```

### Configuration Loading
```json
{
  "path": "$.appConfig",
  "value": "=parse($.configJson)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "configJson": "{\"database\":{\"host\":\"localhost\",\"port\":5432},\"cache\":{\"enabled\":true,\"ttl\":300}}"
}
```

**Result**:
```json
{
  "configJson": "{\"database\":{\"host\":\"localhost\",\"port\":5432},\"cache\":{\"enabled\":true,\"ttl\":300}}",
  "appConfig": {
    "database": {
      "host": "localhost",
      "port": 5432
    },
    "cache": {
      "enabled": true,
      "ttl": 300
    }
  }
}
```

### API Response Parsing
```json
{
  "path": "$.responseData",
  "value": "=parse($.apiResponseString)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "apiResponseString": "{\"status\":\"success\",\"data\":[{\"id\":1,\"name\":\"Item 1\"},{\"id\":2,\"name\":\"Item 2\"}],\"meta\":{\"total\":2,\"page\":1}}"
}
```

**Result**: Converts the string back to a proper JSON structure for processing.

## Fluent API Usage

### Basic Parsing
```csharp
var script = new JLioScript()
    .Add(ParseBuilders.Parse("$.jsonString"))
    .OnPath("$.parsedData");
```

### Configuration Processing
```csharp
var script = new JLioScript()
    .Add(ParseBuilders.Parse("$.configString"))
    .OnPath("$.config")
    .Add(FetchBuilders.Fetch("$.config.database.host"))
    .OnPath("$.dbHost");
```

### Data Transformation Pipeline
```csharp
var script = new JLioScript()
    .Add(ParseBuilders.Parse("$.serializedInput"))
    .OnPath("$.inputData")
    .Add(FetchBuilders.Fetch("$.inputData.users[0].name"))
    .OnPath("$.firstUserName")
    .Add(ToStringBuilders.ToString("$.inputData"))
    .OnPath("$.processedOutput");
```

## Integration Examples

### Message Queue Processing
```csharp
var script = new JLioScript()
    .Add(ParseBuilders.Parse("$.message.body"))
    .OnPath("$.parsedMessage")
    .Add(FetchBuilders.Fetch("$.parsedMessage.eventType"))
    .OnPath("$.eventType")
    .Add(FetchBuilders.Fetch("$.parsedMessage.payload"))
    .OnPath("$.eventPayload")
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.processedAt");
```

### Configuration Management
```csharp
var script = new JLioScript()
    .Add(ParseBuilders.Parse("$.environmentConfig"))
    .OnPath("$.config")
    .Add(FetchBuilders.Fetch("$.config.services"))
    .OnPath("$.activeServices")
    .Add(FetchBuilders.Fetch("$.config.database.connectionString"))
    .OnPath("$.dbConnection");
```

### Data Import Processing
```csharp
var script = new JLioScript()
    .Add(ParseBuilders.Parse("$.importData"))
    .OnPath("$.records")
    .Add(FetchBuilders.Fetch("$.records.users"))
    .OnPath("$.userList")
    .Add(ConcatBuilders.Concat("Imported ", "=toString($.records.users.length)", " users"))
    .OnPath("$.importSummary");
```

## Error Handling

### Invalid JSON Format
```json
{
  "path": "$.result",
  "value": "=parse($.invalidJson)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "invalidJson": "{invalid json format"
}
```

**Result**: Function fails and logs warning, returns original value
```json
{
  "invalidJson": "{invalid json format",
  "result": "{invalid json format"
}
```

### Non-String Input
```json
// When parse() is called on non-string token
{
  "path": "@.parsed",
  "value": "=parse()",
  "command": "add"
}
```

Applied to number token (123):
- Logs warning: "Function parse only works on type string. Current type = Integer!"
- Returns original token unchanged

### Missing Path
```json
{
  "path": "$.result",
  "value": "=parse($.nonExistent)",
  "command": "add"
}
```

Returns null when path doesn't exist.

## Validation and Type Checking

### String Type Requirement
The parse function only works on string tokens:

```json
// Valid - string input
"=parse($.jsonString)"     // Works if $.jsonString is a string

// Invalid - non-string input  
"=parse($.numberValue)"    // Logs warning if $.numberValue is not a string
"=parse($.objectValue)"    // Logs warning if $.objectValue is not a string
```

### JSON Format Validation
Input must be valid JSON format:

```json
// Valid JSON strings
"=parse('{\"key\":\"value\"}')"           // ✓ Valid object
"=parse('[1,2,3]')"                       // ✓ Valid array  
"=parse('\"simple string\"')"             // ✓ Valid string
"=parse('42')"                            // ✓ Valid number
"=parse('true')"                          // ✓ Valid boolean
"=parse('null')"                          // ✓ Valid null

// Invalid JSON strings
"=parse('{key:value}')"                   // ✗ Unquoted keys
"=parse('[1,2,3,]')"                      // ✗ Trailing comma
"=parse('undefined')"                     // ✗ Invalid literal
```

## Performance Considerations

- **Parsing Overhead**: JSON parsing adds computational cost
- **Large Objects**: Parsing large JSON strings requires more memory and time
- **Error Handling**: Invalid JSON requires exception handling overhead
- **Memory Allocation**: Creates new object structures in memory

## Use Cases

### Data Serialization/Deserialization
```json
// Convert stored JSON strings back to objects
{
  "path": "$.userData",
  "value": "=parse($.userDataString)",
  "command": "add"
}
```

### Configuration Loading
```json
// Parse configuration from external sources
{
  "path": "$.appSettings",
  "value": "=parse($.configJson)",
  "command": "add"
}
```

### API Response Processing
```json
// Parse JSON responses stored as strings
{
  "path": "$.apiData",
  "value": "=parse($.responseBody)",
  "command": "add"
}
```

### Message Queue Processing
```json
// Parse message payloads
{
  "path": "$.messagePayload",
  "value": "=parse($.rawMessage)",
  "command": "add"
}
```

### Database Field Processing
```json
// Parse JSON stored in database text fields
{
  "path": "$.metadata",
  "value": "=parse($.metadataJson)",
  "command": "add"
}
```

## Best Practices

1. **Validate Input**: Ensure input is a valid JSON string before parsing
2. **Handle Errors**: Implement error handling for invalid JSON
3. **Type Checking**: Verify input is string type before parsing
4. **Test Edge Cases**: Test with malformed JSON, empty strings, and null values
5. **Performance Awareness**: Consider parsing overhead for large JSON strings
6. **Memory Management**: Be aware of memory usage when parsing large objects
7. **Security Considerations**: Validate JSON content to prevent injection attacks

## Common Patterns

### Config Parsing Pattern
```json
"=parse($.configString)"
```

### Message Body Pattern
```json
"=parse($.message.body)"
```

### Stored Data Pattern
```json
"=parse($.serializedData)"
```

### API Response Pattern
```json
"=parse($.response.data)"
```

### Database Field Pattern
```json
"=parse($.jsonField)"
```

## Security Considerations

### Input Validation
- Always validate JSON strings before parsing
- Be cautious with user-provided JSON content
- Consider size limits for JSON strings to prevent DoS attacks

### Content Filtering
- Validate parsed content structure
- Sanitize any string values that will be used in sensitive contexts
- Consider whitelist approaches for allowed JSON structures

## Debugging and Troubleshooting

### Common Parse Errors
1. **Malformed JSON**: Missing quotes, brackets, or commas
2. **Trailing Commas**: Extra commas in arrays or objects
3. **Unquoted Keys**: Object keys must be quoted in JSON
4. **Invalid Escape Sequences**: Incorrect string escaping
5. **Non-String Input**: Attempting to parse non-string values

### Error Messages
- Function logs warnings when input is not a string
- JSON parsing errors are caught and logged
- Original value is returned when parsing fails

### Validation Steps
1. Check input is string type
2. Verify JSON format validity
3. Test with sample data
4. Monitor logs for parsing errors
5. Implement fallback behavior for parse failures