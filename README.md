# JLio

JLio (jay-lio) is a structured language definition for transforming JSON objects using a transformation script with a JSON notation. 

JLio can create and alter JSON data objects. It allows the transformation of objects, without having the need to code or develop logic. Simply writing the desired commands as a JSON object. Executing the script on a data object will transform the data object following the script commands.

Designed with extensibility in mind, commands, and functions can be added to support the desired functionality. 
The core functionalities provide commands like add, set, copy, move, remove. 

Lowering the number of commands and functions reduces memory consumption and improves performance. The .NET implementation of JLio supports flexible configurations limiting the resources needed.

The extension packs provide additional commands. Functionalities like compare, merge, schema filtering can be added. Writing your own command is a simple task as well as the possibility to override existing ones.

Import-Export function packs give the possibility to transform an JSON object into another structure. The exchange between the different types will be a string notation. Parsing the string notations can convert the string into a proper JSON object.

[[_TOC_]]

## Getting Started
The start point could be a script. To transform into an executable set of commands, it needs to be parsed.

### Adding JLio to your project.

Adding Jlio to your projects is just adding a NuGet package to them. 

```csharp
 dotnet add package JLio.Client
```

### Sample parsing

When you need to parse a JSON string into an executable, you need to use the JlioConver.Parse implementation. The JlioConvert.Parse function will return you a JlioScript that can be executed.

```csharp
var script = JLioConvert.Parse(scriptText);
```

### Fluent API

Alternatively, the script can be composed using the fluent API.

```csharp
 var script = new JLioScript()
                    .Add(new JValue("new Value"))
                    .OnPath("$.demo")
                    .Add(new Datetime())
                    .OnPath("$.this.is.a.long.path.with.a.date");
```
## Execution

Executing the script will alter the object you provide according to the script commands. The script itself has an Execute function to invoke the transformation. The data object has to be a JToken. Converting a String to 

```csharp
var data = JToken.Parse("{\"demoText\":\"Hello World\"}");
var result = script.Execute(JToken.Parse(data));
```
###  Complete Samples

#### Fluent api sample
```csharp
var script = new JLioScript()
                    .Add(new JValue("new Value"))
                    .OnPath("$.demo")
                    .Add(new Datetime())
                    .OnPath("$.this.is.a.long.path.with.a.date");
   
var data = JToken.Parse("{\"demoText\":\"Hello World\"}");             
var result = script.Execute(data);
```

#### parse script sample
```csharp
var script = JLioConvert.Parse("[{\"path\":\"$.myObject.newProperty\",\"value\":\"new value\",\"command\":\"add\"}]");
var data = JToken.Parse("{\"demoText\":\"Hello World\"}");
var result = script.Execute(data);
```

## JsonPath

Jsonpath is used to select the items in the data objects. It uses a simple notation to indicate which element needs to be selected. 

| JSONPath | Description |
|---|---|
| $ |	the root object/element|
| @	| the current object/element|
|. or []	|child operator |
|..	| recursive descent. JSONPath borrows this syntax from E4X.|
|* |	wildcard. All objects/elements regardless their names.| 
|[]	| subscript operator. XPath uses it to iterate over element collections and for predicates. In Javascript and JSON it is the native array operator.|
|[,] |	Union operator in XPath results in a combination of node sets. JSONPath allows alternate names or array indices as a set.|
|[start:end:step] |	array slice operator borrowed from ES4.|
|?()|	applies a filter (script) expression.|
|()|	script expression, using the underlying script engine.|

See: JSONPath expressions - https://goessner.net/

## Commands
Jlio comes with a set of command to transformer. see documentation for details about how to write your own commands. 

### Add

**Definition**

Adds a property to an object, or if the target is an array the value provided will be added to the array. 

The Add command supports the usage of functions.

**Parameters**

|Name|Type|Description|
|---|---|---|
| path | `<string>` | jsonpath notation of the targeted location including the property name to add. If the path doesn't exists the objets and the propertys will be created automatically|
| value | `json` | the value that needs to be added to the newly created property | 

**Sample Json value**

```json
{
  "path": "$.myNewObject.newProperty",
  "value": { "new object": "Added by value" },
  "command": "add"
}
```
When an empty object is provided it will result in: 
```json
{
  "myNewObject": {
    "newProperty": {
      "new object": "Added by value"
    }
  }
}
```

**Sample add to array**

```json
{
  "path": "$.myArray",
  "value": 3,
  "command": "add"
}
```
The data object that the script was executed on
```json
{
  "myArray": [
    1,
    2
  ]
}
```
This will result in: 
```json
{
  "myArray": [
    1,
    2,
    3
  ]
}
```

**Sample add with a function**

```json
{
  "path": "$.myNewObject.newProperty",
  "value": "=newGuid()",
  "command": "add"
}
```
When an empty object is provided it will result in: 
```json
{
  "myNewObject": {
    "newProperty": "4d2c4ec7-30ca-4eea-aeb3-2154fb02eb1d"
  }
}
```

### Set

**Definition**

Sets the value of a property of an object. The set command can be used to set values on an object or set items in an array, depending on the jsonpath notation used.
When the path doesn't retrun any objects nothing will **not** be set. The target should exists. 

The Set command supports the usage of functions.

**Parameters**

|Name|Type|Description|
|---|---|---|
| path | `<string>` | jsonpath notation of the targeted location including the property name to add. If the path doesn't exists the objets and the propertys will be created automatically|
| value | `json` | the value that needs to be added to the newly created property | 

**Sample Json value**

```json
{
  "path": "$.myObject",
  "value": { "new object": "Added by value" },
  "command": "set"
}
```
The data object that the script was executed on
```json
{
  "myObject": {
      "demo" : 1
      }
}
```

Results in:
```json
{
  "myObject": {
    "new object": "Added by value"
  }
}
```

**Sample Json value**

```json
{
  "path": "$.myNewObject.newProperty",
  "value": { "new object": "Added by value" },
  "command": "set"
}
```
When an empty object is provided it will result in: 
```json
{}
```


**Sample add to array item**

```json
{
  "path": "$.myArray[0]",
  "value": 3,
  "command": "set"
}
```
The data object that the script was executed on
```json
{
  "myArray": [
    1,
    2
  ]
}
```
This will result in: 
```json
{
  "myArray": [
    3,
    2
  ]
}
```

**Sample set with a function**

```json
{
  "path": "$.myObject",
  "value": "=newGuid()",
  "command": "set"
}
```
The data object that the script was executed on
```json
{
  "myObject": [
    1,
    2
  ]
}

This will result in

```json
{
  "myObject": {
    "newProperty": "4d2c4ec7-30ca-4eea-aeb3-2154fb02eb1d"
  }
}
```
### Copy
Structure, intent, samples, fluent api
### Move
Structure, intent, samples, fluent api
### Remove
Structure, intent, samples, fluent api
### Merge
Structure, intent, samples, fluent api
### Compare
Structure, intent, samples, fluent api

## Functions
### datetime

#### Samples
- datetime()
- datetime(UTC)
- datetime(startOfDay)
- datetime(startofDayUTC)
- datetime('dd-MM-yyyy HH:mm:ss')
- datetime(UTC, 'dd-MM-yyyy HH:mm:ss')
- datetime(startOfDay,'dd-MM-yyyy HH:mm:ss'
- datetime(startOfDayUTC, 'dd-MM-yyyy HH:mm:ss')

Default: datetime()
timeselection , local time now
format : 2012-04-23T18:25:43.511Z

Sample 

Script
```json



```

Object
```json


```
