using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Shared.Infrastructure.Clients
{
	public class DefaultHttpClient
	{
		private readonly HttpClient _httpClient;
		private readonly IHttpContextAccessor _context;

		public DefaultHttpClient(HttpClient httpClient, IHttpContextAccessor context)
		{
			_httpClient = httpClient;
			_context = context;
		}

		private void AttachAuthHeader(HttpRequestMessage request)
		{
			if (_context.HttpContext?.Request.Cookies.TryGetValue("access_token", out var token) == true)
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
		}

		public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
		{
			AttachAuthHeader(request);
			return await _httpClient.SendAsync(request, cancellationToken);
		}

		public Task<HttpResponseMessage> GetAsync(string uri)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			return SendAsync(request);
		}

		public Task<HttpResponseMessage> PostAsJsonAsync<T>(string uri, T body)
		{
			var request = new HttpRequestMessage(HttpMethod.Post, uri)
			{
				Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
			};
			return SendAsync(request);
		}

		public Task<HttpResponseMessage> PutAsJsonAsync<T>(string uri, T body)
		{
			var request = new HttpRequestMessage(HttpMethod.Put, uri)
			{
				Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
			};
			return SendAsync(request);
		}

		public Task<HttpResponseMessage> DeleteAsync(string uri)
		{
			var request = new HttpRequestMessage(HttpMethod.Delete, uri);
			return SendAsync(request);
		}

		public async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
		{
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<T>();
		}
	}
}
