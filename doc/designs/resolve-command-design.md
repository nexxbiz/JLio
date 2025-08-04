# Resolve Command Design Document

## Overview
The `resolve` command enables reference resolution and data enrichment by matching keys across collections and injecting resolved data into target locations. This command allows for denormalizing relational data structures within JSON objects and supports resolving from multiple collections in a single command.

----
**TL;DR Highlights**

> Core Capabilities
>
> - Multiple Collections: Resolve from multiple reference collections in one command
> - Array Matching: Full support for your scenario (refKey: 1 ↔ keys: [1,2])
> - Flexible Output: Multiple target paths with JLio function expressions
> - Full Object Reference: Use @ to inject complete matched objects
  
**Command Structure Example**

Shows the basic structure with your exact array matching scenario

__Array Matching Scenarios__

1.	Simple → Array: Your use case
2.	Array → Simple: Reverse scenario
3.	Array → Array: Complex intersections
4.	Multiple Matches: One key matching multiple references

__Use Cases__

- User-permission resolution
- Product-category matching
- Multi-level organizational data enrichment
- Tag-based content categorization
- Complex relational data denormalization

## Command Structure

```json
{
  "path": "$.targetCollection[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [
        {
          "keyPath": "@.referenceProperty",
          "referenceKeyPath": "@.keyProperty"
        }
      ],
      "referencesCollectionPath": "$.sourceCollection[*]",
      "values": [
        {
          "targetPath": "@.enrichedData",
          "value": "@"
        },
        {
          "targetPath": "@.customData",
          "value": {
            "name": "=fetch(@.displayName)",
            "metadata": "=fetch(@.details)"
          }
        }
      ]
    },
    {
      "resolveKeys": [
        {
          "keyPath": "@.id",
          "referenceKeyPath": "@.assignedTo"
        }
      ],
      "referencesCollectionPath": "$.permissions[*]",
      "values": [
        {
          "targetPath": "@.userPermissions",
          "value": "@"
        }
      ]
    }
  ]
}
```

## Key Properties

- **path**: JSONPath selecting items to process (each item becomes the current context `@`)
- **resolveSettings**: Array of resolution configurations, allowing multiple collections to be resolved in one command
  - **resolveKeys**: Array of key mappings for matching within this resolution setting
    - **keyPath**: Path to key in current item (uses `@.` for relative)
    - **referenceKeyPath**: Path to matching key in reference collection (uses `@.` for relative, supports array notation like `@.keys[*]`)
  - **referencesCollectionPath**: **CRITICAL**: Must use `[*]` notation to select individual items from the collection (e.g., `$.references[*]` not `$.references`)
  - **values**: Array of output configurations for this resolution setting
    - **targetPath**: Where to store resolved data (relative to current item)
    - **value**: What to store (can be `@` for full object, expressions, or structured objects)

## Critical Implementation Note: Collection Path Selection

⚠️ **IMPORTANT**: The `referencesCollectionPath` must use the `[*]` notation to select individual items from arrays:

- ✅ **Correct**: `"referencesCollectionPath": "$.references[*]"`
- ❌ **Incorrect**: `"referencesCollectionPath": "$.references"`

The `[*]` notation ensures that each reference object is processed individually rather than attempting to match against the entire array structure.

## Array Reference Matching Support

The resolve command supports several array matching scenarios:

### 1. Simple Key to Array Matching
```json
// Source item has simple key: {"refKey": 1}
// Reference has array: {"keys": [1, 2]}
// Configuration: "referenceKeyPath": "@.keys[*]"
// Logic: Check if source key (1) exists within reference array [1, 2]
```

### 2. Array to Simple Key Matching
```json
// Source item has array: {"keys": [1, 2]}  
// Reference has simple key: {"refKey": 1}
// Configuration: "keyPath": "@.keys[*]"
// Logic: Check if any source array value matches reference key
```

### 3. Array to Array Intersection
```json
// Source item has array: {"tags": ["a", "b"]}
// Reference has array: {"keywords": ["b", "c"]}
// Configuration: "keyPath": "@.tags[*]", "referenceKeyPath": "@.keywords[*]"
// Logic: Check if arrays have any common elements
```

