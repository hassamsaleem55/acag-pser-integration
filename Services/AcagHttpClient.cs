using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ACAG_PSER_Integration.Models.Responses;
using ACAG_PSER_Integration.Services.Interfaces;

namespace ACAG_PSER_Integration.Services;

public sealed class AcagHttpClient : IAcagHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AcagHttpClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AcagHttpClient(HttpClient httpClient, ILogger<AcagHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<PserApiResponse<T>> PostAsync<T>(string endpoint, object payload, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(HttpMethod.Post, endpoint, payload, null, cancellationToken);
    }

    public Task<PserApiResponse<T>> PostAsync<T>(string endpoint, object payload, string bearerToken, CancellationToken cancellationToken = default)
    {
        return SendAsync<T>(HttpMethod.Post, endpoint, payload, bearerToken, cancellationToken);
    }

    private async Task<PserApiResponse<T>> SendAsync<T>(HttpMethod method, string endpoint, object payload, string? bearerToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, endpoint);

        request.Content = CreateJsonContent(payload);

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                _logger.LogWarning("ACAG returned an empty response. Endpoint: {Endpoint}, StatusCode: {StatusCode}", endpoint, (int)response.StatusCode);

                return CreateUpstreamError<T>((int)response.StatusCode, "ACAG returned an empty response.");
            }

            var result = JsonSerializer.Deserialize<PserApiResponse<T>>(responseBody, JsonOptions);

            if (result is null)
            {
                _logger.LogWarning("Unable to deserialize ACAG response. Endpoint: {Endpoint}, StatusCode: {StatusCode}", endpoint, (int)response.StatusCode);

                return CreateUpstreamError<T>((int)response.StatusCode, "Invalid response received from ACAG.");
            }

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while communicating with ACAG. Endpoint: {Endpoint}", endpoint);

            return CreateUpstreamError<T>(StatusCodes.Status502BadGateway, "Unable to communicate with ACAG.");
        }
    }

    private static StringContent CreateJsonContent(object payload)
    {
        var json = JsonSerializer.Serialize(payload);

        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static PserApiResponse<T> CreateUpstreamError<T>(int statusCode, string message)
    {
        return new PserApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Error = new PserErrorDetails
            {
                Code = "UPSTREAM_ERROR"
            }
        };
    }
}