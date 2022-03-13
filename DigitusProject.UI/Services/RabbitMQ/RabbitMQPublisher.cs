using RabbitMQ.Client;
using DigitusProject.WebUI.Services.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Services.RabbitMQ
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }
        public void Publish(UserSendedMail userSendedMail)
        {
            var channel = _rabbitMQClientService.Connect();
            var bodyString = JsonSerializer.Serialize(userSendedMail);
            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; //Fiziksel olarak kaydetmek için.

            channel.BasicPublish(RabbitMQClientService.ExchangeName, RabbitMQClientService.RountingWatermark,properties,bodyByte);
        }
    }
}
