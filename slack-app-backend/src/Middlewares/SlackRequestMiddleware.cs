using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SlackAppBackend.Configuration;

namespace SlackAppBackend.Middlewares
{
    public class SlackRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAppConfiguration _configuration;

        private const string _slackRequestTimestampHeader = "X-Slack-Request-Timestamp";
        private const string _slackSignatureHeader = "X-Slack-Signature";

        public SlackRequestMiddleware(
            RequestDelegate next,
            IAppConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            if (request.Method != "POST")
            {
                await SetResponse(context.Response, 
                                  HttpStatusCode.MethodNotAllowed).ConfigureAwait(false);
                return;
            }
           
            await ReplaceRequestStreamWithMemoryStream(context.Request);

            var requestBody = await ReadString(request);
            request.Body.Seek(0, SeekOrigin.Begin);
            
            if (!VerifyRequest(requestBody, request.Headers, _configuration.SlackSignedSecret))
            {
                await SetResponse(context.Response, 
                                HttpStatusCode.BadRequest, "Invalid signature/token");
                return;
            }

            await _next(context);
        }

        private async Task SetResponse(HttpResponse httpResponse, HttpStatusCode statusCode, string message = null)
        {
            httpResponse.StatusCode = (int)statusCode;

            if (message != null)
                await httpResponse.WriteAsync(message).ConfigureAwait(false);
        }

        private static Task<string> ReadString(HttpRequest request) =>
            new StreamReader(request.Body).ReadToEndAsync();

        private async Task ReplaceRequestStreamWithMemoryStream(HttpRequest request)
        {
            var buffer = new MemoryStream();
            await request.Body.CopyToAsync(buffer);
            buffer.Seek(0, SeekOrigin.Begin);

            request.Body = buffer;
        }

        private bool VerifyRequest(string requestBody, IHeaderDictionary headers, string signingSecret)
        {
            var encoding = new UTF8Encoding();
            using (var hmac = new HMACSHA256(encoding.GetBytes(signingSecret)))
            {
                var hash = hmac.ComputeHash(encoding.GetBytes($"v0:{headers[_slackRequestTimestampHeader]}:{requestBody}"));
                var hashString = $"v0={BitConverter.ToString(hash).Replace("-", "").ToLower()}";

                return hashString.Equals(headers[_slackSignatureHeader]);
            }
        }
    }
}