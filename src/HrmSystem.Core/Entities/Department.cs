using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HrmSystem.Core.Entities
{
    public class DepartmentLocation
    {
        [Display(Name = "Tòa nhà")]
        public string Building { get; set; } = "Tòa nhà Innovation";

        [Display(Name = "Tầng")]
        public string Floor { get; set; } = "Tầng 5";

        [Display(Name = "Số phòng")]
        public string RoomNumber { get; set; } = "P.502";

        [Display(Name = "Thành phố")]
        public string City { get; set; } = "TP. Hồ Chí Minh";
    }

    public class Department
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("code")]
        [Required(ErrorMessage = "Mã phòng ban là bắt buộc")]
        [Display(Name = "Mã phòng ban")]
        public string Code { get; set; } = string.Empty;

        [BsonElement("name")]
        [Required(ErrorMessage = "Tên phòng ban là bắt buộc")]
        [Display(Name = "Tên phòng ban")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("description")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [BsonElement("budget")]
        [BsonRepresentation(BsonType.Decimal128)]
        [Display(Name = "Ngân sách (VNĐ)")]
        [Range(0, double.MaxValue, ErrorMessage = "Ngân sách phải lớn hơn hoặc bằng 0")]
        public decimal Budget { get; set; }

        [BsonElement("establishedDate")]
        [Display(Name = "Ngày thành lập")]
        [DataType(DataType.Date)]
        public DateTime EstablishedDate { get; set; } = DateTime.UtcNow;

        [BsonElement("status")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Active"; // Active, Inactive

        [BsonElement("contactEmail")]
        [Display(Name = "Email liên hệ")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? ContactEmail { get; set; }

        [BsonElement("contactPhone")]
        [Display(Name = "Số điện thoại")]
        public string? ContactPhone { get; set; }

        // Embedded Document
        [BsonElement("location")]
        [Display(Name = "Vị trí địa lý")]
        public DepartmentLocation Location { get; set; } = new DepartmentLocation();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
    }
}
