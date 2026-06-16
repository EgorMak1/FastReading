using System.Net;
using System.Text.Json;

namespace MauiApp1.Services;

public sealed class ApiException : Exception
{
    public ApiException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }

    public bool IsUnauthorized => StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
}

internal static class ApiError
{
    public static async Task<ApiException> FromResponseAsync(
        HttpResponseMessage response,
        string fallbackMessage,
        CancellationToken cancellationToken = default)
    {
        var serverMessage = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = NormalizeServerMessage(serverMessage, response.StatusCode, fallbackMessage);

        return new ApiException(message, response.StatusCode);
    }

    public static ApiException FromException(Exception exception, string fallbackMessage)
    {
        return exception switch
        {
            ApiException apiException => apiException,
            HttpRequestException => new ApiException("Не удалось подключиться к серверу. Проверьте интернет-соединение и повторите попытку.", innerException: exception),
            TaskCanceledException => new ApiException("Сервер не ответил вовремя. Повторите попытку позже.", innerException: exception),
            JsonException => new ApiException("Сервер вернул неожиданный ответ. Повторите попытку позже.", innerException: exception),
            _ => new ApiException(fallbackMessage, innerException: exception)
        };
    }

    private static string NormalizeServerMessage(string serverMessage, HttpStatusCode statusCode, string fallbackMessage)
    {
        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return "Сессия истекла. Войдите снова.";
        }

        if (!string.IsNullOrWhiteSpace(serverMessage))
        {
            return serverMessage.Trim().Trim('"');
        }

        if ((int)statusCode >= 500)
        {
            return "Сервер временно недоступен. Повторите попытку позже.";
        }

        return fallbackMessage;
    }
}
