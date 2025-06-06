# Merge Command Documentation

## Overview

The `Merge` command combines JSON structures by intelligently merging source data into target locations. It supports various merge strategies, array handling options, and key-based matching for complex data integration scenarios.

## Syntax

### JSON Script Format
```json
{
  "path": "$.source.data",
  "targetPath": "$.destination.data",
  "command": "merge",
  "settings": {
    "strategy": "fullMerge",
    "arraySettings": [
      {
        "arrayPath": "$.targetArray",
        "keyPaths": ["id"],
        "uniqueItemsWithoutKeys": true
      }
    ],
    "matchSettings": {
      "keyPaths": ["id", "type"]
    }
  }
}
```

### Required Properties
- **path**: JSONPath expression for the source data to merge from
- **targetPath**: JSONPath expression for the destination to merge into  
- **command**: Must be "merge"
- **settings**: Optional configuration object controlling merge behavior

## Programmatic Usage

### Constructor Options

#### Simple Constructor
```csharp
var mergeCommand = new Merge("$.source", "$.target");
```

#### Constructor with Settings
```csharp
var mergeSettings = new MergeSettings
{
    Strategy = MergeSettings.STRATEGY_FULLMERGE,
    ArraySettings = new List<MergeArraySettings>()
};

var mergeCommand = new Merge("$.source", "$.target", mergeSettings);
```

#### Property Initialization
```csharp
var mergeCommand = new Merge
{
    Path = "$.source",
    TargetPath = "$.target", 
    Settings = MergeSettings.CreateDefault()
};
```

### Fluent API
```csharp
var script = new JLioScript()
    .Merge("$.source")
    .With("$.target")
    .UsingDefaultSettings();

// With custom settings
var script = new JLioScript()
    .Merge("$.source")
    .With("$.target")
    .Using(mergeSettings);
```

## Merge Settings

### Merge Strategies

#### Full Merge (Default)
Merges both structure and values:
```csharp
var settings = new MergeSettings
{
    Strategy = MergeSettings.STRATEGY_FULLMERGE
};
```

#### Structure Only
Adds missing properties but preserves existing values:
```csharp
var settings = new MergeSettings
{
    Strategy = MergeSettings.STRATEGY_ONLY_STRUCTURE
};
```

#### Values Only
Updates existing properties but doesn't add new ones:
```csharp
var settings = new MergeSettings
{
    Strategy = MergeSettings.STRATEGY_ONLY_VALUES
};
```

### Array Settings
Configure how arrays are merged:

```csharp
var arraySettings = new MergeArraySettings
{
    ArrayPath = "$.myArray",                    // Path to the target array
    KeyPaths = new List<string> {"id", "type"}, // Properties for object matching
    UniqueItemsWithoutKeys = true               // Prevent duplicate simple values
};
```

### Match Settings
Define how objects are matched for merging:

```csharp
var matchSettings = new MatchSettings
{
    KeyPaths = new List<string> {"id", "category"}  // Properties that must match
};

var settings = new MergeSettings
{
    MatchSettings = matchSettings
};
```

## Merge Behavior

### Object Merging

#### Basic Object Merge
```csharp
// Source: {"common": 4, "onlyFirst": "first value"}
// Target: {"common": 5, "onlySecond": "second value"}
// Result: {"common": 4, "onlySecond": "second value", "onlyFirst": "first value"}

var data = JObject.Parse(@"{
    ""first"": {""common"": 4, ""onlyFirst"": ""first value""},
    ""second"": {""common"": 5, ""onlySecond"": ""second value""}
}");

new Merge("$.first", "$.second").Execute(data, context);
```

#### Structure-Only Merge
```csharp
// Only adds missing properties, preserves existing values
var settings = new MergeSettings 
{ 
    Strategy = MergeSettings.STRATEGY_ONLY_STRUCTURE 
};

// Source: {"common": 4, "onlyFirst": "first value"}  
// Target: {"common": 5, "onlySecond": "second value"}
// Result: {"common": 5, "onlySecond": "second value", "onlyFirst": "first value"}
```

#### Values-Only Merge
```csharp
// Only updates existing properties, doesn't add new ones
var settings = new MergeSettings 
{ 
    Strategy = MergeSettings.STRATEGY_ONLY_VALUES 
};

// Source: {"common": 4, "onlyFirst": "first value"}
// Target: {"common": 5, "onlySecond": "second value"} 
// Result: {"common": 4, "onlySecond": "second value"}
```

### Array Merging

