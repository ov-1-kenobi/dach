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

### cli tools
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
libman
func; azure installed tool for managing application functions and running them locally

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

🛠️ currently in progress
⏳  research  

## Deploy

- Make sure Azure Devloper CLI is installed; you can use 'chocolatey' to check and install.  
  - >choco list -localonly
  - >choco install -g azure-cli  

- Use 'az' to login and attach a subscription to your session
  - >az login (follow prompts, select (enter number) for the subscription you want to run az commands on)  

- Create environments for stage, prod, dev, etc.. using the az cli; current scheme rg[client]-[appName]-[environment]
  - > az group create --name rgCountryFried-DACHsampler-uat --location eastus
  - > az group create --name rgCountryFried-DACHsampler-prod --location eastus

