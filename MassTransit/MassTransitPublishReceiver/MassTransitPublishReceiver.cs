using MassTransit;
using MassTransitPublishReceiver;
using Microsoft.Extensions.DependencyInjection;

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

            config.ReceiveEndpoint("update_order", e => e.Consumer<UpdateOrderConsumer>());
            config.ReceiveEndpoint("order_updated", e => e.Consumer<OrderUpdatedConsumer>());
            config.ReceiveEndpoint("order_updated_email", e => e.Consumer<OrderUpdatedEmailNotificationConsumer>());
        }));
    })
    .BuildServiceProvider();

var busControl = serviceProvider.GetRequiredService<IBusControl>();
var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

await busControl.StartAsync(source.Token);

try
{
    Console.WriteLine("Press enter to exit");
    await Task.Run(Console.ReadLine);
}
finally
{
    await busControl.StopAsync();
}