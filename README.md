# DoXM
A remote control and remote scripting solution, built with .NET Core, Electron, and WebRTC.

Website: https://doxm.app  
Public Server: https://my.doxm.app

## Build Instructions (Windows 10)  
The following steps will configure your Windows 10 machine for building the DoXM server and clients.
* Install .NET Core SDK.
    * Here: https://www.microsoft.com/net/download
    * Or here: https://dot.net/v1/dotnet-install.ps1
* Install Node.js v10.2.0
    * https://nodejs.org/download/release/v10.2.0/
* Install Electron Builder globally with NPM.
    * npm install -g electron-builder
* Install Ubuntu WSL.
    * https://docs.microsoft.com/en-us/windows/wsl/install-win10
    * This is required to build the Linux version of the remote control client.
* Run Publish.ps1 in the Utilities folder.
    * The output folder will now contain the server with the clients in the Downloads folder.
    * Documentation for hosting in IIS can be found here: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/index?view=aspnetcore-2.1

## Build Instructions (Linux)
See Setup_Ubuntu_Builder.sh and DoXM_Server_Install.sh in Utilities.

## TURN Server
* Set up an Ubuntu server.  This will be the TURN server (using Coturn).
    * Run TURN_Install.sh with sudo, passing in necessary arguments.
    * Add the TURN user credentials to the DoXM server's appsettings.json, under IceConfiguration.
    * More documentation can be found at the Coturn project repo: https://github.com/coturn/coturn