using System.Collections.Generic;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;

namespace HrmSystem.Core.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(string id);
        Task<Employee?> GetByCodeAsync(string code);
        Task<List<Employee>> GetByDepartmentAsync(string departmentId);
        Task<List<Employee>> SearchAsync(string? keyword, string? departmentId, string? skill, string? status);
        Task<Employee> CreateAsync(Employee employee);
        Task<bool> UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(string id);

        // Array operations (Atomic push / pull in MongoDB)
        Task<bool> AddSkillAsync(string employeeId, string skill);
        Task<bool> RemoveSkillAsync(string employeeId, string skill);
        Task<bool> AssignProjectAsync(string employeeId, string projectId);
        Task<bool> UnassignProjectAsync(string employeeId, string projectId);
        Task<long> CountAsync();
    }
}
