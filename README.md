## Web File Manager
---
*  __Локальный файловый менеджер работающий через интернет__ 
*  __Создаёт мост для скачивания файлов путём передачи файла через контроллер__

Для использования должен быть установлен  [*ASP NET Core* Runtime v6.0](https://dotnet.microsoft.com/en-us/download) 

В проекте есть файл с директорией поиска __sharedirectory.txt__
Если файла не будет, по-умолчанию будет присвоена директория _C:\\_

```csharp
var execute = Assembly.GetExecutingAssembly().Location;
execute = execute.Substring(0, execute.LastIndexOf("\\"));
try { path = File.ReadAllText(execute + @"\sharedirectory.txt"); }
catch (Exception ex) { Console.WriteLine(ex.Message); path = "C:\\"; }
```


Так же можно обработать и сетевые пути UNC по типу __\\\\myserver\\work__

Внимание!!! НА ДАННЫЙ МОМЕНТ Менеджер не поддерживает исключения папок, такие как _"нету прав"_ или _"системная папка"_ 

Поддержка мобильной версии.

В папках с исполняемым файлом есть пример файла конфигурации __appsettings_example.json__

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Error",
      "Microsoft": "Warning"
    },
    "Debug": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.Hosting": "Trace"
      }
    },
    "EventSource": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  },
  "_comment1": "ПРИМЕР ФАЙЛА КОНФИГУРАЦИИ",
	"_comment2": "Для локального запуска используйте IPv4 локальной сети (пример -> 192.168.0.10:9999)",
	"_comment3": "Для публичного доступа используйте IPv4 0.0.0.0:9999",
	"_comment4": "Сервер будет доступен по внешнему IP адресу, а так же по локальной сети",
  "Kestrel": {
    "EndPoints": {
		"Http": {
        "Url": "http://0.0.0.0:9999"
      }
    }
  },
  "Console": {
    "IncludeScopes": true,
    "LogLevel": {
      "Microsoft.AspNetCore.Mvc.Razor.Internal": "Warning",
      "Microsoft.AspNetCore.Mvc.Razor.Razor": "Debug",
      "Microsoft.AspNetCore.Mvc.Razor": "Error",
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

для IIS настраивается через оснастку упарвления IIS
![Result!](CompanyFileManager/src/IIS.png "Results")

[![License](https://img.shields.io/github/license/anomal3/WebFileManager)](https://sditsoft.ru/%D0%BF%D0%BE%D0%BB%D1%8C%D0%B7%D0%BE%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8C%D1%81%D0%BA%D0%BE%D0%B5-%D1%81%D0%BE%D0%B3%D0%BB%D0%B0%D1%88%D0%B5%D0%BD%D0%B8%D0%B5/)

Visual Studio support

| 2022 (17.4.3) | 2022 (17.2.1) | 2019 (16.6)|
| :----: | :----: | :----------------: |
|  :heavy_check_mark:   |  :x:   | :x: |

<details>
  <summary>Скриншоты интерфейса программы</summary>
  
 ![Result!](CompanyFileManager/src/WebManager1.png "Results")
 
 ![Result!](CompanyFileManager/src/WebManager2.png "Results")
 
 ![Result!](CompanyFileManager/src/WebManager3.png "Results")
 
 ![Result!](CompanyFileManager/src/WebManager4.jpg "Results")
 
</details>

---

P.S. Проект построен с помощью фреймворка DevExpress 2022.3. Без него у Вас не будет пакетов Nuget что приведёт к ошибкам
