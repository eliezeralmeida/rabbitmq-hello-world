using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

    var message = GetMessage();
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "logs", routingKey: "", basicProperties: null, body: body);

    Console.WriteLine(" [x] Sent {0}", message);
}

Console.WriteLine(" [*] End");

string GetMessage()
{
    return args.Length > 0
        ? string.Join(" ", args)
        : throw new ArgumentException("EmitLog need message in args");
}