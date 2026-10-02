using ACAG_PSER_Integration.Models.Requests;
using ACAG_PSER_Integration.Models.Responses;
using ACAG_PSER_Integration.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ACAG_PSER_Integration.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CryptoController : ControllerBase
{
    private readonly IEncryptionService _encryptionService;
    private readonly ICredentialFormatter _credentialFormatter;

    public CryptoController(IEncryptionService encryptionService, ICredentialFormatter credentialFormatter)
    {
        _encryptionService = encryptionService;
        _credentialFormatter = credentialFormatter;
    }

    [HttpPost("generate-eligibility-payload")]
    public ActionResult<GeneratePserPayloadResponse> GeneratePayload([FromBody] GeneratePserPayloadRequest request)
    {
        // Use the dedicated username and password rules
        var formattedUsername = _credentialFormatter.FormatUsername(request.Username);
        var formattedPassword = _credentialFormatter.FormatPassword(request.Password);

        return Ok(new GeneratePserPayloadResponse
        {
            Username = _encryptionService.Encrypt(formattedUsername),
            Password = _encryptionService.Encrypt(formattedPassword)
        });
    }
}