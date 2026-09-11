using System;
using HrmSystem.Core.Interfaces;
using HrmSystem.Infrastructure.Configuration;
using HrmSystem.Infrastructure.Context;
using HrmSystem.Infrastructure.Repositories;
using HrmSystem.Infrastructure.Seeder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrmSystem.Infrastructure
{
    public interface IDatabaseProviderInfo
    {
        string CurrentProvider { get; }
        string DatabaseName { get; }
        string Description { get; }
        string[] SupportedProviders { get; }
    }

    public class DatabaseProviderInfo : IDatabaseProviderInfo
    {
        public string CurrentProvider { get; set; } = "MongoDB";
        public string DatabaseName { get; set; } = "HrmDb_PhamThanhHuy_2001230304";
        public string Description { get; set; } = "MongoDB Document Database với Aggregation Pipeline, Embedded Documents, Arrays và Index tối ưu.";
        public string[] SupportedProviders => new[] { "MongoDB", "Cassandra", "Neo4j", "Redis" };
    }

    public static class DependencyInjection
    {
        public static IServiceCollection AddHrmInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration["DatabaseSettings:Provider"] ?? "MongoDB";
            var dbName = configuration["DatabaseSettings:DatabaseName"] ?? "HrmDb_PhamThanhHuy_2001230304";

            // Đăng ký thông tin Provider để hiển thị trực tiếp trên giao diện và header
            services.AddSingleton<IDatabaseProviderInfo>(new DatabaseProviderInfo
            {
                CurrentProvider = provider,
                DatabaseName = dbName,
                Description = provider.Equals("MongoDB", StringComparison.OrdinalIgnoreCase)
                    ? "MongoDB Document Database (Collections: departments, employees, projects) kèm Aggregation Pipeline & Indexes."
                    : $"Nhà cung cấp {provider} đã sẵn sàng cắm vào hệ thống theo Repository Pattern."
            });

            switch (provider.ToUpperInvariant())
            {
                case "MONGODB":
                    services.Configure<MongoDbSettings>(configuration.GetSection("DatabaseSettings"));
                    services.AddSingleton<MongoDbContext>();
                    services.AddSingleton<MongoIndexInitializer>();
                    services.AddSingleton<MongoDbDataSeeder>();

                    services.AddScoped<IEmployeeRepository, MongoEmployeeRepository>();
                    services.AddScoped<IDepartmentRepository, MongoDepartmentRepository>();
                    services.AddScoped<IProjectRepository, MongoProjectRepository>();
                    services.AddScoped<IStatisticsRepository, MongoStatisticsRepository>();
                    break;

                case "CASSANDRA":
                    // Sẵn sàng mở rộng: Chỉ cần thêm CassandraEmployeeRepository : IEmployeeRepository
                    throw new NotImplementedException("Provider Cassandra đã được khai báo theo Repository Pattern. Vui lòng cung cấp CassandraRepository implementation.");

                case "NEO4J":
                    // Sẵn sàng mở rộng: Chỉ cần thêm Neo4jEmployeeRepository : IEmployeeRepository
                    throw new NotImplementedException("Provider Neo4j đã được khai báo theo Repository Pattern. Vui lòng cung cấp Neo4jRepository implementation.");

                case "REDIS":
                    // Sẵn sàng mở rộng: Chỉ cần thêm RedisEmployeeRepository : IEmployeeRepository
                    throw new NotImplementedException("Provider Redis đã được khai báo theo Repository Pattern. Vui lòng cung cấp RedisRepository implementation.");

                default:
                    throw new NotSupportedException($"Database Provider '{provider}' không được hỗ trợ.");
            }

            return services;
        }
    }
}
