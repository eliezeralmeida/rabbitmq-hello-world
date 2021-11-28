using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientRpc;

public class RpcClientGithubExample
{
    private const string QUEUE_NAME = "rpc_queue";

    private readonly IModel _channel;
    private readonly string _replyQueueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _callbackMapper = new();

    public RpcClientGithubExample()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            RequestedConnectionTimeout = TimeSpan.FromSeconds(10)
        };

        var connection = factory.CreateConnection();

        _channel = connection.CreateModel();
        _replyQueueName = _channel.QueueDeclare(queue: "").QueueName;

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += ConsumerOnReceived;

        _channel.BasicConsume(
            consumer: consumer,
            queue: _replyQueueName,
            autoAck: true);
    }

    private void ConsumerOnReceived(object model, BasicDeliverEventArgs ea)
    {
        if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs)) return;
        var body = ea.Body.ToArray();
        var response = Encoding.UTF8.GetString(body);
        tcs.TrySetResult(response);
    }

    public Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
    {
        var props = _channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = _replyQueueName;

        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tcs = new TaskCompletionSource<string>();
        _callbackMapper.TryAdd(correlationId, tcs);

        _channel.BasicPublish(
            exchange: "",
            routingKey: QUEUE_NAME,
            basicProperties: props,
            body: messageBytes);

        cancellationToken.Register(() => _callbackMapper.TryRemove(correlationId, out var tmp));
        return tcs.Task;
    }

    public void Close()
    {
        _channel?.Close();
    }
}