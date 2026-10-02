using ACAG_PSER_Integration.Models.Responses;

namespace ACAG_PSER_Integration.Services.Interfaces;

public interface IAcagHttpClient
{
    Task<PserApiResponse<T>> PostAsync<T>(string endpoint, object payload, CancellationToken cancellationToken = default);

    Task<PserApiResponse<T>> PostAsync<T>(string endpoint, object payload, string bearerToken, CancellationToken cancellationToken = default);
}