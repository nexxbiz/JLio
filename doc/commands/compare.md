# Compare Command Documentation

## Overview

The `Compare` command performs deep comparison between two JSON structures and generates detailed comparison results. It analyzes differences in values, types, structure, and array compositions, making it ideal for data validation, change detection, and quality assurance workflows.

## Syntax

### JSON Script Format
```json
{
  "firstPath": "$.first.data",
  "secondPath": "$.second.data", 
  "resultPath": "$.comparisonResult",
  "command": "compare",
  "settings": {
    "arraySettings": [
      {
        "arrayPath": "$.myArray",
        "keyPaths": ["id"],
        "uniqueIndexMatching": true
      }
    ],
    "resultTypes": ["TypeDifference", "ValueDifference"]
  }
}
```

### Required Properties
- **firstPath**: JSONPath expression for the first value to compare
- **secondPath**: JSONPath expression for the second value to compare
- **resultPath**: JSONPath expression where comparison results will be stored
- **command**: Must be "compare"
- **settings**: Configuration object controlling comparison behavior

## Programmatic Usage

### Constructor and Configuration
```csharp
var compareCommand = new Compare
{
    FirstPath = "$.first",
    SecondPath = "$.second", 
    ResultPath = "$.result",
    Settings = new CompareSettings()
};
```

### Fluent API
```csharp
var script = new JLioScript()
    .Compare("$.first")
    .With("$.second")
    .SetResultOn("$.result");

// With custom settings
var script = new JLioScript()
    .Compare("$.first")
    .With("$.second")
    .Using(compareSettings)
    .SetResultOn("$.result");
```

## Compare Settings

### Basic Settings
```csharp
var settings = new CompareSettings
{
    ArraySettings = new List<CompareArraySettings>(),
    ResultTypes = new List<DifferenceType>()
};
```

### Array Settings
Configure how arrays are compared:

```csharp
var arraySettings = new CompareArraySettings
{
    ArrayPath = "$.myArray",           // Path to the array
    KeyPaths = new List<string> {"id"}, // Properties to use as keys for matching
    UniqueIndexMatching = true          // Check if items are at same index
};
```

#### Array Comparison Modes

**Simple Array Comparison**
```csharp
// Arrays compared by value equality and order
new CompareArraySettings { ArrayPath = "$.myArray" }
```

**Key-based Object Comparison**
```csharp
// Objects in arrays matched by specified key properties
new CompareArraySettings 
{ 
    ArrayPath = "$.users", 
    KeyPaths = new List<string> {"id"} 
}
```

**Index-sensitive Comparison**
```csharp
// Check if matching items are at the same array index
new CompareArraySettings 
{ 
    ArrayPath = "$.items", 
    UniqueIndexMatching = true 
}
```

**Complex Key Matching**
```csharp
// Match objects using multiple nested key properties
new CompareArraySettings 
{ 
    ArrayPath = "$.data", 
    KeyPaths = new List<string> {"key.id", "key.sub"} 
}
```

### Result Type Filtering
Control which types of differences are reported:

```csharp
var settings = new CompareSettings
{
    ResultTypes = new List<DifferenceType>
    {
        DifferenceType.TypeDifference,     // Different data types
        DifferenceType.ValueDifference,    // Different values
        DifferenceType.StructureDifference // Different object structure
    }
};
```

## Comparison Types

### Value Comparison
Compares actual values within same data types:

```csharp
// Numbers
{"first": 1, "second": 2}        // Different values
{"first": 1.1, "second": 1.1}    // Same values
{"first": 1, "second": 1}        // Same values

// Strings  
{"first": "hello", "second": "world"}  // Different values
{"first": "test", "second": "test"}    // Same values

// Booleans
{"first": true, "second": false}   // Different values
{"first": true, "second": true}    // Same values
```

### Type Comparison
Detects when values have different data types:

```csharp
{"first": 1, "second": "1"}        // Number vs String
{"first": true, "second": 1}       // Boolean vs Number
{"first": [1,2], "second": {"a":1}} // Array vs Object
```

### Structure Comparison
Identifies differences in object properties:

