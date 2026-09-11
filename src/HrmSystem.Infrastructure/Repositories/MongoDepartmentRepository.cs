using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;
using HrmSystem.Core.Interfaces;
using HrmSystem.Infrastructure.Context;
using MongoDB.Driver;

namespace HrmSystem.Infrastructure.Repositories
{
    public class MongoDepartmentRepository : IDepartmentRepository
    {
        private readonly MongoDbContext _context;

        public MongoDepartmentRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _context.Departments.Find(_ => true)
                .SortBy(d => d.Code)
                .ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(string id)
        {
            return await _context.Departments.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Department?> GetByCodeAsync(string code)
        {
            return await _context.Departments.Find(d => d.Code == code).FirstOrDefaultAsync();
        }

        public async Task<Department> CreateAsync(Department department)
        {
            department.CreatedAt = DateTime.UtcNow;
            await _context.Departments.InsertOneAsync(department);
            return department;
        }

        public async Task<bool> UpdateAsync(Department department)
        {
            department.UpdatedAt = DateTime.UtcNow;
            var result = await _context.Departments.ReplaceOneAsync(d => d.Id == department.Id, department);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _context.Departments.DeleteOneAsync(d => d.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _context.Departments.CountDocumentsAsync(_ => true);
        }
    }
}