### 4. Multiple Array Matches
When a key matches multiple reference arrays, the behavior should be:
- Return array of all matching reference objects
- Allow value expressions to process each match
- Support aggregation functions for multiple matches

## Implementation Requirements

### Core Functionality
1. **Path Processing**: Support `$.` (absolute) and `@.` (relative) path notation
2. **Key Matching**: Support multiple keys for composite matching
3. **Array Key Matching**: Support matching simple keys against array values in reference collections
4. **Multiple Collections**: Support array of resolve settings for different collections
5. **Multiple Outputs**: Support array of value configurations for different target paths
6. **Expression Support**: Support JLio functions and fetch operations in value mappings
7. **Full Object Reference**: Support `@` to reference entire matched object
8. **Circular Reference Protection**: Detect and handle circular references
9. **Performance**: Efficient matching for large datasets

### Test Cases and Examples

```json
// Input Data - Exact user scenario
{
  "items": [
    {"refKey": 1, "name": "Item One"}
  ],
  "references": [
    {"a": 1, "keys": [1, 2], "category": "Group A"},
    {"a": 2, "keys": [3, 4], "category": "Group B"}
  ]
}

// Script - Match refKey against keys array
[{
  "path": "$.items[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.refKey", "referenceKeyPath": "@.keys[*]"}],
      "referencesCollectionPath": "$.references[*]",
      "values": [
        {"targetPath": "@.referenceData", "value": "@"},
        {"targetPath": "@.category", "value": "=fetch(@.category)"},
        {"targetPath": "@.a", "value": "=fetch(@.a)"}
      ]
    }
  ]
}]

// Expected Output
{
  "items": [
    {
      "refKey": 1, 
      "name": "Item One",
      "referenceData": {"a": 1, "keys": [1, 2], "category": "Group A"},
      "category": "Group A",
      "a": 1
    }
  ],
  "references": [
    {"a": 1, "keys": [1, 2], "category": "Group A"},
    {"a": 2, "keys": [3, 4], "category": "Group B"}
  ]
}
```

#### Test Case 2: Multiple Items with Array Matching
```json
// Input Data
{
  "items": [
    {"refKey": 1, "name": "Item One"},
    {"refKey": 3, "name": "Item Three"},
    {"refKey": 5, "name": "Item Five"}
  ],
  "references": [
    {"id": "REF001", "keys": [1, 2], "category": "Group A"},
    {"id": "REF002", "keys": [3, 4], "category": "Group B"},
    {"id": "REF003", "keys": [5, 6, 7], "category": "Group C"}
  ]
}

// Script
[{
  "path": "$.items[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.refKey", "referenceKeyPath": "@.keys[*]"}],
      "referencesCollectionPath": "$.references[*]",
      "values": [
        {"targetPath": "@.referenceData", "value": "@"},
        {"targetPath": "@.category", "value": "=fetch(@.category)"},
        {"targetPath": "@.referenceId", "value": "=fetch(@.id)"}
      ]
    }
  ]
}]

// Expected Output
{
  "items": [
    {
      "refKey": 1, 
      "name": "Item One",
      "referenceData": {"id": "REF001", "keys": [1, 2], "category": "Group A"},
      "category": "Group A",
      "referenceId": "REF001"
    },
    {
      "refKey": 3,
      "name": "Item Three", 
      "referenceData": {"id": "REF002", "keys": [3, 4], "category": "Group B"},
      "category": "Group B",
      "referenceId": "REF002"
    },
    {
      "refKey": 5,
      "name": "Item Five",
      "referenceData": {"id": "REF003", "keys": [5, 6, 7], "category": "Group C"},
      "category": "Group C", 
      "referenceId": "REF003"
    }
  ],
  "references": [
    {"id": "REF001", "keys": [1, 2], "category": "Group A"},
    {"id": "REF002", "keys": [3, 4], "category": "Group B"},
    {"id": "REF003", "keys": [5, 6, 7], "category": "Group C"}
  ]
}
```

