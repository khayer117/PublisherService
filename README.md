# Project: Publisher Service

## Technology Stack 
* ASP.NET Core
* RabbitMQ
* Postgresql
* Entity Framework Code First
* Docker Composer
* Kubernetes(ToDo)

## Prerequisite
* Only Docker

## Run Project:
```shell
> docker-compose build
> docker-compose up
```
## Publish to Rabbitmq:
Hit below url in web browser/curl.
http://localhost:8888/api/po/add

## View Data from Postgres: 
Hit below url in web browser/curl.
http://localhost:8889/api/po