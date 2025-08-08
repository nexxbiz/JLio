# Restore Command Documentation

## Overview

The `Restore` command reconstructs the original nested JSON structure from flattened data created by the `Flatten` command. It uses metadata stored during flattening to accurately recreate arrays, objects, and proper data types.

## Syntax

### JSON Script Format
```json
{
  "path": "$.target.path",
  "command": "restore",
  "restoreSettings": {
    "metadataPath": "$",
    "metadataKey": "_flattenMetadata",
    "removeMetadata": true,
    "strictMode": false
  }
}
```

### Required Properties
- **path**: JSONPath expression targeting the flattened objects to restore
- **command**: Must be "restore"
- **restoreSettings**: Configuration object for restoration behavior

## Restore Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `metadataPath` | string | "" | JSONPath to the metadata container |
| `metadataKey` | string | "_flattenMetadata" | Key name where metadata is stored |
| `removeMetadata` | boolean | true | Whether to remove metadata after restoration |
| `useJsonPathColumn` | boolean | false | Use JSONPath column instead of metadata |
| `jsonPathColumn` | string | "_jsonpath" | Column name containing JSONPath for each field |
| `delimiter` | string | "." | Delimiter used in flattened keys |
| `arrayDelimiter` | string | "." | Delimiter used for array indices |
| `strictMode` | boolean | false | Fail if metadata is missing |

## Examples

### Basic Restoration

**Input (Flattened Data):**
```json
{
  "companies": [
    {
      "id": 1,
      "name": "TechCorp",
      "address.street": "123 Tech Ave",
      "address.city": "Silicon Valley",
      "address.country": "USA",
      "departments.0.id": 101,
      "departments.0.name": "Engineering",
      "departments.0.employees.0.id": 1001,
      "departments.0.employees.0.name": "John Doe",
      "departments.0.employees.0.skills.0": "JavaScript",
      "departments.0.employees.0.skills.1": "C#"
    }
  ],
  "_flattenMetadata": {
    "originalStructure": {
      "address": "object",
      "departments": "array[1]",
      "departments.0": "object",
      "departments.0.employees": "array[1]",
      "departments.0.employees.0": "object",
      "departments.0.employees.0.skills": "array[2]"
    },
    "delimiter": ".",
    "arrayDelimiter": ".",
    "includeArrayIndices": true,
    "preserveTypes": true,
    "typeIndicator": "_type",
    "metadataKey": "_flattenMetadata"
  }
}
```

**Script:**
```json
[{
  "path": "$.companies[*]",
  "command": "restore",
  "restoreSettings": {
    "metadataPath": "$",
    "metadataKey": "_flattenMetadata",
    "removeMetadata": true
  }
}]
```

**Output:**
```json
{
  "companies": [
    {
      "id": 1,
      "name": "TechCorp",
      "address": {
        "street": "123 Tech Ave",
        "city": "Silicon Valley",
        "country": "USA"
      },
      "departments": [
        {
          "id": 101,
          "name": "Engineering",
          "employees": [
            {
              "id": 1001,
              "name": "John Doe",
              "skills": [
                {
                  "0": "JavaScript"
                },
                {
                  "1": "C#"
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```

### Roundtrip Example (Flatten + Restore)

**Original Data:**
```json
{
  "users": [
    {
      "id": 1,
      "name": "John Doe",
      "preferences": {
        "theme": "dark",
        "notifications": true
      },
      "tags": ["admin", "user"]
    }
  ]
}
```

**Roundtrip Script:**
```json
[
  {
    "path": "$.users[*]",
    "command": "flatten",
    "flattenSettings": {
      "delimiter": ".",
      "includeArrayIndices": true,
      "metadataPath": "$",
      "metadataKey": "_metadata"
    }
  },
  {
    "path": "$.users[*]",
    "command": "restore",
    "restoreSettings": {
      "metadataPath": "$",
      "metadataKey": "_metadata",
      "removeMetadata": true
    }
  }
]
```

