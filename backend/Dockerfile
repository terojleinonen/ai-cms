FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Cms.sln ./
COPY src/Cms.Domain/*.csproj src/Cms.Domain/
COPY src/Cms.Application/*.csproj src/Cms.Application/
COPY src/Cms.Infrastructure/*.csproj src/Cms.Infrastructure/
COPY src/Cms.Ai/*.csproj src/Cms.Ai/
COPY src/Cms.Api/*.csproj src/Cms.Api/
COPY tests/Cms.Tests/*.csproj tests/Cms.Tests/
RUN dotnet restore
COPY . .
RUN dotnet publish src/Cms.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__Cms="Data Source=/data/cms.db"
RUN mkdir /data && chown -R $APP_UID /data
VOLUME /data
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cms.Api.dll"]
