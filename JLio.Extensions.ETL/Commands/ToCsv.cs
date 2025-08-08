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

        // Cache reusable objects
        private readonly StringBuilder _csvBuilder = new StringBuilder(8192); // Pre-allocate with reasonable size
        private readonly StringBuilder _escapingBuffer = new StringBuilder(512);

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
            if (token is JObject obj)
            {
                // Convert single object to CSV with one data row
                var flatData = ExtractFlatData(obj);
                return ConvertFlatDataToCsv(new[] { flatData }); // Use array instead of List
            }
            else if (token is JArray array)
            {
                // Convert array of objects to CSV with multiple data rows
                var allFlatData = new Dictionary<string, object>[array.Count]; // Pre-allocated array
                var count = 0;
                
                foreach (var item in array)
                {
                    if (item is JObject itemObj)
                    {
                        allFlatData[count++] = ExtractFlatData(itemObj);
                    }
                }
                
                // Trim array if needed
                if (count < array.Count)
                {
                    var trimmed = new Dictionary<string, object>[count];
                    Array.Copy(allFlatData, trimmed, count);
                    allFlatData = trimmed;
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
            var flatData = new Dictionary<string, object>(obj.Count); // Pre-size dictionary
            
            foreach (var property in obj.Properties())
            {
                // Skip metadata unless specifically requested - optimized checks
                var propertyName = property.Name;
                if (!CsvSettings.IncludeMetadata && 
                    (propertyName[0] == '_' || propertyName.Contains("Metadata")))
                {
                    continue;
                }
                
                // Skip type columns unless specifically requested - optimized check
                if (!CsvSettings.IncludeTypeColumns && 
                    propertyName.EndsWith(CsvSettings.TypeColumnSuffix))
                {
                    continue;
                }
                
                var value = ExtractValue(property.Value);
                flatData[propertyName] = value;
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
                    var boolFormats = CsvSettings.BooleanFormat.AsSpan();
                    var commaIndex = boolFormats.IndexOf(',');
                    if (commaIndex >= 0)
                    {
                        return boolValue ? 
                            boolFormats.Slice(0, commaIndex).ToString() : 
                            boolFormats.Slice(commaIndex + 1).ToString();
                    }
                    return boolFormats.ToString();
                    
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

        private string ConvertFlatDataToCsv(IEnumerable<Dictionary<string, object>> allFlatData)
        {
            var flatDataArray = allFlatData as Dictionary<string, object>[] ?? allFlatData.ToArray();
            if (flatDataArray.Length == 0)
            {
                return "";
            }
            
            // Clear and reuse StringBuilder
            _csvBuilder.Clear();
            
            // Get all unique column names efficiently
            var allColumns = new HashSet<string>();
            foreach (var flatData in flatDataArray)
            {
                // Use Keys property directly instead of iterating
                foreach (var key in flatData.Keys)
                {
                    allColumns.Add(key);
                }
            }
            
            // Convert to sorted array for consistent output
            var columnArray = new string[allColumns.Count];
            allColumns.CopyTo(columnArray);
            Array.Sort(columnArray, StringComparer.Ordinal);
            
            // Write headers if requested
            if (CsvSettings.IncludeHeaders)
            {
                WriteHeaderRow(columnArray);
            }
            
            // Write data rows
            WriteDataRows(flatDataArray, columnArray);
            
            // Return and trim line endings
            var result = _csvBuilder.ToString();
            return result.TrimEnd('\r', '\n');
        }

        private void WriteHeaderRow(string[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                if (i > 0)
                {
                    _csvBuilder.Append(CsvSettings.Delimiter);
                }
                _csvBuilder.Append(EscapeCsvField(columns[i]));
            }
            _csvBuilder.AppendLine();
        }

        private void WriteDataRows(Dictionary<string, object>[] flatDataArray, string[] columns)
        {
            foreach (var flatData in flatDataArray)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    if (i > 0)
                    {
                        _csvBuilder.Append(CsvSettings.Delimiter);
                    }
                    
                    var value = flatData.TryGetValue(columns[i], out var val) ? val : null;
                    _csvBuilder.Append(EscapeCsvField(FormatValue(value)));
                }
                _csvBuilder.AppendLine();
            }
        }

        private string FormatValue(object value)
        {
            if (value == null)
            {
                return CsvSettings.NullValueRepresentation;
            }
            
            // Handle numeric values to avoid locale-specific formatting
            switch (value)
            {
                case double doubleValue:
                    return doubleValue.ToString(CultureInfo.InvariantCulture);
                case float floatValue:
                    return floatValue.ToString(CultureInfo.InvariantCulture);
                case decimal decimalValue:
                    return decimalValue.ToString(CultureInfo.InvariantCulture);
                default:
                    return value.ToString() ?? CsvSettings.NullValueRepresentation;
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return CsvSettings.QuoteAllFields ? $"\"{field}\"" : field;
            }
            
            // Optimized quoting check using spans and avoiding multiple string operations
            var delimiter = CsvSettings.Delimiter;
            var escapeChar = CsvSettings.EscapeQuoteChar;
            
            bool needsQuoting = CsvSettings.QuoteAllFields ||
                               field.Contains(delimiter) ||
                               field.Contains(escapeChar) ||
                               field.IndexOfAny(new[] { '\r', '\n' }) >= 0 ||
                               field[0] == ' ' ||
                               field[field.Length - 1] == ' ';
            
            if (needsQuoting)
            {
                // Use reusable buffer for escaping
                _escapingBuffer.Clear();
                _escapingBuffer.Append(escapeChar);
                
                // Escape existing quote characters by doubling them
                foreach (char c in field)
                {
                    _escapingBuffer.Append(c);
                    if (c == escapeChar[0])
                    {
                        _escapingBuffer.Append(escapeChar);
                    }
                }
                
                _escapingBuffer.Append(escapeChar);
                return _escapingBuffer.ToString();
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