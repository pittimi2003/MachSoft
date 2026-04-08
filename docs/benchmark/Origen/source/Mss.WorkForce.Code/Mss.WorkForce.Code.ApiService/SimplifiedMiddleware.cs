using System.Text;

namespace Mss.WorkForce.Code.ApiService
{
	public class SimplifiedMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<SimplifiedMiddleware> _logger;
		private static long NextRequestId = 0;

		public SimplifiedMiddleware(RequestDelegate next, ILogger<SimplifiedMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			long requestId = Interlocked.Increment(ref NextRequestId);
			string url = context.Request.Path;

			// Capturar la respuesta en un MemoryStream
			var originalBodyStream = context.Response.Body;
			using var responseBodyStream = new MemoryStream();
			context.Response.Body = responseBodyStream;

			try
			{
				// Log de la solicitud recibida (incluyendo el cuerpo de la solicitud)
				var logMessage = await BuildLogRequestReceived(context, requestId);
				_logger.LogInformation(logMessage);

				await _next(context);

				// Capturar la respuesta y enviarla al cliente
				var responseBody = await CaptureResponseBody(responseBodyStream);
				_logger.LogInformation("[Request ID: {RequestId}] Response: {ResponseBody}", requestId, responseBody);

				// Escribir la respuesta capturada de vuelta al stream original
				await responseBodyStream.CopyToAsync(originalBodyStream);
			}
			catch (Exception ex)
			{
				var logMessage = await BuildLogRequestReceived(context, requestId);
				var responseBody = await CaptureResponseBody(responseBodyStream);
				_logger.LogError(ex, "[Request ID: {RequestId}] An error occurred while processing the request. Details: {LogMessage}, Response: {ResponseBody}", requestId, logMessage, responseBody);
				await CreateExceptionTextResponse(context.Response, ex, logMessage, responseBody);
			}
			finally
			{
				context.Response.Body = originalBodyStream;  // Asegurar que se restaura el stream original
				_logger.LogInformation("[Request ID: {RequestId}] Finished handling request. Status Code: {StatusCode}, URL: {Url}", requestId, context.Response.StatusCode, url);
			}
		}

		private static async Task<string> BuildLogRequestReceived(HttpContext context, long requestId)
		{
			HttpRequest request = context.Request;
			StringBuilder message = new();

			// IP remota
			string remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			if (remoteIp == "::1" || remoteIp == "127.0.0.1")
				remoteIp = "localhost";

			message.Append($"[Request ID: {requestId}] Request received from IP='{remoteIp}', ");

			// URL
			message.Append($"Method: {request.Method}, Url='{request.Path}'");

			// Query string
			if (request.QueryString.HasValue)
			{
				message.Append($", QueryString='{request.QueryString.Value}'");
			}

			// Headers
			if (request.Headers.Count > 0)
			{
				message.Append(", Headers=[");
				foreach (var header in request.Headers)
				{
					message.Append($"{header.Key}: {header.Value}; ");
				}
				message.Append("]");
			}

			// Body
			if (request.ContentLength > 0)
			{
				request.EnableBuffering();  // Permite que el cuerpo de la solicitud se lea varias veces
				request.Body.Position = 0;  // Asegurarse de que la posición esté en el inicio
				using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
				{
					var bodyContent = await reader.ReadToEndAsync();
					message.Append($", Body='{bodyContent}'");
					request.Body.Position = 0; // Resetea la posición del stream del cuerpo para asegurar que otros componentes puedan leerlo
				}
			}

			return message.ToString();
		}

		private static async Task<string> CaptureResponseBody(Stream responseBodyStream)
		{
			responseBodyStream.Seek(0, SeekOrigin.Begin);
			var text = await new StreamReader(responseBodyStream).ReadToEndAsync();
			responseBodyStream.Seek(0, SeekOrigin.Begin);
			return text;
		}

		private static async Task CreateExceptionTextResponse(HttpResponse response, Exception ex, string logMessage, string responseBody)
		{
			if (response.HasStarted)
			{
				throw ex;
			}

			response.Clear();
			response.StatusCode = StatusCodes.Status500InternalServerError;
			response.ContentType = "text/plain";

			string errorMessage = $"An unexpected error occurred: {ex.Message}\nRequest Details: {logMessage}\nResponse: {responseBody}";
			await response.WriteAsync(errorMessage);
		}
	}
}
