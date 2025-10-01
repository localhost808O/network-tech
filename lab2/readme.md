````markdown
# 2 Лабораторная работа по сетевым технологиям

## Установка и проверка .NET
```bash
linux
sudo apt install dotnet-sdk-8.0
dotnet --version 

macos
brew install dotnet
dotnet --version   # должно быть .NET 8.0 SDK или выше
````

## Клонирование проекта

```bash
git clone https://github.com/localhost808O/network-tech.git
cd network-tech
```

---

## Сервер

1. Измените IP и порт в файле:
   **`Server/Server/Utils/Configs/ServerConfig.json`**

2. Запуск сервера:

```bash
cd Server
dotnet run
```

---

## Клиент

1. Измените IP и порт в файле:
   **`Client/Client/Property/Config.json`**
2. Запуск клиента:

```bash
cd Client
dotnet run
```

