using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false);
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    
    try
    {
        Console.WriteLine(" [x] Received Message:  {0}", message);

        var dots = message.Count(i => i == '.');
        Thread.Sleep(dots * 1000);

        if (DateTime.UtcNow.Second % 2 == 1)
            throw new InvalidOperationException("Even second throws error!");

        Console.WriteLine(" [x] Done");
        ((EventingBasicConsumer)sender!).Model.BasicAck(ea.DeliveryTag, false);
    }
    catch (Exception e)
    {
        ((EventingBasicConsumer)sender!).Model.BasicNack(ea.DeliveryTag, false, true);
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(" [x] Task Message: {0}", message);
        Console.WriteLine(" [x] Error Message: {0}", e.Message);
        Console.ResetColor();
    }
};

Console.WriteLine(" Waiting 2 seconds to start");
Thread.Sleep(2000);

channel.BasicConsume(queue: "task_queue", autoAck: false, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadKey();