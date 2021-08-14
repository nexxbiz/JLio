# JLio

JLio (jay-lio) is a structured language definition for transforming JSON objects using a transformation script with a JSON notation. 

JLio can create and alter JSON data objects. It allows the transformation of objects, without having the need to code or develop logic. Simply writing the desired commands as a JSON object. Executing the script on a data object will transform the data object following the script commands.

Designed with extensibility in mind, commands, and functions can be added to support the desired functionality. 
The core functionalities provide commands like add, set, copy, move, remove. 

Lowering the number of commands and functions reduces memory consumption and improves performance. The .NET implementation of JLio supports flexible configurations limiting the resources needed.

The extension packs provide additional commands. Functionalities like compare, merge, schema filtering can be added. Writing your own command is a simple task as well as the possibility to override existing ones.

Import-Export function packs give the possibility to transform an JSON object into another structure. The exchange between the different types will be a string notation. Parsing the string notations can convert the string into a proper JSON object.


## Getting Started
The start point could be a script. To transform into an executable set of commands, it needs to be parsed.

Adding JLio to your project.

Sample parsing

Alternatively, the script can be composed using the fluent API.

Sample of the fluent API

Finally 

Execution

## Commands

for every commad we need : Structure, intend, samples, fluent api
### Add
### Set
### Copy
### Move
### Remove

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
