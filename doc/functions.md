# Functions

Functions can be used in command arguments by prefixing the value with `=`. Nesting functions is supported.

A function call has the format `=functionName(arg1, arg2)`.

| Function | Arguments | Description |
|---|---|---|
| **concat** | `values...` | Concatenate elements into a string. |
| **datetime** | `time`, `format` | Return the current time in optional format. |
| **fetch** | `url` | Load external JSON. |
| **format** | `value`, `pattern` | Apply string formatting. |
| **newGuid** | *(none)* | Generate a GUID value. |
| **parse** | `jsonpath?` | Parse a string into JSON. |
| **partial** | `jsonpath` | Extract part of a JSON structure. |
| **promote** | `jsonpath` | Move a child element up one level. |
| **toString** | `jsonpath?` | Convert a value to a string. |

Example usage inside a command:

```json
{
  "path": "$.demo",
  "value": "=parse($.source)",
  "command": "set"
}
```

## Function details

- [concat](functions/concat.md)
- [datetime](functions/datetime.md)
- [fetch](functions/fetch.md)
- [format](functions/format.md)
- [newGuid](functions/newGuid.md)
- [parse](functions/parse.md)
- [partial](functions/partial.md)
- [promote](functions/promote.md)
- [toString](functions/toString.md)
- [avg](functions/avg.md)
- [count](functions/count.md)
- [sum](functions/sum.md)
- [filterBySchema](functions/filterBySchema.md)
