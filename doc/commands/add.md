# Add Command Documentation

## Overview

The `Add` command is used to add new properties or values to JSON objects and arrays. It's one of the core commands in JLio and supports various data types, function expressions, and path creation.

## Syntax

### JSON Script Format
```json
{
  "path": "$.property.path",
  "value": "value_to_add",
  "command": "add"
}
```

### Required Properties
- **path**: JSONPath expression targeting where to add the value
- **value**: The value to add (can be a literal value, object, or function expression)
- **command**: Must be "add"

## Programmatic Usage

### Constructor Options

#### Simple Constructor with JToken
```csharp
var addCommand = new Add("$.myObject.newProperty", new JValue("new value"));
```

#### Constructor with Function-Supported Value
```csharp
var functionConverter = new FunctionConverter(ParseOptions.CreateDefault().FunctionsProvider);
var valueToAdd = new FunctionSupportedValue(new FixedValue(new JValue("value"), functionConverter));
var addCommand = new Add("$.path", valueToAdd);
```

#### Property Initialization Constructor
```csharp
var addCommand = new Add
{
    Path = "$.myObject.newProperty",
    Value = new FunctionSupportedValue(new FixedValue(new JValue("value"), functionConverter))
};
```

### Fluent API

#### Adding Simple Values
```csharp
var script = new JLioScript()
    .Add(new JValue("new value"))
    .OnPath("$.myObject.newProperty");
```

#### Adding Function Results
```csharp
var script = new JLioScript()
    .Add(DatetimeBuilders.Datetime())
    .OnPath("$.timestamp");
```

## Behavior

### Target Object Types

#### JSON Objects
- **New Property**: Adds the property if it doesn't exist
- **Existing Property**: Logs a warning and does not modify the existing value
- **Array Property**: If the target property is an array, appends the value to the array

```csharp
// Example: Adding to object
var data = JToken.Parse("{\"myObject\": {\"existingProp\": \"value\"}}");
new Add("$.myObject.newProp", new JValue("new value")).Execute(data, context);
// Result: {"myObject": {"existingProp": "value", "newProp": "new value"}}
```

#### JSON Arrays
- **Direct Array Target**: Appends the value to the array
- **Array Element**: Can target specific array elements for modification

```csharp
// Example: Adding to array
var data = JToken.Parse("{\"myArray\": [1, 2, 3]}");
new Add("$.myArray", new JValue(4)).Execute(data, context);
// Result: {"myArray": [1, 2, 3, 4]}
```

### Path Creation

The Add command automatically creates intermediate objects in the path if they don't exist:

```csharp
// Creates the entire path structure
new Add("$.NewObject.newItem.NewSubItem", new JValue("newData"))
// On empty object {} creates: {"NewObject": {"newItem": {"NewSubItem": "newData"}}}
```

## Value Types

### Literal Values
```json
{
  "path": "$.myObject.stringProp",
  "value": "literal string",
  "command": "add"
}
```

```json
{
  "path": "$.myObject.numberProp", 
  "value": 42,
  "command": "add"
}
```

### Complex Objects
```json
{
  "path": "$.myObject.complexProp",
  "value": {
    "nested": "object",
    "with": {
      "multiple": "levels"
    }
  },
  "command": "add"
}
```

### Function Expressions

Functions are prefixed with `=` and evaluated at execution time:

```json
{
  "path": "$.myObject.guidProp",
  "value": "=newGuid()",
  "command": "add"
}
```

```json
{
  "path": "$.myObject.dateProp",
  "value": "=datetime(UTC)",
  "command": "add"
}
```

```json
{
  "path": "$.myObject.concatProp",
  "value": "=concat('prefix-', $.existingValue, '-suffix')",
  "command": "add"
}
```

## JSONPath Support

### Simple Paths
```csharp
"$.myObject.newItem"           // Add to object property
"$.myArray"                    // Add to array
"$.newProperty"                // Add root-level property
```

### Nested Paths
```csharp
"$.NewObject.newItem.NewSubItem"  // Creates nested structure
"$.myObject.myObject.newItem"     // Deep nesting
```

### Recursive Descent
```csharp
"$..myObject.newItem"   // Adds to all objects named 'myObject' at any level
"$..myArray"            // Adds to all arrays named 'myArray' at any level
```

### Filter Expressions (Limited Support)
```csharp
"$.myObject[?(@.myArray)].newProperty"  // Conditional adding (partial support)
```

## Examples

### Basic Property Addition
```csharp
var data = JToken.Parse("{\"myString\": \"demo\", \"myNumber\": 2.2}");
var result = new Add("$.newProperty", new JValue("newData")).Execute(data, context);
// Result: {"myString": "demo", "myNumber": 2.2, "newProperty": "newData"}
```

### Adding Complex Objects
```csharp
var tokenToAdd = JToken.Parse("{\"demo\": 3}");
var result = new Add("$.complexProperty", tokenToAdd).Execute(data, context);
```

### Adding with Functions
```csharp
var script = "[{\"path\":\"$.newProperty\",\"value\":\"=newGuid()\",\"command\":\"add\"}]";
var parsedScript = JLioConvert.Parse(script);
var result = parsedScript.Execute(data);
```

### Creating Nested Structures
```csharp
// Single command creates entire path
new Add("$.level1.level2.level3.finalProperty", new JValue("deep value"))
```

### Adding to Arrays
```csharp
var data = JToken.Parse("{\"myArray\": [2, 20, 200]}");
new Add("$.myArray", new JValue(2000)).Execute(data, context);
// Result: {"myArray": [2, 20, 200, 2000]}
```

## Error Handling

### Missing Path Property
If the path is empty or null:
```csharp
var result = new Add("", new JValue("value")).Execute(data, context);
// result.Success = false
// Log entry: "Path property for add command is missing"
```

### Existing Property Warning
When trying to add to an existing object property:
```csharp
// Logs warning: "Property {propertyName} already exists on {path}. Add function not applied"
// The operation is skipped and existing value is preserved
```

## Performance Notes

- Path creation is automatic but involves object traversal
- Adding to arrays is generally faster than object property addition
- Function evaluation adds execution overhead
- Recursive descent paths (`$..`) require full tree traversal

## Integration with Other Commands

The Add command works well in combination with other JLio commands:

```csharp
var script = new JLioScript()
    .Add(new JValue("initial"))
    .OnPath("$.demo")
    .Set(new JValue("updated"))     // Can be updated later
    .OnPath("$.demo")
    .Copy("$.demo")                 // Can be copied
    .To("$.backup");
```