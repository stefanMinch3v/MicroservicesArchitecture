# Microservices Architecture
# Architecture of ASP.NET Core Microservices Applications @SoftUni - June 2020

## Project description
The idea behind the project: to use internal System for managing tasks with uploading and downloading files. It is based on Google drive but much simplified.
Every company has department and the Drive is based on that department.

Admin project can:
- add companies
- add departments
- edit users
- see the drive overall statistics

The admin itself cannot be part of the drive in the front end, it has to be a new registered user (username is important).

Drive project basically has all the functionality since folders/files depend on employees and they depend on companies/departments, thats why I decided to keep the logic there and not split it on two projects.

The Gateway project calls statistics and drive for matching the total views and folders for the current user.

Identity project is for login/register.

Notifications project contains only SignalR.

Statistics project is for total files/folders and folder views (on click).

Watchdog project is monitoring all the apps (healthchecks).

### Communication
Messages and projects communication:
Admin project - communicates to Identity, Drive and Statistics with Refit (no messages here).

Drive project - communication goes via messages:
1.Consumer:
- When user is registered, Identity sends message and Drive consumes that message and creates the employee (avoiding 2 step registration on the front end). Then on the angular-client, the user has to select company/department from its profile.

2.Publisher:
- Admin project edits the user then the call goes to Drive and then message is published for EditedUser, Identity consumes that message and updates the user's info. This way we keep both Identity and Drive databases consistent
- On file uploaded message is published so the consumers are Notifications and Statistics
- On file deleted message is published and Statistics is the consumer
- On folder uploaded message is published so the consumers are Notifications and Statistics
- On folder deleted message is published and Statistics is the consumer
- On folder opened (or clicked) message is published and the consumer is Statistics

Identity project:
- just publishes on user registered

### App functionality:
- CRUD for files and folders (as it is on Google drive) + private folders for user.

### Technologies and tools used:
- ASP.NET Core + EF Core 3.1
- DapperORM
- AutoMapper
- Docker
- RabbitMQ
- Healthchecks
- Hangfire
- MassTransit
- Polly
- Swagger
- Angular 8
- Refit
- OpenXml
- JwtBearer
- SignalR
