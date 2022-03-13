using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Services.RabbitMQ
{
    public class RabbitMQClientService:IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "EmailSenderExchange";
        public static string RountingWatermark = "route-email-sender";
        public static string QueueName = "queue-email-sender";
        private readonly ILogger<RabbitMQClientService> _logger;
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }
        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();
            if (_channel is { IsOpen: true })
                return _channel;
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true, false);
            _channel.QueueDeclare(QueueName, true, false, false);
            _channel.QueueBind(QueueName, ExchangeName, RountingWatermark);
            _logger.LogInformation("Connection to RabbitMQ");
            return _channel;
        }
        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();
            _logger.LogInformation("Lost connection to RabbitMQ.");
        }
    }
}