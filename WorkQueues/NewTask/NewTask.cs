using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: "task_queue",
    durable: true,
    exclusive: false,
    autoDelete: false);

foreach (var message in GetMessages())
{
    var body = Encoding.UTF8.GetBytes(message);
    
    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;

    channel.BasicPublish(
        exchange: "",
        routingKey: "task_queue",
        basicProperties: properties,
        body: body);
}

IEnumerable<string> GetMessages()
{
    return args.Length > 0
        ? args
        : throw new ArgumentException("New Task need a message args");
}