#### Test Case 3: Array to Array Intersection
```json
// Input Data
{
  "products": [
    {"id": "PROD001", "tags": ["electronics", "mobile"], "name": "Smartphone"},
    {"id": "PROD002", "tags": ["furniture", "office"], "name": "Desk Chair"}
  ],
  "categories": [
    {"name": "Tech", "keywords": ["electronics", "mobile", "computer"]},
    {"name": "Office", "keywords": ["furniture", "office", "supplies"]},
    {"name": "Home", "keywords": ["furniture", "kitchen", "bedroom"]}
  ]
}

// Script - Match array against array (intersection)
[{
  "path": "$.products[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.tags[*]", "referenceKeyPath": "@.keywords[*]"}],
      "referencesCollectionPath": "$.categories[*]",
      "values": [
        {"targetPath": "@.matchingCategories", "value": "@"},
        {"targetPath": "@.categoryNames", "value": "=fetch(@.name)"}
      ]
    }
  ]
}]

// Expected Output
{
  "products": [
    {
      "id": "PROD001", 
      "tags": ["electronics", "mobile"], 
      "name": "Smartphone",
      "matchingCategories": [
        {"name": "Tech", "keywords": ["electronics", "mobile", "computer"]}
      ],
      "categoryNames": ["Tech"]
    },
    {
      "id": "PROD002", 
      "tags": ["furniture", "office"], 
      "name": "Desk Chair",
      "matchingCategories": [
        {"name": "Office", "keywords": ["furniture", "office", "supplies"]},
        {"name": "Home", "keywords": ["furniture", "kitchen", "bedroom"]}
      ],
      "categoryNames": ["Office", "Home"]
    }
  ],
  "categories": [
    {"name": "Tech", "keywords": ["electronics", "mobile", "computer"]},
    {"name": "Office", "keywords": ["furniture", "office", "supplies"]},
    {"name": "Home", "keywords": ["furniture", "kitchen", "bedroom"]}
  ]
}
```

#### Test Case 4: Multiple Array Matches Edge Case
```json
// Input: One key matches multiple reference arrays
{
  "items": [{"refKey": 1, "name": "Popular Item"}],
  "references": [
    {"id": "REF001", "keys": [1, 2], "category": "Group A"},
    {"id": "REF002", "keys": [1, 3], "category": "Group B"}
  ]
}

// Script
[{
  "path": "$.items[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.refKey", "referenceKeyPath": "@.keys[*]"}],
      "referencesCollectionPath": "$.references[*]",
      "values": [
        {"targetPath": "@.referenceData", "value": "@"},
        {"targetPath": "@.categories", "value": "=fetch(@.category)"}
      ]
    }
  ]
}]

// Expected: Multiple matches returned as array
{
  "items": [
    {
      "refKey": 1, 
      "name": "Popular Item",
      "referenceData": [
        {"id": "REF001", "keys": [1, 2], "category": "Group A"},
        {"id": "REF002", "keys": [1, 3], "category": "Group B"}
      ],
      "categories": ["Group A", "Group B"]
    }
  ],
  "references": [
    {"id": "REF001", "keys": [1, 2], "category": "Group A"},
    {"id": "REF002", "keys": [1, 3], "category": "Group B"}
  ]
}
```

#### Test Case 5: Basic User-Permission Resolution
```json
// Input Data
{
  "users": [
    {"id": "USR100", "name": "Alice"},
    {"id": "USR101", "name": "Bob"}
  ],
  "permissions": [
    {"userId": "USR100", "role": "admin", "scope": "global"},
    {"userId": "USR101", "role": "viewer", "scope": "limited"}
  ]
}

// Script
[{
  "path": "$.users[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.id", "referenceKeyPath": "@.userId"}],
      "values": [
        {"targetPath": "@.fullPermission", "value": "@"},
        {"targetPath": "@.role", "value": "=fetch(@.role)"},
        {"targetPath": "@.accessLevel", "value": "=concat(@.role, ':', @.scope)"}
      ]
    }
  ]
}]

// Expected Output
{
  "users": [
    {
      "id": "USR100", 
      "name": "Alice",
      "fullPermission": {"userId": "USR100", "role": "admin", "scope": "global"},
      "role": "admin",
      "accessLevel": "admin:global"
    },
    {
      "id": "USR101",
      "name": "Bob", 
      "fullPermission": {"userId": "USR101", "role": "viewer", "scope": "limited"},
      "role": "viewer",
      "accessLevel": "viewer:limited"
    }
  ],
  "permissions": [
    {"userId": "USR100", "role": "admin", "scope": "global"},
    {"userId": "USR101", "role": "viewer", "scope": "limited"}
  ]
}
```

