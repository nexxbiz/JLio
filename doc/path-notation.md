# JLio Path Notation Guide

This document describes the path notation system used throughout JLio for navigating and selecting JSON data elements.

## Overview

JLio uses JSONPath-based notation with extensions for relative navigation and context-aware operations. The path system supports both absolute and relative references, with special indicators for current context and parent navigation.

## Path Indicators

### Root Indicator: `$`
The `$` symbol represents the root of the JSON document and creates absolute paths.

```json
// Example data
{
  "users": [
    {"name": "Alice", "age": 30},
    {"name": "Bob", "age": 25}
  ],
  "config": {"version": "1.0"}
}

// Absolute path examples
"$.users"           // Selects the entire users array
"$.users[0]"        // Selects the first user object
"$.users[0].name"   // Selects "Alice"
"$.config.version"  // Selects "1.0"
```

### Current Item Indicator: `@`
The `@` symbol represents the current item context when processing multiple items. This is particularly useful in commands that iterate over arrays or multiple objects.

```json
// Example: Adding current order path to each order
// Input data
{
  "orders": [
    {"id": "ORD-001", "customerId": "C001", "total": 150.00},
    {"id": "ORD-002", "customerId": "C002", "total": 75.50}
  ]
}

// Script: Add current order path to each order
{
  "path": "$.orders[*].orderPath",
  "value": "=path()",  // Returns path to current order object
  "command": "add"
}

// Result
{
  "orders": [
    {"id": "ORD-001", "customerId": "C001", "total": 150.00, "orderPath": "$.orders[0]"},
    {"id": "ORD-002", "customerId": "C002", "total": 75.50, "orderPath": "$.orders[1]"}
  ]
}
```

### Parent Navigation: `<--`
The `<--` indicator allows navigation to parent levels in the JSON hierarchy. This only works with relative path notation (starting with `@`).

```json
// Example: Department and employee relationship
// Input data
{
  "departments": [
    {
      "name": "Engineering",
      "budget": 500000,
      "employees": [
        {"name": "Alice", "role": "Developer", "salary": 75000},
        {"name": "Bob", "role": "Manager", "salary": 90000}
      ]
    },
    {
      "name": "Marketing", 
      "budget": 300000,
      "employees": [
        {"name": "Carol", "role": "Designer", "salary": 65000}
      ]
    }
  ]
}

// Script: Add department path to each employee
{
  "path": "$.departments[*].employees[*].departmentPath",
  "value": "=path(@.<--)",  // @.<-- refers to parent department
  "command": "add"
}

// Result
{
  "departments": [
    {
      "name": "Engineering",
      "budget": 500000,
      "employees": [
        {"name": "Alice", "role": "Developer", "salary": 75000, "departmentPath": "$.departments[0]"},
        {"name": "Bob", "role": "Manager", "salary": 90000, "departmentPath": "$.departments[0]"}
      ]
    },
    {
      "name": "Marketing",
      "budget": 300000, 
      "employees": [
        {"name": "Carol", "role": "Designer", "salary": 65000, "departmentPath": "$.departments[1]"}
      ]
    }
  ]
}
```

## Array Selection Patterns

### Whole Array vs Array Items

Understanding the difference between selecting the entire array versus selecting items within the array is crucial:

#### Whole Array Selection
```json
"$.myArray"    // Selects the entire array as one item
```

```json
// Example
{
  "numbers": [1, 2, 3, 4]
}

// $.numbers selects: [1, 2, 3, 4] (the entire array)
```

#### Array Items Selection
```json
"$.myArray[*]"  // Selects each item in the array individually
```

```json
// Example
{
  "numbers": [1, 2, 3, 4]
}

// $.numbers[*] selects: 1, 2, 3, 4 (four separate items)
```

#### Practical Implications

This distinction affects how commands process the data:

```json
// Adding to whole array (appends to array)
{
  "path": "$.numbers",
  "value": 5,
  "command": "add"
}
// Result: {"numbers": [1, 2, 3, 4, 5]}

// Processing each array item individually
{
  "path": "$.numbers[*].doubled",
  "value": "=calculate('{{ @ }} * 2')",
  "command": "add"
}
// This would add a "doubled" property to each number if they were objects
```

## Path Types

### Absolute Paths
Start with `$` and reference from the document root:

```json
"$.users[0].profile.email"
"$.configuration.settings.timeout"
"$.data.items[*].properties.name"
```

### Relative Paths
Start with `@` and reference from the current execution context:

```json
"@.name"              // Property of current item
"@.profile.email"     // Nested property of current item
"@.items[*]"          // Array items within current item
"@.items[0].value"    // Specific array element property
```

### Parent Navigation Paths
Use `@.<--` to navigate up the hierarchy:

```json
"@.<--"               // Direct parent of current item
"@.<--.name"          // Property of parent item
"@.<--.<--"           // Grandparent of current item
```

## Examples

