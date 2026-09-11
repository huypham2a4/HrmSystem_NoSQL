using System;
using HrmSystem.Core.Entities;
using HrmSystem.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;

namespace HrmSystem.Infrastructure.Context
{
    public class MongoDbContext : IDisposable
    {
        private static MongoDbRunner? _runner;
        private static readonly object _lock = new object();
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;
        private readonly MongoDbSettings _settings;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IOptions<MongoDbSettings> options, ILogger<MongoDbContext> logger)
        {
            _settings = options.Value;
            _logger = logger;

            string connectionString = _settings.ConnectionString;

            // Kiểm tra khả năng kết nối tới MongoDB được cấu hình
            bool connected = false;
            try
            {
                var testClient = new MongoClient(new MongoClientSettings
                {
                    Server = MongoServerAddress.Parse(GetHostPort(connectionString)),
                    ServerSelectionTimeout = TimeSpan.FromSeconds(2)
                });
                testClient.ListDatabaseNames().ToList();
                connected = true;
                _logger.LogInformation("Đã kết nối thành công tới MongoDB Server tại: {ConnectionString}", connectionString);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Không thể kết nối trực tiếp tới MongoDB Server ({ConnectionString}): {Message}", connectionString, ex.Message);
            }

            if (!connected && _settings.AutoStartEmbeddedServerIfUnavailable)
            {
                lock (_lock)
                {
                    if (_runner == null)
                    {
                        try
                        {
                            _logger.LogInformation("Khởi động Mongo2Go embedded MongoDB instance phục vụ kiểm thử và chấm bài...");
                            _runner = MongoDbRunner.Start(singleNodeReplSet: false);
                            _logger.LogInformation("Mongo2Go khởi động thành công tại: {ConnectionString}", _runner.ConnectionString);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi khởi động Mongo2Go");
                        }
                    }
                }

                if (_runner != null)
                {
                    connectionString = _runner.ConnectionString;
                }
            }

            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
            clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            _client = new MongoClient(clientSettings);
            _database = _client.GetDatabase(_settings.DatabaseName);
        }

        private static string GetHostPort(string connectionString)
        {
            try
            {
                var uri = new Uri(connectionString);
                return $"{uri.Host}:{uri.Port}";
            }
            catch
            {
                return "127.0.0.1:27017";
            }
        }

        public IMongoClient Client => _client;
        public IMongoDatabase Database => _database;

        public IMongoCollection<Department> Departments =>
            _database.GetCollection<Department>(_settings.DepartmentsCollection);

        public IMongoCollection<Employee> Employees =>
            _database.GetCollection<Employee>(_settings.EmployeesCollection);

        public IMongoCollection<Project> Projects =>
            _database.GetCollection<Project>(_settings.ProjectsCollection);

        public void Dispose()
        {
            // Không tắt runner static để các request tiếp theo dùng chung
        }
    }
}
