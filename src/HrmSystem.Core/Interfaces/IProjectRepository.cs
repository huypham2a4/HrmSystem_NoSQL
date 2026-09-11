using System.Collections.Generic;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;

namespace HrmSystem.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(string id);
        Task<Project?> GetByCodeAsync(string code);
        Task<List<Project>> GetByDepartmentAsync(string departmentId);
        Task<Project> CreateAsync(Project project);
        Task<bool> UpdateAsync(Project project);
        Task<bool> DeleteAsync(string id);

        // Subdocument / Array operations
        Task<bool> AddMemberAsync(string projectId, ProjectMember member);
        Task<bool> RemoveMemberAsync(string projectId, string employeeId);
        Task<bool> AddMilestoneAsync(string projectId, ProjectMilestone milestone);
        Task<bool> ToggleMilestoneStatusAsync(string projectId, string milestoneTitle, bool isCompleted);
        Task<long> CountAsync();
    }
}
