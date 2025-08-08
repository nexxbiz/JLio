# JLio Documentation

Welcome to the JLio documentation. This section contains detailed information about using the JLio scripting language and its .NET implementation.

## Core Documentation

- [General Concepts](general.md) - Basic JLio concepts and syntax
- [Commands](commands.md) - All available commands including core and ETL commands
- [Functions](functions.md) - Built-in functions for data processing
- [JSONPath Notation](path-notation.md) - JSONPath expression guide
- [Advanced Scenarios](advanced.md) - Complex use cases and performance tuning
- [Alternatives](alternatives.md) - Alternative approaches and patterns

## Specialized Topics

- [ETL (Extract, Transform, Load)](etl.md) - Comprehensive guide to data transformation pipelines
- [Data Transformation Patterns](etl.md#common-data-pipeline-patterns) - Common ETL pipeline patterns
- [Performance Optimization](advanced.md) - Performance tuning and best practices

## Command References

### Core Commands
Detailed files for every core command are in [commands/](commands):
- [add](commands/add.md), [set](commands/set.md), [put](commands/put.md) - Data manipulation
- [copy](commands/copy.md), [move](commands/move.md) - Data movement
- [remove](commands/remove.md) - Data deletion
- [ifElse](commands/ifElse.md), [decisionTable](commands/decisionTable.md) - Conditional logic
- [merge](commands/merge.md), [compare](commands/compare.md) - Data combination and comparison

### ETL Commands
Specialized commands for data transformation workflows:
- [flatten](commands/flatten.md) - Transform nested JSON to flat structures
- [restore](commands/restore.md) - Reconstruct nested structures from flat data
- [resolve](commands/resolve.md) - Cross-reference and enrich data (JOIN-like operations)
- [toCsv](commands/toCsv.md) - Export JSON data to CSV format

### Function References
All available functions are documented in [functions/](functions):
- Mathematical: [avg](functions/avg.md), [sum](functions/sum.md), [calculate](functions/calculate.md)
- String: [concat](functions/concat.md), [format](functions/format.md), [toString](functions/toString.md)
- Data: [count](functions/count.md), [fetch](functions/fetch.md), [parse](functions/parse.md)
- Utility: [datetime](functions/datetime.md), [newGuid](functions/newGuid.md)

## Quick Start Examples

### Basic Data Manipulation
```json
[
  {
    "path": "$.user.fullName",
    "value": "=concat(@.firstName, ' ', @.lastName)",
    "command": "add"
  }
]
```

### ETL Pipeline
```json
[
  {
    "path": "$.orders[*]",
    "command": "resolve",
    "resolveSettings": [
      {
        "resolveKeys": [{"sourceKey": "customerId", "referenceKey": "id"}],
        "referencesCollectionPath": "$.customers[*]",
        "values": [{"sourceProperty": "customerName", "referenceProperty": "name"}]
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

### Conditional Processing
```json
[
  {
    "path": "$.records[*]",
    "command": "ifElse",
    "condition": "=exists(@.complexData)",
    "then": [
      {
        "path": "@.processedData",
        "command": "flatten"
      }
    ],
    "else": [
      {
        "path": "@.simpleFlag",
        "value": true,
        "command": "add"
      }
    ]
  }
]
```

## Extensions and Packages

JLio supports various extension packages for specialized functionality:

- **JLio.Extensions.ETL** - Data transformation and pipeline commands
- **JLio.Extensions.Math** - Mathematical functions and operations  
- **JLio.Extensions.Text** - Advanced string manipulation functions
- **JLio.Extensions.JSchema** - JSON Schema validation and filtering

## Getting Started

1. **Install JLio**: Add the main JLio package and any required extensions
2. **Learn the Basics**: Start with [General Concepts](general.md)
3. **Explore Commands**: Review the [Commands](commands.md) reference
4. **Try Examples**: Use the provided examples as starting points
5. **Build Pipelines**: For data transformation, see the [ETL Guide](etl.md)

For comprehensive examples and use cases, explore the detailed documentation for each command and function.
