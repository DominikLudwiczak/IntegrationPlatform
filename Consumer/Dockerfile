FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

ARG ASPNETCORE_ENVIRONMENT=Demo
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}

COPY Consumer/*.csproj ./Consumer/
RUN dotnet restore ./Consumer/Consumer.csproj

COPY Consumer/. ./Consumer/
WORKDIR /app/Consumer
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build-env /app/Consumer/out ./
ENTRYPOINT ["dotnet", "Consumer.dll"]
EXPOSE 5001
