using System.Net;

namespace Application.Common;

public static class ApplicationConstants
{
    public static readonly Dictionary<int, ErrorMap> ErrorMapping = new()
    {
        [0] = new ErrorMap(HttpStatusCode.OK, null),
        [-1] = new ErrorMap(HttpStatusCode.NotFound, "Obiekt nie istnieje."),
        [-2] = new ErrorMap(HttpStatusCode.Conflict, "Nieobsługiwany wyjątek."),
        [-3] = new ErrorMap(HttpStatusCode.BadRequest, "Niepoprawne żądanie."),
        [-4] = new ErrorMap(HttpStatusCode.InternalServerError, "Wewnętrzny błąd serwera."),
        [-5] = new ErrorMap(HttpStatusCode.RequestTimeout, "Przekroczono czas oczekiwania na odpowiedź.")
    };

    public readonly struct ErrorMap
    {
        public HttpStatusCode StatusCode { get; }
        public string? Message { get; }

        public ErrorMap(HttpStatusCode statusCode, string? message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }


    public static readonly Dictionary<HttpStatusCode, string> HttpStatusCodeInformationMapping = new()
    {
        [HttpStatusCode.NotFound] = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.4",
        [HttpStatusCode.Conflict] = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.8",
        [HttpStatusCode.BadRequest] = "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1",
        [HttpStatusCode.InternalServerError] = "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.1",
        [HttpStatusCode.RequestTimeout] = "https://www.rfc-editor.org/rfc/rfc7231#section-6.6.2"
    };
}