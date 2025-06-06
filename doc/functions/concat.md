# Concat Function Documentation

## Overview

The `Concat` function combines multiple string values into a single concatenated string. It can accept literal values, JSONPath expressions, and other function results, making it ideal for creating composite identifiers, formatted messages, and dynamic content generation.

## Syntax

### Function Expression Format
```json
// Basic concatenation
"=concat('Hello', ' ', 'World')"

// With JSONPath references
"=concat('User: ', $.user.name)"

// With other functions
"=concat('ID_', newGuid(), '_', datetime('yyyyMMdd'))"

// Mixed arguments
"=concat($.prefix, '_', 'FIXED', '_', $.suffix)"
```

### Programmatic Usage
```csharp
// With string arguments
var concatFunction = new Concat("Hello", " ", "World");

// Empty constructor for dynamic arguments
var concatFunction = new Concat();
```

### Builder Pattern
```csharp
var concatFunction = ConcatBuilders.Concat("prefix", "_", "=newGuid()");
```

## Parameters

- **Multiple Arguments**: Accepts any number of string arguments
- **Argument Types**: 
  - Literal strings
  - JSONPath expressions ($.path.to.value)
  - Function expressions (=functionName())
  - Current token values (@.property)
- **Type Conversion**: Automatically converts non-string values to strings

## Examples

### Basic String Concatenation
```json
{
  "path": "$.fullName",
  "value": "=concat($.firstName, ' ', $.lastName)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "firstName": "John",
  "lastName": "Doe"
}
```

**Result**:
```json
{
  "firstName": "John",
  "lastName": "Doe", 
  "fullName": "John Doe"
}
```

### Creating Composite IDs
```json
{
  "path": "$.compositeId",
  "value": "=concat('USER_', $.userId, '_', datetime('yyyyMMdd'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "userId": "12345"
}
```

**Result**:
```json
{
  "userId": "12345",
  "compositeId": "USER_12345_20240315"
}
```

### Building URLs or Paths
```json
{
  "path": "$.apiUrl",
  "value": "=concat($.baseUrl, '/api/v1/users/', $.userId)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "baseUrl": "https://api.example.com",
  "userId": "12345"
}
```

**Result**:
```json
{
  "baseUrl": "https://api.example.com",
  "userId": "12345",
  "apiUrl": "https://api.example.com/api/v1/users/12345"
}
```

### Formatted Messages
```json
{
  "path": "$.message",
  "value": "=concat('User ', $.user.name, ' logged in at ', datetime('HH:mm:ss'))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "user": {
    "name": "Alice"
  }
}
```

**Result**:
```json
{
  "user": {
    "name": "Alice"
  },
  "message": "User Alice logged in at 14:30:45"
}
```

### File Naming
```json
{
  "path": "$.fileName",
  "value": "=concat($.filePrefix, '_', datetime('yyyyMMdd_HHmmss'), '.', $.fileExtension)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "filePrefix": "export",
  "fileExtension": "json"
}
```

**Result**:
```json
{
  "filePrefix": "export",
  "fileExtension": "json",
  "fileName": "export_20240315_143045.json"
}
```

### Combining with Other Functions
```json
{
  "path": "$.trackingId",
  "value": "=concat('TRK_', newGuid(), '_', toString($.requestNumber))",
  "command": "add"
}
```

**Input Data**:
```json
{
  "requestNumber": 12345
}
```

**Result**:
```json
{
  "requestNumber": 12345,
  "trackingId": "TRK_550e8400-e29b-41d4-a716-446655440000_12345"
}
```

## Type Conversion

The concat function automatically handles type conversion:

### Number Conversion
```json
{
  "path": "$.result",
  "value": "=concat('Count: ', $.count, ' items')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "count": 42
}
```

**Result**:
```json
{
  "count": 42,
  "result": "Count: 42 items"
}
```

### Boolean Conversion
```json
{
  "path": "$.status",
  "value": "=concat('Active: ', $.isActive)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "isActive": true
}
```

**Result**:
```json
{
  "isActive": true,
  "status": "Active: true"
}
```

### Object Conversion
```json
{
  "path": "$.debug",
  "value": "=concat('Object: ', $.config)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "config": {"key": "value"}
}
```

**Result**:
```json
{
  "config": {"key": "value"},
  "debug": "Object: {\"key\":\"value\"}"
}
```

## Fluent API Usage