### Current Object Path
```json
// Input data
{
  "customers": [
    {"id": "C001", "name": "Alice Johnson", "type": "premium"},
    {"id": "C002", "name": "Bob Smith", "type": "standard"}
  ]
}

// Script: Add current customer path to each customer
{
  "path": "$.customers[*].customerPath",
  "value": "=path()",  // Returns path to current customer object
  "command": "add"
}

// Result
{
  "customers": [
    {"id": "C001", "name": "Alice Johnson", "type": "premium", "customerPath": "$.customers[0]"},
    {"id": "C002", "name": "Bob Smith", "type": "standard", "customerPath": "$.customers[1]"}
  ]
}
```

### Property Path
```json
// Input data (same as above)

// Script: Add path to specific property of each customer
{
  "path": "$.customers[*].idPath",
  "value": "=path(@.id)",  // Returns path to id property
  "command": "add"
}

// Result
{
  "customers": [
    {"id": "C001", "name": "Alice Johnson", "type": "premium", "idPath": "$.customers[0].id"},
    {"id": "C002", "name": "Bob Smith", "type": "standard", "idPath": "$.customers[1].id"}
  ]
}
```

### Cross-Reference Resolution
```json
// Input data
{
  "books": [
    {"id": 1, "title": "The Great Gatsby", "authorId": 101},
    {"id": 2, "title": "To Kill a Mockingbird", "authorId": 102}
  ],
  "authors": [
    {"id": 101, "name": "F. Scott Fitzgerald"},
    {"id": 102, "name": "Harper Lee"}
  ]
}

// Script: Add author name path to each book
{
  "path": "$.books[*].authorNamePath", 
  "value": "=path($.authors[?(@.id == @.authorId)].name)",  // Cross-reference using @.authorId
  "command": "add"
}

// Result
{
  "books": [
    {"id": 1, "title": "The Great Gatsby", "authorId": 101, "authorNamePath": "$.authors[0].name"},
    {"id": 2, "title": "To Kill a Mockingbird", "authorId": 102, "authorNamePath": "$.authors[1].name"}
  ],
  "authors": [...]
}
```

### Multi-Level Hierarchy
```json
// Input data
{
  "companies": [
    {
      "name": "TechCorp",
      "headquarters": "San Francisco",
      "divisions": [
        {
          "name": "Software",
          "teams": [
            {"name": "Frontend", "size": 5},
            {"name": "Backend", "size": 8}
          ]
        }
      ]
    }
  ]
}

// Script: Add company name to each team
{
  "path": "$.companies[*].divisions[*].teams[*].companyName",
  "value": "=fetch(@.<--.<--.name)",  // Navigate up to company level
  "command": "add"
}

// Result: Each team gets the company name from its grandparent
{
  "companies": [
    {
      "name": "TechCorp",
      "headquarters": "San Francisco", 
      "divisions": [
        {
          "name": "Software",
          "teams": [
            {"name": "Frontend", "size": 5, "companyName": "TechCorp"},
            {"name": "Backend", "size": 8, "companyName": "TechCorp"}
          ]
        }
      ]
    }
  ]
}
```

## Path Function

The `path()` function returns the JSONPath string for a given location:

### Without Arguments
```json
"=path()"  // Returns path to current execution context
```

### With Relative Path
```json
"=path(@.propertyName)"  // Returns absolute path to property
```

### With Absolute Path
```json
"=path($.some.absolute.path)"  // Returns the same absolute path
```

### With Parent Navigation
```json
"=path(@.<--)"  // Returns path to parent of current context
```

## Path Function Examples

### Basic Path Resolution
```json
// Input data
{
  "sample": {
    "myArray": [
      {"myItem": "value1"},
      {"myItem": "value2"}
    ]
  }
}

// Script
[{
  "path": "$.sample.myArray[*].currentPath",
  "value": "=path()",
  "command": "add"
}]

// Result
{
  "sample": {
    "myArray": [
      {"myItem": "value1", "currentPath": "$.sample.myArray[0]"},
      {"myItem": "value2", "currentPath": "$.sample.myArray[1]"}
    ]
  }
}
```

### Relative Path Resolution
```json
// Input data (same as above)

// Script
[{
  "path": "$.sample.myArray[*].relativePath",
  "value": "=path(@.myItem)",
  "command": "add"
}]

// Result
{
  "sample": {
    "myArray": [
      {"myItem": "value1", "relativePath": "$.sample.myArray[0].myItem"},
      {"myItem": "value2", "relativePath": "$.sample.myArray[1].myItem"}
    ]
  }
}
```

### Parent Path Navigation
```json
// Input data
{
  "sample": {
    "myArray": [
      {"myItem": "value1", "otherProp": "other1"},
      {"myItem": "value2", "otherProp": "other2"}
    ]
  }
}

// Script using parent navigation
[{
  "path": "$.sample.myArray[*].path",
  "value": "=path(@.<--)",
  "command": "add"
}]

// Result - each item gets path to its parent (the array element)
{
  "sample": {
    "myArray": [
      {
        "myItem": "value1",
        "otherProp": "other1",
        "path": "$.sample.myArray[0]"
      },
      {
        "myItem": "value2",
        "otherProp": "other2",
        "path": "$.sample.myArray[1]"
      }
    ]
  }
}
```

