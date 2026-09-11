using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;
using HrmSystem.Infrastructure.Context;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HrmSystem.Infrastructure.Configuration
{
    public class CollectionIndexInfo
    {
        public string CollectionName { get; set; } = string.Empty;
        public string IndexName { get; set; } = string.Empty;
        public string Keys { get; set; } = string.Empty;
        public bool IsUnique { get; set; }
    }

    public class MongoIndexInitializer
    {
        private readonly MongoDbContext _context;
        private readonly ILogger<MongoIndexInitializer> _logger;

        public MongoIndexInitializer(MongoDbContext context, ILogger<MongoIndexInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeIndexesAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu khởi tạo các MongoDB Indexes theo đúng đặc tả...");

                // 1. Indexes cho Collection 'departments'
                var deptCodeIndex = new CreateIndexModel<Department>(
                    Builders<Department>.IndexKeys.Ascending(d => d.Code),
                    new CreateIndexOptions { Unique = true, Name = "idx_departments_code_unique" }
                );
                var deptNameIndex = new CreateIndexModel<Department>(
                    Builders<Department>.IndexKeys.Ascending(d => d.Name),
                    new CreateIndexOptions { Name = "idx_departments_name" }
                );
                await _context.Departments.Indexes.CreateManyAsync(new[] { deptCodeIndex, deptNameIndex });

                // 2. Indexes cho Collection 'employees'
                var empCodeIndex = new CreateIndexModel<Employee>(
                    Builders<Employee>.IndexKeys.Ascending(e => e.EmployeeCode),
                    new CreateIndexOptions { Unique = true, Name = "idx_employees_code_unique" }
                );
                var empEmailIndex = new CreateIndexModel<Employee>(
                    Builders<Employee>.IndexKeys.Ascending(e => e.Email),
                    new CreateIndexOptions { Unique = true, Name = "idx_employees_email_unique" }
                );
                // Compound Index: (departmentId, status)
                var empCompoundIndex = new CreateIndexModel<Employee>(
                    Builders<Employee>.IndexKeys.Ascending(e => e.DepartmentId).Ascending(e => e.Status),
                    new CreateIndexOptions { Name = "idx_employees_dept_status_compound" }
                );
                // Multikey Index: skills array
                var empSkillsIndex = new CreateIndexModel<Employee>(
                    Builders<Employee>.IndexKeys.Ascending(e => e.Skills),
                    new CreateIndexOptions { Name = "idx_employees_skills_multikey" }
                );
                // Multikey Index: projectIds array
                var empProjectsIndex = new CreateIndexModel<Employee>(
                    Builders<Employee>.IndexKeys.Ascending(e => e.ProjectIds),
                    new CreateIndexOptions { Name = "idx_employees_projectids_multikey" }
                );
                // Text Index: Full text search trên Họ tên, Chức vụ, Kỹ năng
                var empTextIndex = new CreateIndexModel<Employee>(
                    Builders<Employee>.IndexKeys
                        .Text(e => e.FullName)
                        .Text(e => e.Position)
                        .Text(e => e.Skills),
                    new CreateIndexOptions { Name = "idx_employees_text_search" }
                );

                await _context.Employees.Indexes.CreateManyAsync(new[]
                {
                    empCodeIndex,
                    empEmailIndex,
                    empCompoundIndex,
                    empSkillsIndex,
                    empProjectsIndex,
                    empTextIndex
                });

                // 3. Indexes cho Collection 'projects'
                var projCodeIndex = new CreateIndexModel<Project>(
                    Builders<Project>.IndexKeys.Ascending(p => p.Code),
                    new CreateIndexOptions { Unique = true, Name = "idx_projects_code_unique" }
                );
                // Compound Index: (departmentId, status)
                var projCompoundIndex = new CreateIndexModel<Project>(
                    Builders<Project>.IndexKeys.Ascending(p => p.DepartmentId).Ascending(p => p.Status),
                    new CreateIndexOptions { Name = "idx_projects_dept_status_compound" }
                );
                // Multikey Index trên mảng lồng nhau members.employeeId
                var projMemberEmpIndex = new CreateIndexModel<Project>(
                    Builders<Project>.IndexKeys.Ascending("members.employeeId"),
                    new CreateIndexOptions { Name = "idx_projects_members_empid_multikey" }
                );

                await _context.Projects.Indexes.CreateManyAsync(new[]
                {
                    projCodeIndex,
                    projCompoundIndex,
                    projMemberEmpIndex
                });

                _logger.LogInformation("Khởi tạo MongoDB Indexes thành công hoàn toàn!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình khởi tạo MongoDB Indexes");
            }
        }

        public async Task<List<CollectionIndexInfo>> GetAllIndexDetailsAsync()
        {
            var result = new List<CollectionIndexInfo>();
            try
            {
                // Departments
                using (var cursor = await _context.Departments.Indexes.ListAsync())
                {
                    var indexes = await cursor.ToListAsync();
                    foreach (var idx in indexes)
                    {
                        result.Add(new CollectionIndexInfo
                        {
                            CollectionName = "departments",
                            IndexName = idx.GetValue("name", "").AsString,
                            Keys = idx.GetValue("key", new BsonDocument()).ToJson(),
                            IsUnique = idx.Contains("unique") && idx["unique"].AsBoolean
                        });
                    }
                }

                // Employees
                using (var cursor = await _context.Employees.Indexes.ListAsync())
                {
                    var indexes = await cursor.ToListAsync();
                    foreach (var idx in indexes)
                    {
                        result.Add(new CollectionIndexInfo
                        {
                            CollectionName = "employees",
                            IndexName = idx.GetValue("name", "").AsString,
                            Keys = idx.GetValue("key", new BsonDocument()).ToJson(),
                            IsUnique = idx.Contains("unique") && idx["unique"].AsBoolean
                        });
                    }
                }

                // Projects
                using (var cursor = await _context.Projects.Indexes.ListAsync())
                {
                    var indexes = await cursor.ToListAsync();
                    foreach (var idx in indexes)
                    {
                        result.Add(new CollectionIndexInfo
                        {
                            CollectionName = "projects",
                            IndexName = idx.GetValue("name", "").AsString,
                            Keys = idx.GetValue("key", new BsonDocument()).ToJson(),
                            IsUnique = idx.Contains("unique") && idx["unique"].AsBoolean
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đọc danh sách Indexes");
            }

            return result;
        }
    }
}
