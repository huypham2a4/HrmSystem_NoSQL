using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrmSystem.Core.Dtos;
using HrmSystem.Core.Interfaces;
using HrmSystem.Infrastructure.Context;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HrmSystem.Infrastructure.Repositories
{
    public class MongoStatisticsRepository : IStatisticsRepository
    {
        private readonly MongoDbContext _context;

        public MongoStatisticsRepository(MongoDbContext context)
        {
            _context = context;
        }

        private static decimal SafeGetDecimal(BsonDocument doc, string fieldName)
        {
            if (!doc.Contains(fieldName) || doc[fieldName].IsBsonNull) return 0m;
            var val = doc[fieldName];
            if (val.IsDecimal128) return (decimal)val.AsDecimal128;
            if (val.IsDouble) return (decimal)val.AsDouble;
            if (val.IsInt32) return (decimal)val.AsInt32;
            if (val.IsInt64) return (decimal)val.AsInt64;
            return 0m;
        }

        private static int SafeGetInt(BsonDocument doc, string fieldName)
        {
            if (!doc.Contains(fieldName) || doc[fieldName].IsBsonNull) return 0;
            var val = doc[fieldName];
            if (val.IsInt32) return val.AsInt32;
            if (val.IsInt64) return (int)val.AsInt64;
            if (val.IsDouble) return (int)val.AsDouble;
            if (val.IsDecimal128) return (int)val.AsDecimal128;
            return 0;
        }

        // MongoDB Aggregation Pipeline: Thống kê phòng ban (Nhân sự, lương trung bình, min, max, tổng quỹ lương)
        public async Task<List<DepartmentStatDto>> GetDepartmentStatsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "departmentId", "$departmentId" },
                            { "departmentName", "$departmentName" }
                        }
                    },
                    { "employeeCount", new BsonDocument("$sum", 1) },
                    { "averageSalary", new BsonDocument("$avg", "$salary") },
                    { "minSalary", new BsonDocument("$min", "$salary") },
                    { "maxSalary", new BsonDocument("$max", "$salary") },
                    { "totalSalary", new BsonDocument("$sum", "$salary") }
                }),
                new BsonDocument("$sort", new BsonDocument("employeeCount", -1))
            };

            var aggregateResult = await _context.Employees.Aggregate<BsonDocument>(pipeline).ToListAsync();
            var departments = await _context.Departments.Find(_ => true).ToListAsync();
            var deptMap = departments.ToDictionary(d => d.Id ?? "", d => d);

            var stats = new List<DepartmentStatDto>();
            foreach (var doc in aggregateResult)
            {
                var idDoc = doc["_id"].AsBsonDocument;
                string deptId = idDoc.Contains("departmentId") && !idDoc["departmentId"].IsBsonNull ? idDoc["departmentId"].AsString : "";
                string deptName = idDoc.Contains("departmentName") && !idDoc["departmentName"].IsBsonNull ? idDoc["departmentName"].AsString : "Khác";

                decimal budget = 0;
                if (deptMap.TryGetValue(deptId, out var dept))
                {
                    deptName = dept.Name;
                    budget = dept.Budget;
                }

                stats.Add(new DepartmentStatDto
                {
                    DepartmentId = deptId,
                    DepartmentName = deptName,
                    EmployeeCount = SafeGetInt(doc, "employeeCount"),
                    AverageSalary = Math.Round(SafeGetDecimal(doc, "averageSalary"), 0),
                    MinSalary = SafeGetDecimal(doc, "minSalary"),
                    MaxSalary = SafeGetDecimal(doc, "maxSalary"),
                    TotalSalary = SafeGetDecimal(doc, "totalSalary"),
                    Budget = budget
                });
            }

            // Bổ sung các phòng ban chưa có nhân viên nào
            foreach (var dept in departments)
            {
                if (!stats.Any(s => s.DepartmentId == dept.Id))
                {
                    stats.Add(new DepartmentStatDto
                    {
                        DepartmentId = dept.Id ?? "",
                        DepartmentName = dept.Name,
                        EmployeeCount = 0,
                        AverageSalary = 0,
                        MinSalary = 0,
                        MaxSalary = 0,
                        TotalSalary = 0,
                        Budget = dept.Budget
                    });
                }
            }

            return stats;
        }

        // MongoDB Aggregation Pipeline: $unwind -> $group -> $sort -> $limit
        public async Task<List<SkillDistributionDto>> GetSkillDistributionAsync(int limit = 10)
        {
            var totalEmployees = await _context.Employees.CountDocumentsAsync(_ => true);
            if (totalEmployees == 0) totalEmployees = 1;

            var pipeline = new[]
            {
                new BsonDocument("$unwind", "$skills"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$skills" },
                    { "count", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$sort", new BsonDocument("count", -1)),
                new BsonDocument("$limit", limit)
            };

            var results = await _context.Employees.Aggregate<BsonDocument>(pipeline).ToListAsync();
            var list = new List<SkillDistributionDto>();

            foreach (var doc in results)
            {
                string skill = doc["_id"].AsString;
                int count = SafeGetInt(doc, "count");
                list.Add(new SkillDistributionDto
                {
                    Skill = skill,
                    Count = count,
                    Percentage = Math.Round(((double)count / totalEmployees) * 100, 1)
                });
            }

            return list;
        }

        // MongoDB Aggregation Pipeline: Thống kê trạng thái dự án
        public async Task<List<ProjectStatusStatDto>> GetProjectStatusStatsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$status" },
                    { "count", new BsonDocument("$sum", 1) },
                    { "totalBudget", new BsonDocument("$sum", "$budget") },
                    { "totalMembers", new BsonDocument("$sum", new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$members", new BsonArray() }))) }
                }),
                new BsonDocument("$sort", new BsonDocument("count", -1))
            };

            var results = await _context.Projects.Aggregate<BsonDocument>(pipeline).ToListAsync();
            var list = new List<ProjectStatusStatDto>();

            foreach (var doc in results)
            {
                list.Add(new ProjectStatusStatDto
                {
                    Status = doc["_id"].AsString,
                    Count = SafeGetInt(doc, "count"),
                    TotalBudget = SafeGetDecimal(doc, "totalBudget"),
                    TotalMembers = SafeGetInt(doc, "totalMembers")
                });
            }

            return list;
        }

        // MongoDB Aggregation Pipeline: Phân tích mức lương theo chức danh
        public async Task<List<SalaryByPositionDto>> GetSalaryByPositionStatsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$position" },
                    { "employeeCount", new BsonDocument("$sum", 1) },
                    { "averageSalary", new BsonDocument("$avg", "$salary") }
                }),
                new BsonDocument("$sort", new BsonDocument("averageSalary", -1))
            };

            var results = await _context.Employees.Aggregate<BsonDocument>(pipeline).ToListAsync();
            var list = new List<SalaryByPositionDto>();

            foreach (var doc in results)
            {
                list.Add(new SalaryByPositionDto
                {
                    Position = doc["_id"].AsString,
                    EmployeeCount = SafeGetInt(doc, "employeeCount"),
                    AverageSalary = Math.Round(SafeGetDecimal(doc, "averageSalary"), 0)
                });
            }

            return list;
        }

        // Tổng hợp Dashboard Overview
        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
        {
            var totalEmployees = (int)await _context.Employees.CountDocumentsAsync(_ => true);
            var activeEmployees = (int)await _context.Employees.CountDocumentsAsync(e => e.Status == "Active");
            var totalDepartments = (int)await _context.Departments.CountDocumentsAsync(_ => true);
            var totalProjects = (int)await _context.Projects.CountDocumentsAsync(_ => true);
            var activeProjects = (int)await _context.Projects.CountDocumentsAsync(p => p.Status == "Active");

            var deptStats = await GetDepartmentStatsAsync();
            var skillStats = await GetSkillDistributionAsync(8);
            var projectStats = await GetProjectStatusStatsAsync();
            var positionStats = await GetSalaryByPositionStatsAsync();

            decimal totalPayroll = deptStats.Sum(d => d.TotalSalary);
            decimal avgSalary = totalEmployees > 0 ? deptStats.Sum(d => d.TotalSalary) / totalEmployees : 0;
            string topSkill = skillStats.FirstOrDefault()?.Skill ?? "N/A";

            return new DashboardOverviewDto
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                TotalDepartments = totalDepartments,
                TotalProjects = totalProjects,
                ActiveProjects = activeProjects,
                AverageSalary = Math.Round(avgSalary, 0),
                TotalMonthlyPayroll = totalPayroll,
                TopSkill = topSkill,
                DepartmentStats = deptStats,
                TopSkills = skillStats,
                ProjectStats = projectStats,
                PositionStats = positionStats
            };
        }
    }
}
