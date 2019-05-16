FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["providence-tweeter-bot.csproj", "./"]
RUN dotnet restore "./providence-tweeter-bot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "providence-tweeter-bot.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "providence-tweeter-bot.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "providence-tweeter-bot.dll"]
