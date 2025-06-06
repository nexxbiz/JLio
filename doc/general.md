# JLio Overview

## What is JLio?

**JLio** (jay-lio) is a structured language definition for transforming JSON objects using transformation scripts with JSON notation. JLio allows you to create and alter JSON data objects through declarative commands without the need for complex coding or custom logic development.

## Key Features

- **Declarative JSON Transformations**: Transform JSON objects using simple command-based scripts
- **Extensible Architecture**: Add custom commands and functions to support specific functionality
- **Memory Efficient**: Core functionality provides essential commands with minimal overhead
- **Flexible Configuration**: .NET implementation supports configurable resource limitations
- **Extension Packs**: Additional functionality through extension packs for specialized operations
- **Import-Export Support**: Transform JSON objects between different structures using string notation

## Core Concepts

### Commands
Commands are the building blocks of JLio scripts. Each command performs a specific operation on JSON data:
- **add** - Adds new properties or values
- **set** - Sets or updates existing values
- **put** - Similar to set with different behavior
- **move** - Moves properties between paths
- **copy** - Copies properties to new locations
- **remove** - Removes properties
- **compare** - Compares values at different paths
- **merge** - Merges JSON objects

### JSONPath
JLio uses JSONPath expressions for targeting elements within JSON structures:
- `$.property` - Root level property
- `$.object.nestedProperty` - Nested property access
- `$.array[0]` - Array element access
- `$..property` - Recursive descent
- `$.array[?(@.condition)]` - Filter expressions

### Functions
JLio supports function expressions within values using the `=` prefix:
- `=newGuid()` - Generates a new GUID
- `=datetime()` - Current datetime
- `=concat(arg1, arg2, ...)` - String concatenation
- `=fetch($.path)` - Fetch value from JSONPath

## Script Formats

### JSON Script Format
Scripts are defined as JSON arrays containing command objects:

```json
[
  {
    "path": "$.myObject.newProperty",
    "value": "new value", 
    "command": "add"
  }
]
```

### Fluent API
Scripts can also be built programmatically using a fluent interface:

```csharp
var script = new JLioScript()
    .Add(new JValue("new value"))
    .OnPath("$.myObject.newProperty");
```

## Performance Considerations

- Minimize the number of commands and functions to reduce memory consumption
- Use appropriate JSONPath expressions to avoid unnecessary traversals
- Consider using extension packs only when needed
- Leverage flexible configuration options for resource management

## Extension Development

JLio is designed for extensibility. Custom commands can be developed by:
1. Inheriting from base command classes
2. Implementing required interfaces  
3. Registering commands with the execution engine
4. Optionally creating fluent API builders

