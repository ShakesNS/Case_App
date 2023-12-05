using Case_Service.Context;
using Case_Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Case_Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConnection _rabbitMQConnection;
        private readonly IModel _rabbitMQChannel;
        private readonly ServiceDbContext _context;

        public Worker(ILogger<Worker> logger, ServiceDbContext context)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _rabbitMQConnection = factory.CreateConnection();
            _rabbitMQChannel = _rabbitMQConnection.CreateModel();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                for (int i = 0; i < 10; i++)
                {
                    string word = "";
                    var list = _context.Words.ToList();
                    if(list.Count != 0 && list!=null)
                    {
                        var check = true;

                        while (check)
                        {
                            word = GenerateRandomWord();
                            var hasWord = list.Where(w => w.Content == word).Any();
                            if (hasWord)
                                continue;
                            else
                            {
                                check=false;
                            }
                        }
                    }
                    else
                        word= GenerateRandomWord();
                    
                    
                    SendMessageToRabbitMQ(word);
                    SaveToDatabase(word);
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }


        private static Dictionary<char, int> FindLetterCounts(string word)
        {
            Dictionary<char, int> lettCounts = new Dictionary<char, int>();

            foreach (char harf in word.ToLower())
            {
                if (lettCounts.ContainsKey(harf))
                {
                    lettCounts[harf]++;
                }
                else
                {
                    lettCounts[harf] = 1;
                }
            }

            return lettCounts;
        }
        private void SaveToDatabase(string word)
        {
            Dictionary<char, int> lettCounts = FindLetterCounts(word);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var kvp in lettCounts)
            {
                stringBuilder.Append($"{kvp.Key}: {kvp.Value}, ");
            }

            var letterCounter = stringBuilder.ToString().TrimEnd(',');
            if (letterCounter.EndsWith(", "))
            {
                letterCounter = letterCounter.Substring(0, letterCounter.Length - 2);
            }
            _context.Words.Add(new Word { Content = word,LetterCounter= letterCounter, CreatedAt = DateTime.UtcNow });
            _context.SaveChanges();

            Console.WriteLine($"[x] Word saved to the database: {word}");
        }

        private string GenerateRandomWord()
        {
            Random random = new Random();
            string word = "";

            char firstCharacter = (char)random.Next('A', 'Z' + 1);
            word += firstCharacter;

            for (int i = 0; i < 9; i++)
            {
                char character = (char)random.Next('A', 'Z' + 1);
                word += i % 2 == 0 ? char.ToLower(character) : char.ToUpper(character);
            }

            return word;
        }

        private void SendMessageToRabbitMQ(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _rabbitMQChannel.BasicPublish(exchange: "",
                                          routingKey: "word_queue",
                                          basicProperties: null,
                                          body: body);

            _logger.LogInformation($"[x] Sent message to RabbitMQ: {message}");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _rabbitMQChannel.Close();
            _rabbitMQConnection.Close();
            _context.Dispose();
            await base.StopAsync(cancellationToken);
        }

    }
}