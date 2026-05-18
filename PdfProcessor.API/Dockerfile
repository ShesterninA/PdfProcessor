FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["PdfProcessor.API/PdfProcessor.API.csproj", "PdfProcessor.API/"]
COPY ["PdfProcessor.Data/PdfProcessor.Data.csproj", "PdfProcessor.Data/"]
COPY ["PdfProcessor.Shared/PdfProcessor.Shared.csproj", "PdfProcessor.Shared/"]

RUN dotnet restore "./PdfProcessor.API/PdfProcessor.API.csproj"

COPY . .

WORKDIR "/src/PdfProcessor.API"
RUN dotnet build "./PdfProcessor.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "./PdfProcessor.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PdfProcessor.API.dll"]