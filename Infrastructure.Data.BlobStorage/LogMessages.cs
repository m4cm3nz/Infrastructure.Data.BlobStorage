using Microsoft.Extensions.Logging;
using System;

namespace Infrastructure.Data.BlobStorage
{
    public static class LogMessages
    {
        private static readonly Action<ILogger, string, string, long, Exception> routePerformance;
        private static readonly Action<ILogger, string, string, string, Exception> uploadFailed;
        private static readonly Action<ILogger, string, Exception> dowloadFailed;

        static LogMessages()
        {
            routePerformance = LoggerMessage.Define<string, string, long>(
                LogLevel.Information,
                new EventId(0, "Performance Information"),
                "{RouteName} {Method} executado em {ElapsedMilliseconds}.");

            uploadFailed = LoggerMessage.Define<string, string, string>(
                LogLevel.Error,
                new EventId(1, "Upload Error"),
                "Não foi possível fazer o upload de {typeName} para arquivo {fileName} : {content}");

            dowloadFailed = LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(1, "Upload Error"),
                "Não foi possível fazer o download do arquivo {fileName}");
        }

        public static void UploadFailed(this ILogger logger, string typeName, string fileName, string content, Exception ex)
        {
            uploadFailed(logger, typeName, fileName, content, ex);
        }

        public static void DownloadFailed(this ILogger logger, string fileName, Exception ex)
        {
            dowloadFailed(logger, fileName, ex);
        }

        public static void LogRoutePerformance(this ILogger logger, string pageName, string method, long elapsedMilliseconds)
        {
            routePerformance(logger, pageName, method, elapsedMilliseconds, null);
        }
    }
}
