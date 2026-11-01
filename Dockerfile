FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /App

COPY ["UrlShortner.csproj", "./"]

RUN dotnet restore

COPY . .

RUN dotnet publish -c Reslease -o ./Publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /App

COPY --from=build /App/Publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "UrlShortner.dll"]