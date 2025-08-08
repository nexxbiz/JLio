# ETL (Extract, Transform, Load) Commands

The JLio ETL extension provides powerful data transformation capabilities for working with JSON data structures. These commands enable complex data pipelines for processing, flattening, resolving relationships, and exporting data.

## Overview

The ETL commands work together to provide a complete data transformation pipeline:

1. **[Flatten](commands/flatten.md)** - Transform nested structures into flat key-value pairs
2. **[Restore](commands/restore.md)** - Reconstruct original nested structures from flattened data
3. **[Resolve](commands/resolve.md)** - Cross-reference and enrich data from multiple collections
4. **[ToCsv](commands/toCsv.md)** - Export data to CSV format with proper encoding and escaping

## Installation and Registration

### Installing the ETL Extension

The ETL commands are available in the `JLio.Extensions.ETL` package.

```csharp
// Register ETL commands with parse options
var parseOptions = ParseOptions.CreateDefault().RegisterETL();
var executionContext = ExecutionContext.CreateDefault();

// Parse and execute scripts with ETL commands
var script = JLioConvert.Parse(scriptJson, parseOptions);
var result = script.Execute(data, executionContext);
```

## Common Data Pipeline Patterns

### Pattern 1: JSON to CSV Export

Transform complex nested JSON data into CSV format suitable for Excel, databases, or analysis tools.

```json
[
  {
    "path": "$.records[*]",
    "command": "flatten",
    "flattenSettings": {
      "delimiter": ".",
      "includeArrayIndices": false,
      "preserveTypes": false
    }
  },
  {
    "path": "$.records[*]",
    "command": "toCsv",
    "csvSettings": {
      "delimiter": ",",
      "includeHeaders": true,
      "quoteAllFields": false
    }
  }
]
```

### Pattern 2: Data Denormalization with Resolution

Enrich normalized data structures with related information from lookup tables.

```json
[
  {
    "path": "$.orders[*]",
    "command": "resolve",
    "resolveSettings": [
      {
        "resolveKeys": [
          {
            "sourceKey": "customerId",
            "referenceKey": "id",
            "asArray": false
          }
        ],
        "referencesCollectionPath": "$.customers[*]",
        "values": [
          {
            "sourceProperty": "customerName",
            "referenceProperty": "name"
          },
          {
            "sourceProperty": "customerEmail",
            "referenceProperty": "email"
          }
        ]
      }
    ]
  },
  {
    "path": "$.orders[*]",
    "command": "flatten"
  },
  {
    "path": "$.orders[*]",
    "command": "toCsv"
  }
]
```

### Pattern 3: Data Processing with Roundtrip

Process flattened data while preserving the ability to restore original structure.

```json
[
  {
    "path": "$.entities[*]",
    "command": "flatten",
    "flattenSettings": {
      "delimiter": ".",
      "includeArrayIndices": true,
      "preserveTypes": true,
      "metadataPath": "$",
      "metadataKey": "_flattenMetadata"
    }
  },
  {
    "path": "$.entities[*]",
    "command": "processData"
  },
  {
    "path": "$.entities[*]",
    "command": "restore",
    "restoreSettings": {
      "metadataPath": "$",
      "metadataKey": "_flattenMetadata",
      "removeMetadata": true
    }
  }
]
```

### Pattern 4: Multi-Source Data Integration

Combine and resolve data from multiple sources into a unified structure.

```json
[
  {
    "path": "$.transactions[*]",
    "command": "resolve",
    "resolveSettings": [
      {
        "resolveKeys": [
          {
            "sourceKey": "userId",
            "referenceKey": "id",
            "asArray": false
          }
        ],
        "referencesCollectionPath": "$.users[*]",
        "values": [
          {
            "sourceProperty": "userName",
            "referenceProperty": "name"
          }
        ]
      },
      {
        "resolveKeys": [
          {
            "sourceKey": "productId",
            "referenceKey": "id",
            "asArray": false
          }
        ],
        "referencesCollectionPath": "$.products[*]",
        "values": [
          {
            "sourceProperty": "productName",
            "referenceProperty": "name"
          }
        ]
      }
    ]
  },
  {
    "path": "$.transactions[*]",
    "command": "flatten"
  },
  {
    "path": "$.transactions[*]",
    "command": "toCsv",
    "csvSettings": {
      "delimiter": "|",
      "nullValueRepresentation": "\\N"
    }
  }
]
```

## Command Combinations

### Flatten + ToCsv
The most common combination for data export:
- **Use Case**: Export complex JSON to Excel-compatible CSV
- **Benefits**: Handles nested structures, arrays, and special characters
- **Best Practice**: Use `includeArrayIndices: false` for cleaner CSV output

### Resolve + Flatten + ToCsv
Complete denormalization pipeline:
- **Use Case**: Create comprehensive reports with all related data
- **Benefits**: Single CSV contains all context without lookups
- **Best Practice**: Resolve first, then flatten for optimal structure

