# JLio Advanced Scenarios and Real-World Applications

## Overview

This document demonstrates advanced JLio capabilities through complex, real-world scenarios that showcase the full power of combining commands, functions, and decision logic using JSON script format.

---

## Scenario 1: E-commerce Order Processing Pipeline

### Business Requirements
- Process incoming orders with dynamic pricing rules
- Apply customer tier-based discounts
- Calculate shipping costs and taxes
- Generate order confirmations with audit trails

### JSON Script Implementation

```json
[
  {
    "path": "$.audit.originalOrder",
    "value": "=fetch($)",
    "command": "copy"
  },
  {
    "path": "$.audit.receivedAt",
    "value": "=datetime('UTC')",
    "command": "add"
  },
  {
    "path": "$.audit.processingId",
    "value": "=newGuid()",
    "command": "add"
  },
  {
    "command": "decisionTable",
    "path": "$.customer",
    "decisionTable": {
      "inputs": [
        {"name": "tierLevel", "path": "@.tier"},
        {"name": "yearsActive", "path": "@.yearsActive"},
        {"name": "totalOrders", "path": "@.totalOrders"}
      ],
      "outputs": [
        {"name": "discountRate", "path": "@.discountRate"},
        {"name": "priorityLevel", "path": "@.priorityLevel"},
        {"name": "freeShippingEligible", "path": "@.freeShippingEligible"}
      ],
      "rules": [
        {
          "priority": 1,
          "conditions": {"tierLevel": "platinum", "yearsActive": ">=3"},
          "results": {"discountRate": "0.20", "priorityLevel": "high", "freeShippingEligible": "true"}
        },
        {
          "priority": 2,
          "conditions": {"tierLevel": "gold", "totalOrders": ">50"},
          "results": {"discountRate": "0.15", "priorityLevel": "medium", "freeShippingEligible": "true"}
        }
      ],
      "defaultResults": {"discountRate": "0.05", "priorityLevel": "normal", "freeShippingEligible": "false"}
    }
  },
  {
    "path": "$.calculations.subtotal",
    "value": "=sum($.items[*].price)",
    "command": "add"
  },
  {
    "path": "$.calculations.itemCount",
    "value": "=count($.items)",
    "command": "add"
  },
  {
    "command": "decisionTable",
    "path": "$.shipping.address",
    "decisionTable": {
      "inputs": [
        {"name": "state", "path": "@.state"},
        {"name": "country", "path": "@.country"}
      ],
      "outputs": [
        {"name": "taxRate", "path": "@.taxRate"},
        {"name": "shippingCost", "path": "@.shippingCost"}
      ],
      "rules": [
        {
          "conditions": {"state": "CA", "country": "US"},
          "results": {"taxRate": "0.0875", "shippingCost": "15.00"}
        },
        {
          "conditions": {"country": "US"},
          "results": {"taxRate": "0.06", "shippingCost": "12.00"}
        }
      ],
      "defaultResults": {"taxRate": "0.00", "shippingCost": "25.00"}
    }
  },
  {
    "first": "=fetch($.customer.freeShippingEligible)",
    "second": true,
    "ifScript": [
      {"path": "$.calculations.finalShipping", "value": 0, "command": "add"}
    ],
    "elseScript": [
      {"path": "$.calculations.finalShipping", "value": "=fetch($.shipping.address.shippingCost)", "command": "add"}
    ],
    "command": "ifElse"
  },
  {
    "path": "$.calculations.taxAmount",
    "value": "=sum($.calculations.subtotal) * =fetch($.shipping.address.taxRate)",
    "command": "add"
  },
  {
    "path": "$.calculations.discountAmount",
    "value": "=sum($.calculations.subtotal) * =fetch($.customer.discountRate)",
    "command": "add"
  },
  {
    "path": "$.calculations.finalTotal",
    "value": "=sum($.calculations.subtotal, $.calculations.taxAmount, $.calculations.finalShipping) - =sum($.calculations.discountAmount)",
    "command": "add"
  },
  {
    "path": "$.confirmation.message",
    "value": "=concat('Order confirmed for ', fetch($.customer.name), ' - Total: $', toString($.calculations.finalTotal), ' (Saved: $', toString($.calculations.discountAmount), ')')",
    "command": "add"
  },
  {
    "path": "$.confirmation.orderNumber",
    "value": "=concat('ORD-', datetime('yyyyMMdd'), '-', newGuid())",
    "command": "add"
  }
]
```

