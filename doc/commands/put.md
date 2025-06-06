# Put Command Documentation

## Overview

The `Put` command is a hybrid operation that combines the behavior of both `Add` and `Set` commands. It can both create new properties and update existing ones, making it the most flexible property modification command in JLio.

## Syntax

### JSON Script Format
```json
{
  "path": "$.property.path",
  "value": "value_to_put",
  "command": "put"
}
```

### Required Properties
- **path**: JSONPath expression targeting where to put the value
- **value**: The value to put (can be a literal value, object, or function expression)
- **command**: Must be "put"

## Programmatic Usage

### Constructor Options

#### Simple Constructor with JToken
```csharp
var putCommand = new Put("$.myProperty", new JValue("new value"));
```

#### Constructor with Function-Supported Value
```csharp
var functionConverter = new FunctionConverter(ParseOptions.CreateDefault().FunctionsProvider);
var valueToPut = new FunctionSupportedValue(new FixedValue(new JValue("value"), functionConverter));
var putCommand = new Put("$.path", valueToPut);
```

#### Property Initialization Constructor
```csharp
var putCommand = new Put
{
    Path = "$.myProperty",
    Value = new FunctionSupportedValue(new FixedValue(new JValue("value"), functionConverter))
};
```

### Fluent API

#### Putting Simple Values
```csharp
var script = new JLioScript()
    .Put(new JValue("new value"))
    .OnPath("$.myProperty");
```

#### Putting Function Results
```csharp
var script = new JLioScript()
    .Put(DatetimeBuilders.Datetime())
    .OnPath("$.timestamp");
```

## Behavior

### Target Object Types

#### JSON Objects
- **Existing Properties**: Updates the existing property value (like Set)
- **Non-existing Properties**: Creates new property (like Add)
- **Array Properties**: Replaces existing array or creates new array property

```csharp
// Example: Put on existing property (acts like Set)
var data = JToken.Parse("{\"myString\": \"old value\", \"myNumber\": 1.1}");
new Put("$.myString", new JValue("new value")).Execute(data, context);
// Result: {"myString": "new value", "myNumber": 1.1}

// Example: Put on non-existing property (acts like Add)
new Put("$.newProperty", new JValue("new value")).Execute(data, context);
// Result: {"myString": "new value", "myNumber": 1.1, "newProperty": "new value"}
```

#### JSON Arrays
- **Existing Arrays**: Replaces the entire array
- **Array Elements**: Can target and replace array elements through parent object paths

```csharp
// Example: Replacing existing array
var data = JToken.Parse("{\"myArray\": [1, 2, 3]}");
new Put("$.myArray", new JValue("replaced")).Execute(data, context);
// Result: {"myArray": "replaced"}
```

### Path Creation

The Put command automatically creates intermediate objects in the path if they don't exist (like Add):

```csharp
// Creates the entire path structure
new Put("$.NewObject.newItem.NewSubItem", new JValue("newData"))
// On empty object {} creates: {"NewObject": {"newItem": {"NewSubItem": "newData"}}}
```

## Value Types

### Literal Values
```json
{
  "path": "$.myProperty",
  "value": "literal string",
  "command": "put"
}
```

```json
{
  "path": "$.myNumber",
  "value": 42.5,
  "command": "put"
}
```

### Complex Objects
```json
{
  "path": "$.myObject",
  "value": {
    "nested": "structure",
    "with": {
      "multiple": "levels"
    }
  },
  "command": "put"
}
```

### Function Expressions

Functions are prefixed with `=` and evaluated at execution time:

```json
{
  "path": "$.timestamp",
  "value": "=datetime(UTC)",
  "command": "put"
}
```

```json
{
  "path": "$.generatedId",
  "value": "=newGuid()",
  "command": "put"
}
```

## JSONPath Support

### Simple Paths
```csharp
"$.myProperty"             // Put at root-level property
"$.myObject.nestedProp"    // Put at nested property
"$.newProperty"            // Create new root-level property
```

### Nested Paths
```csharp
"$.NewObject.newItem.NewSubItem"  // Creates nested structure if needed
"$.myObject.myArray"              // Replace or create nested array
```

### Recursive Descent
```csharp
"$..myProperty"   // Put to all 'myProperty' at any level
"$..myArray"      // Put to all 'myArray' properties at any level
```

## Examples

### Basic Property Operations
```csharp
var data = JToken.Parse("{\"myString\": \"demo\", \"myNumber\": 2.2}");

// Update existing property
var result = new Put("$.myString", new JValue("updated")).Execute(data, context);
// Result: {"myString": "updated", "myNumber": 2.2}

// Create new property
var result = new Put("$.newProperty", new JValue("new value")).Execute(data, context);
// Result: {"myString": "updated", "myNumber": 2.2, "newProperty": "new value"}
```

