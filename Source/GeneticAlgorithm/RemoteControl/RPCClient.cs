using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.RemoteControl
{
    public class RPCClient
    {
        private IConnection connection;
        private IModel channel;
        private string replyQueueName;
        private QueueingBasicConsumer consumer;
        private string QueueName;

        public RPCClient(string QueueName, string Host = "localhost", int Port = 5672, string Username = "", string Password = "", string AMQPURL = "")
        {
            this.QueueName = QueueName;

            ConnectionFactory factory = null;

            if (AMQPURL == null || AMQPURL == "")
            {
                factory = new ConnectionFactory()
                {
                    HostName = Host,
                    Port = Port,
                };
                if (Username != "")
                {
                    factory.UserName = Username;
                }

                if (Password != "")
                {
                    factory.Password = Password;
                }
            } else
            {
                factory = new ConnectionFactory()
                {
                    Uri = AMQPURL,
                };
            }

            factory.RequestedHeartbeat = 30;
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queue: replyQueueName,
                                 noAck: true,
                                 consumer: consumer);
        }

        public string CallRunGame(string message)
        {
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = corrId;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "",
                                 routingKey: QueueName,
                                 basicProperties: props,
                                 body: messageBytes);

            while (true)
            {
                var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                if (ea.BasicProperties.CorrelationId == corrId)
                {
                    return Encoding.UTF8.GetString(ea.Body);
                }
            }
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
