FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["PdfProcessor.Worker/PdfProcessor.Worker.csproj", "PdfProcessor.Worker/"]
COPY ["PdfProcessor.Data/PdfProcessor.Data.csproj", "PdfProcessor.Data/"]
COPY ["PdfProcessor.Shared/PdfProcessor.Shared.csproj", "PdfProcessor.Shared/"]

RUN dotnet restore "./PdfProcessor.Worker/PdfProcessor.Worker.csproj"

COPY . .

WORKDIR "/src/PdfProcessor.Worker"
RUN dotnet build "./PdfProcessor.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "./PdfProcessor.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PdfProcessor.Worker.dll"]