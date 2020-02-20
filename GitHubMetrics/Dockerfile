FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["GitHubMetrics/GitHubMetrics.csproj", "GitHubMetrics/"]
RUN dotnet restore "GitHubMetrics/GitHubMetrics.csproj"
COPY . .
WORKDIR "/src/GitHubMetrics"
RUN dotnet build "GitHubMetrics.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GitHubMetrics.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitHubMetrics.dll"]