# Set Command Documentation

## Overview

The `Set` command is used to update existing properties in JSON objects. Unlike the `Add` command, `Set` only modifies existing properties and will not create new ones. It's designed for updating values at existing paths.

## Syntax

### JSON Script Format
```json
{
  "path": "$.property.path",
  "value": "new_value",
  "command": "set"
}
```

### Required Properties
- **path**: JSONPath expression targeting existing properties to update
- **value**: The new value to set (can be a literal value, object, or function expression)
- **command**: Must be "set"

## Programmatic Usage

### Constructor Options

#### Simple Constructor with JToken
```csharp
var setCommand = new Set("$.existingProperty", new JValue("new value"));
```

#### Constructor with Function-Supported Value
```csharp
var functionConverter = new FunctionConverter(ParseOptions.CreateDefault().FunctionsProvider);
var valueToSet = new FunctionSupportedValue(new FixedValue(new JValue("value"), functionConverter));
var setCommand = new Set("$.path", valueToSet);
```

#### Property Initialization Constructor
```csharp
var setCommand = new Set
{
    Path = "$.existingProperty",
    Value = new FunctionSupportedValue(new FixedValue(new JValue("value"), functionConverter))
};
```

### Fluent API

#### Setting Simple Values
```csharp
var script = new JLioScript()
    .Set(new JValue("new value"))
    .OnPath("$.existingProperty");
```

#### Setting Function Results
```csharp
var script = new JLioScript()
    .Set(DatetimeBuilders.Datetime())
    .OnPath("$.timestamp");
```

## Behavior

### Target Object Types

#### JSON Objects
- **Existing Properties**: Updates the value of the existing property
- **Non-existing Properties**: Logs an info message and does not create the property
- **Array Properties**: Cannot set values directly on arrays - logs info message

```csharp
// Example: Setting existing object property
var data = JToken.Parse("{\"myString\": \"old value\", \"myNumber\": 1.1}");
new Set("$.myString", new JValue("new value")).Execute(data, context);
// Result: {"myString": "new value", "myNumber": 1.1}

// Example: Attempting to set non-existing property
new Set("$.nonExisting", new JValue("value")).Execute(data, context);
// Logs: "Property nonExisting does not exists on [path]. Set function not applied."
// No changes made to data
```

#### JSON Arrays
- **Direct Array Target**: Cannot set values on arrays directly
- **Array Elements**: Can target and update specific array elements via their parent objects

```csharp
// Example: Attempting to set on array
var data = JToken.Parse("{\"myArray\": [1, 2, 3]}");
new Set("$.myArray", new JValue("new value")).Execute(data, context);
// Logs: "can't set value on a array on [path]. Set functionality not applied."
```

## Value Types

### Literal Values
```json
{
  "path": "$.existingString",
  "value": "new string value",
  "command": "set"
}
```

```json
{
  "path": "$.existingNumber",
  "value": 42.5,
  "command": "set"
}
```

### Empty Strings
```csharp
// Set property to empty string
new Set("$.myString", new JValue("")).Execute(data, context);
// Result: Property value becomes ""
```

### Complex Objects
```json
{
  "path": "$.existingObject",
  "value": {
    "newStructure": "value",
    "nested": {
      "property": "data"
    }
  },
  "command": "set"
}
```

### Function Expressions

Functions are prefixed with `=` and evaluated at execution time:

```json
{
  "path": "$.timestamp",
  "value": "=datetime(UTC)",
  "command": "set"
}
```

```json
{
  "path": "$.calculatedValue",
  "value": "=concat('Updated: ', $.originalValue, ' at ', datetime())",
  "command": "set"
}
```

## JSONPath Support

### Simple Paths
```csharp
"$.existingProperty"           // Set root-level property
"$.myObject.existingProp"      // Set nested property
"$.myString"                   // Set string property
```

### Multiple Target Paths
```csharp
"$..existingProperty"   // Set all 'existingProperty' at any level
"$..myArray"           // Set all 'myArray' properties at any level (if they exist)
```

### Object Property Paths
```csharp
"$.myObject.myArray"    // Set nested array property
"$.myObject.myObject"   // Set nested object property
```

## Examples

### Basic Property Updates
```csharp
var data = JToken.Parse("{\"myString\": \"old\", \"myNumber\": 1, \"myBoolean\": false}");

// Update string property
var result = new Set("$.myString", new JValue("updated")).Execute(data, context);
// Result: {"myString": "updated", "myNumber": 1, "myBoolean": false}

// Update all matching properties
var result = new Set("$..myString", new JValue("new value")).Execute(data, context);
```