```csharp
// Missing properties
{"first": {"a": 1}, "second": {"a": 1, "b": 2}}

// Different property sets
{"first": {"a": 1, "c": 3}, "second": {"a": 1, "b": 2}}
```

### Array Comparison
Analyzes array contents and structure:

```csharp
// Different lengths
{"first": [1, 2], "second": [1, 2, 3]}

// Different order (with UniqueIndexMatching)
{"first": [1, 2], "second": [2, 1]}

// Different items
{"first": [1, 2], "second": [1, 3]}
```

## Examples

### Basic Value Comparison
```csharp
var data = JToken.Parse("{\"first\": 1, \"second\": 2}");
var result = new Compare
{
    FirstPath = "$.first",
    SecondPath = "$.second", 
    ResultPath = "$.result",
    Settings = new CompareSettings()
}.Execute(data, context);

// Result contains difference details in $.result
```

### Object Structure Comparison
```csharp
var data = JToken.Parse(@"{
    ""first"": {""a"": 1, ""b"": 2},
    ""second"": {""a"": 1, ""c"": 3}
}");

var result = new Compare
{
    FirstPath = "$.first",
    SecondPath = "$.second",
    ResultPath = "$.result", 
    Settings = new CompareSettings()
}.Execute(data, context);

// Detects structural differences (missing/extra properties)
```

### Array Comparison with Simple Items
```csharp
var data = JToken.Parse("{\"first\": [1, 2], \"second\": [2, 1]}");

// Order-sensitive comparison
var settings = new CompareSettings
{
    ArraySettings = new List<CompareArraySettings>
    {
        new CompareArraySettings 
        { 
            ArrayPath = "$.first", 
            UniqueIndexMatching = true 
        }
    }
};

var result = new Compare
{
    FirstPath = "$.first",
    SecondPath = "$.second",
    ResultPath = "$.result",
    Settings = settings
}.Execute(data, context);
```

### Complex Object Array Comparison
```csharp
var data = JToken.Parse(@"{
    ""first"": [
        {""id"": 1, ""value"": 1},
        {""id"": 2, ""value"": 2}
    ],
    ""second"": [
        {""id"": 2, ""value"": 2}, 
        {""id"": 1, ""value"": 1}
    ]
}");

var settings = new CompareSettings
{
    ArraySettings = new List<CompareArraySettings>
    {
        new CompareArraySettings
        {
            ArrayPath = "$.first",
            KeyPaths = new List<string> {"id"}  // Match by ID
        }
    }
};

var result = new Compare
{
    FirstPath = "$.first",
    SecondPath = "$.second", 
    ResultPath = "$.result",
    Settings = settings
}.Execute(data, context);

// Objects matched by ID, order differences ignored
```

### Multi-level Key Matching
```csharp
var data = JToken.Parse(@"{
    ""first"": [
        {""key"": {""id"": ""1"", ""sub"": ""a""}, ""value"": 1}
    ],
    ""second"": [
        {""key"": {""id"": ""1"", ""sub"": ""a""}, ""value"": 2}
    ]
}");

var settings = new CompareSettings
{
    ArraySettings = new List<CompareArraySettings>
    {
        new CompareArraySettings
        {
            ArrayPath = "$.first",
            KeyPaths = new List<string> {"key.id", "key.sub"}
        }
    }
};
```

### Different Level Comparison
```csharp
var data = JToken.Parse(@"{
    ""first"": {""sub"": {""a"": 1}},
    ""second"": {""a"": 1}
}");

var result = new Compare
{
    FirstPath = "$.first.sub",  // Compare nested object
    SecondPath = "$.second",    // With root level object
    ResultPath = "$.result",
    Settings = new CompareSettings()
}.Execute(data, context);
```

### Filtered Result Types
```csharp
var settings = new CompareSettings
{
    ResultTypes = new List<DifferenceType>
    {
        DifferenceType.TypeDifference  // Only report type differences
    }
};

var script = new JLioScript()
    .Compare("$.first")
    .With("$.second")
    .Using(settings)
    .SetResultOn("$.result");
```

## Comparison Results

### Result Structure
The comparison generates a `CompareResults` object containing detailed difference information:

