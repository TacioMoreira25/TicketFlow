var builder = DistributedApplication.CreateBuilder(args);

var mysql = builder.AddMySql("mysql-server")
                   .WithDataVolume()
                   .AddDatabase("ticketflow-db");

var redis = builder.AddRedis("redis-cache");

var rabbitmq = builder.AddRabbitMQ("rabbitmq-bus");

builder.AddProject<Projects.TicketFlow_Api>("api")
       .WithReference(mysql)
       .WithReference(redis)
       .WithReference(rabbitmq);

builder.Build().Run();