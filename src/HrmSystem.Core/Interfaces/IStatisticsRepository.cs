using System.Collections.Generic;
using System.Threading.Tasks;
using HrmSystem.Core.Dtos;

namespace HrmSystem.Core.Interfaces
{
    public interface IStatisticsRepository
    {
        // Aggregation: Group by department, calculate count, avg salary, min, max, total payroll
        Task<List<DepartmentStatDto>> GetDepartmentStatsAsync();

        // Aggregation: $unwind on $skills, $group by skill, $count, $sort desc
        Task<List<SkillDistributionDto>> GetSkillDistributionAsync(int limit = 10);

        // Aggregation: Group projects by status
        Task<List<ProjectStatusStatDto>> GetProjectStatusStatsAsync();

        // Aggregation: Group employees by position
        Task<List<SalaryByPositionDto>> GetSalaryByPositionStatsAsync();

        // High-level KPI summary for Dashboard
        Task<DashboardOverviewDto> GetDashboardOverviewAsync();
    }
}
