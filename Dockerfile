# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Studwork.Api/Studwork.Api.csproj Studwork.Api/
RUN dotnet restore Studwork.Api/Studwork.Api.csproj
COPY Studwork.Api/ Studwork.Api/
RUN dotnet publish Studwork.Api/Studwork.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN mkdir -p /app/data /app/uploads
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
VOLUME ["/app/data", "/app/uploads"]
ENTRYPOINT ["dotnet", "Studwork.Api.dll"]
