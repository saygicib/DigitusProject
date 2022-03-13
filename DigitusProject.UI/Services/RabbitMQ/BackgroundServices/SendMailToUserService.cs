using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using DigitusProject.Business.Services.Mail;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Services.RabbitMQ.BackgroundServices
{
    public class SendMailToUserService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private IModel _channel;

        public SendMailToUserService(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var dto = JsonSerializer.Deserialize<UserSendedMail>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                MailHelper.SendMail(dto.Email, dto.MessageTitle,dto.MessageBody);
                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception)
            {

                throw;
            }
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
