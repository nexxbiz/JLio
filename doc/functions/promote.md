# Promote Function

## Overview

The `Promote` function wraps a value in a new object with a specified property name. It's useful for restructuring data, creating wrapper objects, and transforming flat values into structured objects.

## Syntax

### Function Expression Format
```json
// Promote current token with new property name
"=promote('newPropertyName')"

// Promote specific path value
"=promote($.sourceValue, 'wrapperProperty')"
```

### Programmatic Usage
```csharp
// Promote current token
var promoteFunction = new Promote("newPropertyName");

// Promote specific path
var promoteFunction = new Promote("$.sourceValue", "wrapperProperty");
```

### Builder Pattern
```csharp
var promoteFunction = PromoteBuilders.Promote("newPropertyName");
var pathFunction = PromoteBuilders.Promote("$.value", "wrappedValue");
```

## Parameters

- **Property Name (Required)**: The name for the new wrapper property
- **Path (Optional)**: JSONPath to value to promote
- **Two Argument Form**: `promote(path, propertyName)`
- **One Argument Form**: `promote(propertyName)` - promotes current token

## Examples

### Basic Value Promotion
```json
{
  "path": "$.wrappedUser",
  "value": "=promote($.userName, 'name')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "userName": "John Doe"
}
```

**Result**:
```json
{
  "userName": "John Doe",
  "wrappedUser": {
    "name": "John Doe"
  }
}
```

### Complex Object Promotion
```json
{
  "path": "$.userContainer",
  "value": "=promote($.user, 'userData')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "user": {
    "id": 123,
    "name": "John Doe",
    "email": "john@example.com"
  }
}
```

**Result**:
```json
{
  "user": {
    "id": 123,
    "name": "John Doe", 
    "email": "john@example.com"
  },
  "userContainer": {
    "userData": {
      "id": 123,
      "name": "John Doe",
      "email": "john@example.com"
    }
  }
}
```

### Array Promotion
```json
{
  "path": "$.itemsWrapper",
  "value": "=promote($.items, 'list')",
  "command": "add"
}
```

**Input Data**:
```json
{
  "items": [1, 2, 3, 4, 5]
}
```

**Result**:
```json
{
  "items": [1, 2, 3, 4, 5],
  "itemsWrapper": {
    "list": [1, 2, 3, 4, 5]
  }
}
```

### Using Without Path (Current Token)
```json
// Applied to a simple value token
{
  "path": "@.promoted",
  "value": "=promote('value')",
  "command": "add"
}
```

When applied to string token `"Hello World"`:
```json
{
  "original": "Hello World",
  "promoted": {
    "value": "Hello World"
  }
}
```