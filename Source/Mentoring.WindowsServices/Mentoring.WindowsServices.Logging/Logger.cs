using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Castle.DynamicProxy;
using Newtonsoft.Json;

namespace Mentoring.WindowsServices.Logging
{
    public class Logger : IInterceptor
    {
        private string log = "log.txt";

        public void Intercept(IInvocation invocation)
        {
            var curDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var filePath = Path.Combine(curDir, log);

            invocation.Proceed();

            var logInfo = new LogInfo
            {
                InParams = string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()),
                MethodName = invocation.Method.Name,
                Time = DateTime.Now.ToLongTimeString(),
                ReturnedValue = invocation.ReturnValue.ToString()
            };

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(JsonConvert.SerializeObject(logInfo));
            }
        }
    }
}
