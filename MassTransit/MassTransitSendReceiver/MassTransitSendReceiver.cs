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

            config.ReceiveEndpoint("order_update",
                e => e.Handler<UpdateOrder>(async a => await Console.Out.WriteLineAsync(a.Message.ToString())));
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