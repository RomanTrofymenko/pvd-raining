FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["ProvidenceTwitterBot/ProvidenceTwitterBot.csproj", "ProvidenceTwitterBot/"]
RUN dotnet restore "ProvidenceTwitterBot/ProvidenceTwitterBot.csproj"
COPY . .
WORKDIR "/src/ProvidenceTwitterBot"
RUN dotnet build "ProvidenceTwitterBot.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ProvidenceTwitterBot.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ProvidenceTwitterBot.dll"]