**Result:** The original structure is reconstructed (with some array formatting differences due to current implementation).

### Restoration without Metadata (Best Effort)

When metadata is not available, restore attempts to reconstruct structure based on key patterns:

**Script:**
```json
[{
  "path": "$.data[*]",
  "command": "restore",
  "restoreSettings": {
    "strictMode": false,
    "delimiter": "."
  }
}]
```

This will attempt to restore structure by parsing dot-notation keys, though results may not be perfect without original metadata.

### Using JSONPath Column

For data with explicit JSONPath information:

**Input:**
```json
{
  "data": [
    {
      "field": "value1",
      "_jsonpath": "$.root.field1"
    },
    {
      "field": "value2", 
      "_jsonpath": "$.root.field2"
    }
  ]
}
```

**Script:**
```json
[{
  "path": "$.data",
  "command": "restore",
  "restoreSettings": {
    "useJsonPathColumn": true,
    "jsonPathColumn": "_jsonpath"
  }
}]
```

## Restoration Modes

### 1. Metadata-Based Restoration (Recommended)
Uses stored metadata for accurate reconstruction:
- Preserves original array structures
- Maintains proper data types
- Handles complex nesting correctly

### 2. Best-Effort Restoration
When metadata is unavailable:
- Parses dot-notation keys to infer structure
- May not handle arrays perfectly
- Simple objects usually restore correctly

### 3. JSONPath-Based Restoration
Uses explicit JSONPath columns:
- Requires JSONPath information for each field
- Useful for external data integration
- Can handle arbitrary structures

## Use Cases

### 1. Data Pipeline Restoration
```json
[
  { "path": "$.data[*]", "command": "flatten" },
  { "path": "$.data[*]", "command": "processData" },
  { "path": "$.data[*]", "command": "restore" }
]
```

### 2. CSV Import and Reconstruction
Import CSV data and restore to original JSON structure:
```json
[
  { "path": "$.records", "command": "fromCsv" },
  { "path": "$.records[*]", "command": "restore" }
]
```

### 3. Database to JSON
Restore hierarchical structures from flattened database records.

## Programmatic Usage

### Constructor with Settings
```csharp
var restoreCommand = new Restore
{
    Path = "$.data[*]",
    RestoreSettings = new RestoreSettings
    {
        MetadataPath = "$",
        MetadataKey = "_flattenMetadata",
        RemoveMetadata = true,
        StrictMode = false
    }
};
```

### Validation

The command validates:
- Path property is not empty
- RestoreSettings is not null
- Delimiter is not empty (when provided)
- JsonPathColumn is specified when UseJsonPathColumn is true

## Performance Considerations

- **Metadata Size**: Large metadata can impact memory usage
- **Complex Structures**: Deeply nested objects take more processing time
- **Array Reconstruction**: Large arrays require more memory allocation
- **Type Conversion**: Type preservation adds processing overhead

## Error Handling

### Strict Mode
With `strictMode: true`, restoration fails if metadata is missing:
```csharp
// Logs error and returns failure result
context.LogError("No flatten metadata found and strict mode is enabled");
```

### Graceful Degradation
With `strictMode: false` (default), attempts best-effort restoration without metadata.

### Invalid Targets
Warnings are logged when trying to restore non-object targets:
```csharp
context.LogWarning("Restore can only be applied to objects, found Array");
```

## Best Practices

1. **Always preserve metadata** when using flatten/restore pipelines
2. **Use consistent metadata keys** across your application
3. **Test restoration** with representative data sets
4. **Consider memory usage** for large datasets
5. **Use strict mode** in production for data integrity
6. **Clean up metadata** with `removeMetadata: true` when done

## Limitations

- Array elements are currently restored as objects with numeric keys (known issue)
- Complex circular references are not supported
- Very deep nesting may impact performance

## Related Commands

- [Flatten](flatten.md) - Create flattened structure with metadata
- [ToCsv](toCsv.md) - Convert flattened data to CSV
- [Resolve](resolve.md) - Cross-reference and resolve data relationships