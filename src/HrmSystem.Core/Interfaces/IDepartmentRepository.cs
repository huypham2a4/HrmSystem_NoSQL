using System.Collections.Generic;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;

namespace HrmSystem.Core.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(string id);
        Task<Department?> GetByCodeAsync(string code);
        Task<Department> CreateAsync(Department department);
        Task<bool> UpdateAsync(Department department);
        Task<bool> DeleteAsync(string id);
        Task<long> CountAsync();
    }
}
