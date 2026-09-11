using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HrmSystem.Core.Entities
{
    // Embedded Document bên trong mảng Members của Project
    public class ProjectMember
    {
        [BsonElement("employeeId")]
        public string EmployeeId { get; set; } = string.Empty;

        [BsonElement("employeeCode")]
        [Display(Name = "Mã NV")]
        public string EmployeeCode { get; set; } = string.Empty;

        [BsonElement("fullName")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("role")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = "Thành viên"; // Project Manager, Tech Lead, Developer, QA, Business Analyst

        [BsonElement("joinedDate")]
        [Display(Name = "Ngày tham gia")]
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        [BsonElement("allocatedHoursPerWeek")]
        [Display(Name = "Số giờ/tuần")]
        [Range(1, 60)]
        public int AllocatedHoursPerWeek { get; set; } = 40;
    }

    // Embedded Document bên trong mảng Milestones
    public class ProjectMilestone
    {
        [BsonElement("title")]
        [Display(Name = "Tên cột mốc")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("dueDate")]
        [Display(Name = "Hạn chót")]
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        [BsonElement("isCompleted")]
        [Display(Name = "Đã hoàn thành")]
        public bool IsCompleted { get; set; } = false;
    }

    // Collection: projects
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("code")]
        [Required(ErrorMessage = "Mã dự án là bắt buộc")]
        [Display(Name = "Mã dự án")]
        public string Code { get; set; } = string.Empty;

        [BsonElement("name")]
        [Required(ErrorMessage = "Tên dự án là bắt buộc")]
        [Display(Name = "Tên dự án")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        [Display(Name = "Mô tả dự án")]
        public string? Description { get; set; }

        [BsonElement("departmentId")]
        [Display(Name = "Phòng ban phụ trách")]
        public string DepartmentId { get; set; } = string.Empty;

        [BsonElement("departmentName")]
        [Display(Name = "Tên phòng ban")]
        public string DepartmentName { get; set; } = string.Empty;

        [BsonElement("startDate")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [BsonElement("endDate")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [BsonElement("budget")]
        [BsonRepresentation(BsonType.Decimal128)]
        [Display(Name = "Ngân sách dự án (VNĐ)")]
        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; }

        [BsonElement("status")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Active"; // Planning, Active, Completed, OnHold

        // Array of Embedded Documents: Danh sách thành viên tham gia
        [BsonElement("members")]
        [Display(Name = "Thành viên dự án")]
        public List<ProjectMember> Members { get; set; } = new List<ProjectMember>();

        // Array of Embedded Documents: Các mốc quan trọng
        [BsonElement("milestones")]
        [Display(Name = "Cột mốc dự án")]
        public List<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
