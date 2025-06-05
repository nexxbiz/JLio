using JLio.Commands;
using JLio.Commands.Models;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Functions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JLio.UnitTests.CommandsTests.DecisionTableTesting;

public class DecisionTableAdvancedTests
{
    private JToken data;
    private IExecutionContext executeOptions;

    [SetUp]
    public void Setup()
    {
        executeOptions = ExecutionContext.CreateDefault();
        data = JToken.Parse(@"{
                ""products"": [
                    {
                        ""id"": 1,
                        ""price"": 25.50,
                        ""category"": ""electronics"",
                        ""stock"": 100,
                        ""rating"": 4.2
                    },
                    {
                        ""id"": 2,
                        ""price"": 75.00,
                        ""category"": ""clothing"",
                        ""stock"": 5,
                        ""rating"": 3.8
                    },
                    {
                        ""id"": 3,
                        ""price"": 150.00,
                        ""category"": ""electronics"",
                        ""stock"": 50,
                        ""rating"": 4.8
                    }
                ],
                ""testProduct"": {
                    ""price"": 45.00,
                    ""category"": ""books"",
                    ""stock"": 25,
                    ""rating"": 4.5
                }
            }");
    }

    [Test]
    public void CanValidateExecutionStrategy()
    {
        var command = new DecisionTable
        {
            Path = "$.testProduct",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "price", Path = "@.price", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "result", Path = "@.result" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken> { { "price", ">0" } },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "result", new FunctionSupportedValue(new FixedValue(JValue.CreateString("positive"))) }
                        }
                    }
                },
                ExecutionStrategy = null // Should use defaults
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Should work with default execution strategy
        var product = data.SelectToken("$.testProduct");
        Assert.AreEqual("positive", product.SelectToken("$.result").ToString());
    }


    [Test]
    public void CanExecuteAllMatchesStrategy()
    {
        var command = new DecisionTable
        {
            Path = "$.products[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "price", Path = "@.price", Type = "number" },
                    new DecisionInput { Name = "stock", Path = "@.stock", Type = "number" },
                    new DecisionInput { Name = "rating", Path = "@.rating", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "tags", Path = "@.tags" },
                    new DecisionOutput { Name = "bonusPoints", Path = "@.bonusPoints" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Priority = 1,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "price", ">=50" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "tags", new FunctionSupportedValue(new FixedValue(new JArray("premium"))) },
                            { "bonusPoints", new FunctionSupportedValue(new FixedValue(JToken.FromObject(50))) }
                        }
                    },
                    new DecisionRule
                    {
                        Priority = 2,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "rating", ">=4.0" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "tags", new FunctionSupportedValue(new FixedValue(new JArray("high_rated"))) },
                            { "bonusPoints", new FunctionSupportedValue(new FixedValue(JToken.FromObject(25))) }
                        }
                    },
                    new DecisionRule
                    {
                        Priority = 3,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "stock", "<=10" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "tags", new FunctionSupportedValue(new FixedValue(new JArray("limited"))) },
                            { "bonusPoints", new FunctionSupportedValue(new FixedValue(JToken.FromObject(10))) }
                        }
                    }
                },
                ExecutionStrategy = new ExecutionStrategy
                {
                    Mode = "allMatches",
                    ConflictResolution = "merge",
                    StopOnError = false
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Product 2: price=75, rating=3.8, stock=5 -> should match rules 1 and 3
        var product2 = data.SelectToken("$.products[1]");
        var tags = product2.SelectToken("$.tags") as JArray;
        Assert.IsNotNull(tags);
        Assert.IsTrue(tags.Any(t => t.ToString() == "premium"));
        Assert.IsTrue(tags.Any(t => t.ToString() == "limited"));

        // bonusPoints should be max of matching rules (50 > 10)
        Assert.AreEqual(50, product2.SelectToken("$.bonusPoints").ToObject<decimal>());
    }

    [Test]
    public void CanExecuteBestMatchStrategy()
    {
        var command = new DecisionTable
        {
            Path = "$.testProduct",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "price", Path = "@.price", Type = "number" },
                    new DecisionInput { Name = "category", Path = "@.category", Type = "string" },
                    new DecisionInput { Name = "rating", Path = "@.rating", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "recommendation", Path = "@.recommendation" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Priority = 1,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "price", ">=40" },
                            { "rating", ">=4.0" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "recommendation", new FunctionSupportedValue(new FixedValue(JValue.CreateString("good_value"))) }
                        }
                    },
                    new DecisionRule
                    {
                        Priority = 3,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "category", "books" },
                            { "price", ">=30" },
                            { "rating", ">=4.0" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "recommendation", new FunctionSupportedValue(new FixedValue(JValue.CreateString("excellent_book"))) }
                        }
                    },
                    new DecisionRule
                    {
                        Priority = 5,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "rating", ">=4.0" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "recommendation", new FunctionSupportedValue(new FixedValue(JValue.CreateString("highly_rated"))) }
                        }
                    }
                },
                ExecutionStrategy = new ExecutionStrategy
                {
                    Mode = "bestMatch",
                    ConflictResolution = "priority",
                    StopOnError = false
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Should choose rule 2 (excellent_book) because it has 3 matching conditions (300 + 3 = 303)
        // vs rule 1 with 2 conditions (200 + 5 = 205) vs rule 3 with 1 condition (100 + 1 = 101)
        var product = data.SelectToken("$.testProduct");
        Assert.AreEqual("excellent_book", product.SelectToken("$.recommendation").ToString());
    }

    [Test]
    public void CanHandleLastWinsStrategy()
    {
        var command = new DecisionTable
        {
            Path = "$.products[0]", // First product
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "category", Path = "@.category", Type = "string" },
                    new DecisionInput { Name = "rating", Path = "@.rating", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "status", Path = "@.status" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "category", "electronics" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "status", new FunctionSupportedValue(new FixedValue(JValue.CreateString("tech_product"))) }
                        }
                    },
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "rating", ">=4.0" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "status", new FunctionSupportedValue(new FixedValue(JValue.CreateString("quality_product"))) }
                        }
                    }
                },
                ExecutionStrategy = new ExecutionStrategy
                {
                    Mode = "allMatches",
                    ConflictResolution = "lastWins",
                    StopOnError = false
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Both rules match, but lastWins should give us "quality_product"
        var product = data.SelectToken("$.products[0]");
        Assert.AreEqual("quality_product", product.SelectToken("$.status").ToString());
    }

    [Test]
    public void CanHandleComplexConditions()
    {
        var command = new DecisionTable
        {
            Path = "$.products[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "price", Path = "@.price", Type = "number" },
                    new DecisionInput { Name = "stock", Path = "@.stock", Type = "number" },
                    new DecisionInput { Name = "rating", Path = "@.rating", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "tier", Path = "@.tier" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "price", ">=50 && <=100" }, // Complex AND condition
                            { "rating", ">4.0" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "tier", new FunctionSupportedValue(new FixedValue(JValue.CreateString("sweet_spot"))) }
                        }
                    },
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "stock", "<=10 || >=100" }, // Complex OR condition
                            { "price", "<30" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "tier", new FunctionSupportedValue(new FixedValue(JValue.CreateString("budget_special"))) }
                        }
                    },
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "price", ">100" },
                            { "rating", ">=4.5 && <=5.0" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "tier", new FunctionSupportedValue(new FixedValue(JValue.CreateString("premium"))) }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, IFunctionSupportedValue>
                {
                    { "tier", new FunctionSupportedValue(new FixedValue(JValue.CreateString("standard"))) }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Product 1: price=25.50, stock=100, rating=4.2 -> budget_special (stock>=100 && price<30)
        var product1 = data.SelectToken("$.products[0]");
        Assert.AreEqual("budget_special", product1.SelectToken("$.tier").ToString());

        // Product 2: price=75.00, stock=5, rating=3.8 -> standard (no rules match)
        var product2 = data.SelectToken("$.products[1]");
        Assert.AreEqual("standard", product2.SelectToken("$.tier").ToString());

        // Product 3: price=150.00, stock=50, rating=4.8 -> premium (price>100 && rating>=4.5)
        var product3 = data.SelectToken("$.products[2]");
        Assert.AreEqual("premium", product3.SelectToken("$.tier").ToString());
    }

    [Test]
    public void CanHandleAndOrConditions()
    {
        var command = new DecisionTable
        {
            Path = "$.testProduct",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "price", Path = "@.price", Type = "number" },
                    new DecisionInput { Name = "stock", Path = "@.stock", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "action", Path = "@.action" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "price", ">=40 && <=50" },
                            { "stock", ">=20 && <=30" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "action", new FunctionSupportedValue(new FixedValue(JValue.CreateString("perfect_match"))) }
                        }
                    },
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "price", "<20 || >100" },
                            { "stock", "<=5 || >=100" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "action", new FunctionSupportedValue(new FixedValue(JValue.CreateString("extreme_case"))) }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, IFunctionSupportedValue>
                {
                    { "action", new FunctionSupportedValue(new FixedValue(JValue.CreateString("no_match"))) }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // testProduct: price=45.00, stock=25 -> perfect_match (both AND conditions satisfied)
        var product = data.SelectToken("$.testProduct");
        Assert.AreEqual("perfect_match", product.SelectToken("$.action").ToString());
    }

    [Test]
    public void CanHandleStopOnErrorTrue()
    {
        var command = new DecisionTable
        {
            Path = "$.nonExistentPath", // This will cause an error
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "test", Path = "@.test", Type = "string" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "result", Path = "@.result" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken> { { "test", "value" } },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "result", new FunctionSupportedValue(new FixedValue(JValue.CreateString("success"))) }
                        }
                    }
                },
                ExecutionStrategy = new ExecutionStrategy
                {
                    StopOnError = true
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success); // Command validation passes, but no tokens selected is not an error
    }

    [Test]
    public void CanHandleMergeConflictResolution()
    {
        var command = new DecisionTable
        {
            Path = "$.products[2]", // High-end electronics
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "price", Path = "@.price", Type = "number" },
                    new DecisionInput { Name = "category", Path = "@.category", Type = "string" },
                    new DecisionInput { Name = "rating", Path = "@.rating", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "benefits", Path = "@.benefits" },
                    new DecisionOutput { Name = "score", Path = "@.score" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "price", ">=100" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "benefits", new FunctionSupportedValue(new FixedValue(new JArray("premium_shipping", "extended_warranty"))) },
                            { "score", new FunctionSupportedValue(new FixedValue(JToken.FromObject(75))) }
                        }
                    },
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "category", "electronics" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "benefits", new FunctionSupportedValue(new FixedValue(new JArray("tech_support", "software_bundle"))) },
                            { "score", new FunctionSupportedValue(new FixedValue(JToken.FromObject(85))) }
                        }
                    },
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "rating", ">=4.5" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "benefits", new FunctionSupportedValue(new FixedValue(new JArray("priority_support"))) },
                            { "score", new FunctionSupportedValue(new FixedValue(JToken.FromObject(90))) }
                        }
                    }
                },
                ExecutionStrategy = new ExecutionStrategy
                {
                    Mode = "allMatches",
                    ConflictResolution = "merge",
                    StopOnError = false
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        var product = data.SelectToken("$.products[2]");

        // Benefits should be merged array
        var benefits = product.SelectToken("$.benefits") as JArray;
        Assert.IsNotNull(benefits);
        Assert.IsTrue(benefits.Count >= 5); // All benefits merged
        Assert.IsTrue(benefits.Any(b => b.ToString() == "premium_shipping"));
        Assert.IsTrue(benefits.Any(b => b.ToString() == "tech_support"));
        Assert.IsTrue(benefits.Any(b => b.ToString() == "priority_support"));

        // Score should be maximum (90)
        Assert.AreEqual(90, product.SelectToken("$.score").ToObject<decimal>());
    }

    [Test]
    public void CanHandlePriorityOrdering()
    {
        var command = new DecisionTable
        {
            Path = "$.testProduct",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "category", Path = "@.category", Type = "string" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "handler", Path = "@.handler" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Priority = 10,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "category", "books" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "handler", new FunctionSupportedValue(new FixedValue(JValue.CreateString("general_books"))) }
                        }
                    },
                    new DecisionRule
                    {
                        Priority = 1,
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "category", "books" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "handler", new FunctionSupportedValue(new FixedValue(JValue.CreateString("premium_books"))) }
                        }
                    },
                    new DecisionRule
                    {
                        Priority = 5, // Medium priority
                        Conditions = new Dictionary<string, JToken>
                        {
                            { "category", "books" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "handler", new FunctionSupportedValue(new FixedValue(JValue.CreateString("standard_books"))) }
                        }
                    }
                },
                ExecutionStrategy = new ExecutionStrategy
                {
                    Mode = "firstMatch",
                    ConflictResolution = "priority",
                    StopOnError = false
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Should choose highest priority rule (priority = 1, lowest number = highest priority)
        var product = data.SelectToken("$.testProduct");
        Assert.AreEqual("premium_books", product.SelectToken("$.handler").ToString());
    }

    [Test]
    public void CanHandleComplexAndOrCombinations()
    {
        var command = new DecisionTable
        {
            Path = "$.products[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
                {
                    new DecisionInput { Name = "price", Path = "@.price", Type = "number" },
                    new DecisionInput { Name = "stock", Path = "@.stock", Type = "number" },
                    new DecisionInput { Name = "rating", Path = "@.rating", Type = "number" }
                },
                Outputs = new List<DecisionOutput>
                {
                    new DecisionOutput { Name = "status", Path = "@.status" }
                },
                Rules = new List<DecisionRule>
                {
                    new DecisionRule
                    {
                        Conditions = new Dictionary<string, JToken>
                        {
                            // (price >= 20 AND price <= 30) OR (price >= 70 AND price <= 80)
                            { "price", ">=20 && <=30 || >=70 && <=80" },
                            { "rating", ">3.5" }
                        },
                        Results = new Dictionary<string, IFunctionSupportedValue>
                        {
                            { "status", new FunctionSupportedValue(new FixedValue(JValue.CreateString("target_range"))) }
                        }
                    }
                },
                DefaultResults = new Dictionary<string, IFunctionSupportedValue>
                {
                    { "status", new FunctionSupportedValue(new FixedValue(JValue.CreateString("outside_range"))) }
                }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Product 1: price=25.50, rating=4.2 -> target_range (in 20-30 range)
        var product1 = data.SelectToken("$.products[0]");
        Assert.AreEqual("target_range", product1.SelectToken("$.status").ToString());

        // Product 2: price=75.00, rating=3.8 -> target_range (in 70-80 range)
        var product2 = data.SelectToken("$.products[1]");
        Assert.AreEqual("target_range", product2.SelectToken("$.status").ToString());

        // Product 3: price=150.00, rating=4.8 -> outside_range (not in either range)
        var product3 = data.SelectToken("$.products[2]");
        Assert.AreEqual("outside_range", product3.SelectToken("$.status").ToString());
    }

    [Test]
    public void CanExecuteFirstMatchStrategy()
    {
        var command = new DecisionTable
        {
            Path = "$.products[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
            {
                new DecisionInput { Name = "price", Path = "@.price", Type = "number" },
                new DecisionInput { Name = "category", Path = "@.category", Type = "string" }
            },
                Outputs = new List<DecisionOutput>
            {
                new DecisionOutput { Name = "discount", Path = "@.discount" },
                new DecisionOutput { Name = "promotion", Path = "@.promotion" }
            },
                Rules = new List<DecisionRule>
            {
                new DecisionRule
                {
                    Priority = 1,
                    Conditions = new Dictionary<string, JToken>
                    {
                        { "category", "electronics" },
                        { "price", ">=100" }
                    },
                    Results = new Dictionary<string, IFunctionSupportedValue>
                    {
                        { "discount", new FunctionSupportedValue(new FixedValue(JToken.FromObject(0.15))) },
                        { "promotion", new FunctionSupportedValue(new FixedValue(JValue.CreateString("premium_electronics"))) }
                    }
                },
                new DecisionRule
                {
                    Priority = 2,
                    Conditions = new Dictionary<string, JToken>
                    {
                        { "category", "electronics" }
                    },
                    Results = new Dictionary<string, IFunctionSupportedValue>
                    {
                        { "discount", new FunctionSupportedValue(new FixedValue(JToken.FromObject(0.05))) },
                        { "promotion", new FunctionSupportedValue(new FixedValue(JValue.CreateString("basic_electronics"))) }
                    }
                }
            },
                ExecutionStrategy = new ExecutionStrategy
                {
                    Mode = "firstMatch",
                    ConflictResolution = "priority",
                    StopOnError = false
                },
                DefaultResults = new Dictionary<string, IFunctionSupportedValue>
            {
                { "discount", new FunctionSupportedValue(new FixedValue(JToken.FromObject(0.0))) },
                { "promotion", new FunctionSupportedValue(new FixedValue(JValue.CreateString("none"))) }
            }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Product 1: electronics, price=25.50 -> should get basic_electronics (first rule doesn't match price)
        var product1 = data.SelectToken("$.products[0]");
        Assert.AreEqual(0.05, product1.SelectToken("$.discount").ToObject<decimal>());
        Assert.AreEqual("basic_electronics", product1.SelectToken("$.promotion").ToString());

        // Product 3: electronics, price=150.00 -> should get premium_electronics (first rule matches)
        var product3 = data.SelectToken("$.products[2]");
        Assert.AreEqual(0.15, product3.SelectToken("$.discount").ToObject<decimal>());
        Assert.AreEqual("premium_electronics", product3.SelectToken("$.promotion").ToString());
    }

    [Test]
    public void CanHandleNotEqualConditions()
    {
        var command = new DecisionTable
        {
            Path = "$.products[*]",
            DecisionTableConfig = new DecisionTableConfig
            {
                Inputs = new List<DecisionInput>
            {
                new DecisionInput { Name = "category", Path = "@.category", Type = "string" },
                new DecisionInput { Name = "price", Path = "@.price", Type = "number" }
            },
                Outputs = new List<DecisionOutput>
            {
                new DecisionOutput { Name = "eligible", Path = "@.eligible" }
            },
                Rules = new List<DecisionRule>
            {
                new DecisionRule
                {
                    Conditions = new Dictionary<string, JToken>
                    {
                        { "category", "!=clothing" },
                        { "price", "!=75.00" }
                    },
                    Results = new Dictionary<string, IFunctionSupportedValue>
                    {
                        { "eligible", new FunctionSupportedValue(new FixedValue(JToken.FromObject(true))) }
                    }
                }
            },
                DefaultResults = new Dictionary<string, IFunctionSupportedValue>
            {
                { "eligible", new FunctionSupportedValue(new FixedValue(JToken.FromObject(false))) }
            }
            }
        };

        var result = command.Execute(data, executeOptions);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Success);

        // Product 1: electronics, 25.50 -> eligible (not clothing, not 75.00)
        var product1 = data.SelectToken("$.products[0]");
        Assert.AreEqual(true, product1.SelectToken("$.eligible").ToObject<bool>());

        // Product 2: clothing, 75.00 -> not eligible (is clothing AND is 75.00)
        var product2 = data.SelectToken("$.products[1]");
        Assert.AreEqual(false, product2.SelectToken("$.eligible").ToObject<bool>());

        // Product 3: electronics, 150.00 -> eligible (not clothing, not 75.00)
        var product3 = data.SelectToken("$.products[2]");
        Assert.AreEqual(true, product3.SelectToken("$.eligible").ToObject<bool>());
    }
}