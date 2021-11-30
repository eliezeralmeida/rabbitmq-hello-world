using MassTransit;
using Shared;

namespace MassTransitPublishReceiver;

public class UpdateOrderConsumer : IConsumer<UpdateOrder>
{
    public Task Consume(ConsumeContext<UpdateOrder> context)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($" [x] Update Order: {context.Message.OrderId}");

        return Task.CompletedTask;
    }
}

public class OrderUpdatedConsumer : IConsumer<OrderUpdated>
{
    public Task Consume(ConsumeContext<OrderUpdated> context)
    {
        Thread.Sleep(300);
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($" [x] Order Updated: {context.Message}");

        return Task.CompletedTask;
    }
}

public class OrderUpdatedEmailNotificationConsumer : IConsumer<OrderUpdated>
{
    public Task Consume(ConsumeContext<OrderUpdated> context)
    {
        Thread.Sleep(500);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" [x] Email Sent: {context.Message.OrderId}");

        return Task.CompletedTask;
    }
}