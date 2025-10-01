Проект для лабораторных работ по сетевым технологиям.

brew install dotnet
dotnet --version (проверить что .NET 8.0 SDK или выше)

git clone https://github.com/localhost808O/network-tech.git
cd network-tech

Сервер
(Изменить ip port на свой в Server/Server/Utils/Configs/ServerConfig.json)
cd Server
dotnet run

Клиент
(Изменить ip port на свой в Client/Client/Property/Config.json)
cd Client
dotnet run
