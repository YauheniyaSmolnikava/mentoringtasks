using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace Mentoring.WindowsServices.Logging
{
    [PSerializable]
    public class LoggerPostSharpAttribute : OnMethodBoundaryAspect
    {
        private LogInfo logInfo;
        private string log = "logPostSharp.txt";

        public override void OnEntry(MethodExecutionArgs args)
        {
            logInfo = new LogInfo
            {
                MethodName = args.Method.Name,
                InParams = string.Join(", ", args.Arguments.Select(a => (a ?? "").ToString()).ToArray()),
                Time = DateTime.Now.ToLongTimeString()
            };
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var curDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var filePath = Path.Combine(curDir, log);

            logInfo.ReturnedValue = args.ReturnValue != null ? args.ReturnValue.ToString() : string.Empty;

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(JsonConvert.SerializeObject(logInfo));
            }
        }
    }
}
