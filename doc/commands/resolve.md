# Resolve Command Documentation

## Overview

The `Resolve` command performs cross-referencing and data resolution operations between collections. It matches records based on keys and enriches data by copying values from reference collections, similar to SQL JOIN operations but for JSON data structures.

## Syntax

### JSON Script Format
```json
{
  "path": "$.target.collection[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [
        {
          "sourceKey": "userId",
          "referenceKey": "id",
          "asArray": false
        }
      ],
      "referencesCollectionPath": "$.users[*]",
      "values": [
        {
          "sourceProperty": "userName",
          "referenceProperty": "name"
        },
        {
          "sourceProperty": "userEmail",
          "referenceProperty": "email"
        }
      ]
    }
  ]
}
```

### Required Properties
- **path**: JSONPath expression targeting the collection to enrich
- **command**: Must be "resolve"
- **resolveSettings**: Array of resolution configurations

## Resolution Settings Structure

### ResolveSettings Properties
| Property | Type | Description |
|----------|------|-------------|
| `resolveKeys` | ResolveKey[] | Array of key matching configurations |
| `referencesCollectionPath` | string | JSONPath to the reference collection |
| `values` | ResolveValue[] | Array of value copying configurations |

### ResolveKey Properties
| Property | Type | Description |
|----------|------|-------------|
| `sourceKey` | string | Property name in source records |
| `referenceKey` | string | Property name in reference records |
| `asArray` | boolean | Whether source key contains array values |

### ResolveValue Properties
| Property | Type | Description |
|----------|------|-------------|
| `sourceProperty` | string | Target property name in source records |
| `referenceProperty` | string | Source property name in reference records |

## Examples

### Basic User-Permission Resolution

**Input Data:**
```json
{
  "userPermissions": [
    {
      "userId": 1,
      "permissionId": "read"
    },
    {
      "userId": 2,
      "permissionId": "write"
    }
  ],
  "users": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "john@example.com"
    },
    {
      "id": 2,
      "name": "Jane Smith",
      "email": "jane@example.com"
    }
  ],
  "permissions": [
    {
      "id": "read",
      "description": "Read access",
      "level": 1
    },
    {
      "id": "write",
      "description": "Write access",
      "level": 2
    }
  ]
}
```

**Script:**
```json
[{
  "path": "$.userPermissions[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [
        {
          "sourceKey": "userId",
          "referenceKey": "id",
          "asArray": false
        }
      ],
      "referencesCollectionPath": "$.users[*]",
      "values": [
        {
          "sourceProperty": "userName",
          "referenceProperty": "name"
        },
        {
          "sourceProperty": "userEmail",
          "referenceProperty": "email"
        }
      ]
    },
    {
      "resolveKeys": [
        {
          "sourceKey": "permissionId",
          "referenceKey": "id",
          "asArray": false
        }
      ],
      "referencesCollectionPath": "$.permissions[*]",
      "values": [
        {
          "sourceProperty": "permissionDescription",
          "referenceProperty": "description"
        },
        {
          "sourceProperty": "permissionLevel",
          "referenceProperty": "level"
        }
      ]
    }
  ]
}]
```

**Output:**
```json
{
  "userPermissions": [
    {
      "userId": 1,
      "permissionId": "read",
      "userName": "John Doe",
      "userEmail": "john@example.com",
      "permissionDescription": "Read access",
      "permissionLevel": 1
    },
    {
      "userId": 2,
      "permissionId": "write",
      "userName": "Jane Smith",
      "userEmail": "jane@example.com",
      "permissionDescription": "Write access",
      "permissionLevel": 2
    }
  ],
  "users": [
    {
      "id": 1,
      "name": "John Doe",
      "email": "john@example.com"
    },
    {
      "id": 2,
      "name": "Jane Smith",
      "email": "jane@example.com"
    }
  ],
  "permissions": [
    {
      "id": "read",
      "description": "Read access",
      "level": 1
    },
    {
      "id": "write",
      "description": "Write access",
      "level": 2
    }
  ]
}
```

