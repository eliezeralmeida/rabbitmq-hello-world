using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientRpc;

public class RpcClientTutorialExample
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly BlockingCollection<string> _respQueue = new();
    private readonly IBasicProperties _props;

    public RpcClientTutorialExample()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        var replyQueueName = _channel.QueueDeclare().QueueName;

        var consumer = new EventingBasicConsumer(_channel);

        _props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        _props.CorrelationId = correlationId;
        _props.ReplyTo = replyQueueName;

        consumer.Received += (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                _respQueue.Add(response);
            }
        };

        _channel.BasicConsume(
            consumer: consumer,
            queue: replyQueueName,
            autoAck: true);
    }

    public string Call(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: "",
            routingKey: "rpc_queue",
            basicProperties: _props,
            body: messageBytes);

        return _respQueue.Take();
    }

    public void Close()
    {
        _connection.Close();
    }
}