### Sample Input Data
```json
{
  "customer": {
    "name": "Alice Johnson",
    "tier": "gold",
    "yearsActive": 4,
    "totalOrders": 75
  },
  "items": [
    {"name": "Gaming Laptop", "price": 1299.99},
    {"name": "Wireless Mouse", "price": 79.99},
    {"name": "Mechanical Keyboard", "price": 149.99}
  ],
  "shipping": {
    "address": {
      "state": "CA",
      "country": "US"
    }
  }
}
```

---

## Scenario 2: Customer Data Integration and Analytics

### Business Requirements
- Merge customer data from multiple sources
- Calculate comprehensive customer health scores
- Generate analytics dashboard metrics
- Identify at-risk customers

### JSON Script Implementation

```json
[
  {
    "path": "$.integration.startTime",
    "value": "=datetime('UTC')",
    "command": "add"
  },
  {
    "path": "$.integration.jobId",
    "value": "=newGuid()",
    "command": "add"
  },
  {
    "fromPath": "$.sources.crm.customers[*]",
    "toPath": "$.enrichedCustomers[*]",
    "command": "merge",
    "settings": {
      "arraySettings": [
        {
          "arrayPath": "$.enrichedCustomers",
          "keyPaths": ["customerId", "email"]
        }
      ]
    }
  },
  {
    "fromPath": "$.sources.support.tickets[*]",
    "toPath": "$.enrichedCustomers[*]",
    "command": "merge",
    "settings": {
      "matchSettings": {
        "keyPaths": ["customerId"]
      }
    }
  },
  {
    "path": "$.enrichedCustomers[*].metrics.totalRevenue",
    "value": "=sum(@.transactions[*].amount)",
    "command": "add"
  },
  {
    "path": "$.enrichedCustomers[*].metrics.transactionCount",
    "value": "=count(@.transactions)",
    "command": "add"
  },
  {
    "path": "$.enrichedCustomers[*].metrics.averageTransaction",
    "value": "=avg(@.transactions[*].amount)",
    "command": "add"
  },
  {
    "path": "$.enrichedCustomers[*].metrics.openTickets",
    "value": "=count(@.supportTickets[?(@.status == 'open')])",
    "command": "add"
  },
  {
    "command": "decisionTable",
    "path": "$.enrichedCustomers[*]",
    "decisionTable": {
      "inputs": [
        {"name": "revenue", "path": "@.metrics.totalRevenue"},
        {"name": "tickets", "path": "@.metrics.openTickets"},
        {"name": "transactions", "path": "@.metrics.transactionCount"}
      ],
      "outputs": [
        {"name": "healthScore", "path": "@.metrics.healthScore"},
        {"name": "riskLevel", "path": "@.metrics.riskLevel"},
        {"name": "segment", "path": "@.metrics.segment"}
      ],
      "rules": [
        {
          "priority": 1,
          "conditions": {"revenue": ">10000", "tickets": "<=2", "transactions": ">20"},
          "results": {"healthScore": "95", "riskLevel": "low", "segment": "champion"}
        },
        {
          "priority": 2,
          "conditions": {"revenue": ">5000", "tickets": "<=5"},
          "results": {"healthScore": "80", "riskLevel": "medium", "segment": "loyal"}
        }
      ],
      "defaultResults": {"healthScore": "60", "riskLevel": "high", "segment": "at_risk"}
    }
  },
  {
    "path": "$.analytics.totalCustomers",
    "value": "=count($.enrichedCustomers)",
    "command": "add"
  },
  {
    "path": "$.analytics.totalRevenue",
    "value": "=sum($.enrichedCustomers[*].metrics.totalRevenue)",
    "command": "add"
  },
  {
    "path": "$.analytics.averageHealthScore",
    "value": "=avg($.enrichedCustomers[*].metrics.healthScore)",
    "command": "add"
  },
  {
    "path": "$.analytics.highRiskCustomers",
    "value": "=count($.enrichedCustomers[?(@.metrics.riskLevel == 'high')])",
    "command": "add"
  },
  {
    "path": "$.analytics.championCustomers",
    "value": "=count($.enrichedCustomers[?(@.metrics.segment == 'champion')])",
    "command": "add"
  },
  {
    "path": "$.executiveSummary",
    "value": "=concat('Customer Analytics: ', toString($.analytics.totalCustomers), ' customers, ', toString($.analytics.highRiskCustomers), ' high risk, Avg Health Score: ', toString($.analytics.averageHealthScore))",
    "command": "add"
  }
]
```

---

## Scenario 3: Financial Risk Assessment

### Business Requirements
- Assess credit risk for loan applications
- Apply KYC and AML compliance rules
- Calculate risk-adjusted pricing
- Generate regulatory reports

