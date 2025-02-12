FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy project files and Restores the project dependencies, downloading the NuGet packages required for the application.
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application files, Compile and publish
COPY . ./

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
 
ENV ASPNETCORE_ENVIRONMENT=Development

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "userauthjwt.dll"]