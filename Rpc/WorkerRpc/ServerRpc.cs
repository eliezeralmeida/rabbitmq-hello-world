using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "rpc_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
channel.BasicQos(0, 1, false);

Console.WriteLine(" [x] Awaiting RPC requests");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += ConsumerReceive;

channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

static int CalculateFibbonacci(int n)
{
    if (n is 0 or 1) return n;
    return CalculateFibbonacci(n - 1) + CalculateFibbonacci(n - 2);
}

static void ConsumerReceive(object? sender, BasicDeliverEventArgs ea)
{
    var channel = (sender as EventingBasicConsumer)?.Model ?? throw new ArgumentException();
    var response = string.Empty;

    var body = ea.Body.ToArray();
    var receivedProps = ea.BasicProperties;
    var replyProps = channel.CreateBasicProperties();
    replyProps.CorrelationId = receivedProps.CorrelationId;

    try
    {
        var message = Encoding.UTF8.GetString(body);
        var n = int.Parse(message);

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($" [.] Calculating Fibbonacci for {n}");
        Console.ResetColor();

        response = CalculateFibbonacci(n).ToString();
    }
    catch (Exception e)
    {
        response = string.Empty;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" [.] {e.Message}");
        Console.ResetColor();
    }
    finally
    {
        var responseBytes = Encoding.UTF8.GetBytes(response);
        channel.BasicPublish(
            exchange: "",
            routingKey: receivedProps.ReplyTo,
            basicProperties: replyProps,
            body: responseBytes);

        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
    }
}