using LineNotifier.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;

namespace LineNotifier.Services
{
    public class LoginApiHelper
    {
        private readonly string _authUrl = "https://access.line.me/oauth2/v2.1/authorize";
        private readonly string _tokenUrl = "https://api.line.me/oauth2/v2.1/token";
        private readonly string _verifyUrl = "https://api.line.me/oauth2/v2.1/verify";

        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public LoginApiHelper(IConfiguration config, IHttpClientFactory clientFactory)
        {
            _config = config;
            _clientFactory = clientFactory;
        }

        public string GetAuthUrl()
        {
            var query = new Dictionary<string, string>()
            {
                { "response_type", "code" },
                { "client_id", _config["LineLogin:ClientId"] },
                { "redirect_uri", _config["LineLogin:CallbackUrl"] },
                { "state", "123123" },
                { "scope", "profile openid" }
            };

            return QueryHelpers.AddQueryString(_authUrl, query);
        }

        public LoginTokenResult GetTokenResult(string code)
        {
            var data = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _config["LineLogin:CallbackUrl"] },
                { "client_id", _config["LineLogin:ClientId"] },
                { "client_secret", _config["LineLogin:ClientSecret"] }
            };

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_tokenUrl);
            request.Content = new FormUrlEncodedContent(data);

            var client = _clientFactory.CreateClient();
            var response = client.Send(request);
            var responseContent = response.Content.ReadAsStringAsync().Result;

            return JsonSerializer.Deserialize<LoginTokenResult>(responseContent);
        }

        public LoginVerifyResult GetVerifyResult(string idToken)
        {
            var data = new Dictionary<string, string>()
            {
                { "id_token", idToken },
                { "client_id", _config["LineLogin:ClientId"] }
            };

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_verifyUrl);
            request.Content = new FormUrlEncodedContent(data);

            var client = _clientFactory.CreateClient();
            var response = client.Send(request);
            var responseContent = response.Content.ReadAsStringAsync().Result;

            var result = JsonSerializer.Deserialize<LoginVerifyResult>(responseContent);

            if (string.IsNullOrEmpty(result.sub))
            {
                throw new Exception("Unexpected verify result content: " + responseContent);
            }

            return result;
        }
    }
}