```json
// Input Data
{
  "users": [
    {"id": "USR100", "name": "Alice", "departmentId": "DEPT001"},
    {"id": "USR101", "name": "Bob", "departmentId": "DEPT002"}
  ],
  "permissions": [
    {"userId": "USR100", "role": "admin", "scope": "global"},
    {"userId": "USR101", "role": "viewer", "scope": "limited"}
  ],
  "departments": [
    {"id": "DEPT001", "name": "Engineering", "budget": 500000},
    {"id": "DEPT002", "name": "Marketing", "budget": 250000}
  ]
}

// Script - Resolve from multiple collections
[{
  "path": "$.users[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.id", "referenceKeyPath": "@.userId"}],
      "values": [
        {"targetPath": "@.permissions", "value": "@"},
        {"targetPath": "@.accessLevel", "value": "=fetch(@.role)"}
      ]
    },
    {
      "resolveKeys": [{"keyPath": "@.departmentId", "referenceKeyPath": "@.id"}],
      "values": [
        {"targetPath": "@.department", "value": "@"},
        {"targetPath": "@.departmentName", "value": "=fetch(@.name)"},
        {"targetPath": "@.departmentBudget", "value": "=fetch(@.budget)"}
      ]
    }
  ]
}]

// Expected Output
{
  "users": [
    {
      "id": "USR100", 
      "name": "Alice",
      "departmentId": "DEPT001",
      "permissions": {"userId": "USR100", "role": "admin", "scope": "global"},
      "accessLevel": "admin",
      "department": {"id": "DEPT001", "name": "Engineering", "budget": 500000},
      "departmentName": "Engineering",
      "departmentBudget": 500000
    },
    {
      "id": "USR101",
      "name": "Bob",
      "departmentId": "DEPT002", 
      "permissions": {"userId": "USR101", "role": "viewer", "scope": "limited"},
      "accessLevel": "viewer",
      "department": {"id": "DEPT002", "name": "Marketing", "budget": 250000},
      "departmentName": "Marketing",
      "departmentBudget": 250000
    }
  ],
  "permissions": [
    {"userId": "USR100", "role": "admin", "scope": "global"},
    {"userId": "USR101", "role": "viewer", "scope": "limited"}
  ],
  "departments": [
    {"id": "DEPT001", "name": "Engineering", "budget": 500000},
    {"id": "DEPT002", "name": "Marketing", "budget": 250000}
  ]
}
```

#### Test Case 3: Multiple Key Matching with Multiple Collections
```json
// Input Data
{
  "orders": [
    {"customerId": "C001", "productId": "P001", "quantity": 2, "salesRepId": "REP001"},
    {"customerId": "C002", "productId": "P002", "quantity": 1, "salesRepId": "REP002"}
  ],
  "inventory": [
    {"customerId": "C001", "productId": "P001", "stock": 100, "price": 25.99},
    {"customerId": "C002", "productId": "P002", "stock": 50, "price": 15.50}
  ],
  "salesReps": [
    {"id": "REP001", "name": "John Smith", "commission": 0.05},
    {"id": "REP002", "name": "Jane Doe", "commission": 0.03}
  ]
}

// Script
[{
  "path": "$.orders[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [
        {"keyPath": "@.customerId", "referenceKeyPath": "@.customerId"},
        {"keyPath": "@.productId", "referenceKeyPath": "@.productId"}
      ],
      "referencesCollectionPath": "$.inventory",
      "values": [
        {"targetPath": "@.inventoryData", "value": "@"},
        {"targetPath": "@.unitPrice", "value": "=fetch(@.price)"},
        {"targetPath": "@.totalPrice", "value": "=calculate(@.price * $.current.quantity)"}
      ]
    },
    {
      "resolveKeys": [{"keyPath": "@.salesRepId", "referenceKeyPath": "@.id"}],
      "referencesCollectionPath": "$.salesReps",
      "values": [
        {"targetPath": "@.salesRep", "value": "@"},
        {"targetPath": "@.salesRepName", "value": "=fetch(@.name)"},
        {"targetPath": "@.commission", "value": "=calculate($.current.totalPrice * @.commission)"}
      ]
    }
  ]
}]

// Expected Output
{
  "orders": [
    {
      "customerId": "C001", 
      "productId": "P001", 
      "quantity": 2,
      "salesRepId": "REP001",
      "inventoryData": {"customerId": "C001", "productId": "P001", "stock": 100, "price": 25.99},
      "unitPrice": 25.99,
      "totalPrice": 51.98,
      "salesRep": {"id": "REP001", "name": "John Smith", "commission": 0.05},
      "salesRepName": "John Smith",
      "commission": 2.599
    },
    {
      "customerId": "C002", 
      "productId": "P002", 
      "quantity": 1,
      "salesRepId": "REP002",
      "inventoryData": {"customerId": "C002", "productId": "P002", "stock": 50, "price": 15.50},
      "unitPrice": 15.50,
      "totalPrice": 15.50,
      "salesRep": {"id": "REP002", "name": "Jane Doe", "commission": 0.03},
      "salesRepName": "Jane Doe",
      "commission": 0.465
    }
  ],
  "inventory": [...],
  "salesReps": [...]
}
```

