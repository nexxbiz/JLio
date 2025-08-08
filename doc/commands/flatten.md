# Flatten Command Documentation

## Overview

The `Flatten` command transforms nested JSON objects into a flat structure with dot-notation keys. This is particularly useful for data transformation, CSV generation, and simplifying complex hierarchical data structures.

## Syntax

### JSON Script Format
```json
{
  "path": "$.target.path",
  "command": "flatten",
  "flattenSettings": {
    "delimiter": ".",
    "includeArrayIndices": true,
    "preserveTypes": true,
    "metadataPath": "$",
    "metadataKey": "_flattenMetadata"
  }
}
```

### Required Properties
- **path**: JSONPath expression targeting the objects to flatten
- **command**: Must be "flatten"
- **flattenSettings**: Configuration object for flattening behavior

## Flatten Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `delimiter` | string | "." | Separator for nested property names |
| `arrayDelimiter` | string | "." | Separator for array indices |
| `includeArrayIndices` | boolean | true | Whether to include numeric indices for array elements |
| `preserveTypes` | boolean | true | Whether to add type information columns |
| `typeIndicator` | string | "_type" | Suffix for type information columns |
| `metadataPath` | string | "$" | JSONPath where to store flatten metadata |
| `metadataKey` | string | "_flattenMetadata" | Key name for metadata storage |

## Examples

### Basic Flattening

**Input:**
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
              "skills": ["JavaScript", "C#"]
            }
          ]
        }
      ]
    }
  ]
}
```

**Script:**
```json
[{
  "path": "$.companies[*]",
  "command": "flatten",
  "flattenSettings": {
    "delimiter": ".",
    "includeArrayIndices": true,
    "preserveTypes": true,
    "metadataPath": "$",
    "metadataKey": "_flattenMetadata"
  }
}]
```

**Output:**
```json
{
  "companies": [
    {
      "id": 1,
      "id_type": "Integer",
      "name": "TechCorp",
      "name_type": "String",
      "address.street": "123 Tech Ave",
      "address.street_type": "String",
      "address.city": "Silicon Valley",
      "address.city_type": "String",
      "address.country": "USA",
      "address.country_type": "String",
      "departments.0.id": 101,
      "departments.0.id_type": "Integer",
      "departments.0.name": "Engineering",
      "departments.0.name_type": "String",
      "departments.0.employees.0.id": 1001,
      "departments.0.employees.0.id_type": "Integer",
      "departments.0.employees.0.name": "John Doe",
      "departments.0.employees.0.name_type": "String",
      "departments.0.employees.0.skills.0": "JavaScript",
      "departments.0.employees.0.skills.0_type": "String",
      "departments.0.employees.0.skills.1": "C#",
      "departments.0.employees.0.skills.1_type": "String"
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
    "timestamp": "2024-01-07T15:30:00Z",
    "version": "1.0",
    "rootPath": "$.[0]",
    "metadataKey": "_flattenMetadata"
  }
}
```

### Custom Delimiters

**Script:**
```json
[{
  "path": "$.report",
  "command": "flatten",
  "flattenSettings": {
    "delimiter": "_",
    "includeArrayIndices": true,
    "metadataPath": "$",
    "metadataKey": "_meta"
  }
}]
```

**Input:**
```json
{
  "report": {
    "metadata": {
      "generatedDate": "2024-01-01T10:00:00Z",
      "version": "1.0"
    },
    "sections": [
      {
        "title": "Summary",
        "data": {
          "totalRecords": 100,
          "categories": ["A", "B", "C"]
        }
      }
    ]
  }
}
```

**Output:**
```json
{
  "report": {
    "metadata_generatedDate": "2024-01-01T10:00:00Z",
    "metadata_version": "1.0",
    "sections.0_title": "Summary",
    "sections.0_data_totalRecords": 100,
    "sections.0_data_categories.0": "A",
    "sections.0_data_categories.1": "B",
    "sections.0_data_categories.2": "C"
  }
}
```

### Without Array Indices

**Script:**
```json
[{
  "path": "$.data",
  "command": "flatten",
  "flattenSettings": {
    "delimiter": ".",
    "includeArrayIndices": false,
    "preserveTypes": false
  }
}]
```

This configuration will flatten without numeric array indices, treating arrays as simple values.

## Use Cases

### 1. CSV Preparation
Flatten complex JSON for easy CSV conversion:
```json
[
  { "path": "$.records[*]", "command": "flatten" },
  { "path": "$.records[*]", "command": "toCsv" }
]
```

### 2. Database Import
Prepare nested data for relational database import where each property becomes a column.

### 3. Data Analysis
Transform hierarchical data into tabular format for analysis tools.

### 4. Configuration Flattening
Convert nested configuration objects into flat key-value pairs.

## Programmatic Usage

### Constructor with Settings
```csharp
var flattenCommand = new Flatten
{
    Path = "$.data[*]",
    FlattenSettings = new FlattenSettings
    {
        Delimiter = ".",
        IncludeArrayIndices = true,
        PreserveTypes = true,
        MetadataPath = "$",
        MetadataKey = "_flattenMetadata"
    }
};
```

### Validation

The command validates:
- Path property is not empty
- FlattenSettings is not null
- Delimiter is not empty (when provided)

## Performance Considerations

- **Large Objects**: Flattening deeply nested objects can create many properties
- **Array Handling**: Large arrays with indices can significantly increase property count
- **Type Preservation**: Adds additional type columns, doubling property count
- **Metadata Storage**: Stores structural information for restoration

## Best Practices

1. **Choose appropriate delimiters** that don't conflict with your data
2. **Consider disabling array indices** for very large arrays if indices aren't needed
3. **Use consistent metadata keys** across your pipeline
4. **Preserve metadata** if you plan to restore the original structure
5. **Test with representative data** to understand the output size

## Related Commands

- [Restore](restore.md) - Reverse the flattening process
- [ToCsv](toCsv.md) - Convert flattened data to CSV format
- [Resolve](resolve.md) - Cross-reference flattened data