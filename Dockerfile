FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

ARG ASPNETCORE_ENVIRONMENT=Demo
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}

# copy csproj and restore as distinct layers
COPY *.sln .
COPY IntegrationPlatform/*.csproj ./IntegrationPlatform/
COPY Application/*.csproj ./Application/
COPY Domain/*.csproj ./Domain/
COPY Infrastructure/*.csproj ./Infrastructure/
COPY OperationProcessor/*.csproj ./OperationProcessor/
COPY Worker/*.csproj ./Worker/

WORKDIR /app

RUN dotnet restore ./IntegrationPlatform/IntegrationPlatform.csproj

# copy and publish app and libraries
WORKDIR /app/
COPY IntegrationPlatform/. ./IntegrationPlatform/
COPY Application/. ./Application/
COPY Domain/. ./Domain/
COPY Infrastructure/. ./Infrastructure/
COPY OperationProcessor/. ./OperationProcessor/
COPY Worker/. ./Worker/
WORKDIR /app/IntegrationPlatform

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build-env /app/IntegrationPlatform/out ./
ENTRYPOINT ["dotnet", "IntegrationPlatform.dll"]
EXPOSE 5000