#### Simple Array Merge
```csharp
// Appends all items from source to target
var data = JObject.Parse(@"{
    ""first"": [1, 2, 3],
    ""second"": [4, 5, 6]
}");

new Merge("$.first", "$.second").Execute(data, context);
// Result: second becomes [4, 5, 6, 1, 2, 3]
```

#### Unique Items Without Keys
```csharp
var settings = new MergeSettings
{
    ArraySettings = new List<MergeArraySettings>
    {
        new MergeArraySettings 
        { 
            ArrayPath = "$.second", 
            UniqueItemsWithoutKeys = true 
        }
    }
};

// Source: [{"item": "1"}, {"item": 2}, {"item": 3.1}]
// Target: [{"item": "4"}, {"item": 5}, {"item": 3.1}]  
// Result: Target gets unique items, duplicates ignored
```

#### Key-Based Object Merging
```csharp
var settings = new MergeSettings
{
    ArraySettings = new List<MergeArraySettings>
    {
        new MergeArraySettings
        {
            ArrayPath = "$.second",
            KeyPaths = new List<string> {"key.id"}
        }
    }
};

// Objects with matching key.id values are merged
// Objects without matches are appended
```

## Examples

### Basic Object Property Merge
```csharp
var data = JObject.Parse(@"{
    ""first"": {""common"": 4, ""onlyFirst"": ""first value""},
    ""second"": {""common"": 5, ""onlySecond"": ""second value""}
}");

var result = new Merge("$.first", "$.second").Execute(data, context);

// Result in $.second:
// {
//   "common": 4,                    // Value from source (first)
//   "onlySecond": "second value",   // Preserved from target  
//   "onlyFirst": "first value"      // Added from source
// }
```

### Complex Array Merge with Keys
```csharp
var data = JObject.Parse(@"{
    ""first"": [
        {""key"": {""id"": ""1""}, ""valueFirst"": ""first id 1"", ""valueCommon"": ""common first id 1""},
        {""key"": {""id"": ""2""}, ""valueFirst"": ""first id 2"", ""valueCommon"": ""common first id 2""},
        {""key"": {""id"": ""3""}, ""valueFirst"": ""first id 3"", ""valueCommon"": ""common first id 3""}
    ],
    ""second"": [
        {""key"": {""id"": ""4""}, ""valueSecond"": ""second id 4"", ""valueCommon"": ""common second id 4""},
        {""key"": {""id"": ""2""}, ""valueSecond"": ""second id 2"", ""valueCommon"": ""common second id 2""},
        {""key"": {""id"": ""1""}, ""valueSecond"": ""second id 1"", ""valueCommon"": ""common second id 1""}
    ]
}");

var settings = new MergeSettings
{
    ArraySettings = new List<MergeArraySettings>
    {
        new MergeArraySettings 
        { 
            ArrayPath = "$.second", 
            KeyPaths = new List<string> {"key.id"} 
        }
    }
};

var result = new Merge("$.first", "$.second", settings).Execute(data, context);

// Objects with matching key.id are merged
// id "1" and "2" objects are merged, id "3" is appended, id "4" remains unchanged
```

### Multi-Level Key Matching
```csharp
var settings = new MergeSettings
{
    ArraySettings = new List<MergeArraySettings>
    {
        new MergeArraySettings
        {
            ArrayPath = "$.second",
            KeyPaths = new List<string> {"key.id", "key.sub"}  // Multiple key properties
        }
    }
};

// Objects must match on both key.id AND key.sub to be merged
```

### Recursive Descent Merge
```csharp
var data = JObject.Parse(@"{
    ""first"": [
        {""PP"": {""PP_EXTERN"": 1, ""PP_MYBRA"": ""extern 1""}},
        {""PP"": {""PP_EXTERN"": 2, ""PP_MYBRA"": ""extern 2""}}
    ],
    ""second"": {
        ""PP"": {""PP_EXTERN"": 1},
        ""previous"": {
            ""PP"": {""PP_EXTERN"": 2}
        }
    }
}");

var settings = new MergeSettings
{
    MatchSettings = new MatchSettings
    {
        KeyPaths = new List<string> {"PP_EXTERN"}
    }
};

// Merge using recursive descent paths
var result = new Merge("$.first..PP", "$.second..PP", settings).Execute(data, context);
```

