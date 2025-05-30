using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SeConselhoFosseBom.Class.ApiClients
{
    public class HttpClientHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientHelper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateClient(string clientName, string? bearerToken = null)
        {
            var client = _httpClientFactory.CreateClient(clientName);
            if (!string.IsNullOrEmpty(bearerToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
            return client;
        }

        public async Task<T?> GetAsync<T>(string clientName, string url, string? token = null)
        {
            var client = CreateClient(clientName, token);
            var response = await client.GetAsync(url);
            return await HandleResponse<T>(response);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string clientName, string url, TRequest body, string? token = null)
        {
            var client = CreateClient(clientName, token);
            var content = CreateJsonContent(body);
            var response = await client.PostAsync(url, content);
            return await HandleResponse<TResponse>(response);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string clientName, string url, TRequest body, string? token = null)
        {
            var client = CreateClient(clientName, token);
            var content = CreateJsonContent(body);
            var response = await client.PutAsync(url, content);
            return await HandleResponse<TResponse>(response);
        }

        public async Task<bool> DeleteAsync(string clientName, string url, string? token = null)
        {
            var client = CreateClient(clientName, token);
            var response = await client.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string clientName, string url, TRequest body, string? token = null)
        {
            var client = CreateClient(clientName, token);
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, url)
            {
                Content = CreateJsonContent(body)
            };
            var response = await client.SendAsync(request);
            return await HandleResponse<TResponse>(response);
        }

        private static StringContent CreateJsonContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
        }

        private static async Task<T?> HandleResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro {response.StatusCode}: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