### Array Key Matching

When source keys contain arrays of values:

**Input Data:**
```json
{
  "orders": [
    {
      "orderId": 1,
      "productIds": [101, 102],
      "customerId": 1
    }
  ],
  "products": [
    {
      "id": 101,
      "name": "Laptop",
      "price": 999.99
    },
    {
      "id": 102,
      "name": "Mouse",
      "price": 29.99
    }
  ]
}
```

**Script:**
```json
[{
  "path": "$.orders[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [
        {
          "sourceKey": "productIds",
          "referenceKey": "id",
          "asArray": true
        }
      ],
      "referencesCollectionPath": "$.products[*]",
      "values": [
        {
          "sourceProperty": "productNames",
          "referenceProperty": "name"
        },
        {
          "sourceProperty": "productPrices",
          "referenceProperty": "price"
        }
      ]
    }
  ]
}]
```

**Output:**
```json
{
  "orders": [
    {
      "orderId": 1,
      "productIds": [101, 102],
      "customerId": 1,
      "productNames": ["Laptop", "Mouse"],
      "productPrices": [999.99, 29.99]
    }
  ],
  "products": [
    {
      "id": 101,
      "name": "Laptop",
      "price": 999.99
    },
    {
      "id": 102,
      "name": "Mouse",
      "price": 29.99
    }
  ]
}
```

### Multiple Collections Resolution

Resolving from multiple reference collections:

**Input Data:**
```json
{
  "transactions": [
    {
      "id": 1,
      "userId": 101,
      "productId": 201,
      "storeId": 301,
      "amount": 150.00
    }
  ],
  "users": [
    {
      "id": 101,
      "name": "Alice Johnson",
      "tier": "premium"
    }
  ],
  "products": [
    {
      "id": 201,
      "name": "Wireless Headphones",
      "category": "Electronics"
    }
  ],
  "stores": [
    {
      "id": 301,
      "name": "Downtown Store",
      "city": "New York"
    }
  ]
}
```

**Script:**
```json
[{
  "path": "$.transactions[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [
        {
          "sourceKey": "userId",
          "referenceKey": "id",
          "asArray": false
        }
      ],
      "referencesCollectionPath": "$.users[*]",
      "values": [
        {
          "sourceProperty": "customerName",
          "referenceProperty": "name"
        },
        {
          "sourceProperty": "customerTier",
          "referenceProperty": "tier"
        }
      ]
    },
    {
      "resolveKeys": [
        {
          "sourceKey": "productId",
          "referenceKey": "id",
          "asArray": false
        }
      ],
      "referencesCollectionPath": "$.products[*]",
      "values": [
        {
          "sourceProperty": "productName",
          "referenceProperty": "name"
        },
        {
          "sourceProperty": "productCategory",
          "referenceProperty": "category"
        }
      ]
    },
    {
      "resolveKeys": [
        {
          "sourceKey": "storeId",
          "referenceKey": "id",
          "asArray": false
        }
      ],
      "referencesCollectionPath": "$.stores[*]",
      "values": [
        {
          "sourceProperty": "storeName",
          "referenceProperty": "name"
        },
        {
          "sourceProperty": "storeCity",
          "referenceProperty": "city"
        }
      ]
    }
  ]
}]
```

**Output:**
```json
{
  "transactions": [
    {
      "id": 1,
      "userId": 101,
      "productId": 201,
      "storeId": 301,
      "amount": 150.00,
      "customerName": "Alice Johnson",
      "customerTier": "premium",
      "productName": "Wireless Headphones",
      "productCategory": "Electronics",
      "storeName": "Downtown Store",
      "storeCity": "New York"
    }
  ]
}
```

## Resolution Strategies

### 1. Single Key Resolution
Match on one key from source to reference collection.

### 2. Array Key Resolution
When source contains arrays of keys, match each array element.

### 3. Multiple Reference Collections
Resolve from multiple collections in a single command execution.

### 4. Partial Matches
Handle cases where not all keys find matches in reference collections.