### Unique Array Items
```csharp
var data = JObject.Parse(@"{
    ""first"": [
        {""item"": ""1""}, 
        {""item"": 2}, 
        {""item"": 3.1}
    ],
    ""second"": [
        {""item"": ""4""}, 
        {""item"": 5}, 
        {""item"": 3.1}
    ]
}");

var settings = new MergeSettings
{
    ArraySettings = new List<MergeArraySettings>
    {
        new MergeArraySettings 
        { 
            ArrayPath = "$.second", 
            UniqueItemsWithoutKeys = true 
        }
    }
};

var result = new Merge("$.first", "$.second", settings).Execute(data, context);

// Duplicate items (like {item: 3.1}) are not added again
```

## Strategy Comparison

### Full Merge Example
```csharp
// Source: {"key": {"id": "1"}, "item": "1", "valueFirst": "first id 1"}
// Target: {"key": {"id": "1"}, "item": "4", "valueSecond": "second id 1"}  
// Result: {"key": {"id": "1"}, "item": "1", "valueSecond": "second id 1", "valueFirst": "first id 1"}
```

### Structure Only Example  
```csharp
// Source: {"key": {"id": "1"}, "item": "1", "valueFirst": "first id 1"}
// Target: {"key": {"id": "1"}, "item": "4", "valueSecond": "second id 1"}
// Result: {"key": {"id": "1"}, "item": "4", "valueSecond": "second id 1", "valueFirst": "first id 1"}
// Note: Existing values preserved, only structure added
```

### Values Only Example
```csharp  
// Source: {"key": {"id": "1"}, "item": "1", "valueFirst": "first id 1"}
// Target: {"key": {"id": "1"}, "item": "4", "valueSecond": "second id 1"}
// Result: {"key": {"id": "1"}, "item": "1", "valueSecond": "second id 1"}  
// Note: New properties not added, only existing values updated
```

## Validation and Error Handling

### Required Properties Validation
```csharp
// Missing Path
// Log: "Path property for merge command is missing"

// Missing TargetPath
// Log: "Target Path property for merge command is missing"
```

### Type Compatibility
```csharp
// Different types replace target completely
// Source: "string value"
// Target: {"object": "value"}
// Result: "string value" (target replaced)
```

## Performance Considerations

### Merge Complexity
- **Simple Objects**: Linear with property count
- **Key-based Arrays**: O(n*m) where n=source size, m=target size  
- **Deep Structures**: Exponential with nesting depth
- **Recursive Paths**: Requires full tree traversal

### Optimization Strategies
- **Use Specific Paths**: Avoid recursive descent when possible
- **Limit Key Properties**: Use minimal necessary keys for matching
- **Choose Appropriate Strategy**: Structure-only is faster than full merge
- **Batch Operations**: Group related merges for efficiency

## Advanced Features

### Path Resolution
The merge command automatically resolves array paths:

```csharp
// Automatically detects array context for settings
// $.myArray maps to array settings for that specific path
// Nested arrays inherit parent settings when not specified
```

### Match Settings Integration
```csharp
// Objects must match on specified keys to be merged
var matchSettings = new MatchSettings
{
    KeyPaths = new List<string> {"id", "type"}
};

// Only merge objects where both id AND type match
// Non-matching objects are appended to target
```

### Type Handling
```csharp
// Same types: Recursive merge for objects/arrays
// Different types: Source replaces target
// Null handling: Treated as different type
```

## Best Practices

1. **Define Clear Key Paths**: Use meaningful, stable properties for object matching
2. **Choose Appropriate Strategy**: Match strategy to use case requirements  
3. **Handle Large Arrays**: Consider performance implications of key matching
4. **Validate Input Types**: Ensure source and target are compatible for merging
5. **Test Edge Cases**: Verify behavior with null values and mixed types
6. **Use Unique Keys**: Ensure key properties uniquely identify objects
7. **Monitor Memory Usage**: Large merges can consume significant memory
8. **Document Merge Rules**: Clearly specify expected merge behavior

## Integration Examples

### Data Synchronization
```csharp
var script = new JLioScript()
    .Copy("$.currentData")
    .To("$.backup")                    // Backup current state
    .Merge("$.incomingUpdates")
    .With("$.currentData")
    .Using(updateSettings)             // Apply updates
    .Add(new JValue("=datetime()"))
    .OnPath("$.lastUpdated");          // Add timestamp
```

### Configuration Merging
```csharp
var defaultSettings = new MergeSettings 
{ 
    Strategy = MergeSettings.STRATEGY_ONLY_STRUCTURE 
};

var script = new JLioScript()
    .Merge("$.defaultConfig")
    .With("$.userConfig")
    .Using(defaultSettings)            // Add missing defaults only
    .Merge("$.environmentConfig")
    .With("$.userConfig")              // Override with environment settings
    .UsingDefaultSettings();
```