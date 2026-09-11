using System.Collections.Generic;

namespace HrmSystem.Core.Dtos
{
    // Kết quả Aggregation Pipeline thống kê theo phòng ban
    public class DepartmentStatDto
    {
        public string DepartmentId { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal Budget { get; set; }
    }

    // Kết quả Aggregation Pipeline $unwind("skills") -> $group("skills")
    public class SkillDistributionDto
    {
        public string Skill { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    // Kết quả Aggregation Pipeline thống kê dự án theo trạng thái
    public class ProjectStatusStatDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalBudget { get; set; }
        public int TotalMembers { get; set; }
    }

    // Kết quả Aggregation thống kê mức lương theo chức vụ
    public class SalaryByPositionDto
    {
        public string Position { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal AverageSalary { get; set; }
    }

    // Tổng hợp KPI toàn diện trên Dashboard
    public class DashboardOverviewDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal TotalMonthlyPayroll { get; set; }
        public string TopSkill { get; set; } = string.Empty;

        public List<DepartmentStatDto> DepartmentStats { get; set; } = new List<DepartmentStatDto>();
        public List<SkillDistributionDto> TopSkills { get; set; } = new List<SkillDistributionDto>();
        public List<ProjectStatusStatDto> ProjectStats { get; set; } = new List<ProjectStatusStatDto>();
        public List<SalaryByPositionDto> PositionStats { get; set; } = new List<SalaryByPositionDto>();
    }
}
