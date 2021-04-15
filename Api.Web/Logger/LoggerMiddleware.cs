using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Api.Web.Logger
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        public LoggerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            //_logger = loggerFactory.CreateLogger<LoggerMiddleware>();
        }
        public async Task InvokeAsync(HttpContext context, ILogStorage logStorage)
        {
            var log = new LoggerModel
            {
                Path = context.Request.Path,
                Method = context.Request.Method,
                QueryString = context.Request.QueryString.ToString()
            };
            if (context.Request.Method == "POST")
            {
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body)//что пользователь отправил в теле запроса
                    .ReadToEndAsync();
                context.Request.Body.Position = 0;
                log.RequestBody = body;
            }
            log.RequestedOn = DateTimeOffset.Now;

            var originalBodyStream = context.Response.Body;

            using (var responseBodyStream = new MemoryStream())
            {
                context.Response.Body = responseBodyStream;

                await _next.Invoke(context);

                responseBodyStream.Position = 0;
                var response = await new StreamReader(responseBodyStream)
                    .ReadToEndAsync();
                responseBodyStream.Position = 0;

                log.Response = response;
                log.ResponseCode = context.Response.StatusCode.ToString();
                //log.RespondedOn = DateTime.UtcNow;
                log.RespondedOn = DateTimeOffset.Now;

                logStorage.Store(log);

                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            //_logger.LogInformation(
            //    "Request {header.Value} {method} {url} => {statusCode}",
            //    context.Request?.Headers,
            //    context.Request?.Method,
            //    context.Request?.Path.Value,
            //    context.Response?.StatusCode);

        }
    }
}