### Flatten + Restore
Data processing with structure preservation:
- **Use Case**: Process flattened data while maintaining original hierarchy
- **Benefits**: Enables complex transformations with guaranteed restoration
- **Best Practice**: Always preserve metadata for accurate restoration

### Resolve + Resolve (Multiple)
Multi-stage data enrichment:
- **Use Case**: Complex data models with multiple relationships
- **Benefits**: Progressive enrichment from multiple reference sources
- **Best Practice**: Order resolve operations by dependency

## Performance Optimization

### Memory Management
```json
// Process data in chunks for large datasets
{
  "path": "$.largeDataset[0:1000]",
  "command": "flatten"
}
```

### Selective Processing
```json
// Only process records that need transformation
{
  "path": "$.records[?(@.needsProcessing == true)]",
  "command": "flatten"
}
```

### Efficient CSV Export
```json
{
  "path": "$.data[*]",
  "command": "toCsv",
  "csvSettings": {
    "includeTypeColumns": false,
    "includeMetadata": false,
    "quoteAllFields": false
  }
}
```

## Error Handling Strategies

### Validation Before Processing
```json
[
  {
    "path": "$",
    "command": "ifElse",
    "condition": "=exists($.data) && count($.data[*]) > 0",
    "then": [
      {
        "path": "$.data[*]",
        "command": "flatten"
      }
    ],
    "else": [
      {
        "path": "$.error",
        "value": "No data to process",
        "command": "add"
      }
    ]
  }
]
```

### Graceful Degradation
```json
{
  "path": "$.records[*]",
  "command": "restore",
  "restoreSettings": {
    "strictMode": false,
    "metadataPath": "$",
    "metadataKey": "_metadata"
  }
}
```

## Integration Examples

### With Standard Commands
```json
[
  // Add processing timestamp
  {
    "path": "$.processedAt",
    "value": "=datetime()",
    "command": "add"
  },
  // Resolve relationships
  {
    "path": "$.records[*]",
    "command": "resolve",
    "resolveSettings": [...]
  },
  // Flatten for export
  {
    "path": "$.records[*]",
    "command": "flatten"
  },
  // Export to CSV
  {
    "path": "$.records[*]",
    "command": "toCsv"
  },
  // Remove original complex data
  {
    "path": "$.originalData",
    "command": "remove"
  }
]
```

### With Functions
```json
[
  // Calculate totals before flattening
  {
    "path": "$.orders[*].total",
    "value": "=sum(@.items[*].price)",
    "command": "add"
  },
  // Flatten with totals
  {
    "path": "$.orders[*]",
    "command": "flatten"
  }
]
```

## Best Practices

### 1. Plan Your Pipeline
- Understand your source data structure
- Define your target output format
- Map the transformation steps required

### 2. Handle Edge Cases
- Empty arrays and null values
- Missing reference data in resolve operations
- Large datasets that may cause memory issues

### 3. Test with Real Data
- Use representative sample data for testing
- Validate output formats with target systems
- Performance test with expected data volumes

### 4. Monitor and Log
- Use execution context logging
- Validate command success
- Monitor memory usage for large operations

### 5. Version Control Settings
- Keep ETL configurations in version control
- Document the purpose of complex pipelines
- Test configuration changes thoroughly

## Troubleshooting Guide

### Common Issues

**Issue**: CSV output contains unwanted quotes
**Solution**: Use `quoteAllFields: false` and check for special characters

**Issue**: Arrays not restoring correctly
**Solution**: Ensure metadata is preserved during flatten operation

**Issue**: Resolve operation not finding matches
**Solution**: Verify JSONPath expressions and key data types

**Issue**: Memory errors with large datasets
**Solution**: Process data in smaller chunks or optimize pipeline

### Debug Strategies

1. **Incremental Testing**: Test each command in isolation
2. **Sample Data**: Use small, representative datasets for debugging
3. **Logging**: Enable detailed logging to trace execution
4. **Validation**: Add validation commands between pipeline stages

## Advanced Topics

### Custom Data Types
Handle custom data types in CSV export:
```json
{
  "csvSettings": {
    "booleanFormat": "YES,NO",
    "nullValueRepresentation": "NULL",
    "dateFormat": "yyyy-MM-dd"
  }
}
```

### Conditional Processing
Apply ETL operations conditionally:
```json
{
  "path": "$.records[*]",
  "command": "ifElse",
  "condition": "=exists(@.complexData)",
  "then": [
    {
      "path": "@",
      "command": "flatten"
    }
  ]
}
```

### Pipeline Optimization
Optimize complex pipelines:
```json
// Combine operations where possible
[
  {
    "path": "$.data[*]",
    "command": "resolve",
    "resolveSettings": [...] // All resolve operations at once
  },
  {
    "path": "$.data[*]",
    "command": "flatten",
    "flattenSettings": {
      "preserveTypes": false, // Skip if not needed
      "includeArrayIndices": false // Simplify output
    }
  }
]
```

## See Also

- [Commands Overview](commands.md)
- [Functions Reference](functions.md)  
- [JSONPath Guide](path-notation.md)
- [Performance Tuning](advanced.md)