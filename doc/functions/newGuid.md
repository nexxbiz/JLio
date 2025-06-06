# NewGuid Function Documentation

## Overview

The `NewGuid` function generates a new globally unique identifier (GUID) and returns it as a string value. This function is useful for creating unique identifiers for records, sessions, or any scenario requiring unique values.

## Syntax

### Function Expression Format
```json
{
  "path": "$.myProperty",
  "value": "=newGuid()",
  "command": "add"
}
```

### Programmatic Usage
```csharp
var newGuidFunction = new NewGuid();
var result = newGuidFunction.Execute(currentToken, dataContext, context);
```

### Builder Pattern
```csharp
var guidFunction = NewGuidBuilders.NewGuid();
```

## Function Characteristics

- **Parameters**: None - NewGuid takes no arguments
- **Return Type**: String (JValue)
- **Context Independent**: Does not depend on current token or data context
- **Deterministic**: No - generates a new unique value each time

## Examples

### Basic GUID Generation
```json
{
  "path": "$.id",
  "value": "=newGuid()",
  "command": "add"
}
```

**Result**: Adds a property with a new GUID
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Multiple GUID Generation
```json
[
  {
    "path": "$.sessionId",
    "value": "=newGuid()",
    "command": "add"
  },
  {
    "path": "$.requestId", 
    "value": "=newGuid()",
    "command": "add"
  }
]
```

**Result**: Each call generates a different GUID
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "requestId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8"
}
```

### Using with Other Functions
```json
{
  "path": "$.compositeId",
  "value": "=concat('USER_', newGuid(), '_', datetime('yyyy'))",
  "command": "add"
}
```

**Result**: Combines GUID with other values
```json
{
  "compositeId": "USER_550e8400-e29b-41d4-a716-446655440000_2024"
}
```

## Fluent API Usage

### Direct Function Creation
```csharp
var script = new JLioScript()
    .Add(NewGuidBuilders.NewGuid())
    .OnPath("$.uniqueId");
```

### In Complex Expressions
```csharp
var script = new JLioScript()
    .Add(ConcatBuilders.Concat("PREFIX_", "=newGuid()", "_SUFFIX"))
    .OnPath("$.compositeId");
```

## Use Cases

### Record Identification
```json
{
  "path": "$.user.id",
  "value": "=newGuid()",
  "command": "add"
}
```

### Session Management
```json
{
  "path": "$.session.sessionId",
  "value": "=newGuid()",
  "command": "add"
}
```

### Correlation IDs
```json
{
  "path": "$.logging.correlationId",
  "value": "=newGuid()",
  "command": "add"
}
```

### Temporary File Names
```json
{
  "path": "$.processing.tempFileName",
  "value": "=concat('temp_', newGuid(), '.tmp')",
  "command": "add"
}
```

## GUID Format

The function generates GUIDs in the standard format:
- **Pattern**: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- **Character Set**: Hexadecimal (0-9, a-f)
- **Length**: 36 characters including hyphens
- **Version**: Standard .NET Guid.NewGuid() implementation

## Performance Considerations

- **Generation Speed**: Very fast, minimal overhead
- **Uniqueness**: Cryptographically strong randomness
- **Memory Usage**: Minimal - generates string representation
- **Thread Safety**: Safe for concurrent use

## Integration Examples

### User Registration Workflow
```csharp
var script = new JLioScript()
    .Add(NewGuidBuilders.NewGuid())
    .OnPath("$.user.id")
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.user.createdAt")
    .Add(new JValue("active"))
    .OnPath("$.user.status");
```

### Audit Trail Creation
```csharp
var script = new JLioScript()
    .Copy("$.originalData")
    .To("$.audit.before")
    .Add(NewGuidBuilders.NewGuid())
    .OnPath("$.audit.auditId")
    .Add(DatetimeBuilders.Datetime())
    .OnPath("$.audit.timestamp");
```

### Batch Processing
```csharp
var script = new JLioScript()
    .Add(NewGuidBuilders.NewGuid())
    .OnPath("$.batch.batchId")
    .Add(new JValue("processing"))
    .OnPath("$.batch.status")
    .Add(DatetimeBuilders.Datetime())
    .OnPath("$.batch.startedAt");
```

## Error Handling

The NewGuid function is highly reliable and rarely fails:

- **No Arguments Required**: Cannot fail due to invalid parameters
- **No Context Dependencies**: Does not depend on external data
- **System Dependencies**: Only depends on .NET runtime GUID generation

## Best Practices

1. **Use for Unique Identifiers**: Ideal for primary keys, correlation IDs, and session identifiers
2. **Combine with Prefixes**: Use with concat() for more readable identifiers
3. **Store as Strings**: GUIDs are returned as strings for JSON compatibility
4. **Document Purpose**: Clearly document what each GUID represents in your schema
5. **Consider Alternatives**: For sequential IDs, consider using other approaches
6. **Database Considerations**: Be aware of GUID performance implications in database indexes

## Common Patterns

### Entity Creation Pattern
```json
[
  {
    "path": "$.entity.id",
    "value": "=newGuid()",
    "command": "add"
  },
  {
    "path": "$.entity.createdAt",
    "value": "=datetime('UTC')",
    "command": "add"
  },
  {
    "path": "$.entity.version",
    "value": 1,
    "command": "add"
  }
]
```

### Request Tracking Pattern
```json
{
  "path": "$.request.traceId",
  "value": "=newGuid()",
  "command": "add"
}
```

### File Processing Pattern
```json
{
  "path": "$.file.processingId",
  "value": "=concat('PROC_', newGuid())",
  "command": "add"
}
```