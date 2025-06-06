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

Arguments can reference JSONPath locations and may use functions in the `value` field.

## Command details

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
