using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;
using HrmSystem.Core.Interfaces;
using HrmSystem.Infrastructure.Context;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HrmSystem.Infrastructure.Repositories
{
    public class MongoEmployeeRepository : IEmployeeRepository
    {
        private readonly MongoDbContext _context;

        public MongoEmployeeRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees.Find(_ => true)
                .SortByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _context.Employees.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByCodeAsync(string code)
        {
            return await _context.Employees.Find(e => e.EmployeeCode == code).FirstOrDefaultAsync();
        }

        public async Task<List<Employee>> GetByDepartmentAsync(string departmentId)
        {
            return await _context.Employees.Find(e => e.DepartmentId == departmentId).ToListAsync();
        }

        public async Task<List<Employee>> SearchAsync(string? keyword, string? departmentId, string? skill, string? status)
        {
            var filterBuilder = Builders<Employee>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var escapedKeyword = Regex.Escape(keyword.Trim());
                var regex = new BsonRegularExpression(escapedKeyword, "i");
                var textFilter = filterBuilder.Or(
                    filterBuilder.Regex(e => e.FullName, regex),
                    filterBuilder.Regex(e => e.EmployeeCode, regex),
                    filterBuilder.Regex(e => e.Email, regex),
                    filterBuilder.Regex(e => e.Position, regex),
                    filterBuilder.AnyEq(e => e.Skills, keyword.Trim())
                );
                filter &= textFilter;
            }

            if (!string.IsNullOrWhiteSpace(departmentId))
            {
                filter &= filterBuilder.Eq(e => e.DepartmentId, departmentId);
            }

            if (!string.IsNullOrWhiteSpace(skill))
            {
                filter &= filterBuilder.AnyEq(e => e.Skills, skill);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filter &= filterBuilder.Eq(e => e.Status, status);
            }

            return await _context.Employees.Find(filter)
                .SortByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<Employee> CreateAsync(Employee employee)
        {
            employee.CreatedAt = DateTime.UtcNow;
            await _context.Employees.InsertOneAsync(employee);
            return employee;
        }

        public async Task<bool> UpdateAsync(Employee employee)
        {
            employee.UpdatedAt = DateTime.UtcNow;
            var result = await _context.Employees.ReplaceOneAsync(e => e.Id == employee.Id, employee);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _context.Employees.DeleteOneAsync(e => e.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> AddSkillAsync(string employeeId, string skill)
        {
            var update = Builders<Employee>.Update.AddToSet(e => e.Skills, skill);
            var result = await _context.Employees.UpdateOneAsync(e => e.Id == employeeId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RemoveSkillAsync(string employeeId, string skill)
        {
            var update = Builders<Employee>.Update.Pull(e => e.Skills, skill);
            var result = await _context.Employees.UpdateOneAsync(e => e.Id == employeeId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> AssignProjectAsync(string employeeId, string projectId)
        {
            var update = Builders<Employee>.Update.AddToSet(e => e.ProjectIds, projectId);
            var result = await _context.Employees.UpdateOneAsync(e => e.Id == employeeId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UnassignProjectAsync(string employeeId, string projectId)
        {
            var update = Builders<Employee>.Update.Pull(e => e.ProjectIds, projectId);
            var result = await _context.Employees.UpdateOneAsync(e => e.Id == employeeId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _context.Employees.CountDocumentsAsync(_ => true);
        }
    }
}