### JSON Script Implementation

```json
[
  {
    "path": "$.assessment.startTime",
    "value": "=datetime('UTC')",
    "command": "add"
  },
  {
    "path": "$.assessment.referenceId",
    "value": "=newGuid()",
    "command": "add"
  },
  {
    "path": "$.calculations.totalAssets",
    "value": "=sum($.application.financials.assets[*].value)",
    "command": "add"
  },
  {
    "path": "$.calculations.totalLiabilities",
    "value": "=sum($.application.financials.liabilities[*].amount)",
    "command": "add"
  },
  {
    "path": "$.calculations.netWorth",
    "value": "=sum($.calculations.totalAssets) - =sum($.calculations.totalLiabilities)",
    "command": "add"
  },
  {
    "path": "$.calculations.monthlyIncome",
    "value": "=avg($.application.financials.monthlyIncome[*])",
    "command": "add"
  },
  {
    "command": "decisionTable",
    "path": "$.application.customer",
    "decisionTable": {
      "inputs": [
        {"name": "age", "path": "@.age"},
        {"name": "hasValidId", "path": "@.identification.isValid"},
        {"name": "addressVerified", "path": "@.address.verified"}
      ],
      "outputs": [
        {"name": "kycStatus", "path": "@.compliance.kycStatus"},
        {"name": "kycRiskLevel", "path": "@.compliance.kycRiskLevel"}
      ],
      "rules": [
        {
          "conditions": {"age": ">=18", "hasValidId": "true", "addressVerified": "true"},
          "results": {"kycStatus": "approved", "kycRiskLevel": "low"}
        },
        {
          "conditions": {"age": ">=18", "hasValidId": "true"},
          "results": {"kycStatus": "pending", "kycRiskLevel": "medium"}
        }
      ],
      "defaultResults": {"kycStatus": "rejected", "kycRiskLevel": "high"}
    }
  },
  {
    "command": "decisionTable",
    "path": "$",
    "decisionTable": {
      "inputs": [
        {"name": "creditScore", "path": "@.application.credit.score"},
        {"name": "netWorth", "path": "@.calculations.netWorth"},
        {"name": "monthlyIncome", "path": "@.calculations.monthlyIncome"}
      ],
      "outputs": [
        {"name": "creditRiskGrade", "path": "@.riskAssessment.grade"},
        {"name": "approvalStatus", "path": "@.riskAssessment.status"},
        {"name": "interestRate", "path": "@.riskAssessment.rate"}
      ],
      "rules": [
        {
          "priority": 1,
          "conditions": {"creditScore": ">=750", "netWorth": ">100000"},
          "results": {"creditRiskGrade": "A", "approvalStatus": "approved", "interestRate": "3.5"}
        },
        {
          "priority": 2,
          "conditions": {"creditScore": ">=650", "monthlyIncome": ">5000"},
          "results": {"creditRiskGrade": "B", "approvalStatus": "approved", "interestRate": "5.5"}
        }
      ],
      "defaultResults": {"creditRiskGrade": "C", "approvalStatus": "rejected", "interestRate": "0"}
    }
  },
  {
    "first": "=fetch($.riskAssessment.status)",
    "second": "approved",
    "ifScript": [
      {
        "path": "$.calculations.monthlyPayment",
        "value": "=sum($.application.loanAmount) / sum($.application.loanTermMonths)",
        "command": "add"
      }
    ],
    "elseScript": [
      {
        "path": "$.calculations.monthlyPayment",
        "value": 0,
        "command": "add"
      }
    ],
    "command": "ifElse"
  },
  {
    "path": "$.executiveDecision",
    "value": "=concat('Risk Assessment: ', fetch($.riskAssessment.status), ' | Grade: ', fetch($.riskAssessment.grade), ' | Rate: ', toString($.riskAssessment.rate), '% | KYC: ', fetch($.application.customer.compliance.kycStatus))",
    "command": "add"
  }
]
```

---

## Scenario 4: Inventory Management and Forecasting

### Business Requirements
- Analyze current inventory levels
- Forecast demand based on historical data
- Calculate optimal reorder points
- Identify low stock and out-of-stock items

### JSON Script Implementation

