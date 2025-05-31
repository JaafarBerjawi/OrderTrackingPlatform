# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

# Stage 1: Base image for running the application
# This stage sets up the basic environment for the ASP.NET Core application using a Windows Nano Server base image.
# Nano Server is a minimal footprint option for Windows containers.
# It's used for both fast mode debugging in Visual Studio and as the base for the final production image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
# Sets the working directory in the container.
WORKDIR /app
# Exposes port 8080 and 8081 for the application to listen on. (Though typically a client-side app might only need one for serving, or this might be for server-side rendering aspects)
EXPOSE 8080
EXPOSE 8081


# Stage 2: Build stage
# This stage compiles the .NET project using the .NET SDK image for Windows Nano Server.
FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
# Defines a build argument for the build configuration (e.g., Release or Debug).
# Note: %BUILD_CONFIGURATION% is used in RUN commands, typical for Windows environments.
ARG BUILD_CONFIGURATION=Release
# Sets the working directory for build operations.
WORKDIR /src
# Copies the .csproj file into the container and places it in the Frontend.App.Client/ directory.
# This is done first to leverage Docker's layer caching for faster subsequent builds if only code changes.
COPY ["Frontend.App.Client.csproj", "Frontend.App.Client/"]
# Restores NuGet packages for the project.
RUN dotnet restore "./Frontend.App.Client/Frontend.App.Client.csproj"
# Sets the working directory to the root of the source files for the COPY . . command.
WORKDIR "/src/"
# Copies all source files from the build context into the container at the current WORKDIR (/src/).
COPY . .
# Sets the working directory to the project folder to run the build command.
WORKDIR "/src/Frontend.App.Client"
# Builds the project. The output is placed in /app/build.
# Uses %BUILD_CONFIGURATION% for variable substitution, common in Windows CMD.
RUN dotnet build "./Frontend.App.Client.csproj" -c %BUILD_CONFIGURATION% -o /app/build

# Stage 3: Publish stage
# This stage publishes the application, creating a deployment-ready package.
# It builds upon the 'build' stage.
FROM build AS publish
# Defines a build argument for the build configuration.
ARG BUILD_CONFIGURATION=Release
# Publishes the application. The output is placed in /app/publish.
# /p:UseAppHost=false means it won't generate a native executable, just the DLL.
# Uses %BUILD_CONFIGURATION% for variable substitution.
RUN dotnet publish "./Frontend.App.Client.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

# Stage 4: Final stage
# This stage creates the final, lean image for production.
# It uses the 'base' image created in Stage 1, which contains only the ASP.NET runtime for Nano Server.
FROM base AS final
# Sets the working directory in the final image.
WORKDIR /app
# Copies the published application files from the 'publish' stage (/app/publish) into the final image.
COPY --from=publish /app/publish .
# Defines the command to run when the container starts. This will execute the application.
ENTRYPOINT ["dotnet", "Frontend.App.Client.dll"]