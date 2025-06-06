# Partial Function

## Overview

The `Partial` function creates a subset of a JSON object by selecting specific properties and removing all others. It's the opposite of selection - it removes everything except the specified paths.

## Syntax

### Function Expression Format
```json
// Create partial from current token
"=partial('$.property1', '$.property2')"

// Create partial from specific path
"=partial($.sourceObject, '$.keepThis', '$.keepThat')"
```

### Programmatic Usage
```csharp
// Partial from current token
var partialFunction = new Partial("$.property1", "$.property2");

// Partial with source path
// First argument is source, rest are paths to keep
```

### Builder Pattern
```csharp
var partialFunction = PartialBuilders.Partial("$.property1", "$.property2", "$.property3");
```

## Parameters

- **First Argument**: Can be source path (if starts with $) or property to keep
- **Remaining Arguments**: Properties/paths to keep in the result
- **Path Selection**: Supports JSONPath expressions for complex selections

## Examples

### Basic Property Selection
```json
{
  "path": "$.userSummary",
  "value": "=partial('$.name', '$.email')",
  "command": "add"
}
```

**Input Data** (current token):
```json
{
  "id": 123,
  "name": "John Doe",
  "email": "john@example.com",
  "password": "secret",
  "internalId": "xyz789",
  "lastLogin": "2024-03-15T10:30:00Z"
}
```

**Result**:
```json
{
  "id": 123,
  "name": "John Doe",
  "email": "john@example.com",
  "password": "secret",
  "internalId": "xyz789", 
  "lastLogin": "2024-03-15T10:30:00Z",
  "userSummary": {
    "name": "John Doe",
    "email": "john@example.com"
  }
}
```

### Nested Property Selection
```json
{
  "path": "$.configSubset",
  "value": "=partial('$.database.host', '$.database.port', '$.cache.enabled')",
  "command": "add"
}
```

**Input Data** (current token):
```json
{
  "database": {
    "host": "localhost",
    "port": 5432,
    "username": "admin",
    "password": "secret"
  },
  "cache": {
    "enabled": true,
    "ttl": 300,
    "provider": "redis"
  },
  "logging": {
    "level": "info",
    "file": "/var/log/app.log"
  }
}
```

**Result**:
```json
{
  "database": {
    "host": "localhost",
    "port": 5432,
    "username": "admin",
    "password": "secret"
  },
  "cache": {
    "enabled": true,
    "ttl": 300,
    "provider": "redis"
  },
  "logging": {
    "level": "info",
    "file": "/var/log/app.log"
  },
  "configSubset": {
    "database": {
      "host": "localhost",
      "port": 5432
    },
    "cache": {
      "enabled": true
    }
  }
}
```

### Array Element Selection
```json
{
  "path": "$.selectedUsers",
  "value": "=partial($.users, '$.id', '$.name', '$.active')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "users": [
    {
      "id": 1,
      "name": "Alice",
      "email": "alice@example.com",
      "password": "secret1",
      "active": true,
      "lastLogin": "2024-03-15T09:00:00Z"
    },
    {
      "id": 2,
      "name": "Bob", 
      "email": "bob@example.com",
      "password": "secret2",
      "active": false,
      "lastLogin": "2024-03-10T15:30:00Z"
    }
  ]
}
```

**Result**: Creates a subset containing only id, name, and active properties for each user.

## Integration Examples

### Data Sanitization with Promote
```csharp
var script = new JLioScript()
    .Add(PromoteBuilders.Promote("$.user.publicProfile", "profile"))
    .OnPath("$.response.user")
    .Add(PromoteBuilders.Promote("$.settings.preferences", "userPrefs"))
    .OnPath("$.response.settings")
    .Remove("$.user.password")
    .Remove("$.user.internalId");
```

### API Response Filtering with Partial
```csharp
var script = new JLioScript()
    .Add(PartialBuilders.Partial("$.name", "$.email", "$.status"))
    .OnPath("$.publicUser")
    .Add(PartialBuilders.Partial("$.config", "$.database.host", "$.cache.enabled"))
    .OnPath("$.safeConfig")
    .Remove("$.config.database.password");
```

### Data Transformation Pipeline
```csharp
var script = new JLioScript()
    // Create subset of user data
    .Add(PartialBuilders.Partial("$.user", "$.id", "$.name", "$.email"))
    .OnPath("$.userBasics")
    // Promote to create wrapper
    .Add(PromoteBuilders.Promote("$.userBasics", "userData"))
    .OnPath("$.response")
    // Add metadata
    .Add(DatetimeBuilders.Datetime("UTC"))
    .OnPath("$.response.timestamp")
    .Add(NewGuidBuilders.NewGuid())
    .OnPath("$.response.requestId");
```

## Error Handling

### Promote Function Errors

#### Missing Arguments
```json
// No arguments provided
"=promote()"  // Logs error: "promote requires 2 arguments (path, newPropertyName)"
```

#### Invalid Argument Count
```json
// Too many arguments
"=promote('path', 'name', 'extra')"  // Logs error: "promote requires 2 arguments"
```

### Partial Function Errors

#### No Arguments
```json
// No selection paths provided
"=partial()"  // Logs error: "partial requires at least 1 argument"
```

#### Invalid Source Path
```json
// Source path returns multiple items
"=partial