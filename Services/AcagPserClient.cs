using ACAG_PSER_Integration.Models.Configuration;
using ACAG_PSER_Integration.Models.Responses;
using ACAG_PSER_Integration.Requests.Models;
using ACAG_PSER_Integration.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace ACAG_PSER_Integration.Services;

public sealed class AcagPserClient : IAcagPserClient
{
    private readonly IAcagHttpClient _acagHttpClient;
    private readonly string _connectionString;
    private readonly ILogger<AcagPserClient> _logger;

    public AcagPserClient(IAcagHttpClient acagHttpClient, IOptions<PserApiSettings> options, ILogger<AcagPserClient> logger)
    {
        _acagHttpClient = acagHttpClient;
        _connectionString = options.Value.ConnectionString;
        _logger = logger;
    }

    public Task<PserApiResponse<PserLoginData>> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var payload = new AcagLoginRequest
        {
            Username = username,
            Password = password
        };

        return _acagHttpClient.PostAsync<PserLoginData>("/api/auth/login", payload, cancellationToken);
    }

    public async Task<PserApiResponse<PserEligibilityData>> CheckEligibilityAsync(string cnic, string token, CancellationToken cancellationToken = default)
    {
        // ---------------------------------------------------------
        // 1. Call third-party eligibility API
        // ---------------------------------------------------------

        var payload = new { cnic };

        //var apiResponse = new PserApiResponse<PserEligibilityData>
        //{
        //    Success = true,
        //    StatusCode = 200,
        //    Message = "CNIC is eligible",
        //    Data = new PserEligibilityData
        //    {
        //        Cnic = cnic,
        //        PserEligibility = "Up to 50",
        //        ReasonCode = "VALID"
        //    },
        //    Error = null
        //};

        var apiResponse = await _acagHttpClient.PostAsync<PserEligibilityData>("/api/pser/eligibility", payload, token, cancellationToken);

        // ---------------------------------------------------------
        // 2. Pass third-party response values to stored procedure
        // ---------------------------------------------------------

        await SaveEligibilityResponseAsync(cnic, apiResponse, cancellationToken);

        // ---------------------------------------------------------
        // 3. Return the SAME response received from third-party API
        // ---------------------------------------------------------

        return apiResponse;
    }

    private async Task SaveEligibilityResponseAsync(string cnic, PserApiResponse<PserEligibilityData> response, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);

        await using var command = new SqlCommand("spUpdatePSEREligibility", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        command.Parameters.Add(
        new SqlParameter("@CNIC", SqlDbType.VarChar, 13)
        {
            Value = cnic
        });

        // Message
        command.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar, 500)
        {
            Value = (object?)response.Message ?? DBNull.Value
        });

        // Eligibility
        command.Parameters.Add(new SqlParameter("@Eligibility", SqlDbType.NVarChar, 100)
        {
            Value = (object?)response.Data?.PserEligibility ?? DBNull.Value
        });

        // ReasonCode
        command.Parameters.Add(new SqlParameter("@ReasonCode", SqlDbType.NVarChar, 100)
        {
            Value = (object?)response.Data?.ReasonCode ?? DBNull.Value
        });

        // ErrorCode
        command.Parameters.Add(new SqlParameter("@ErrorCode", SqlDbType.NVarChar, 100)
        {
            Value = (object?)response.Error?.Code ?? DBNull.Value
        });

        try
        {
            await connection.OpenAsync(cancellationToken);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (SqlException ex)
        {
            _logger.LogError(
                ex,
                "Database error while saving PSER eligibility response.");

            throw;
        }
    }
}