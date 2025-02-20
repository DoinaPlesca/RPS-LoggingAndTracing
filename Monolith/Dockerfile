
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY /Monolith.csproj ./
RUN dotnet restore

COPY Monolith/. ./
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

COPY --from=build /publish .

EXPOSE 5341 9411

CMD ["dotnet", "Monolith.dll"]
