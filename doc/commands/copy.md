# Copy Command Documentation

## Overview

The `Copy` command duplicates values from one JSONPath location to another within the same JSON document. The original value remains unchanged, and the copied value is placed at the destination path. This command supports complex JSONPath expressions and can copy single values, objects, arrays, or multiple values.

## Syntax

### JSON Script Format
```json
{
  "fromPath": "$.source.path",
  "toPath": "$.destination.path",
  "command": "copy"
}
```

### Required Properties
- **fromPath**: JSONPath expression specifying the source location to copy from
- **toPath**: JSONPath expression specifying the destination location to copy to
- **command**: Must be "copy"

## Programmatic Usage

### Constructor Options

#### Simple Constructor
```csharp
var copyCommand = new Copy("$.source.path", "$.destination.path");
```

#### Property Initialization Constructor
```csharp
var copyCommand = new Copy
{
    FromPath = "$.source.path",
    ToPath = "$.destination.path"
};
```

### Fluent API

```csharp
var script = new JLioScript()
    .Copy("$.source.path")
    .To("$.destination.path");
```

## Behavior

### Target Types

#### Object Properties
- **Property to Property**: Copies value from one property to another
- **Property to New Property**: Creates new property with copied value
- **Property to Nested Path**: Creates intermediate objects as needed

```csharp
// Example: Copy property to new location
var data = JToken.Parse("{\"myString\": \"demo2\", \"myNumber\": 2.2}");
new Copy("$.myString", "$.copiedString").Execute(data, context);
// Result: {"myString": "demo2", "myNumber": 2.2, "copiedString": "demo2"}

// Example: Copy to nested path (creates intermediate objects)
new Copy("$.myString", "$.myNewObject.newItem").Execute(data, context);
// Result: {"myString": "demo2", "myNumber": 2.2, "myNewObject": {"newItem": "demo2"}}
```

#### Arrays
- **Value to Array**: Appends copied value to destination array
- **Array to Array**: Appends entire source array to destination array
- **Array Elements to Array**: Copies filtered or selected elements

```csharp
// Example: Copy value to array
var data = JToken.Parse("{\"myString\": \"demo2\", \"myArray\": [2, 20, 200, 2000]}");
new Copy("$.myString", "$.myArray").Execute(data, context);
// Result: {"myString": "demo2", "myArray": [2, 20, 200, 2000, "demo2"]}

// Example: Copy array elements with filter
new Copy("$.myArray[?(@ > 20)]", "$.myArray").Execute(data, context);
// Result: {"myString": "demo2", "myArray": [2, 20, 200, 2000, 200, 2000]}
```

#### Complex Objects
- **Object to Property**: Copies entire object structure
- **Nested Objects**: Preserves object hierarchy in copy

```csharp
// Example: Copy complex object
var data = JToken.Parse(@"{
    ""myObject"": {
        ""myArray"": [2, 20, 200, 2000],
        ""nested"": {""value"": ""test""}
    }
}");
new Copy("$.myObject", "$.copiedObject").Execute(data, context);
// Result: Original myObject remains, copiedObject is identical copy
```

## JSONPath Support

### Simple Paths
```csharp
"$.myString"           // Copy from root-level property
"$.myObject.nested"    // Copy from nested property
"$.myArray"            // Copy entire array
```

### Array Element Selection
```csharp
"$.myArray[*]"         // Copy all array elements
"$.myArray[0]"         // Copy first element
"$.myArray[?(@ > 20)]" // Copy elements matching filter
```

### Recursive Descent
```csharp
"$..myObject"          // Copy all objects named 'myObject' at any level
"$..myArray"           // Copy all arrays named 'myArray' at any level
```

### Complex Expressions
```csharp
"$.myData[*].demo[*].demo2"               // Copy from nested array structures
"$.myData[*].demo[*].demo2.oldProperty"   // Copy specific nested properties
```

## Examples

### Basic Property Copying
```csharp
var data = JToken.Parse("{\"myString\": \"demo2\", \"myNumber\": 2.2}");

// Copy string to new property
var result = new Copy("$.myString", "$.copiedString").Execute(data, context);
// Result: {"myString": "demo2", "myNumber": 2.2, "copiedString": "demo2"}

// Copy number to new property
var result = new Copy("$.myNumber", "$.copiedNumber").Execute(data, context);
// Result: adds "copiedNumber": 2.2
```

### Array Operations
```csharp
var data = JToken.Parse("{\"myArray\": [2, 20, 200, 2000], \"targetArray\": []}");

// Copy all elements from one array to another
new Copy("$.myArray[*]", "$.targetArray").Execute(data, context);
// Result: {"myArray": [2, 20, 200, 2000], "targetArray": [2, 20, 200, 2000]}

// Copy filtered elements
new Copy("$.myArray[?(@ > 20)]", "$.targetArray").Execute(data, context);
// Result: targetArray gets [200, 2000]
```

