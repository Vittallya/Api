using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Web.Logger
{
    public class FileLoggerStorage : ILogStorage
    {
        //string path = @"Middlewares\Updownloads\log.txt";
        string path = @"Logger\logger.txt";

        public void Store(LoggerModel log)
        {
            string[] lines =
            {
                $"Path: {log.Path}",
                $"QueryString: {log.QueryString}",
                $"Method: {log.Method}",
                $"RequestBody: {log.RequestBody}",
                $"Requested at: {log.RequestedOn}",
                $"Response: {log.Response}",
                $"ResponseCode: {log.ResponseCode}",
                $"Responded at: {log.RespondedOn}",
                Environment.NewLine
            };

            System.IO.File.AppendAllLines(path, lines);
        }
    }
}
