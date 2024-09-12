<img src="https://github.com/user-attachments/assets/f63c4554-f000-47ff-a78f-8dcf2a236ad8" alt="Dach Logo" width="128" height="128">

# Dach (noun; German) [neuter]
#### (English 'roof')
### Pronunciation: /dock/ (dahkh)

- A structure that covers the top of a building and provides protection from the elements.
- A part of a vehicle that covers the interior space.

# DACH Stack Overview
A lightweight collection of technologies that gets out of the way and lets you decide how much JavaScript you need. Develop hosted locally off of Azurite services, deliver HTML/X from your backend, use the HTMX attributes for interaction and Daisy+Tailwind CSS for pre-made components and themes. Finally, deploy to Azure via Bicep files with dotnet cli

## Current Status
I am currently working on expanding the example todo application and adding more small example apps to demonstrate full stack scenarios including (file uploads, api's, security).

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


## Roadmap
~~- **Milestone 0:POC**: Todo app proof of concept.  (Aug. 2024)-(05 Sept. 2024)~~
- **Milestone 1:FullStack**: Complete, fullstack operations on local development environment using Azurite. Add 2 projects [fileUpload], [fileReader].
   - ***fileUpload*** a simple file uploader that uploads to the local azurite blob service.  (Sept. 2024) 🛠️
   - ***fileReader*** a simple file "browser". display a list of the uploaded files, clicking on a file brings up a viewer modal (Sept. 2024) 🛠️
   - ***Todo*** update the todo app, save values into an Azurite table service table. (Sept. 2024) ⏳
- **Milestone 2:DevOps**: Bicep and csharp script plugged in to allow full control of resources via bicep files in dev (with .csx file helpers) and dotnet cli deployment. Fullstack deployed to Azure cloud service account with setup instructions for standing up an Azure subscription and resources and where to place the configuration secrets/ids (Oct. 2024) 
- **Milestone 3:DachWorkers**: Built-in utility/service for engaging with web workers to enable crisper/faster UI/X interactions and responses. Build ways to do batch file uploads and firing/managing multiple tasks/requests at the same time. (Dec. 2024)
- **Milestone 4:Security**: Work on examples to allow using google, facebook, microsoft authentications therefore allowing users to leverage their current authentication providers (Jan. 2025)
- **Milestone 5a:=>AWS POC<=**: Complete, fullstack operations on local development environment using AWS LocalStack. (Mar. 2025)
- **Milestone 5b:=>AWS DevOps<=**: Deploy code and resources for environments in AWS (Jun. 2025)

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Acknowledgments

- **HTMX:** For making frontend development simpler and more efficient.
- **DaisyUI:** For providing beautiful, customizable UI components.
- **Azurite:** For making local Azure Storage emulation possible.
- **Bicep:** For simplifying infrastructure as code.

🛠️ currently in progress
⏳  research 