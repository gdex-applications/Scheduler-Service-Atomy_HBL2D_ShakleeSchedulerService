using Newtonsoft.Json;
using System.Diagnostics;

namespace AtmyHbl2dShkleBrnCdSchedulerService.Functions
{
    internal class CleaningData
    {
        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        public static string CutdownException(Exception ex)
        {
            var st = new StackTrace(ex, true);
            // Get the top stack frame
            var frame = st.GetFrame(st.FrameCount - 1);
            // Get the line number from the stack frame
            var line = frame.GetFileLineNumber();
            var name = frame.GetMethod().DeclaringType;
            //var query = st.GetFrames()         // get the frames
            //  .Select(frame => new
            //  {                   // get the info
            //      FileName = frame.GetFileName(),
            //      LineNumber = frame.GetFileLineNumber(),
            //      ColumnNumber = frame.GetFileColumnNumber(),
            //      Method = frame.GetMethod(),
            //      Class = frame.GetMethod().DeclaringType,
            //  });
            return $"{ex.Message}{Environment.NewLine}Function: {name}{Environment.NewLine}Line: {line}";
        }

        public static bool TryDeserializeObject<T>(string json, out T result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(json);
                return result != null;
            }
            catch (JsonException)
            {
                result = default;
                return false;
            }
        }
    }
}