```json
[
  {
    "path": "$.analysis.timestamp",
    "value": "=datetime('UTC')",
    "command": "add"
  },
  {
    "path": "$.metrics.totalInventoryValue",
    "value": "=sum($.inventory.products[*].value)",
    "command": "add"
  },
  {
    "path": "$.metrics.totalUnits",
    "value": "=sum($.inventory.products[*].quantity)",
    "command": "add"
  },
  {
    "path": "$.metrics.lowStockItems",
    "value": "=count($.inventory.products[?(@.quantity <= @.reorderPoint)])",
    "command": "add"
  },
  {
    "path": "$.metrics.outOfStockItems",
    "value": "=count($.inventory.products[?(@.quantity == 0)])",
    "command": "add"
  },
  {
    "path": "$.forecasting.averageDailySales",
    "value": "=avg($.historicalData.sales[*].dailyVolume)",
    "command": "add"
  },
  {
    "path": "$.inventory.products[*].forecast.averageDemand",
    "value": "=avg(@.salesHistory[*].quantity)",
    "command": "add"
  },
  {
    "command": "decisionTable",
    "path": "$.inventory.products[*]",
    "decisionTable": {
      "inputs": [
        {"name": "category", "path": "@.category"},
        {"name": "seasonality", "path": "@.seasonality"},
        {"name": "averageDemand", "path": "@.forecast.averageDemand"}
      ],
      "outputs": [
        {"name": "forecastedDemand", "path": "@.forecast.predicted"},
        {"name": "confidenceLevel", "path": "@.forecast.confidence"}
      ],
      "rules": [
        {
          "conditions": {"category": "seasonal", "seasonality": "high"},
          "results": {"forecastedDemand": "=sum(@.forecast.averageDemand) * 1.3", "confidenceLevel": "medium"}
        },
        {
          "conditions": {"category": "staple"},
          "results": {"forecastedDemand": "=fetch(@.forecast.averageDemand)", "confidenceLevel": "high"}
        }
      ],
      "defaultResults": {"forecastedDemand": "=sum(@.forecast.averageDemand) * 1.1", "confidenceLevel": "low"}
    }
  },
  {
    "path": "$.inventory.products[*].optimization.recommendedOrder",
    "value": "=sum(@.forecast.predicted) - =sum(@.quantity)",
    "command": "add"
  },
  {
    "path": "$.procurement.totalRecommendedValue",
    "value": "=sum($.inventory.products[?(@.optimization.recommendedOrder > 0)].optimization.recommendedOrder)",
    "command": "add"
  },
  {
    "path": "$.procurement.itemsToOrder",
    "value": "=count($.inventory.products[?(@.optimization.recommendedOrder > 0)])",
    "command": "add"
  },
  {
    "first": "=count($.inventory.products[?(@.quantity <= @.reorderPoint)])",
    "second": 0,
    "ifScript": [
      {"path": "$.alerts.urgentReorders", "value": "No urgent reorders needed", "command": "add"}
    ],
    "elseScript": [
      {
        "path": "$.alerts.urgentReorders", 
        "value": "=concat('URGENT: ', toString(count($.inventory.products[?(@.quantity <= @.reorderPoint)])), ' products need immediate reordering')",
        "command": "add"
      }
    ],
    "command": "ifElse"
  },
  {
    "path": "$.dashboard.summary",
    "value": "=concat('Inventory Analysis - Total Value: $', toString($.metrics.totalInventoryValue), ' | Low Stock: ', toString($.metrics.lowStockItems), ' items | Items to Order: ', toString($.procurement.itemsToOrder))",
    "command": "add"
  }
]
```

---

## Key Patterns and Best Practices

### 1. **Modular Script Design**
Break complex workflows into logical steps:
- Data preparation and validation
- Business rule application
- Calculations and aggregations
- Decision making and routing
- Report generation and summaries

### 2. **Decision Table Strategies**
- **Priority-based rules** for complex business logic
- **Default results** to handle edge cases
- **Multi-input conditions** for sophisticated decision making
- **Cascading decisions** where outputs become inputs for subsequent rules

### 3. **Mathematical Operations**
- Use **sum()** for totals and accumulations
- Use **avg()** for performance metrics and benchmarks
- Use **count()** for inventory and capacity planning
- Combine functions for complex calculations: `"=sum($.a) - =sum($.b)"`

### 4. **Conditional Logic**
- **IfElse commands** for binary decisions
- **Decision tables** for complex multi-variable logic
- **Nested conditions** for sophisticated business rules

### 5. **Data Integration**
- **Merge commands** with key-based matching
- **Copy commands** for audit trails
- **Compare commands** for change detection

### 6. **Audit and Traceability**
- Always include timestamps with `=datetime('UTC')`
- Generate unique identifiers with `=newGuid()`
- Create audit trails by copying original data
- Build executive summaries for stakeholder communication

These patterns demonstrate JLio's power in handling enterprise-grade data transformation scenarios through declarative JSON scripts that business users can understand and technical teams can maintain.