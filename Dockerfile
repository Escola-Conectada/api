FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ESCOLA_API.csproj", "./"]
RUN dotnet restore "./ESCOLA_API.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet publish "ESCOLA_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ESCOLA_API.dll"]
