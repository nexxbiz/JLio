
# **Documentation Guide for the `Add` Command**

## **Overview**
The `Add` command is used to insert a value at a specified path in a JSON structure. It supports various target types, including objects, arrays, and even creating new paths or nested structures dynamically.

## **Key Features**
- Supports JSONPath syntax to target specific locations in the data.
- Handles a variety of data types such as strings, integers, objects, arrays, null values, and more.
- Can create new properties or arrays dynamically if they do not exist.
- Provides robust feedback on the success or failure of the operation.

---

## **Syntax**
```csharp
var putCommand = new Add(string path, IValueProvider valueToAdd);
```

- **`path`**: A JSONPath string specifying where the value should be added.
- **`valueToAdd`**: The value to insert, wrapped in an implementation of `IValueProvider` (e.g., `FixedValue`, `FunctionSupportedValue`).

---

## **Execution**
The `Add` command is executed using the `Execute` method:
```csharp
var result = putCommand.Execute(JToken data, ExecutionContext context);
```

- **`data`**: The JSON object (`JToken`) on which the `Add` operation is performed.
- **`context`**: An execution context that manages options and configurations for the command.

---

## **Examples**

### **1. Add a New Property to an Object**
#### Input
```json
{
  "myObject": {}
}
```
#### Command
```csharp
var putCommand = new Add("$.myObject.newProperty", new FixedValue("newValue"));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "myObject": {
    "newProperty": "newValue"
  }
}
```

---

### **2. Add a Value to an Existing Array**
#### Input
```json
{
  "myArray": [1, 2, 3]
}
```
#### Command
```csharp
var putCommand = new Add("$.myArray", new FixedValue(4));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "myArray": [1, 2, 3, 4]
}
```

---

### **3. Create a Nested Object**
#### Input
```json
{}
```
#### Command
```csharp
var putCommand = new Add("$.newObject.nestedProperty", new FixedValue("nestedValue"));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "newObject": {
    "nestedProperty": "nestedValue"
  }
}
```

---

### **4. Add a Value to a Non-Existing Array**
#### Input
```json
{
  "myObject": {}
}
```
#### Command
```csharp
var putCommand = new Add("$.myObject.newArray[0]", new FixedValue("firstElement"));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "myObject": {
    "newArray": ["firstElement"]
  }
}
```

---

### **5. Add a Value to the Root**
#### Input
```json
{}
```
#### Command
```csharp
var putCommand = new Add("$.rootProperty", new FixedValue("rootValue"));
putCommand.Execute(data, context);
```
#### Result
```json
{
  "rootProperty": "rootValue"
}
```

---

## **Edge Cases**

### **1. Adding to a Non-Existing Path**
If the path does not exist, the `Add` command can create it dynamically. For instance:
```csharp
var putCommand = new Add("$.new.nested.path", new FixedValue("value"));
```
Result:
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

### **2. Adding to a Nested Array**
For deeply nested arrays:
```csharp
var putCommand = new Add("$.nestedArray[1]", new FixedValue("newValue"));
```
Input:
```json
{
  "nestedArray": [[1, 2], [3, 4]]
}
```
Result:
```json
{
  "nestedArray": [[1, 2], [3, 4, "newValue"]]
}
```

---

## **Error Handling**
- **Invalid Path**: If the provided JSONPath is invalid, the `Execute` method will return `Success: false`.
- **Immutable Types**: If the target is an immutable type (e.g., primitive values), the operation will fail.
- **Conflicting Types**: Adding a value where the type of the target conflicts with the operation (e.g., trying to append to a string) will fail.

---

## **Best Practices**
1. Always validate the `Success` property of the result to handle failures gracefully.
2. Use descriptive paths to ensure clarity in operations.
3. Use execution context options to customize behavior as needed.

---

## **Further Enhancements**
- Combine the `Add` command with other commands like `Remove` or `Replace` for complex transformations.
- Implement custom value providers for dynamic value generation.

This concludes the guide. Let me know if you'd like to add anything specific!