### Complex Object Copying
```csharp
var data = JToken.Parse(@"{
    ""myObject"": {
        ""myObject"": {""myArray"": [2, 20, 200, 2000]},
        ""myArray"": [2, 20, 200, 2000]
    }
}");

// Copy entire complex object
var result = new Copy("$.myObject", "$.backupObject").Execute(data, context);
// Result: Creates identical copy of myObject at backupObject
```

### Nested Array Structures
```csharp
var data = JToken.Parse(@"{
    ""firstArray"": [
        {
            ""target"": [],
            ""secondArray"": [
                {""id"": ""item1"", ""sub"": {""name"": ""item 1""}},
                {""id"": ""item2"", ""sub"": {""name"": ""item 2""}}
            ]
        }
    ]
}");

// Copy nested objects to target arrays
new Copy("$.firstArray[*].secondArray[*].sub", "$.firstArray[*].target").Execute(data, context);
// Result: Copies all 'sub' objects to corresponding 'target' arrays
```

### Layered Array Copying
```csharp
var startObject = @"{
    ""myData"": [
        {""demo"": [{""demo2"": 3}]},
        {""demo"": [{""demo2"": 4}]},
        {""demo"": [{""demo2"": 5}]}
    ]
}";

// Copy nested properties
new Copy("$.myData[*].demo[*].demo2", "$.myData[*].demo[*].new").Execute(data, context);
// Result: Creates 'new' properties with copied values from 'demo2'
```

## Validation and Error Handling

### Missing Required Properties
```csharp
// Missing FromPath
var result = new Copy("", "$.destination").Execute(data, context);
// result.Success = false
// Log entry: "FromPath property for copy command is missing"

// Missing ToPath
var result = new Copy("$.source", "").Execute(data, context);
// result.Success = false
// Log entry: "ToPath property for copy command is missing"
```

### Non-existent Source Paths
```csharp
// When source path doesn't exist
var result = new Copy("$.nonExistent", "$.destination").Execute(data, context);
// result.Success = true (execution continues)
// No copy operation performed, destination unchanged
```

## Fluent API Examples

### Simple Copy Operation
```csharp
var script = new JLioScript()
    .Copy("$.sourceProperty")
    .To("$.destinationProperty");

var result = script.Execute(data);
```

### Multiple Copy Operations
```csharp
var script = new JLioScript()
    .Copy("$.original")
    .To("$.backup1")
    .Copy("$.original")
    .To("$.backup2");

var result = script.Execute(data);
```

### Copy with Path Creation
```csharp
var script = new JLioScript()
    .Copy("$.existingData")
    .To("$.archive.year2024.backup");

var result = script.Execute(data);
```

### Combined Operations
```csharp
var script = new JLioScript()
    .Copy("$.demo")
    .To("$.copiedDemo")
    .Move("$.copiedDemo")
    .To("$.result");

var result = script.Execute(data);
// Result: demo is copied, then the copy is moved to result
```

## Case Sensitivity

Copy operations are case-sensitive:

```csharp
var data = JToken.Parse("{\"demo\": []}");

new Copy("$.demo", "$.Demo").Execute(data, context);
// Result: {"demo": [], "Demo": []} - creates new property with different case
```

## Performance Considerations

- **Deep Copying**: Objects and arrays are deep copied, not referenced
- **JSONPath Evaluation**: Complex paths require tree traversal for both source and destination
- **Multiple Matches**: When source path matches multiple elements, each is copied
- **Path Creation**: Destination paths are created automatically if they don't exist
- **Memory Usage**: Large objects/arrays consume additional memory when copied

## Logging and Execution Context

### Successful Operations
```csharp
// Logs completion messages for successful copy operations
// Inherited from base CopyMove class
```

### Error Conditions
```csharp
// Validation errors are logged as warnings
// Missing FromPath: "FromPath property for copy command is missing"
// Missing ToPath: "ToPath property for copy command is missing"
```

## Advanced Use Cases

### Backup Operations
```csharp
var script = new JLioScript()
    .Copy("$.importantData")
    .To("$.backup.importantData")
    .Copy("$.configuration")
    .To("$.backup.configuration");
```

### Data Transformation
```csharp
var script = new JLioScript()
    .Copy("$.user.profile.name")
    .To("$.summary.userName")
    .Copy("$.user.profile.email")
    .To("$.summary.userEmail");
```

### Array Consolidation
```csharp
var script = new JLioScript()
    .Copy("$.section1.items[*]")
    .To("$.allItems")
    .Copy("$.section2.items[*]")
    .To("$.allItems");
```

## Best Practices

1. **Validate source paths** exist before copying to avoid silent failures
2. **Consider memory usage** when copying large objects or arrays
3. **Use specific paths** rather than broad recursive patterns for better performance
4. **Check execution results** to ensure copies were successful
5. **Combine with validation** to verify both source and destination operations
6. **Plan destination paths** to avoid overwriting important data
7. **Use for creating backups** before performing destructive operations