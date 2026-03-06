FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY . .

# Restore i publish
RUN dotnet restore EShop.API/EShop.API.csproj
RUN dotnet publish EShop.API/EShop.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5238
ENV ASPNETCORE_URLS=http://+:5238
ENTRYPOINT ["dotnet", "EShop.API.dll"]