```json
// Input Data
{
  "groups": [
    {"id": "GRP001", "name": "Admins", "members": ["USR100", "USR101"], "managerId": "USR100"}
  ],
  "users": [
    {"id": "USR100", "displayName": "Alice", "email": "alice@test.com"},
    {"id": "USR101", "displayName": "Bob", "email": "bob@test.com"}
  ],
  "managers": [
    {"id": "USR100", "title": "Senior Manager", "department": "IT"}
  ]
}

// Script
[{
  "path": "$.groups[*]",
  "command": "resolve", 
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.members[*]", "referenceKeyPath": "@.id"}],
      "values": [
        {"targetPath": "@.memberDetails", "value": "@"},
        {"targetPath": "@.memberNames", "value": "=fetch(@.displayName)"}
      ]
    },
    {
      "resolveKeys": [{"keyPath": "@.managerId", "referenceKeyPath": "@.id"}],
      "values": [
        {"targetPath": "@.managerInfo", "value": "@"},
        {"targetPath": "@.managerTitle", "value": "=fetch(@.title)"}
      ]
    }
  ]
}]

// Expected Output
{
  "groups": [
    {
      "id": "GRP001", 
      "name": "Admins", 
      "members": ["USR100", "USR101"],
      "managerId": "USR100",
      "memberDetails": [
        {"id": "USR100", "displayName": "Alice", "email": "alice@test.com"},
        {"id": "USR101", "displayName": "Bob", "email": "bob@test.com"}
      ],
      "memberNames": ["Alice", "Bob"],
      "managerInfo": {"id": "USR100", "title": "Senior Manager", "department": "IT"},
      "managerTitle": "Senior Manager"
    }
  ],
  "users": [...],
  "managers": [...]
}
```

```json
    {
  ]
}

// Script
[{
  "command": "resolve",
  "resolveSettings": [
    {
      "values": [
      ]
    {
      ]
    {
      ]
    }
  ]
}]

{
    {
    }
  ],
}
```

#### Test Case 6: Edge Cases with Multiple Collections

##### Empty Collections
```json
// Input: One collection empty, others populated
{
  "users": [{"id": "USR100", "name": "Alice", "deptId": "DEPT001"}], 
  "permissions": [],
  "departments": [{"id": "DEPT001", "name": "IT"}]
}

// Script
[{
  "path": "$.users[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.id", "referenceKeyPath": "@.userId"}],
      "values": [{"targetPath": "@.permissions", "value": "@"}]
    },
    {
      "resolveKeys": [{"keyPath": "@.deptId", "referenceKeyPath": "@.id"}],
      "values": [{"targetPath": "@.department", "value": "@"}]
    }
  ]
}]

// Expected: Department resolved, permissions empty, no errors
{
  "users": [
    {
      "id": "USR100", 
      "name": "Alice", 
      "deptId": "DEPT001",
      "department": {"id": "DEPT001", "name": "IT"}
    }
  ],
  "permissions": [],
  "departments": [{"id": "DEPT001", "name": "IT"}]
}
```

