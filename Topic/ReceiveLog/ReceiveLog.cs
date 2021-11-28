using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);

var queueName = channel.QueueDeclare().QueueName;
var bindingTopicKeys = GetTopicKeys();

foreach (var bindingTopKey in bindingTopicKeys)
{
    channel.QueueBind(
        queue: queueName,
        exchange: "topic_logs",
        routingKey: bindingTopKey);
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine(" [*] Waiting for topic keys: {0}", string.Join(", ", bindingTopicKeys));
Console.ResetColor();

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    var topicKey = ea.RoutingKey;

    Console.ForegroundColor = GetConsoleColor(topicKey);
    Console.WriteLine(" [x] {0}: {1}", topicKey, message);
    Console.ResetColor();
};

channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

string[] GetTopicKeys()
{
    return args.Length > 1
        ? args.Select(i => i.ToLower()).ToArray()
        : throw new ArgumentException("Usage args ex: info.auth warn.* error.*");
}

ConsoleColor GetConsoleColor(string topicKey)
{
    return topicKey.Split(".")[0] switch
    {
        "error" => ConsoleColor.Red,
        "warn" => ConsoleColor.Yellow,
        "info" => ConsoleColor.Blue,
        _ => ConsoleColor.DarkBlue
    };
}