using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using RabbitMQ.Client;

namespace AERReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var input = Console.ReadKey();
                if (input.Key == ConsoleKey.Escape)
                {
                    break;
                }

                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "hello",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

                        string message = CreateMessage();
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                            routingKey: "hello",
                            basicProperties: null,
                            body: body);
                        Console.WriteLine(" [x] Sent {0}", message);
                    }
                }
            }
        }

        private static string CreateMessage()
        {
            Random r = new Random();
            var m = new AERMessage { Id = r.Next().ToString() };
            var r3 = new R3 { Name = "name", StartDate = DateTime.Now, Type = "R3" };
            var r3Xml = ToXml(r3);
            var r3XmlBytes = Encoding.UTF8.GetBytes(r3Xml);
            var r3XmlBase64 = Convert.ToBase64String(r3XmlBytes);
            m.Content = r3XmlBase64;
            var xmlString = ToXml(m);
            return xmlString;
        }

        public static string ToXml<T>(T message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                var settings = new XmlWriterSettings
                {
                    Indent = false,
                    OmitXmlDeclaration = true
                };

                using (var writer = XmlWriter.Create(stream, settings))
                {
                    serializer.Serialize(writer, message);
                    return Encoding.UTF8.GetString(stream.GetBuffer());
                }
            }
        }
    }

    [XmlRoot(ElementName = "AERMessage", Namespace = "http://tempuri.org/")]
    public class AERMessage
    {
        [XmlElement(ElementName = "Content", Namespace = "http://tempuri.org/")]
        public string Content { get; set; }

        [XmlElement(ElementName = "Id", Namespace = "http://tempuri.org/")]
        public string Id { get; set; }
    }

    public class R3
    {
        public string Type { get; set; }

        public DateTime StartDate { get; set; }

        public string Name { get; set; }
    }
}
