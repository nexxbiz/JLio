using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class JLioExecutionResult
    {
        public JLioExecutionResult(bool success, JToken data)
        {
            Success = success;
            Data = data;
        }

        public JToken Data { get; }

        public bool Success { get; }

        public static JLioExecutionResult Failed(JToken data)
        {
            return new JLioExecutionResult(false, data);
        }

        public static JLioExecutionResult SuccessFul(JToken data)
        {
            return new JLioExecutionResult(true, data);
        }
    }
}