# Move Command Documentation

## Overview

The `Move` command transfers values from one JSONPath location to another within the same JSON document. Unlike the `Copy` command, `Move` removes the value from the source location and places it at the destination. This is equivalent to a copy operation followed by a remove operation.

## Syntax

### JSON Script Format
```json
{
  "fromPath": "$.source.path",
  "toPath": "$.destination.path",
  "command": "move"
}
```

### Required Properties
- **fromPath**: JSONPath expression specifying the source location to move from
- **toPath**: JSONPath expression specifying the destination location to move to
- **command**: Must be "move"

## Programmatic Usage

### Constructor Options

#### Simple Constructor
```csharp
var moveCommand = new Move("$.source.path", "$.destination.path");
```

#### Property Initialization Constructor
```csharp
var moveCommand = new Move
{
    FromPath = "$.source.path",
    ToPath = "$.destination.path"
};
```

### Fluent API

```csharp
var script = new JLioScript()
    .Move("$.source.path")
    .To("$.destination.path");
```

## Behavior

### Target Types

#### Object Properties
- **Property to Property**: Moves value from one property to another, removing source
- **Property to New Property**: Creates new property with moved value, removes source
- **Property to Nested Path**: Creates intermediate objects as needed, removes source

```csharp
// Example: Move property to new location
var data = JToken.Parse("{\"myString\": \"demo2\", \"myNumber\": 2.2}");
new Move("$.myString", "$.movedString").Execute(data, context);
// Result: {"myNumber": 2.2, "movedString": "demo2"} // myString is removed
```

#### Arrays
- **Value to Array**: Appends moved value to destination array, removes from source
- **Array to Array**: Appends entire source array to destination, removes source array
- **Array Elements to Array**: Moves filtered or selected elements

```csharp
// Example: Move value to array
var data = JToken.Parse("{\"myString\": \"demo2\", \"myArray\": [2, 20, 200, 2000]}");
new Move("$.myString", "$.myArray").Execute(data, context);
// Result: {"myArray": [2, 20, 200, 2000, "demo2"]} // myString property removed

// Example: Move array elements with filter
new Move("$.myArray[?(@ > 20)]", "$.myArray").Execute(data, context);
// Result: Elements > 20 are moved to end, original positions removed
// [2, 20, 200, 2000] becomes [2, 20] (elements > 20 removed from original positions)
```

#### Root Level Operations
- **Move to Root**: Can move entire objects to root level

```csharp
// Example: Move object to root
var data = JToken.Parse("{\"myObject\": {\"nested\": \"value\"}, \"other\": \"data\"}");
new Move("$.myObject", "$").Execute(data, context);
// Result: {"nested": "value"} // myObject moved to root, other properties removed
```

## JSONPath Support

### Simple Paths
```csharp
"$.myString"           // Move from root-level property
"$.myObject.nested"    // Move from nested property
"$.myArray"            // Move entire array
```

### Array Element Selection
```csharp
"$.myArray[*]"         // Move all array elements
"$.myArray[0]"         // Move first element
"$.myArray[?(@ > 20)]" // Move elements matching filter
```

### Recursive Descent
```csharp
"$..myObject"          // Move all objects named 'myObject' at any level
"$..myArray"           // Move all arrays named 'myArray' at any level
```

### Complex Expressions
```csharp
"$.myData[*].demo[*].demo2"               // Move from nested array structures
"$.myData[*].demo[*].demo2.oldProperty"   // Move specific nested properties
```

## Examples

### Basic Property Moving
```csharp
var data = JToken.Parse("{\"myString\": \"demo2\", \"myNumber\": 2.2}");

// Move string to new property
var result = new Move("$.myString", "$.newLocation").Execute(data, context);
// Result: {"myNumber": 2.2, "newLocation": "demo2"} // myString removed

// Move with path creation
var result = new Move("$.myNumber", "$.myNewObject.newItem").Execute(data, context);
// Result: {"newLocation": "demo2", "myNewObject": {"newItem": 2.2}} // myNumber removed
```

### Array Operations
```csharp
var data = JToken.Parse("{\"myArray\": [2, 20, 200, 2000], \"targetArray\": [1, 2]}");

// Move specific elements
new Move("$.myArray[?(@ > 20)]", "$.targetArray").Execute(data, context);
// Result: {"myArray": [2, 20], "targetArray": [1, 2, 200, 2000]}

// Move entire array
new Move("$.myArray", "$.movedArray").Execute(data, context);
// Result: {"targetArray": [1, 2, 200, 2000], "movedArray": [2, 20]} // myArray removed
```

### Complex Object Moving
```csharp
var data = JToken.Parse(@"{
    ""myObject"": {
        ""myObject"": {""myArray"": [2, 20, 200, 2000]},
        ""myArray"": [2, 20, 200, 2000]
    },
    ""otherData"": ""preserved""
}");

// Move entire complex object
var result = new Move("$.myObject", "$.relocated").Execute(data, context);
// Result: {"otherData": "preserved", "relocated": {entire myObject structure}}
// Original myObject is removed
```

### Layered Array Moving
```csharp
var startObject = @"{
    ""myData"": [
        {""demo"": [{""old"": 3}]},
        {""demo"": [{""old"": 4}]},
        {""demo"": [{""old"": 5}]}
    ]
}";

// Move nested properties
new Move("$.myData[*].demo", "$.myData[*].new").Execute(data, context);
// Result: demo arrays are moved to 'new' properties, original demo properties removed
```

