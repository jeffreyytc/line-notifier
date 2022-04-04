using LineNotifier.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text.Json;

namespace LineNotifier.Services
{
    public class NotifyApiHelper
    {
        private readonly string _authUrl = "https://notify-bot.line.me/oauth/authorize";
        private readonly string _tokenUrl = "https://notify-bot.line.me/oauth/token";
        private readonly string _statusUrl = "https://notify-api.line.me/api/status";
        private readonly string _revokeUrl = "https://notify-api.line.me/api/revoke";
        private readonly string _notifyUrl = "https://notify-api.line.me/api/notify";

        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public NotifyApiHelper(IConfiguration config, IHttpClientFactory clientFactory)
        {
            _config = config;
            _clientFactory = clientFactory;
        }

        public string GetAuthUrl()
        {
            var query = new Dictionary<string, string>()
            {
                { "response_type", "code" },
                { "client_id", _config["LineNotify:ClientId"] },
                { "state", "123123" },
                { "scope", "notify" },
                { "redirect_uri", _config["LineNotify:CallbackUrl"] }
            };

            return QueryHelpers.AddQueryString(_authUrl, query);
        }

        public NotifyTokenResult GetTokenResult(string code)
        {
            var data = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _config["LineNotify:CallbackUrl"] },
                { "client_id", _config["LineNotify:ClientId"] },
                { "client_secret", _config["LineNotify:ClientSecret"] }
            };

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_tokenUrl);
            request.Content = new FormUrlEncodedContent(data);

            var client = _clientFactory.CreateClient();
            var response = client.Send(request);
            var responseContent = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<NotifyTokenResult>(responseContent);
        }

        public NotifyStatusResult GetStatusResult(string accessToken)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri(_statusUrl);
            request.Headers.Add(HeaderNames.Authorization, $"Bearer {accessToken}");

            var client = _clientFactory.CreateClient();
            var response = client.Send(request);
            var responseContent = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<NotifyStatusResult>(responseContent);
        }

        public void Revoke(string accessToken)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_revokeUrl);
            request.Headers.Add(HeaderNames.Authorization, $"Bearer {accessToken}");

            var client = _clientFactory.CreateClient();
            client.Send(request);
        }

        public void Notify(string accessToken, string message)
        {
            var data = new Dictionary<string, string>()
            {
                { "message", message }
            };

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_notifyUrl);
            request.Content = new FormUrlEncodedContent(data);
            request.Headers.Add(HeaderNames.Authorization, $"Bearer {accessToken}");

            var client = _clientFactory.CreateClient();
            client.Send(request);
        }
    }
}
