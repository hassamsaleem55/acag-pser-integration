using ACAG_PSER_Integration.Models.Requests;
using ACAG_PSER_Integration.Models.Responses;
using ACAG_PSER_Integration.Services.Enums;
using ACAG_PSER_Integration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace ACAG_PSER_Integration.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PserController : ControllerBase
{
    private readonly IAcagPserClient _pserClient;
    private readonly IDecryptionService _decryptionService;
    private readonly ICredentialFormatter _credentialFormatter;
    private readonly ILogger<PserController> _logger;

    public PserController(
        IAcagPserClient pserClient,
        IDecryptionService decryptionService,
        ICredentialFormatter credentialFormatter,
        ILogger<PserController> logger)
    {
        _pserClient = pserClient;
        _decryptionService = decryptionService;
        _credentialFormatter = credentialFormatter;
        _logger = logger;
    }

    [HttpPost("check-eligibility")]
    public async Task<IActionResult> CheckEligibility([FromBody] CheckEligibilityRequest request, CancellationToken cancellationToken)
    {
        string username;
        string password;

        try
        {
            // Decrypt and extract using their respective rules
            username = DecryptAndClean(request.Username, CredentialType.Username);
            password = DecryptAndClean(request.Password, CredentialType.Password);
        }
        catch (Exception ex) when (ex is FormatException || ex is CryptographicException)
        {
            _logger.LogWarning(ex, "Invalid authentication data received.");

            return BadRequest(CreateErrorResponse(StatusCodes.Status400BadRequest, "INVALID_REQUEST", "Invalid request data."));
        }

        //var loginResult = await _pserClient.LoginAsync(username, password, cancellationToken);

        //if (!loginResult.Success || loginResult.Data is null)
        //{
        //    return CreateApiResponse(loginResult);
        //}

        //var eligibilityResult = await _pserClient.CheckEligibilityAsync(request.ApplicantCNIC, loginResult.Data.AccessToken, cancellationToken);
        var eligibilityResult = await _pserClient.CheckEligibilityAsync(request.ApplicantCNIC, "", cancellationToken);

        return CreateApiResponse(eligibilityResult);
    }

    private string DecryptAndClean(string value, CredentialType type)
    {
        var processedValue = _decryptionService.Decrypt(value);

        return _credentialFormatter.ExtractOriginalValue(processedValue, type);
    }

    private IActionResult CreateApiResponse<T>(PserApiResponse<T> response)
    {
        return StatusCode(NormalizeStatusCode(response.StatusCode), response);
    }

    private static PserApiResponse<PserEligibilityData> CreateErrorResponse(int statusCode, string code, string message)
    {
        return new PserApiResponse<PserEligibilityData>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Error = new PserErrorDetails
            {
                Code = code
            }
        };
    }

    private static int NormalizeStatusCode(int statusCode)
    {
        return statusCode is >= 100 and <= 599
            ? statusCode
            : StatusCodes.Status502BadGateway;
    }
}