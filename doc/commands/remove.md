# Remove Command Documentation

## Overview

The `Remove` command is used to delete properties from JSON objects or remove elements from arrays. It supports JSONPath expressions to target specific elements for removal.

## Syntax

### JSON Script Format
```json
{
  "path": "$.property.path",
  "command": "remove"
}
```

### Required Properties
- **path**: JSONPath expression targeting what to remove
- **command**: Must be "remove"

## Programmatic Usage

### Constructor Options

#### Simple Constructor
```csharp
var removeCommand = new Remove("$.propertyToRemove");
```

#### Property Initialization Constructor
```csharp
var removeCommand = new Remove
{
    Path = "$.propertyToRemove"
};
```

### Fluent API

```csharp
var script = new JLioScript()
    .Remove("$.propertyToRemove");
```

## Behavior

### Target Types

#### JSON Object Properties
- **Existing Properties**: Removes the property completely from the object
- **Non-existing Properties**: Logs a warning but continues execution successfully

```csharp
// Example: Removing object properties
var data = JToken.Parse("{\"myString\": \"demo\", \"myNumber\": 2.2}");
new Remove("$.myString").Execute(data, context);
// Result: {"myNumber": 2.2}
```

#### Array Elements
- **Specific Index**: Removes element at the specified index
- **Filter Expressions**: Removes elements matching the filter criteria
- **Out of Bounds**: Handles gracefully without errors

```csharp
// Example: Removing array elements
var data = JToken.Parse("{\"myArray\": [2, 20, 200, 2000]}");

// Remove by index
new Remove("$.myArray[1]").Execute(data, context);
// Result: {"myArray": [2, 200, 2000]}

// Remove by condition
new Remove("$.myArray[?(@ == 20)]").Execute(data, context);
// Result: {"myArray": [2, 200, 2000]}

// Remove multiple by condition
new Remove("$.myArray[?(@ > 20)]").Execute(data, context);
// Result: {"myArray": [2, 20]}
```

## JSONPath Support

### Simple Property Paths
```csharp
"$.myString"           // Remove root-level property
"$.myObject.myArray"   // Remove nested property
"$.myNull"             // Remove null properties
```

### Array Element Paths
```csharp
"$.myArray[0]"         // Remove first element
"$.myArray[100]"       // Remove element at index 100 (handles out of bounds)
"$.myArray[1]"         // Remove element at specific index
```

### Filter Expressions
```csharp
"$.myArray[?(@ == 20)]"    // Remove elements equal to 20
"$.myArray[?(@ > 20)]"     // Remove elements greater than 20
"$.myArray[?(@ < 100)]"    // Remove elements less than 100
```

### Recursive Descent
```csharp
"$..myObject.myArray"  // Remove all 'myArray' properties within 'myObject' at any level
"$..myArray"           // Remove all 'myArray' properties at any level
```

## Examples

### Basic Property Removal
```csharp
var data = JToken.Parse("{\"myString\": \"demo\", \"myNumber\": 2.2, \"myBoolean\": true}");

// Remove single property
var result = new Remove("$.myString").Execute(data, context);
// Result: {"myNumber": 2.2, "myBoolean": true}

// Remove multiple properties in sequence
var script = new JLioScript()
    .Remove("$.myString")
    .Remove("$.myBoolean");
var result = script.Execute(data);
// Result: {"myNumber": 2.2}
```

### Array Element Removal
```csharp
var data = JToken.Parse("{\"myArray\": [2, 20, 200, 2000]}");

// Remove by index
new Remove("$.myArray[1]").Execute(data, context);
// Result: {"myArray": [2, 200, 2000]}

// Remove by filter condition
new Remove("$.myArray[?(@ == 20)]").Execute(data, context);
// Result: {"myArray": [2, 200, 2000]}
```

### Complex Object Removal
```csharp
var data = JToken.Parse(@"{
    ""myObject"": {
        ""myArray"": [2, 20, 200, 2000],
        ""nestedObject"": {
            ""myArray"": [1, 2, 3]
        }
    }
}");

// Remove nested arrays using recursive descent
new Remove("$..myArray").Execute(data, context);
// Result: {"myObject": {"nestedObject": {}}}
```

### Multiple Element Removal
```csharp
var data = JToken.Parse("{\"myArray\": [2, 20, 200, 2000]}");

// Remove all elements greater than 20
new Remove("$.myArray[?(@ > 20)]").Execute(data, context);
// Result: {"myArray": [2, 20]}
```

## Validation and Error Handling

### Missing Path Property
```csharp
var result = new Remove("").Execute(data, context);
// result.Success = false
// Log entry: "Path property for remove command is missing"
```

### Non-existent Paths
```csharp
// When targeting non-existent paths
new Remove("$.nonExistentProperty").Execute(data, context);
// Logs warning: "$.nonExistentProperty did not retrieve any items"
// Execution continues successfully
```

### Invalid Remove Operations
```csharp
// When trying to remove from unsupported targets
// Logs error: "Remove only possible on properties or array items"
```

## Fluent API Examples

### Single Property Removal
```csharp
var script = new JLioScript()
    .Remove("$.obsoleteProperty");

var result = script.Execute(data);
```

### Multiple Property Removal
```csharp
var script = new JLioScript()
    .Remove("$.demo")
    .Remove("$.demo2");

var result = script.Execute(data);
```

### Combined with Other Commands
```csharp
var script = new JLioScript()
    .Add(new JValue("temporary"))
    .OnPath("$.temp")
    .Copy("$.important")
    .To("$.backup")
    .Remove("$.temp")
    .Remove("$.important");

var result = script.Execute(data);
```

## Logging and Execution Context

### Successful Removal
```csharp
// Logs info message: "Remove: completed for {Path}"
```

### Warning Messages
```csharp
// When no items found: "{Path} did not retrieve any items"
// When validation fails: "Path property for remove command is missing"
```

### Error Messages
```csharp
// When invalid removal attempted: "Remove only possible on properties or array items"
```

## Performance Considerations

- **JSONPath Evaluation**: Complex paths with filters may require full tree traversal
- **Array Operations**: Removing elements from large arrays requires reindexing
- **Recursive Descent**: Using `$..` patterns can be expensive on large documents
- **Multiple Removals**: Consider the order of operations when removing multiple elements

## Best Practices

1. **Validate paths** before removal in production environments
2. **Use specific paths** rather than broad recursive patterns when possible
3. **Consider order** when removing multiple array elements (remove from highest index first)
4. **Check execution results** to ensure removals were successful
5. **Use logging** to track what was actually removed during execution