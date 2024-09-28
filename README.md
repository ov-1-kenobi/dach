<img src="https://github.com/user-attachments/assets/f63c4554-f000-47ff-a78f-8dcf2a236ad8" alt="Dach Logo" width="128" height="128">

# Dach (noun; German) [neuter]
#### (English 'roof')
### Pronunciation: /dock/ (dahkh)

- A structure that covers the top of a building and provides protection from the elements.
- A part of a vehicle that covers the interior space.

# DACH Stack Overview
A lightweight collection of technologies that celebrates simplicity. Develop hosted locally off of Azurite services, deliver HTML/X from your C# backend, use the HTMX attributes for interaction and Daisy+Tailwind CSS for pre-made components and themes. Finally, deploy to Azure via Bicep files with dotnet cli.

The purpose is to remove as many dependencies as possible and deliver the same, or better, experience without the overhead of js frameworks like React running through local services like docker, node, et al which require their own specific domain knowledge, usually in extreme situations and always at the worst time. Another goal is to remove the reliance on dedicating to a specific db technology and allow the system to dictate/inform which db technologies to use during development. Most, if not all types, are presumed to be provided by Azurite.

Maybe HTMX helps to deliver with some of these concepts by giving simple wiring for my DOM interactions, I want to take a deep dive through it's features and find out. Azurite offers a compelling, easy to setup and manage local host that mocks the Azure services you need for developing complete solutions without requiring a large footprint of services and ecosystems to get running. Daisy + Tailwind css gives access to a nice set of extensible UI tools and theming built in. All of these together should offer a full, rich set of stack features enabling developers to proceed at full speed, without feeling locked in and having to make concessions due to rigidity of stack options

I'm also hoping to mitigate a lot of uneccessary reproduction of state and logic while delivering an easy, straightforward development experience. I'm probably wrong, but I've never done one of these things before, so we're already learning. If you would like to follow along or would like to contribute, please feel free to do so.

Thank you for your time

## Current Status
- (28 Sept. '24) Completed the file upload and combined with viewing, working on a bug with the delete dialog; also working on updating the todo app to use a local table for storage. Added a chat room app to the pipeline to research htmx's ability to work with signalr and websockets.
*EXAMPLES*
- **Todo List**
- **Image Upload & Viewer**
- **Chat Room**

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
   - ~~***fileUpload*** a simple file uploader that uploads to the local azurite blob service.  (Sept. 2024) 🛠️~~
   - ~~***fileReader*** a simple file "browser". display a list of the uploaded files, clicking on a file brings up a viewer modal (Sept. 2024) 🛠️~~
   - ***todo*** update the todo app, save values into an Azurite table service table. (September 2024) ⏳
- **Milestone 1b: Networked live chat**
   - ***chat*** a simple chat app; allows connections with monikers in a lobby/room environment (htmx-signalr & websockets) (October 2024)
- **Milestone 2:DevOps**: Bicep and csharp script plugged in to allow full control of resources via bicep files in dev (with .csx file helpers) and dotnet cli deployment. Fullstack deployed to Azure cloud service account with setup instructions for standing up an Azure subscription and resources and where to place the configuration secrets/ids (October 2024) 
- **Milestone 3:DachWorkers**: Research a "built-in" utility/service for engaging with web workers to enable crisper/faster UI/X interactions and responses. Build ways to do batch file uploads and firing/managing multiple tasks/requests at the same time. (November 2024)
- **Milestone 4:Security**: Work on examples to allow using google, facebook, microsoft authentications therefore allowing users to leverage their current authentication providers (December 2024)
- **Milestone ALPHA:=> TESTS PERFORMANCE DEPLOYMENT <=**: Performance, testing, and Deploying complete systems (code and resources) for environments in AZURE (Jun. 2025)
- **Milestone BRAVO:=> AWS POC OF ALL FEATURES <=**: Work on Azure DACH fullstack operations on local development environment using AWS LocalStack and deploy to AWS. (March 2025)
- **Milestone GOLD:=> ALL STACK <=**:Support mode; manage issues & requests, future features & hosts. (June 2025-)

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Acknowledgments

- **HTMX:** For making frontend development simpler and more efficient.
- **DaisyUI:** For providing beautiful, customizable UI components.
- **Azurite:** For making local Azure Storage emulation possible.
- **Bicep:** For simplifying infrastructure as code.

🛠️ currently in progress
⏳  research 