### Setting Empty Values
```csharp
var data = JToken.Parse("{\"myString\": \"old value\", \"myNumber\": 42}");

// Set to empty string
new Set("$.myString", new JValue("")).Execute(data, context);
// Result: {"myString": "", "myNumber": 42}
```

### Complex Object Replacement
```csharp
var data = JToken.Parse("{\"config\": {\"old\": \"structure\"}}");
var newConfig = JToken.Parse("{\"new\": \"structure\", \"enhanced\": true}");

new Set("$.config", newConfig).Execute(data, context);
// Result: {"config": {"new": "structure", "enhanced": true}}
```

### Multiple Property Updates
```csharp
var script = new JLioScript()
    .Set(new JValue("new value"))
    .OnPath("$.demo")
    .Set(DatetimeBuilders.Datetime())
    .OnPath("$.demo2");

var result = script.Execute(data);
```

### Nested Property Updates
```csharp
var data = JToken.Parse(@"{
    ""myObject"": {
        ""myArray"": [1, 2, 3],
        ""myString"": ""old""
    }
}");

// Update nested property
new Set("$.myObject.myString", new JValue("updated")).Execute(data, context);
// Result: {"myObject": {"myArray": [1, 2, 3], "myString": "updated"}}
```

## Validation and Error Handling

### Missing Path Property
```csharp
var result = new Set("", new JValue("value")).Execute(data, context);
// result.Success = false
// Log entry: "Path property for set command is missing"
```

### Non-existent Properties
```csharp
var result = new Set("$.nonExistent", new JValue("value")).Execute(data, context);
// result.Success = true (execution continues)
// Log entry: "Property nonExistent does not exists on [path]. Set function not applied."
```

### Array Target Handling
```csharp
var result = new Set("$.myArray", new JValue("value")).Execute(data, context);
// result.Success = true (execution continues)
// Log entry: "can't set value on a array on [path]. Set functionality not applied."
```

## Fluent API Examples

### Simple Property Update
```csharp
var script = new JLioScript()
    .Set(new JValue("new value"))
    .OnPath("$.existingProperty");

var result = script.Execute(data);
```

### Multiple Updates
```csharp
var script = new JLioScript()
    .Set(new JValue("updated demo"))
    .OnPath("$.demo")
    .Set(new JValue("updated demo2"))
    .OnPath("$.demo2");

var result = script.Execute(data);
```

### Function-based Updates
```csharp
var script = new JLioScript()
    .Set(DatetimeBuilders.Datetime())
    .OnPath("$.lastModified")
    .Set(new JValue("=newGuid()"))
    .OnPath("$.sessionId");

var result = script.Execute(data);
```

### Combined with Other Commands
```csharp
var script = new JLioScript()
    .Add(new JValue("initial"))
    .OnPath("$.newProperty")           // Create new property
    .Set(new JValue("updated"))
    .OnPath("$.newProperty")           // Update the new property
    .Copy("$.newProperty")
    .To("$.backup");                   // Backup the updated value

var result = script.Execute(data);
```

## Logging and Execution Context

### Successful Updates
```csharp
// Logs info message: "Set: completed for {targetPath}"
```

### Informational Messages
```csharp
// Non-existent property: "Property {propertyName} does not exists on {path}. Set function not applied."
// Array target: "can't set value on a array on {path}. Set functionality not applied."
```

### Validation Messages
```csharp
// Missing path: "Path property for set command is missing"
```

## Performance Considerations

- **Existing Property Check**: Requires property existence validation before update
- **JSONPath Evaluation**: Complex paths require tree traversal to find targets
- **Value Replacement**: Complete value replacement rather than merge
- **Multiple Targets**: Recursive descent patterns may impact performance on large documents

## Difference from Add Command

| Aspect | Set Command | Add Command |
|--------|-------------|-------------|
| **Existing Properties** | Updates existing properties | Warns but doesn't modify existing properties |
| **Non-existing Properties** | Logs info, doesn't create | Creates new properties |
| **Path Creation** | Does not create intermediate paths | Creates intermediate objects as needed |
| **Arrays** | Cannot set on arrays directly | Appends to arrays |
| **Use Case** | Updating existing data | Creating new data structures |

## Best Practices

1. **Verify property existence** before using Set if creation is needed
2. **Use Add for new properties** and Set for updates
3. **Check execution logs** to confirm updates were applied
4. **Combine with validation** to ensure target properties exist
5. **Consider using Put** if you need both create and update behavior