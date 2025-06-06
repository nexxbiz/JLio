# General Concepts

JLio is a language for describing JSON transformations. A script is composed of commands, each command altering the JSON data.

## Parsing Scripts

Scripts are written in JSON notation. They can be parsed into a `JLioScript` object using the `JLioConvert.Parse` method:

```csharp
var script = JLioConvert.Parse(scriptText);
```

Scripts can also be constructed using the fluent API:

```csharp
var script = new JLioScript()
                .Add(new JValue("new Value"))
                .OnPath("$.demo")
                .Add(new Datetime())
                .OnPath("$.this.is.a.long.path.with.a.date");
```

Executing the script applies the commands to a `JToken` data object:

```csharp
var data = JToken.Parse("{\"demoText\":\"Hello World\"}");
var result = script.Execute(data);
```

## JSONPath Basics

JLio uses [JSONPath](https://goessner.net/articles/JsonPath/) to select elements in the data object.

| Notation | Description |
|---|---|
| `$` | the root element |
| `@` | the current element |
| `.` or `[]` | child operator |
| `..` | recursive descent |
| `*` | wildcard |
| `[]` | array subscript |
| `[,]` | union operator |
| `[start:end:step]` | array slice |
| `?()` | filter expression |

JSONPath expressions can be combined to select nodes and filter results.

take a look at the alternatives : 