using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public class Partial : FunctionBase
    {
        public Partial()
        {
        }

        public Partial(params string[] arguments)
        {
            arguments.ToList().ForEach(a =>
                this.arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{a}\"")))));
        }

        public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            if (!Validate(currentToken, context)) return new JLioFunctionResult(false, currentToken);

            var currentPath = currentToken.Path;
            var values = GetArguments(arguments, currentToken, dataContext, context);
            var result = new JObject();

            values.ForEach(i => AddValue(result, i, currentPath, context));
            return new JLioFunctionResult(true, result);
        }

        private bool Validate(JToken currentToken, IExecutionContext context)
        {
            if (arguments.Count < 1)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"failed: {FunctionName} requires at least 1 argument");
                {
                    return false;
                }
            }

            return true;
        }

        private void AddValue(JObject target, JToken value, string path, IExecutionContext context)
        {
            var targetPath = value.Path.Substring(path.Length + 1);
            var names = new JsonSplittedPath(
                $"{context.ItemsFetcher.RootPathIndicator}{context.ItemsFetcher.PathDelimiter}{targetPath}");
            if (names.Elements.Count() == 1)
            {
                target.Add(targetPath, value);
            }
            else
            {
                JsonMethods.CheckOrCreateParentPath(target, new JsonSplittedPath(targetPath), context.ItemsFetcher,
                    null);
                var newTarget = context.ItemsFetcher.SelectToken(names.ParentElements.ToPathString(), target);
                if (newTarget.Type == JTokenType.Object) ((JObject) newTarget).Add(names.LastElement.ToString(), value);
            }
        }
    }
}