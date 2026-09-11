using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;
using HrmSystem.Core.Interfaces;
using HrmSystem.Infrastructure.Context;
using MongoDB.Driver;

namespace HrmSystem.Infrastructure.Repositories
{
    public class MongoProjectRepository : IProjectRepository
    {
        private readonly MongoDbContext _context;

        public MongoProjectRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllAsync()
        {
            return await _context.Projects.Find(_ => true)
                .SortByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(string id)
        {
            return await _context.Projects.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Project?> GetByCodeAsync(string code)
        {
            return await _context.Projects.Find(p => p.Code == code).FirstOrDefaultAsync();
        }

        public async Task<List<Project>> GetByDepartmentAsync(string departmentId)
        {
            return await _context.Projects.Find(p => p.DepartmentId == departmentId).ToListAsync();
        }

        public async Task<Project> CreateAsync(Project project)
        {
            project.CreatedAt = DateTime.UtcNow;
            await _context.Projects.InsertOneAsync(project);
            return project;
        }

        public async Task<bool> UpdateAsync(Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            var result = await _context.Projects.ReplaceOneAsync(p => p.Id == project.Id, project);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _context.Projects.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> AddMemberAsync(string projectId, ProjectMember member)
        {
            // Kiểm tra xem thành viên đã có trong dự án chưa để tránh trùng lặp
            var filter = Builders<Project>.Filter.And(
                Builders<Project>.Filter.Eq(p => p.Id, projectId),
                Builders<Project>.Filter.ElemMatch(p => p.Members, m => m.EmployeeId == member.EmployeeId)
            );
            var exists = await _context.Projects.Find(filter).AnyAsync();

            if (exists)
            {
                // Cập nhật thông tin thành viên hiện có
                var updateExisting = Builders<Project>.Update
                    .Set("members.$.role", member.Role)
                    .Set("members.$.allocatedHoursPerWeek", member.AllocatedHoursPerWeek);
                var res = await _context.Projects.UpdateOneAsync(filter, updateExisting);
                return res.ModifiedCount > 0;
            }
            else
            {
                // Push thành viên mới vào mảng members
                var update = Builders<Project>.Update.Push(p => p.Members, member);
                var res = await _context.Projects.UpdateOneAsync(p => p.Id == projectId, update);
                return res.ModifiedCount > 0;
            }
        }

        public async Task<bool> RemoveMemberAsync(string projectId, string employeeId)
        {
            var update = Builders<Project>.Update.PullFilter(p => p.Members, m => m.EmployeeId == employeeId);
            var result = await _context.Projects.UpdateOneAsync(p => p.Id == projectId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> AddMilestoneAsync(string projectId, ProjectMilestone milestone)
        {
            var update = Builders<Project>.Update.Push(p => p.Milestones, milestone);
            var result = await _context.Projects.UpdateOneAsync(p => p.Id == projectId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ToggleMilestoneStatusAsync(string projectId, string milestoneTitle, bool isCompleted)
        {
            var filter = Builders<Project>.Filter.And(
                Builders<Project>.Filter.Eq(p => p.Id, projectId),
                Builders<Project>.Filter.ElemMatch(p => p.Milestones, m => m.Title == milestoneTitle)
            );
            var update = Builders<Project>.Update.Set("milestones.$.isCompleted", isCompleted);
            var result = await _context.Projects.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _context.Projects.CountDocumentsAsync(_ => true);
        }
    }
}
