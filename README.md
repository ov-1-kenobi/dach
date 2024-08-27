<img src="https://github.com/user-attachments/assets/f63c4554-f000-47ff-a78f-8dcf2a236ad8" alt="Dach Logo" width="128" height="128">

# DACH Stack Project Template

Welcome to the DACH Stack project template! This repository provides a starter template for building modern web applications using the **DACH Stack**:

- **D**aisyUI for UI components, built on top of Tailwind CSS.
- **A**zure/Azurite for local storage emulation and cloud deployment.
- **C**# for backend logic with ASP.NET Core.
- **H**TMX for lightweight frontend interactivity.

## Getting Started

### Prerequisites

Ensure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (`npm install -g azurite`)
- [Bicep CLI](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/install)
- [dotnet-script](https://github.com/filipw/dotnet-script) (optional, for running C# scripts)

### Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/ov-1-kenobi/dach.git
   cd dach-stack-template
   ```

2. **Install Dependencies:**

   ```bash
   npm install
   ```

3. **Run Azurite Locally:**

   ```bash
   azurite --silent --location ./Azurite --debug ./Azurite/debug.log
   ```

4. **Build and Run the Project:**

   ```bash
   dotnet build
   dotnet run
   ```

   The application will be available at `https://localhost:5263`.

### Securing the API with JWT and Azure AD

1. **Register the Application in Azure AD:**
   - Register your application in Azure AD, configure API permissions, and obtain the Client ID and Tenant ID.

2. **Install JWT Authentication Packages:**

   ```bash
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   dotnet add package Microsoft.Identity.Web
   ```

3. **Configure JWT Authentication:**
   - Update `Program.cs` with JWT authentication settings:

   ```csharp
   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options =>
       {
           options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0";
           options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidIssuer = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0",
               ValidateAudience = true,
               ValidAudience = builder.Configuration["AzureAd:ClientId"],
               ValidateLifetime = true,
           };
       });

   builder.Services.AddControllers();
   builder.Services.AddAuthorization();

   var app = builder.Build();

   app.UseHttpsRedirection();
   app.UseAuthentication();
   app.UseAuthorization();

   app.MapControllers();

   app.Run();
   ```

   - Add `AzureAd:TenantId` and `AzureAd:ClientId` to `appsettings.json`:

   ```json
   {
     "AzureAd": {
       "TenantId": "your-tenant-id",
       "ClientId": "your-client-id"
     }
   }
   ```

4. **Secure API Endpoints:**
   - Use `[Authorize]` to secure your API controllers:

   ```csharp
   [Authorize]
   [ApiController]
   [Route("api/[controller]")]
   public class ToDoController : ControllerBase
   {
       // Your API methods
   }
   ```

5. **Configure HTMX to Include JWT Tokens:**

   ```javascript
   document.body.addEventListener('htmx:configRequest', function(event) {
       let token = localStorage.getItem('jwt');
       if (token) {
           event.detail.headers['Authorization'] = `Bearer ${token}`;
       }
   });
   ```

### Bicep Deployment

Refer to the Bicep section for deploying resources to Azure, including setting up local Azurite resources using `createLocalResource.csx`.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Acknowledgments

- **HTMX:** For making frontend development simpler and more efficient.
- **DaisyUI:** For providing beautiful, customizable UI components.
- **Azurite:** For making local Azure Storage emulation possible.
- **Bicep:** For simplifying infrastructure as code.
