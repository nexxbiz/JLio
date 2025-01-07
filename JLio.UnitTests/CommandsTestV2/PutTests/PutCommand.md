# **Documentation Guide for the `put` Command**

## **Overview**
The `put` command is used to add or update a value at a specified path in a JSON structure. It supports dynamic path creation, allowing you to modify deeply nested structures or create new ones if they do not exist.

## **Key Features**
- Leverages JSONPath syntax to target specific locations in the data.
- Handles various data types, including strings, numbers, objects, arrays, and null values.
- Dynamically creates intermediate paths if they do not exist.
- Ensures robust feedback on the operation's success or failure.

---

## **Syntax**
```csharp
var putCommand = new Put(string path, IValueProvider valueToPut);
```

- **`path`**: A JSONPath string specifying where the value should be added or updated.
- **`valueToPut`**: The value to assign, encapsulated in an implementation of `IValueProvider` (e.g., `FixedValue`, `DynamicValueProvider`).

---

## **Execution**
The `put` command is executed using the `Execute` method:
```csharp
var result = putCommand.Execute(JToken data, ExecutionContext context);
```

- **`data`**: The JSON object (`JToken`) to which the `put` operation is applied.
- **`context`**: An execution context that manages options and configurations for the command.

---

## **Examples**

### **1. Add a New Property to the Root**
#### Input
```json
{}
```
#### Command
```csharp
var putCommand = new Put("$.rootProperty", new FixedValue("rootValue"));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "rootProperty": "rootValue"
}
```

---

### **2. Add or Update a Nested Property**
#### Input
```json
{
  "myObject": {}
}
```
#### Command
```csharp
var putCommand = new Put("$.myObject.nestedProperty", new FixedValue("nestedValue"));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "myObject": {
    "nestedProperty": "nestedValue"
  }
}
```

---

### **3. Add a Value to an Array**
#### Input
```json
{
  "myArray": [1, 2, 3]
}
```
#### Command
```csharp
var putCommand = new Put("$.myArray[3]", new FixedValue(4));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "myArray": [1, 2, 3, 4]
}
```

---

### **4. Create a Nested Structure Dynamically**
#### Input
```json
{}
```
#### Command
```csharp
var putCommand = new Put("$.new.nested.path", new FixedValue("value"));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "new": {
    "nested": {
      "path": "value"
    }
  }
}
```

---

## **Edge Cases**

### **1. Adding to a Non-Existing Path**
If the path does not exist, the `put` command creates it dynamically:
```csharp
var putCommand = new Put("$.nonExistent.path", new FixedValue("newValue"));
```
Result:
```json
{
  "nonExistent": {
    "path": "newValue"
  }
}
```

### **2. Overwriting Null Values**
Input:
```json
{
  "property": null
}
```
Command:
```csharp
var putCommand = new Put("$.property", new FixedValue("newValue"));
```
Result:
```json
{
  "property": "newValue"
}
```

---

## **Error Handling**
- **Invalid Path**: Returns `Success: false` if the JSONPath is invalid.
- **Immutable Types**: If the target is immutable, the operation fails.
- **Conflicting Types**: Adding a value where type conflicts exist (e.g., attempting to append to a primitive type) results in failure.

---

## **Best Practices**
1. Always validate the `Success` property in the result for robust error handling.
2. Use descriptive paths to enhance readability and maintainability.
3. Combine with other commands (e.g., `remove`, `replace`) for complex JSON transformations.

