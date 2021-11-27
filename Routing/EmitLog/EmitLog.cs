using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);

    var serverity = GetSeverity();
    var message = GetMessage();
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(
        exchange: "direct_logs",
        routingKey: serverity,
        basicProperties: null,
        body: body);

    Console.ForegroundColor = GetConsoleColor(serverity);
    Console.WriteLine(" [x] Sent {0}: {1}", serverity, message);
    Console.ResetColor();
}

Console.WriteLine(" [*] End");

string GetSeverity()
{
    return args.Length > 0
        ? args[0].ToUpper()
        : throw new ArgumentException("EmitLog need severity at first args: Info, Warning or Error");
}

string GetMessage()
{
    return args.Length > 1
        ? string.Join(" ", args.Skip(1).ToArray())
        : throw new ArgumentException("EmitLog need message at second args");
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