```csharp
var compareResults = result.Data.SelectToken("$.result")?.ToObject<CompareResults>();
bool hasDifferences = compareResults?.ContainsIsDifferenceResult();
```

### Difference Types
- **TypeDifference**: Values have different data types
- **ValueDifference**: Values have different content
- **StructureDifference**: Objects have different properties
- **SameValue**: Values are identical
- **ArrayCountDifference**: Arrays have different lengths
- **ArrayItemDifference**: Array items differ
- **ArrayIndexDifference**: Same items at different indexes

### Result Analysis
```csharp
// Check if there are any differences
bool different = compareResults?.ContainsIsDifferenceResult();

// Filter results by type
var typeDifferences = compareResults.Where(r => r.DifferenceType == DifferenceType.TypeDifference);

// Get specific difference details
foreach (var difference in compareResults)
{
    Console.WriteLine($"Type: {difference.DifferenceType}");
    Console.WriteLine($"Path: {difference.Path}");
    Console.WriteLine($"Details: {difference.Description}");
}
```

## Validation and Error Handling

### Required Properties Validation
```csharp
// Missing FirstPath
// Log: "FirstPath property for compare command is missing"

// Missing SecondPath  
// Log: "SecondPath property for compare command is missing"

// Missing ResultPath
// Log: "ResultPath property for compare command is missing"

// Missing Settings
// Log: "Settings property for compare command is missing"
```

### Execution Behavior
```csharp
// Multiple source/target combinations
// Compares each FirstPath match with each SecondPath match
// Results are aggregated into single CompareResults object
```

## Performance Considerations

### Comparison Complexity
- **Simple Values**: Fast equality checks
- **Objects**: Recursive property comparison
- **Arrays**: Potentially O(n²) for key matching
- **Deep Structures**: Exponential with nesting depth

### Optimization Strategies
- **Use Key Paths**: For large object arrays, specify key properties
- **Filter Result Types**: Only generate needed difference types
- **Limit Depth**: Compare specific paths rather than entire structures
- **Batch Processing**: Use multiple compare operations for large datasets

## Advanced Features

### Multiple Target Results
```csharp
var data = JToken.Parse(@"{
    ""result"": [{}, {}],
    ""source"": {""data"": ""value""}
}");

// Results written to multiple locations
var result = new Compare
{
    FirstPath = "$.source",
    SecondPath = "$.source", 
    ResultPath = "$.result[*]",  // Multiple targets
    Settings = new CompareSettings()
}.Execute(data, context);
```

### Number Comparison Precision
```csharp
// Float comparison uses epsilon tolerance (0.0000000001)
{"first": 1.0000000001, "second": 1.0}  // Considered equal

// Integer comparison is exact
{"first": 1, "second": 2}  // Different values
```

### Date and Time Comparison
```csharp
// Dates compared as strings unless converted
{"first": "2009-02-15T00:00:00Z", "second": "2009-02-16T00:00:00Z"}  // Different
```

## Best Practices

1. **Define Clear Key Paths**: Use meaningful properties for object array matching
2. **Filter Result Types**: Only collect needed difference information
3. **Validate Inputs**: Ensure comparison paths exist before execution
4. **Handle Large Arrays**: Consider performance implications of complex matching
5. **Use Appropriate Settings**: Configure array and matching settings for your use case
6. **Test Edge Cases**: Verify behavior with null values and mixed types
7. **Monitor Performance**: Profile comparison operations on large datasets
8. **Document Expectations**: Clearly specify what constitutes a meaningful difference

## Integration Examples

### Data Validation Workflow
```csharp
var script = new JLioScript()
    .Copy("$.originalData")
    .To("$.backup")                    // Backup original
    .Set(transformedData)
    .OnPath("$.transformedData")       // Apply transformation
    .Compare("$.originalData")
    .With("$.transformedData")
    .SetResultOn("$.validationResult"); // Compare results
```

### Change Detection Pipeline
```csharp
var script = new JLioScript()
    .Compare("$.previousVersion")
    .With("$.currentVersion")
    .Using(changeDetectionSettings)
    .SetResultOn("$.changes")
    .Add(new JValue("=datetime()"))
    .OnPath("$.comparedAt");           // Add timestamp
```