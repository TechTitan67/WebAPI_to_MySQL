using System.Net.Http.Headers;
using System.Text.Json;

public class PayPalPaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _secret;

    public PayPalPaymentService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _clientId = config["PayPal:ClientId"] ?? throw new ArgumentNullException("PayPal:ClientId configuration is missing.");
        _secret = config["PayPal:Secret"] ?? throw new ArgumentNullException("PayPal:Secret configuration is missing.");
    }

    public async Task<bool> VerifyPaymentAsync(string paymentId)
    {
        // 1. Get OAuth token from PayPal
        var authToken = await GetPayPalAccessTokenAsync();

        // 2. Call PayPal Orders API to verify payment
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        var response = await _httpClient.GetAsync($"https://api-m.sandbox.paypal.com/v2/checkout/orders/{paymentId}");

        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var status = doc.RootElement.GetProperty("status").GetString();

        return status == "COMPLETED";
    }

    private async Task<string?> GetPayPalAccessTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api-m.sandbox.paypal.com/v1/oauth2/token");
        var byteArray = System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_secret}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("access_token", out var accessTokenElement))
        {
            return accessTokenElement.GetString();
        }

        return null; // Explicitly return null if "access_token" is not found
    }
}