### Basic Concatenation
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat("Hello", " ", "World"))
    .OnPath("$.greeting");
```

### Dynamic Content
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat("=fetch($.firstName)", " ", "=fetch($.lastName)"))
    .OnPath("$.fullName");
```

### Complex Expressions
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat(
        "BATCH_",
        "=datetime('yyyyMMdd')",
        "_",
        "=newGuid()"
    ))
    .OnPath("$.batchId");
```

## Advanced Examples

### Conditional Concatenation
```json
{
  "path": "$.displayName",
  "value": "=concat($.firstName, $.middleName ? concat(' ', $.middleName) : '', ' ', $.lastName)",
  "command": "add"
}
```

### Multi-level Path Building
```json
{
  "path": "$.resourcePath", 
  "value": "=concat('/api/', $.version, '/', $.resource, '/', $.id, '/', $.action)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "version": "v2",
  "resource": "users", 
  "id": "12345",
  "action": "profile"
}
```

**Result**:
```json
{
  "version": "v2",
  "resource": "users",
  "id": "12345", 
  "action": "profile",
  "resourcePath": "/api/v2/users/12345/profile"
}
```

### Log Message Formatting
```json
{
  "path": "$.logEntry",
  "value": "=concat('[', datetime('yyyy-MM-dd HH:mm:ss'), '] ', $.level, ': ', $.message)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "level": "INFO",
  "message": "User authentication successful"
}
```

**Result**:
```json
{
  "level": "INFO",
  "message": "User authentication successful",
  "logEntry": "[2024-03-15 14:30:45] INFO: User authentication successful"
}
```

## Integration Examples

### User Profile Generation
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat("=fetch($.firstName)", " ", "=fetch($.lastName)"))
    .OnPath("$.profile.displayName")
    .Add(ConcatBuilders.Concat("=fetch($.firstName)", ".", "=fetch($.lastName)", "@company.com"))
    .OnPath("$.profile.email")
    .Add(ConcatBuilders.Concat("USER_", "=newGuid()"))
    .OnPath("$.profile.userId");
```

### Report Generation
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat("Monthly Report - ", "=datetime('MMMM yyyy')"))
    .OnPath("$.report.title")
    .Add(ConcatBuilders.Concat("REPORT_", "=datetime('yyyyMM')", "_", "=newGuid()"))
    .OnPath("$.report.id")
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.report.generatedAt");
```

### API Response Formatting
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat("Successfully processed ", "=toString($.processedCount)", " records"))
    .OnPath("$.response.message")
    .Add(ConcatBuilders.Concat("REQ_", "=datetime('yyyyMMddHHmmss')", "_", "=newGuid()"))
    .OnPath("$.response.requestId");
```

## Error Handling

### Null Value Handling
```json
// Null values are converted to empty strings
"=concat('Hello ', $.nullValue, 'World')"  // Result: "Hello World"
```

### Missing Property Handling
```json
// Missing properties are treated as null/empty
"=concat('User: ', $.nonExistentProperty)"  // Result: "User: "
```

### Function Errors
If a nested function fails, it may return null or error values that get converted to strings.

## Performance Considerations

- **String Allocation**: Creates new string instances for each concatenation
- **Large Concatenations**: Many arguments may impact performance
- **Function Nesting**: Nested function calls add overhead
- **Type Conversion**: Minimal overhead for basic type conversions

## Best Practices

1. **Limit Arguments**: Keep argument count reasonable for performance
2. **Handle Nulls**: Consider null/empty value scenarios in your logic
3. **Use Delimiters**: Include appropriate separators between values
4. **Validate Inputs**: Ensure source paths exist when using JSONPath expressions
5. **Document Format**: Clearly document expected output formats
6. **Test Edge Cases**: Test with null, empty, and special character values
7. **Consider Encoding**: Be aware of special characters in concatenated strings

## Common Patterns

### ID Generation Pattern
```json
"=concat($.entityType, '_', newGuid())"
```

### Timestamp Pattern
```json
"=concat($.action, ' at ', datetime('yyyy-MM-dd HH:mm:ss'))"
```

### Path Building Pattern
```json
"=concat($.basePath, '/', $.resourceType, '/', $.resourceId)"
```

### Message Formatting Pattern
```json
"=concat('[', $.level, '] ', $.source, ': ', $.message)"
```

### File Naming Pattern
```json
"=concat($.prefix, '_', datetime('yyyyMMdd'), '.', $.extension)"
```