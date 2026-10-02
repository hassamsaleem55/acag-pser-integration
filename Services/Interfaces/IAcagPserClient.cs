using ACAG_PSER_Integration.Models.Responses;

namespace ACAG_PSER_Integration.Services.Interfaces;

public interface IAcagPserClient
{
    Task<PserApiResponse<PserLoginData>> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    Task<PserApiResponse<PserEligibilityData>> CheckEligibilityAsync(string cnic, string token, CancellationToken cancellationToken = default);
}