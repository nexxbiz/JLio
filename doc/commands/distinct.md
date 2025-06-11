# Distinct Command Documentation

## Overview

The `Distinct` command removes duplicate items from arrays. It leverages the merge logic so that when duplicate objects are found they are merged together. Primitive values are de-duplicated as well.

## Syntax

### JSON Script Format
```json
{
  "path": "$.array.path",
  "command": "distinct",
  "keyPaths": ["id"]
}
```

### Required Properties
- **path**: JSONPath to the array(s) to process
- **command**: Must be `distinct`

### Optional Properties
- **keyPaths**: List of property paths used to match objects

## Programmatic Usage

### Constructors
```csharp
var command = new Distinct("$.items");
var commandWithKeys = new Distinct("$.items") { KeyPaths = new List<string>{"id"} };
```

### Fluent API
```csharp
var script = new JLioScript()
    .Distinct("$.numbers")
    .Distinct("$.objects", new List<string>{"id"});
```

## Behavior
- Primitive arrays are reduced to unique values.
- Arrays of objects use `keyPaths` to identify duplicates. Matching objects are merged using the `Merge` command logic.

