using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Azure;

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
        private readonly string _containerName = "authstorage";
        private readonly string _storageAccountConnectionString = "UseDevelopmentStorage=true"; // or your actual connection string

        public AuthController(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
        }
        [HttpGet("auth-status")]
        public IActionResult AuthStatus()
        {
            var RetHTML = string.Empty;
            if (User?.Identity?.IsAuthenticated??false)
            {
                // Already signed in, just go back to your main page (e.g. /)
                RetHTML += $"""
                    <div>
                        User: {User.Identity.Name} is authenticated.
                    </div>
                    <div>
                        Logout: <a href="/api/auth/logout">Logout</a>
                    </div>
                """;
                //return new OkResult();// RedirectToAction("Index", "Home");
            }
            else
            {
                // Not authenticated, so force a full OIDC challenge (redirect to B2C).
                RetHTML += $"""
                <a href="api/auth/login" hx-target="#auth-status">Sign In</a>
                """;
            } 
            return Ok(RetHTML);
        }
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            if (User?.Identity?.IsAuthenticated??false)
            {
                var RetHTML = string.Empty;
                var bob = SignOut();
                var callBack = Url.Content("~/");
                return SignOut(new AuthenticationProperties { RedirectUri = callBack }, Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            //     return SignOut(
            // new AuthenticationProperties
            // {
            //     RedirectUri = "https://localhost:7140/signin-oidc" // Redirect to the home page after logout
            // },
            // Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme
            //     );
            }
            else
            {
                // Not authenticated, so just redirect to the home page or show a message
                return RedirectToAction("auth-status", "auth");
            }
        }
        [HttpGet("login")]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated??false)
            {
                // Already signed in, just go back to your main page (e.g. /)
                return Redirect("/");
                //return new OkResult();// RedirectToAction("Index", "Home");
            }
            else
            {
                // Not authenticated, so force a full OIDC challenge (redirect to B2C).
                return Challenge(
                    new AuthenticationProperties { RedirectUri = "/" }, 
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
