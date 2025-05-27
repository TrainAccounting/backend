using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Trainacc.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла необработанная ошибка");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var errorId = Guid.NewGuid().ToString();
            var errorResponse = new
            {
                message = "Произошла ошибка при обработке запроса.",
                errorId,
                details = exception.Message,
                help = GetHelpText(exception)
            };
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }

        private static string GetHelpText(Exception ex)
        {
            // Можно будет в будущем расширить список типовых ошибок, которые здесь будут переведены на русский
            if (ex is DbUpdateException)
                return "Ошибка базы данных. Проверьте корректность данных и соединение с БД.";
            if (ex is UnauthorizedAccessException)
                return "Доступ запрещён. Проверьте права доступа.";
            if (ex is ArgumentNullException)
                return "Передан пустой или некорректный параметр.";
            if (ex is InvalidOperationException)
                return "Некорректная операция. Проверьте логику запроса.";
            if (ex is FormatException)
                return "Ошибка формата данных. Проверьте правильность введённых значений.";
            if (ex is NotImplementedException)
                return "Функционал не реализован.";
            if (ex is TimeoutException)
                return "Превышено время ожидания операции.";
            return "Обратитесь к администратору или в поддержку, указав errorId.";
        }
    }
}
