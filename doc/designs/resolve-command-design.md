# Resolve Command Design Document

## Overview
The `resolve` command enables reference resolution and data enrichment by matching keys across collections and injecting resolved data into target locations. This command allows for denormalizing relational data structures within JSON objects and supports resolving from multiple collections in a single command.

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
      "referencesCollectionPath": "$.sourceCollection",
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
      "referencesCollectionPath": "$.permissions",
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
    - **referenceKeyPath**: Path to matching key in reference collection (uses `@.` for relative)
  - **referencesCollectionPath**: Absolute path to collection containing reference data
  - **values**: Array of output configurations for this resolution setting
    - **targetPath**: Where to store resolved data (relative to current item)
    - **value**: What to store (can be `@` for full object, expressions, or structured objects)

## Implementation Requirements

### Core Functionality
1. **Path Processing**: Support `$.` (absolute) and `@.` (relative) path notation
2. **Key Matching**: Support multiple keys for composite matching
3. **Multiple Collections**: Support array of resolve settings for different collections
4. **Multiple Outputs**: Support array of value configurations for different target paths
5. **Expression Support**: Support JLio functions and fetch operations in value mappings
6. **Full Object Reference**: Support `@` to reference entire matched object
7. **Circular Reference Protection**: Detect and handle circular references
8. **Performance**: Efficient matching for large datasets

### Test Cases and Examples

#### Test Case 1: Basic User-Permission Resolution
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
      "referencesCollectionPath": "$.permissions",
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

