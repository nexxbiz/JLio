using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Extensions.ETL.Commands.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.ETL.Commands
{
    public class ToCsv : CommandBase
    {
        [JsonProperty("path")]
        public string Path { get; set; } = "$";

        [JsonProperty("csvSettings")]
        public CsvSettings CsvSettings { get; set; } = new();

        public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
        {
            try
            {
                var validationResult = ValidateCommandInstance();
                if (!validationResult.IsValid)
                {
                    validationResult.ValidationMessages.ForEach(msg =>
                        context.LogWarning(CoreConstants.CommandExecution, msg));
                    return new JLioExecutionResult(false, dataContext);
                }

                var targetTokens = context.ItemsFetcher.SelectTokens(Path, dataContext);
                
                foreach (var token in targetTokens)
                {
                    var csvData = ConvertToCsv(token, context);
                    
                    // Replace the original token with CSV data
                    ReplaceTokenWithCsvData(token, csvData, dataContext);
                }

                return new JLioExecutionResult(true, dataContext);
            }
            catch (Exception ex)
            {
                context.LogError(CoreConstants.CommandExecution, $"Error in ToCsv command: {ex.Message}");
                return new JLioExecutionResult(false, dataContext);
            }
        }

        private string ConvertToCsv(JToken token, IExecutionContext context)
        {
            var csvBuilder = new StringBuilder();
            
            if (token is JObject obj)
            {
                // Convert single object to CSV with one data row
                var flatData = ExtractFlatData(obj);
                return ConvertFlatDataToCsv(new List<Dictionary<string, object>> { flatData });
            }
            else if (token is JArray array)
            {
                // Convert array of objects to CSV with multiple data rows
                var allFlatData = new List<Dictionary<string, object>>();
                
                foreach (var item in array)
                {
                    if (item is JObject itemObj)
                    {
                        allFlatData.Add(ExtractFlatData(itemObj));
                    }
                }
                
                return ConvertFlatDataToCsv(allFlatData);
            }
            else
            {
                context.LogWarning(CoreConstants.CommandExecution, 
                    "ToCsv command can only be applied to objects or arrays of objects");
                return "";
            }
        }

        private Dictionary<string, object> ExtractFlatData(JObject obj)
        {
            var flatData = new Dictionary<string, object>();
            
            foreach (var property in obj.Properties())
            {
                // Skip metadata unless specifically requested
                if (!CsvSettings.IncludeMetadata && 
                    (property.Name.StartsWith("_") || property.Name.Contains("Metadata")))
                {
                    continue;
                }
                
                // Skip type columns unless specifically requested
                if (!CsvSettings.IncludeTypeColumns && 
                    property.Name.EndsWith(CsvSettings.TypeColumnSuffix))
                {
                    continue;
                }
                
                var value = ExtractValue(property.Value);
                flatData[property.Name] = value;
            }
            
            return flatData;
        }

        private object ExtractValue(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return CsvSettings.NullValueRepresentation;
                    
                case JTokenType.Boolean:
                    var boolValue = token.Value<bool>();
                    var boolFormats = CsvSettings.BooleanFormat.Split(',');
                    return boolValue ? boolFormats[0] : (boolFormats.Length > 1 ? boolFormats[1] : boolFormats[0]);
                    
                case JTokenType.Integer:
                    return token.Value<long>();
                    
                case JTokenType.Float:
                    return token.Value<double>();
                    
                case JTokenType.String:
                    return token.Value<string>();
                    
                case JTokenType.Date:
                    return token.Value<DateTime>().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                    
                case JTokenType.Object:
                case JTokenType.Array:
                    return token.ToString(Formatting.None);
                    
                default:
                    return token.ToString();
            }
        }

        private string ConvertFlatDataToCsv(List<Dictionary<string, object>> allFlatData)
        {
            if (allFlatData.Count == 0)
            {
                return "";
            }
            
            var csvBuilder = new StringBuilder();
            
            // Get all unique column names from all rows
            var allColumns = new HashSet<string>();
            foreach (var flatData in allFlatData)
            {
                foreach (var key in flatData.Keys)
                {
                    allColumns.Add(key);
                }
            }
            
            var columnList = allColumns.OrderBy(c => c).ToList();
            
            // Write headers if requested
            if (CsvSettings.IncludeHeaders)
            {
                var headerRow = string.Join(CsvSettings.Delimiter, 
                    columnList.Select(EscapeCsvField));
                csvBuilder.AppendLine(headerRow);
            }
            
            // Write data rows
            foreach (var flatData in allFlatData)
            {
                var values = new List<string>();
                foreach (var column in columnList)
                {
                    var value = flatData.ContainsKey(column) ? flatData[column] : null;
                    values.Add(EscapeCsvField(FormatValue(value)));
                }
                
                var dataRow = string.Join(CsvSettings.Delimiter, values);
                csvBuilder.AppendLine(dataRow);
            }
            
            return csvBuilder.ToString().TrimEnd('\r', '\n');
        }

        private string FormatValue(object value)
        {
            if (value == null)
            {
                return CsvSettings.NullValueRepresentation;
            }
            
            // Handle numeric values to avoid locale-specific formatting
            if (value is double doubleValue)
            {
                return doubleValue.ToString(CultureInfo.InvariantCulture);
            }
            if (value is float floatValue)
            {
                return floatValue.ToString(CultureInfo.InvariantCulture);
            }
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString(CultureInfo.InvariantCulture);
            }
            
            return value.ToString() ?? CsvSettings.NullValueRepresentation;
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return CsvSettings.QuoteAllFields ? $"\"{field}\"" : field;
            }
            
            // Check if field needs to be quoted
            bool needsQuoting = CsvSettings.QuoteAllFields ||
                               field.Contains(CsvSettings.Delimiter) ||
                               field.Contains(CsvSettings.EscapeQuoteChar) ||
                               field.Contains('\r') ||
                               field.Contains('\n') ||
                               field.StartsWith(" ") ||
                               field.EndsWith(" ");
            
            if (needsQuoting)
            {
                // Escape existing quote characters by doubling them
                var escapedField = field.Replace(CsvSettings.EscapeQuoteChar, 
                    CsvSettings.EscapeQuoteChar + CsvSettings.EscapeQuoteChar);
                return $"{CsvSettings.EscapeQuoteChar}{escapedField}{CsvSettings.EscapeQuoteChar}";
            }
            
            return field;
        }

        private void ReplaceTokenWithCsvData(JToken originalToken, string csvData, JToken dataContext)
        {
            // Create a new JValue containing the CSV data
            var csvToken = new JValue(csvData);
            
            if (originalToken.Parent is JProperty prop)
            {
                prop.Value = csvToken;
            }
            else if (originalToken.Parent is JArray arr)
            {
                var index = arr.IndexOf(originalToken);
                arr[index] = csvToken;
            }
            else if (originalToken == dataContext)
            {
                dataContext.Replace(csvToken);
            }
        }

        public override ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrWhiteSpace(Path))
                result.ValidationMessages.Add("Path property is required for toCsv command");

            if (CsvSettings == null)
                result.ValidationMessages.Add("CsvSettings property is required for toCsv command");
            else
            {
                if (string.IsNullOrEmpty(CsvSettings.Delimiter))
                    result.ValidationMessages.Add("Delimiter cannot be empty in CsvSettings");
                    
                if (string.IsNullOrEmpty(CsvSettings.EscapeQuoteChar))
                    result.ValidationMessages.Add("EscapeQuoteChar cannot be empty in CsvSettings");
                    
                if (string.IsNullOrEmpty(CsvSettings.BooleanFormat) || !CsvSettings.BooleanFormat.Contains(","))
                    result.ValidationMessages.Add("BooleanFormat must contain comma-separated true,false values");
            }

            return result;
        }
    }
}