### Array Replacement
```csharp
var data = JToken.Parse("{\"myArray\": [1, 2, 3], \"otherProp\": \"value\"}");

// Replace existing array
new Put("$.myArray", new JValue("not an array anymore")).Execute(data, context);
// Result: {"myArray": "not an array anymore", "otherProp": "value"}

// Create new array property
new Put("$.newArray", JToken.Parse("[4, 5, 6]")).Execute(data, context);
// Result: {"myArray": "not an array anymore", "otherProp": "value", "newArray": [4, 5, 6]}
```

### Complex Object Replacement
```csharp
var data = JToken.Parse("{\"config\": {\"old\": \"value\"}}");
var newConfig = JToken.Parse("{\"new\": \"structure\", \"enhanced\": true}");

new Put("$.config", newConfig).Execute(data, context);
// Result: {"config": {"new": "structure", "enhanced": true}}
```

### Creating Nested Structures
```csharp
var data = JToken.Parse("{}");

// Single command creates entire path
new Put("$.level1.level2.level3.finalProperty", new JValue("deep value")).Execute(data, context);
// Result: {"level1": {"level2": {"level3": {"finalProperty": "deep value"}}}}
```

### Token-based Values
```csharp
var data = JToken.Parse("{\"existing\": \"property\"}");
var tokenToPut = JToken.Parse("{\"demo\": 3}");

new Put("$.newComplexProperty", tokenToPut).Execute(data, context);
// Result: {"existing": "property", "newComplexProperty": {"demo": 3}}
```

## Validation and Error Handling

### Missing Path Property
```csharp
var result = new Put("", new JValue("value")).Execute(data, context);
// result.Success = false
// Log entry: "Path property for put command is missing"
```

### Successful Operations
Unlike Set command, Put always succeeds when given valid paths, as it can both create and update properties.

## Fluent API Examples

### Simple Property Put
```csharp
var script = new JLioScript()
    .Put(new JValue("new value"))
    .OnPath("$.myProperty");

var result = script.Execute(data);
```

### Multiple Puts
```csharp
var script = new JLioScript()
    .Put(new JValue("value1"))
    .OnPath("$.prop1")
    .Put(new JValue("value2"))
    .OnPath("$.prop2");

var result = script.Execute(data);
```

### Function-based Puts
```csharp
var script = new JLioScript()
    .Put(DatetimeBuilders.Datetime())
    .OnPath("$.timestamp")
    .Put(new JValue("=newGuid()"))
    .OnPath("$.sessionId");

var result = script.Execute(data);
```

### Creating Complex Structures
```csharp
var script = new JLioScript()
    .Put(new JValue("new value"))
    .OnPath("$.demo")
    .Put(DatetimeBuilders.Datetime())
    .OnPath("$.this.is.a.long.path.with.a.date");

var result = script.Execute(new JObject());
```

### Combined with Other Commands
```csharp
var script = new JLioScript()
    .Put(new JValue("initial"))
    .OnPath("$.data")              // Create or update
    .Copy("$.data")
    .To("$.backup")                // Backup the value
    .Put(new JValue("modified"))
    .OnPath("$.data");             // Update again

var result = script.Execute(data);
```

## Logging and Execution Context

### Successful Operations
```csharp
// Inherits logging from PropertyChangeCommand base class
// Logs completion messages for successful operations
```

### Validation Messages
```csharp
// Missing path: "Path property for put command is missing"
```

## Performance Considerations

- **Path Creation**: Automatically creates intermediate objects when needed
- **Value Replacement**: Complete value replacement rather than merge operations
- **JSONPath Evaluation**: Complex paths require tree traversal
- **Flexible Operation**: Combines both creation and update logic

## Comparison with Add and Set Commands

| Aspect | Put Command | Add Command | Set Command |
|--------|-------------|-------------|-------------|
| **Existing Properties** | Updates existing properties | Warns, doesn't modify | Updates existing properties |
| **Non-existing Properties** | Creates new properties | Creates new properties | Logs info, doesn't create |
| **Path Creation** | Creates intermediate paths | Creates intermediate paths | Does not create paths |
| **Arrays** | Replaces arrays | Appends to arrays | Cannot set on arrays |
| **Use Case** | Create or update | Create new data | Update existing data |
| **Flexibility** | Highest | Medium | Lowest |

## Best Practices

1. **Use Put for upsert operations** when you need to create or update
2. **Consider existing data** - Put will replace entire values, not merge
3. **Validate paths** when path creation behavior is important
4. **Check execution results** to ensure operations completed successfully
5. **Use for configuration updates** where properties may or may not exist
6. **Combine with other commands** for complex data transformation workflows

## When to Use Put vs Add vs Set

- **Use Put when**: You need flexible create-or-update behavior
- **Use Add when**: You specifically want to create new data and preserve existing
- **Use Set when**: You specifically want to update only existing properties