### Property Renaming
```csharp
var data = JToken.Parse("{\"oldName\": \"value\", \"other\": \"data\"}");

// Rename property by moving
new Move("$.oldName", "$.newName").Execute(data, context);
// Result: {"other": "data", "newName": "value"} // oldName removed
```

### Nested Property Restructuring
```csharp
var data = JToken.Parse(@"{
    ""myData"": [
        {""demo"": [{""demo2"": 3}]},
        {""demo"": [{""demo2"": 4}]},
        {""demo"": [{""demo2"": 5}]}
    ]
}");

// Move and restructure nested properties
new Move("$.myData[*].demo[*].demo2", "$.myData[*].demo[*].new").Execute(data, context);
// Result: demo2 values moved to 'new' properties, demo2 properties removed
// [{"demo": [{"new": 3}]}, {"demo": [{"new": 4}]}, {"demo": [{"new": 5}]}]
```

## Validation and Error Handling

### Missing Required Properties
```csharp
// Missing FromPath
var result = new Move("", "$.destination").Execute(data, context);
// result.Success = false
// Log entry: "FromPath property for move command is missing"

// Missing ToPath
var result = new Move("$.source", "").Execute(data, context);
// result.Success = false
// Log entry: "ToPath property for move command is missing"
```

### Non-existent Source Paths
```csharp
// When source path doesn't exist
var result = new Move("$.nonExistent", "$.destination").Execute(data, context);
// result.Success = true (execution continues)
// No move operation performed, data unchanged
```

### Same Source and Destination
```csharp
// Moving to same location
var result = new Move("$.myProperty", "$.myProperty").Execute(data, context);
// Result: No change to data structure (source equals destination)
```

## Fluent API Examples

### Simple Move Operation
```csharp
var script = new JLioScript()
    .Move("$.sourceProperty")
    .To("$.destinationProperty");

var result = script.Execute(data);
```

### Multiple Move Operations
```csharp
var script = new JLioScript()
    .Move("$.oldProperty1")
    .To("$.newProperty1")
    .Move("$.oldProperty2")
    .To("$.newProperty2");

var result = script.Execute(data);
```

### Move with Path Creation
```csharp
var script = new JLioScript()
    .Move("$.temporaryData")
    .To("$.archive.year2024.movedData");

var result = script.Execute(data);
```

### Combined Operations
```csharp
var script = new JLioScript()
    .Copy("$.demo")
    .To("$.copiedDemo")          // Create backup first
    .Move("$.copiedDemo")
    .To("$.result");             // Then move the copy

var result = script.Execute(data);
// Result: demo is preserved, copiedDemo is moved to result
```

## Case Sensitivity

Move operations are case-sensitive:

```csharp
var data = JToken.Parse("{\"demo\": [1, 2, 3]}");

new Move("$.demo", "$.Demo").Execute(data, context);
// Result: {"Demo": [1, 2, 3]} - original 'demo' removed, new 'Demo' created
```

## Performance Considerations

- **Two-Phase Operation**: Move involves both copying to destination and removing from source
- **JSONPath Evaluation**: Requires evaluation of both source and destination paths
- **Memory Efficiency**: More memory efficient than separate copy + remove operations
- **Path Creation**: Destination paths are created automatically if they don't exist
- **Source Cleanup**: Original source locations are properly cleaned up

## Logging and Execution Context

### Successful Operations
```csharp
// Logs completion messages for successful move operations
// Inherited from base CopyMove class
```

### Error Conditions
```csharp
// Validation errors are logged as warnings
// Missing FromPath: "FromPath property for move command is missing"
// Missing ToPath: "ToPath property for move command is missing"
```

## Difference from Copy Command

| Aspect | Move Command | Copy Command |
|--------|--------------|--------------|
| **Source After Operation** | Removed/deleted | Preserved unchanged |
| **Memory Usage** | Lower (no duplication) | Higher (creates copy) |
| **Use Case** | Restructuring, renaming | Backup, duplication |
| **Reversibility** | Requires another move | Source remains for reversal |
| **Data Safety** | Less safe (original lost) | Safer (original preserved) |

## Advanced Use Cases

### Data Restructuring
```csharp
var script = new JLioScript()
    .Move("$.user.personalInfo.firstName")
    .To("$.profile.name.first")
    .Move("$.user.personalInfo.lastName")
    .To("$.profile.name.last");
```

### Property Renaming
```csharp
var script = new JLioScript()
    .Move("$.old_property_name")
    .To("$.newPropertyName")
    .Move("$.another_old_name")
    .To("$.anotherNewName");
```

### Temporary Storage and Reorganization
```csharp
var script = new JLioScript()
    .Move("$.section1.data")
    .To("$.temp.data1")
    .Move("$.section2.data")
    .To("$.temp.data2")
    .Move("$.temp")
    .To("$.reorganized");
```

## Best Practices

1. **Create backups** before moving critical data (use Copy first)
2. **Validate source paths** exist before moving to avoid silent failures
3. **Test move operations** on sample data before production use
4. **Consider order** of operations when moving multiple related properties
5. **Use for refactoring** JSON structure and property renaming
6. **Plan destination paths** carefully to avoid data loss
7. **Combine with validation** to ensure both source removal and destination creation
8. **Use Copy instead** when you need to preserve original data