FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RankedChoiceServices/RankedChoiceServices.csproj", "RankedChoiceServices/"]
RUN dotnet restore "RankedChoiceServices/RankedChoiceServices.csproj"
COPY . .
WORKDIR "/src/RankedChoiceServices"
RUN dotnet build "RankedChoiceServices.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RankedChoiceServices.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RankedChoiceServices.dll"]
