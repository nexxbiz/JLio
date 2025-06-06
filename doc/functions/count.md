# Count Function Documentation

## Overview

The `Count` function counts elements in collections or the number of provided arguments. Unlike Sum and Avg functions, Count handles all data types and provides different counting behavior based on the input type (arrays, objects, or individual values).

## Installation

### Extension Pack Registration
```csharp
// Register math functions extension pack
var parseOptions = ParseOptions.CreateDefault().RegisterMath();

// Use in script parsing
var script = JLioConvert.Parse(scriptJson, parseOptions);
```

## Syntax

### Function Expression Format
```json
// Count arguments
"=count(1, 2, 3)"

// Count array elements
"=count($.items)"

// Count object properties
"=count($.configuration)"

// Count multiple collections
"=count($.users, $.products, $.orders)"

// Count nested structures
"=count($.departments[*].employees[*])"
```

### Programmatic Usage
```csharp
// With literal arguments
var countFunction = new Count("1", "2", "3");

// Empty constructor for dynamic arguments
var countFunction = new Count();
```

### Builder Pattern
```csharp
var countFunction = CountBuilders.Count("$.items");
var multiCount = CountBuilders.Count("$.users", "$.products");
```

## Parameters

- **Multiple Arguments**: Accepts any number of arguments of any type
- **Argument Types**: 
  - Arrays (counts elements)
  - Objects (counts properties)
  - Individual values (each counts as 1)
  - JSONPath expressions
- **Return Type**: Integer (JValue)

## Counting Behavior

### Arrays
Returns the number of elements in the array:
```json
"=count($.items)"  // [1, 2, 3, 4, 5] → 5
```

### Objects
Returns the number of properties in the object:
```json
"=count($.config)"  // {"host": "localhost", "port": 8080} → 2
```

### Individual Values
Each individual value counts as 1:
```json
"=count($.a, $.b, $.c)"  // Any three values → 3
```

### Mixed Types
Combines counting rules for different types:
```json
"=count($.singleValue, $.arrayValue, $.objectValue)"
// Single value (1) + Array length + Object property count
```

## Examples

### Basic Array Count
```json
{
  "path": "$.itemCount",
  "value": "=count($.items)",
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
  "itemCount": 5
}
```

### Object Property Count
```json
{
  "path": "$.configProperties",
  "value": "=count($.configuration)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "configuration": {
    "host": "localhost",
    "port": 8080,
    "debug": true,
    "ssl": false
  }
}
```

**Result**:
```json
{
  "configuration": {...},
  "configProperties": 4
}
```

### Multiple Argument Count
```json
{
  "path": "$.totalArguments",
  "value": "=count($.a, $.b, $.c)",
  "command": "add"
}
```

**Input Data**:
```json
{
  "a": 1,
  "b": "hello",
  "c": true
}
```

**Result**:
```json
{
  "a": 1,
  "b": "hello",
  "c": true,
  "totalArguments": 3
}
```

### Employee Count by Department
```json
{
  "path": "$.totalEmployees",
  "value": "=count($.departments[*].employees[*])",
  "command": "add"
}
```

**Input Data**:
```json
{
  "departments": [
    {
      "name": "Engineering",
      "employees": [