##### Partial Matches Across Collections
```json
// Input: Some collections have matches, others don't
{
  "users": [{"id": "USR100", "name": "Alice", "deptId": "DEPT999"}], 
  "permissions": [{"userId": "USR100", "role": "admin"}],
  "departments": [{"id": "DEPT001", "name": "IT"}]
}

// Expected: Permissions resolved, department not resolved
{
  "users": [
    {
      "id": "USR100", 
      "name": "Alice", 
      "deptId": "DEPT999",
      "permissions": {"userId": "USR100", "role": "admin"}
    }
  ],
  "permissions": [{"userId": "USR100", "role": "admin"}],
  "departments": [{"id": "DEPT001", "name": "IT"}]
}
```

## Error Handling Requirements

1. **Invalid JSONPath**: Log warning, skip invalid paths
2. **Missing Reference Collection**: Log error, skip that resolution setting
4. **Circular References**: Detect and prevent infinite loops
5. **Memory Limits**: Handle large datasets efficiently
6. **Resolution Setting Failures**: Continue with other resolution settings if one fails
7. **Array Processing Errors**: Handle malformed arrays gracefully

## Performance Requirements


## Integration Requirements

1. **Fluent API**: Support fluent builder pattern with multiple collections
2. **Extension Registration**: Register with parse options
3. **Function Support**: Full compatibility with JLio function system
4. **Validation**: Comprehensive input validation for all resolution settings
5. **Logging**: Detailed execution logging for debugging

## Builder Pattern Example
```csharp
var script = new JLioScript()
    .Resolve("$.users[*]")
    .WithResolveSettings(settings => settings
        .AddResolution(resolution => resolution
            .WithKeys("@.id", "@.userId")
            .AddValue("@.permissions", "@")
            .AddValue("@.role", "=fetch(@.role)"))
        .AddResolution(resolution => resolution
            .WithKeys("@.deptId", "@.id") 
            .AddValue("@.department", "@")
            .AddValue("@.deptName", "=fetch(@.name)")));
```

## Unit Test Structure

Following the pattern established in `AvgTests.cs`, the resolve command should include comprehensive unit tests:

```csharp
[TestCase("simple key to array matching", "key-to-array-input.json", "key-to-array-expected.json")]
[TestCase("multiple items array matching", "multi-items-array-input.json", "multi-items-array-expected.json")]
[TestCase("array to array intersection", "array-intersection-input.json", "array-intersection-expected.json")]
[TestCase("multiple array matches", "multi-array-input.json", "multi-array-expected.json")]
[TestCase("basic user-permission resolution", "input.json", "expected.json")]
[TestCase("multiple collections resolution", "multi-collection-input.json", "multi-collection-expected.json")]
[TestCase("empty collections", "empty-input.json", "empty-expected.json")]
[TestCase("partial matches", "partial-input.json", "partial-expected.json")]
[TestCase("complex multi-collection", "complex-input.json", "complex-expected.json")]
public void ResolveTests(string scenario, string inputFile, string expectedFile)
{
    // Test implementation following AvgTests pattern
}

[Test]
{
    var testData = JToken.Parse(@"{
        ""items"": [{""refKey"": 1, ""name"": ""Popular Item""}],
        ""references"": [
            {""id"": ""REF001"", ""keys"": [1, 2], ""category"": ""Group A""},
            {""id"": ""REF002"", ""keys"": [1, 3], ""category"": ""Group B""}
        ]
    }");

    var script = new JLioScript()
        .WithResolveSettings(settings => settings
            .AddResolution(resolution => resolution
        
    var result = script.Execute(testData);
}

[Test]
public void CanResolveFromMultipleCollections()
{
    var script = new JLioScript()
        .Resolve("$.users[*]")
        .WithResolveSettings(settings => settings
            .AddResolution(resolution => resolution
                .WithKeys("@.id", "@.userId")
                .AddValue("@.permissions", "@"))
            .AddResolution(resolution => resolution
                .WithKeys("@.deptId", "@.id")
                .AddValue("@.department", "@")));
        
    var result = script.Execute(testData);
    // Assertions for both permissions and departments resolved
}
```

## Implementation Notes

1. **Command Registration**: The resolve command should be registered as an extension, similar to Math and Text extensions