### Multiple Path Operations
```json
// Input data
{
  "sample": {
    "myArray": [
      {"myItem": "value1", "otherProp": "other1"},
      {"myItem": "value2", "otherProp": "other2"}
    ]
  }
}

// Complex script with multiple path operations
[
  {
    "path": "$.sample.myArray[*].itemPath",
    "value": "=path(@.myItem)",
    "command": "add"
  },
  {
    "path": "$.sample.myArray[*].metadata",
    "value": {"originalPath": "=path(@.otherProp)"},
    "command": "add"
  }
]

// Result
{
  "sample": {
    "myArray": [
      {
        "myItem": "value1", 
        "otherProp": "other1",
        "itemPath": "$.sample.myArray[0].myItem",
        "metadata": {"originalPath": "$.sample.myArray[0].otherProp"}
      },
      {
        "myItem": "value2",
        "otherProp": "other2", 
        "itemPath": "$.sample.myArray[1].myItem",
        "metadata": {"originalPath": "$.sample.myArray[1].otherProp"}
      }
    ]
  }
}
```

### Real-World Book/Author Example
```json
// Input data
{
  "books": [
    {"id": 1, "title": "The Great Gatsby", "author": {"id": 101, "name": "F. Scott Fitzgerald"}},
    {"id": 2, "title": "To Kill a Mockingbird", "author": {"id": 102, "name": "Harper Lee"}}
  ],
  "authors": [
    {"id": 101, "name": "F. Scott Fitzgerald"},
    {"id": 102, "name": "Harper Lee"}
  ]
}

// Script: Count books for each author using cross-reference
[{
  "path": "$.authors[*].bookCount",
  "value": "=count($.books[?(@.author.id == @.id)])",  // @ refers to current author
  "command": "add"
}]

// Result
{
  "books": [...],
  "authors": [
    {"id": 101, "name": "F. Scott Fitzgerald", "bookCount": 1},
    {"id": 102, "name": "Harper Lee", "bookCount": 1}
  ]
}
```

## Understanding Path Context

It's important to understand what the current execution context is when using path functions:

### Current Object vs Property Path
```json
// When processing $.orders[*], the current token is each order object

// To get path to the current order object:
"=path()"     // Returns "$.orders[0]", "$.orders[1]", etc.
"=path(@)"    // Same as above

// To get path to a property of the current order:
"=path(@.id)"           // Returns "$.orders[0].id", "$.orders[1].id", etc.
"=path(@.customerId)"   // Returns "$.orders[0].customerId", etc.
```

## Best Practices

1. **Use Absolute Paths (`$`) for**:
   - Root-level operations
   - Cross-document references
   - Configuration and lookup values

2. **Use Relative Paths (`@`) for**:
   - Processing array items
   - Operations within current context
   - Nested transformations

3. **Use Parent Navigation (`<--`) for**:
   - Accessing parent properties during child processing
   - Building hierarchical relationships
   - Context-aware transformations

4. **Array Selection Guidelines**:
   - Use `$.array` when working with the array as a whole
   - Use `$.array[*]` when processing individual items
   - Always use `[*]` notation for collection processing in commands like resolve

5. **Path Function Usage**:
   - Use `=path()` to get current execution context path (the object being processed)
   - Use `=path(@.property)` to build absolute paths from relative property references
   - Use for debugging and metadata operations

## Common Pitfalls

1. **Array Selection Confusion**:
   ```json
   // Wrong - selects whole array
   "$.items"
   
   // Correct - selects individual items
   "$.items[*]"
   ```

2. **Mixing Path Types**:
   ```json
   // Wrong - mixing absolute and relative
   "$.@.property"
   
   // Correct - use one or the other
   "$.property" or "@.property"
   ```

3. **Parent Navigation on Absolute Paths**:
   ```json
   // Wrong - parent navigation only works with relative paths
   "$.<--"
   
   // Correct - use with relative paths
   "@.<--"
   ```

4. **Path Function Context Confusion**:
   ```json
   // Common mistake - confusing object path vs property path
   
   // When processing $.customers[*]:
   "=path(@.id)"    // Returns path to ID property: "$.customers[0].id"
   "=path()"        // Returns path to customer object: "$.customers[0]"
   "=path(@)"       // Same as path(): "$.customers[0]"
   ```

5. **Context Confusion in Cross-References**:
   ```json
   // Input data context is important!
   {
     "orders": [{"customerId": "C001"}],
     "customers": [{"id": "C001", "name": "Alice"}]
   }
   
   // Wrong - @ context is unclear
   "$.customers[?(@.id == @.customerId)]"
   
   // Better - be explicit about which @ you mean
   // When processing $.orders[*]:
   "$.customers[?(@.id == @.customerId)]"  // First @ is customer, second @ is current order
   ```

This path notation system provides powerful and flexible ways to navigate and manipulate JSON data structures in JLio scripts.