## Use Cases

### 1. Data Denormalization
Transform normalized data structures into denormalized format:
```json
// Convert foreign keys to actual data for reporting
[
  { "path": "$.orders[*]", "command": "resolve" }
]
```

### 2. Data Enrichment
Add contextual information from reference data:
```json
// Add user details to activity logs
[
  { "path": "$.activities[*]", "command": "resolve" }
]
```

### 3. Report Generation
Create comprehensive reports with resolved relationships:
```json
// Generate customer order reports with product and customer details
[
  { "path": "$.orderSummaries[*]", "command": "resolve" },
  { "path": "$.orderSummaries[*]", "command": "flatten" },
  { "path": "$.orderSummaries[*]", "command": "toCsv" }
]
```

### 4. API Response Enhancement
Enhance API responses with related data without multiple API calls.

## Programmatic Usage

### Constructor with Settings
```csharp
var resolveCommand = new Resolve
{
    Path = "$.transactions[*]",
    ResolveSettings = new List<ResolveSetting>
    {
        new ResolveSetting
        {
            ResolveKeys = new List<ResolveKey>
            {
                new ResolveKey
                {
                    SourceKey = "userId",
                    ReferenceKey = "id",
                    AsArray = false
                }
            },
            ReferencesCollectionPath = "$.users[*]",
            Values = new List<ResolveValue>
            {
                new ResolveValue
                {
                    SourceProperty = "userName",
                    ReferenceProperty = "name"
                }
            }
        }
    }
};
```

### Validation

The command validates:
- Path property is not empty
- ResolveSettings is not null and not empty
- Each ResolveSetting has required properties
- ReferencesCollectionPath is not empty
- ResolveKeys and Values collections are not empty

## Performance Considerations

- **Collection Size**: Performance scales with reference collection size
- **Key Complexity**: Array key resolution requires more processing
- **Memory Usage**: Large reference collections are held in memory
- **Multiple References**: Each reference collection is processed independently

## Error Handling

### Missing References
When reference collections are empty or not found:
```csharp
context.LogWarning("No items found at reference path: $.missingCollection[*]");
```

### Key Mismatches
When keys don't match between collections:
```csharp
// Silently skips unmatched items
// Use logging to track resolution statistics
```

### Invalid Paths
When JSONPath expressions are invalid:
```csharp
context.LogError("Invalid JSONPath expression in resolve command");
```

## Best Practices

1. **Index Reference Data**: Keep reference collections as flat as possible
2. **Use Consistent Keys**: Ensure key types match between collections
3. **Handle Missing Matches**: Design for cases where references don't exist
4. **Monitor Performance**: Profile with representative data sizes
5. **Validate Paths**: Test JSONPath expressions with actual data
6. **Order Operations**: Consider the order of resolution operations
7. **Memory Management**: Be aware of memory usage with large datasets

## Advanced Scenarios

### Nested Subcollections
```json
{
  "path": "$.orders[*].items[*]",
  "command": "resolve",
  "resolveSettings": [
    {
      "resolveKeys": [
        {
          "sourceKey": "productId",
          "referenceKey": "id",
          "asArray": false
        }
      ],
      "referencesCollectionPath": "$.catalog.products[*]",
      "values": [
        {
          "sourceProperty": "productDetails",
          "referenceProperty": "details"
        }
      ]
    }
  ]
}
```

### Conditional Resolution
Combine with `ifElse` for conditional resolution:
```json
[
  {
    "path": "$.records[*]",
    "command": "ifElse",
    "condition": "=exists(@.foreignKey)",
    "then": [
      {
        "path": "@",
        "command": "resolve",
        "resolveSettings": [...]
      }
    ]
  }
]
```

## Related Commands

- [Flatten](flatten.md) - Prepare resolved data for further processing
- [ToCsv](toCsv.md) - Export resolved data to CSV format
- [Merge](../merge.md) - Combine objects after resolution
- [IfElse](../ifElse.md) - Conditional resolution logic