using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);

    var topicKey = GetTopicKey();
    var message = GetMessage();
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(
        exchange: "topic_logs",
        routingKey: topicKey,
        basicProperties: null,
        body: body);

    Console.ForegroundColor = GetConsoleColor(topicKey);
    Console.WriteLine(" [x] Sent {0}: {1}", topicKey, message);
    Console.ResetColor();
}

Console.WriteLine(" [*] End");

string GetTopicKey()
{
    return args.Length > 0
        ? args[0].ToLower()
        : throw new ArgumentException(
            "Need the topic at first args, ex: info.auth warn.network error.payment");
}

string GetMessage()
{
    return args.Length > 1
        ? string.Join(" ", args.Skip(1).ToArray())
        : throw new ArgumentException("Need the message at second args");
}

ConsoleColor GetConsoleColor(string topic)
{
    return topic.Split(".")[0] switch
    {
        "error" => ConsoleColor.Red,
        "warn" => ConsoleColor.Yellow,
        _ => ConsoleColor.Blue
    };
}