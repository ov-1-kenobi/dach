using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Authorization;

namespace DachStackApp.api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly int maxSessionDuration = 60 * 60 * 4; // 4 hours
        private static int _sessionCount = 0;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "devtodostorage";
        private readonly string _storageAccountConnectionString = "UseDevelopmentStorage=true"; // or your actual connection string

        public AuthController(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
        }
        [HttpGet("login")]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated??false)
            {
                // Already signed in, just go back to your main page (e.g. /)
                var RetHTML = string.Empty;
                RetHTML += $"""
                    <div>
                        User: {User.Identity.Name} is authenticated.
                    </div>
                """;
                return Redirect("/");
                //return new OkResult();// RedirectToAction("Index", "Home");
            }
            else
            {
                // Not authenticated, so force a full OIDC challenge (redirect to B2C).
                return Challenge(
                    new AuthenticationProperties { RedirectUri = "https://localhost:7140/signin-oidc" }, 
                    Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme
                );
            } 
        }
        [HttpGet("sas")]
        public IActionResult GetSasToken()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobName = "example.txt"; // Replace with your blob name

            // Create a SAS token for the blob
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(maxSessionDuration) // Set expiration time as needed
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential("your-account-name", "your-account-key")).ToString();

            return Ok(new { SasToken = sasToken });
        }
    }
}