#### Test Case 2: Multiple Collections Resolution
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
      "referencesCollectionPath": "$.permissions",
      "values": [
        {"targetPath": "@.permissions", "value": "@"},
        {"targetPath": "@.accessLevel", "value": "=fetch(@.role)"}
      ]
    },
    {
      "resolveKeys": [{"keyPath": "@.departmentId", "referenceKeyPath": "@.id"}],
      "referencesCollectionPath": "$.departments",
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

#### Test Case 4: Array Reference Matching with Multiple Collections
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
      "referencesCollectionPath": "$.users",
      "values": [
        {"targetPath": "@.memberDetails", "value": "@"},
        {"targetPath": "@.memberNames", "value": "=fetch(@.displayName)"}
      ]
    },
    {
      "resolveKeys": [{"keyPath": "@.managerId", "referenceKeyPath": "@.id"}],
      "referencesCollectionPath": "$.managers",
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

#### Test Case 5: Complex Multi-Collection Resolution
```json
// Input Data
{
  "projects": [
    {
      "id": "PROJ001", 
      "name": "Website Redesign", 
      "teamLeadId": "USR100",
      "assignedUsers": ["USR100", "USR101", "USR102"],
      "departmentId": "DEPT001"
    }
  ],
  "users": [
    {"id": "USR100", "name": "Alice", "role": "Senior Dev"},
    {"id": "USR101", "name": "Bob", "role": "Junior Dev"},
    {"id": "USR102", "name": "Carol", "role": "Designer"}
  ],
  "departments": [
    {"id": "DEPT001", "name": "Engineering", "budget": 500000}
  ],
  "teamLeads": [
    {"id": "USR100", "experience": "5 years", "certifications": ["AWS", "Azure"]}
  ]
}

// Script
[{
  "path": "$.projects[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [{"keyPath": "@.assignedUsers[*]", "referenceKeyPath": "@.id"}],
      "referencesCollectionPath": "$.users",
      "values": [
        {"targetPath": "@.teamMembers", "value": "@"},
        {"targetPath": "@.teamRoles", "value": "=fetch(@.role)"},
        {"targetPath": "@.teamSize", "value": "=count(@.assignedUsers)"}
      ]
    },
    {
      "resolveKeys": [{"keyPath": "@.departmentId", "referenceKeyPath": "@.id"}],
      "referencesCollectionPath": "$.departments", 
      "values": [
        {"targetPath": "@.departmentInfo", "value": "@"},
        {"targetPath": "@.availableBudget", "value": "=fetch(@.budget)"}
      ]
    },
    {
      "resolveKeys": [{"keyPath": "@.teamLeadId", "referenceKeyPath": "@.id"}],
      "referencesCollectionPath": "$.teamLeads",
      "values": [
        {"targetPath": "@.leadInfo", "value": "@"},
        {"targetPath": "@.leadExperience", "value": "=fetch(@.experience)"}
      ]
    }
  ]
}]

// Expected Output
{
  "projects": [
    {
      "id": "PROJ001", 
      "name": "Website Redesign", 
      "teamLeadId": "USR100",
      "assignedUsers": ["USR100", "USR101", "USR102"],
      "departmentId": "DEPT001",
      "teamMembers": [
        {"id": "USR100", "name": "Alice", "role": "Senior Dev"},
        {"id": "USR101", "name": "Bob", "role": "Junior Dev"},
        {"id": "USR102", "name": "Carol", "role": "Designer"}
      ],
      "teamRoles": ["Senior Dev", "Junior Dev", "Designer"],
      "teamSize": 3,
      "departmentInfo": {"id": "DEPT001", "name": "Engineering", "budget": 500000},
      "availableBudget": 500000,
      "leadInfo": {"id": "USR100", "experience": "5 years", "certifications": ["AWS", "Azure"]},
      "leadExperience": "5 years"
    }
  ],
  "users": [...],
  "departments": [...],
  "teamLeads": [...]
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
      "referencesCollectionPath": "$.permissions",
      "values": [{"targetPath": "@.permissions", "value": "@"}]
    },
    {
      "resolveKeys": [{"keyPath": "@.deptId", "referenceKeyPath": "@.id"}],
      "referencesCollectionPath": "$.departments",
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
3. **Type Mismatches**: Handle gracefully, log warnings
4. **Circular References**: Detect and prevent infinite loops
5. **Memory Limits**: Handle large datasets efficiently
6. **Resolution Setting Failures**: Continue with other resolution settings if one fails

## Performance Requirements

1. **Indexing**: Build internal indexes for each reference collection
2. **Lazy Evaluation**: Only resolve when target paths are accessed
3. **Memory Efficiency**: Stream processing for large datasets
4. **Caching**: Cache resolved references within single execution
5. **Parallel Processing**: Process multiple resolution settings concurrently when possible

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
            .FromCollection("$.permissions")
            .AddValue("@.permissions", "@")
            .AddValue("@.role", "=fetch(@.role)"))
        .AddResolution(resolution => resolution
            .WithKeys("@.deptId", "@.id") 
            .FromCollection("$.departments")
            .AddValue("@.department", "@")
            .AddValue("@.deptName", "=fetch(@.name)")));
```

## Unit Test Structure

Following the pattern established in `AvgTests.cs`, the resolve command should include comprehensive unit tests:

```csharp
[TestCase("basic user-permission resolution", "input.json", "expected.json")]
[TestCase("multiple collections resolution", "multi-collection-input.json", "multi-collection-expected.json")]
[TestCase("multiple key matching", "multi-key-input.json", "multi-key-expected.json")]
[TestCase("array reference matching", "array-input.json", "array-expected.json")]
[TestCase("empty collections", "empty-input.json", "empty-expected.json")]
[TestCase("partial matches", "partial-input.json", "partial-expected.json")]
[TestCase("complex multi-collection", "complex-input.json", "complex-expected.json")]
public void ResolveTests(string scenario, string inputFile, string expectedFile)
{
    // Test implementation following AvgTests pattern
}

[Test]
public void CanBeUsedInFluentApi()
{
    var script = new JLioScript()
        .Resolve("$.users[*]")
        .WithResolveSettings(settings => settings
            .AddResolution(resolution => resolution
                .WithKeys("@.id", "@.userId")
                .FromCollection("$.permissions")
                .AddValue("@.permissions", "@")));
        
    var result = script.Execute(testData);
    // Assertions
}

[Test]
public void CanResolveFromMultipleCollections()
{
    var script = new JLioScript()
        .Resolve("$.users[*]")
        .WithResolveSettings(settings => settings
            .AddResolution(resolution => resolution
                .WithKeys("@.id", "@.userId")
                .FromCollection("$.permissions")
                .AddValue("@.permissions", "@"))
            .AddResolution(resolution => resolution
                .WithKeys("@.deptId", "@.id")
                .FromCollection("$.departments")
                .AddValue("@.department", "@")));
        
    var result = script.Execute(testData);
    // Assertions for both permissions and departments resolved
}
```

## Implementation Notes

1. **Command Registration**: The resolve command should be registered as an extension, similar to Math and Text extensions
2. **Performance Optimization**: For large datasets, consider building hash indexes of reference collections
3. **Memory Management**: Implement streaming for very large reference collections
4. **Circular Reference Detection**: Track resolution paths to prevent infinite loops
5. **Error Recovery**: Continue processing other resolution settings even if some fail
6. **Collection Indexing**: Build separate indexes for each reference collection to optimize lookups
7. **Concurrent Processing**: Consider parallel processing of different resolution settings when they don't interfere with each other