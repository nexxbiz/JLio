# Commands

A JLio script is an array of command definitions. Each command has a `command` property that names the action and other properties that configure the action.

## Structure

```json
{
  "path": "$.target.path",
  "value": "some value",
  "command": "add"
}
```

The table below lists the available commands and their main arguments.

## Core Commands

| Command | Arguments | Description |
|---|---|---|
| **add** | `path`, `value` | Add a property or append to an array. |
| **set** | `path`, `value` | Replace an existing value. |
| **copy** | `path`, `from` | Copy a value from one location to another. |
| **move** | `path`, `from` | Move a value from one location to another. |
| **put** | `path`, `value` | Create a value only when the target does not exist. |
| **remove** | `path` | Remove a property or array element. |
| **decisionTable** | custom | Execute commands based on a lookup. |
| **ifElse** | `condition`, `then`, `else` | Execute nested scripts conditionally. |
| **merge** | `path`, `value` | Merge objects and arrays. |
| **compare** | `path`, `value` | Compare two values according to settings. |

## ETL (Extract, Transform, Load) Commands

The ETL commands require the `JLio.Extensions.ETL` package and provide powerful data transformation capabilities:

| Command | Arguments | Description |
|---|---|---|
| **flatten** | `path`, `flattenSettings` | Transform nested JSON objects into flat structures with dot-notation keys. |
| **restore** | `path`, `restoreSettings` | Reconstruct original nested structures from flattened data using metadata. |
| **resolve** | `path`, `resolveSettings` | Cross-reference and enrich data from multiple collections (like SQL JOINs). |
| **toCsv** | `path`, `csvSettings` | Convert JSON objects to CSV format with proper encoding and escaping. |

### ETL Registration

To use ETL commands, register the extension:

```csharp
var parseOptions = ParseOptions.CreateDefault().RegisterETL();
```

Arguments can reference JSONPath locations and may use functions in the `value` field.

## Command Details

### Core Commands
- [add](commands/add.md)
- [set](commands/set.md)
- [copy](commands/copy.md)
- [move](commands/move.md)
- [put](commands/put.md)
- [remove](commands/remove.md)
- [decisionTable](commands/decisionTable.md)
- [ifElse](commands/ifElse.md)
- [merge](commands/merge.md)
- [compare](commands/compare.md)

### ETL Commands
- [flatten](commands/flatten.md)
- [restore](commands/restore.md)
- [resolve](commands/resolve.md)
- [toCsv](commands/toCsv.md)

## ETL Pipeline Examples

### JSON to CSV Export
```json
[
  {
    "path": "$.records[*]",
    "command": "flatten",
    "flattenSettings": {
      "delimiter": ".",
      "includeArrayIndices": false
    }
  },
  {
    "path": "$.records[*]",
    "command": "toCsv",
    "csvSettings": {
      "delimiter": ",",
      "includeHeaders": true
    }
  }
]
```

### Data Enrichment Pipeline
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

For comprehensive ETL documentation and patterns, see [ETL Guide](etl.md)
