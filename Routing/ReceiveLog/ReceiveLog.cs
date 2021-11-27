using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);

var queueName = channel.QueueDeclare().QueueName;
var severities = GetSeverities();

foreach (var severity in severities)
{
    channel.QueueBind(
        queue: queueName,
        exchange: "direct_logs",
        routingKey: severity);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(" [*] Waiting for: {0}", string.Join(", ", severities));
Console.ResetColor();

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    var severity = ea.RoutingKey;

    Console.ForegroundColor = GetConsoleColor(severity);
    Console.WriteLine(" [x] {0}: {1}", severity, message);
    Console.ResetColor();
};

channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

string[] GetSeverities()
{
    return args.Length > 1
        ? args.Select(i => i.ToUpper()).ToArray()
        : throw new ArgumentException("Usage Args: [info] [warning] [error]");
}

ConsoleColor GetConsoleColor(string severity)
{
    return severity switch
    {
        "ERROR" => ConsoleColor.Red,
        "WARNING" => ConsoleColor.Yellow,
        _ => ConsoleColor.Blue
    };
}