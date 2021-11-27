using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

    var queueName = channel.QueueDeclare().QueueName;
    channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "");

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(" [*] Waiting for logs.");
    Console.ResetColor();

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (sender, ea) =>
    {
        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
        Console.ForegroundColor = ConsoleColor.Blue;
        
        Console.WriteLine(" [x] {0}", message);
        Console.ResetColor();
    };

    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}