# AdminService Alpha 0.1
| Master                                                                                                                           | Develop                                                                                                                           |
|----------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| [![Build Status](https://travis-ci.com/SkyMen-Lab/AdminService.svg?branch=master)](https://travis-ci.com/SkyMen-Lab/AdminService)| [![Build Status](https://travis-ci.com/SkyMen-Lab/AdminService.svg?branch=develop)](https://travis-ci.com/SkyMen-Lab/AdminService)|

## Intro 
Admin service is a dashboard for admins and moderators of the project thep0ng.io to manage teams, connections of configs and game session in real time. 

For now, the dashboard is a basic ASP.NET Core MVC application with no database on side. All of the data is pulled from the [Storage Service](https://github.com/SkyMen-Lab/StorageService) via REST API. However, it is in plans to implement SPA on side and have small MongoDB database for storing users with roles.

## Tech Stack: 
- ASP.NET Core 3.1 MVC
- Newtonsoft.JSON 
- REST API
- Serilog

## Setup
### Upstream API Address Setup
Please set the correct ServerAddress in appSettings.json before running the web app.
```
"ServerAddress":{
    "StorageServerAddress": "http://<your-upstream-storage-server>:<port>"
  }
```

## Contribution
All developers are always welcome to contribute to the project and open issues and pull-requests with appropriate messages.

## License
The code is licensed under the GNU General Public License v2.0
