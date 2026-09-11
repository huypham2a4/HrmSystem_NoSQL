using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HrmSystem.Core.Entities
{
    // Embedded Document: Địa chỉ thường trú
    public class Address
    {
        [Display(Name = "Số nhà, tên đường")]
        public string Street { get; set; } = string.Empty;

        [Display(Name = "Phường/Xã")]
        public string Ward { get; set; } = string.Empty;

        [Display(Name = "Quận/Huyện")]
        public string District { get; set; } = string.Empty;

        [Display(Name = "Tỉnh/Thành phố")]
        public string City { get; set; } = "TP. Hồ Chí Minh";

        [Display(Name = "Quốc gia")]
        public string Country { get; set; } = "Việt Nam";

        public override string ToString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Street)) parts.Add(Street);
            if (!string.IsNullOrWhiteSpace(Ward)) parts.Add(Ward);
            if (!string.IsNullOrWhiteSpace(District)) parts.Add(District);
            if (!string.IsNullOrWhiteSpace(City)) parts.Add(City);
            return parts.Count > 0 ? string.Join(", ", parts) : "Chưa cập nhật";
        }
    }

    // Embedded Document: Người liên hệ khẩn cấp
    public class EmergencyContact
    {
        [Display(Name = "Họ tên người thân")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Mối quan hệ")]
        public string Relationship { get; set; } = "Người thân"; // Cha mẹ, Vợ/Chồng, Anh/Chị/Em

        [Display(Name = "Số điện thoại khẩn cấp")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    // Embedded Document bên trong mảng Certifications
    public class Certification
    {
        [Display(Name = "Tên chứng chỉ")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Đơn vị cấp")]
        public string IssuedBy { get; set; } = string.Empty;

        [Display(Name = "Năm cấp")]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [Display(Name = "Mã/Link xác thực")]
        public string? CredentialUrl { get; set; }
    }

    // Collection: employees
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("employeeCode")]
        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        [Display(Name = "Mã nhân viên")]
        public string EmployeeCode { get; set; } = string.Empty;

        [BsonElement("fullName")]
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("email")]
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng")]
        [Display(Name = "Email công việc")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("phoneNumber")]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("position")]
        [Required(ErrorMessage = "Chức vụ là bắt buộc")]
        [Display(Name = "Chức danh / Vị trí")]
        public string Position { get; set; } = "Nhân viên";

        [BsonElement("departmentId")]
        [Required(ErrorMessage = "Vui lòng chọn phòng ban")]
        [Display(Name = "Phòng ban")]
        public string DepartmentId { get; set; } = string.Empty;

        [BsonElement("departmentName")]
        [Display(Name = "Tên phòng ban")]
        public string DepartmentName { get; set; } = string.Empty;

        [BsonElement("salary")]
        [BsonRepresentation(BsonType.Decimal128)]
        [Display(Name = "Mức lương (VNĐ)")]
        [Range(0, double.MaxValue, ErrorMessage = "Mức lương phải lớn hơn hoặc bằng 0")]
        public decimal Salary { get; set; }

        [BsonElement("hireDate")]
        [Display(Name = "Ngày vào làm")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        [Display(Name = "Trạng thái nhân sự")]
        public string Status { get; set; } = "Active"; // Active, Probation, OnLeave, Terminated

        // 1. Embedded Document: Địa chỉ
        [BsonElement("address")]
        [Display(Name = "Địa chỉ thường trú")]
        public Address Address { get; set; } = new Address();

        // 2. Embedded Document: Người liên hệ khẩn cấp
        [BsonElement("emergencyContact")]
        [Display(Name = "Liên hệ khẩn cấp")]
        public EmergencyContact EmergencyContact { get; set; } = new EmergencyContact();

        // 3. Array Field: Kỹ năng chuyên môn
        [BsonElement("skills")]
        [Display(Name = "Kỹ năng chuyên môn")]
        public List<string> Skills { get; set; } = new List<string>();

        // 4. Array Field: Danh sách chứng chỉ (Embedded documents in array)
        [BsonElement("certifications")]
        [Display(Name = "Chứng chỉ chuyên môn")]
        public List<Certification> Certifications { get; set; } = new List<Certification>();

        // 5. Array Field: Danh sách ID dự án đang tham gia
        [BsonElement("projectIds")]
        [Display(Name = "Dự án tham gia")]
        public List<string> ProjectIds { get; set; } = new List<string>();

        [BsonElement("avatarUrl")]
        [Display(Name = "Ảnh đại diện")]
        public string? AvatarUrl { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
