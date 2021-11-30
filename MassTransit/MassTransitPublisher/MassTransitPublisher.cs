// See https://aka.ms/new-console-template for more information

using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Shared;

var serviceProvider = new ServiceCollection()
    .AddMassTransit(x =>
    {
        x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(config =>
        {
            config.Host(new Uri("rabbitmq://localhost"), h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }));
    })
    .BuildServiceProvider();

EndpointConvention.Map<UpdateOrder>(new Uri("queue:update_order"));

var guid = Guid.NewGuid().ToString();

var bus = serviceProvider.GetRequiredService<IBus>();

var orderUpdate = new UpdateOrder(guid);
await bus.Send(orderUpdate);

var orderUpdated = new OrderUpdated(guid);
await bus.Publish(orderUpdated);

Console.WriteLine($"Send: {orderUpdate}");
Console.WriteLine($"Publish: {orderUpdated}");