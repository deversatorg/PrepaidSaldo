# Базовый образ для сборки и публикации
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ApplicationAuth/ApplicationAuth.csproj", "ApplicationAuth/"]
COPY ["ApplicationAuth.Common/ApplicationAuth.Common.csproj", "ApplicationAuth.Common/"]
COPY ["ApplicationAuth.DAL/ApplicationAuth.DAL.csproj", "ApplicationAuth.DAL/"]
COPY ["ApplicationAuth.Domain/ApplicationAuth.Domain.csproj", "ApplicationAuth.Domain/"]
COPY ["ApplicationAuth.Models/ApplicationAuth.Models.csproj", "ApplicationAuth.Models/"]
COPY ["ApplicationAuth.ResourceLibrary/ApplicationAuth.ResourceLibrary.csproj", "ApplicationAuth.ResourceLibrary/"]
COPY ["ApplicationAuth.Services/ApplicationAuth.Services.csproj", "ApplicationAuth.Services/"]
RUN dotnet restore "ApplicationAuth/ApplicationAuth.csproj"
COPY . .
WORKDIR "/src/ApplicationAuth"
RUN dotnet build "ApplicationAuth.csproj" -c Release -o /app/build

# Образ для отладки
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS debug
WORKDIR /src
COPY --from=build /app/build ./

# Установка Chrome и ChromeDriver в контейнере отладки
USER root
RUN apt-get update && apt-get install -y wget curl unzip libglib2.0-0 libnss3 libgconf-2-4 libfontconfig1
RUN wget -O /tmp/chromedriver.zip https://chromedriver.storage.googleapis.com/94.0.4606.41/chromedriver_linux64.zip
RUN unzip /tmp/chromedriver.zip -d /usr/local/bin/
RUN chmod +x /usr/local/bin/chromedriver

# Установка Chrome
RUN curl -sSL https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb -o /tmp/chrome.deb
RUN dpkg -i /tmp/chrome.deb || apt-get install -y --fix-broken
RUN apt-get install -y xvfb

# Порт для отладки
EXPOSE 1312

# Запуск приложения в режиме отладки
ENTRYPOINT ["dotnet", "ApplicationAuth.dll"]