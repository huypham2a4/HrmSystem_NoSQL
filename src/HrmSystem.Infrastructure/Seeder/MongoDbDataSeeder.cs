using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;
using HrmSystem.Infrastructure.Context;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace HrmSystem.Infrastructure.Seeder
{
    public class MongoDbDataSeeder
    {
        private readonly MongoDbContext _context;
        private readonly ILogger<MongoDbDataSeeder> _logger;

        public MongoDbDataSeeder(MongoDbContext context, ILogger<MongoDbDataSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync(bool force = false)
        {
            try
            {
                if (force)
                {
                    _logger.LogInformation("Làm mới và nạp lại toàn bộ dữ liệu mẫu...");
                    await _context.Departments.DeleteManyAsync(_ => true);
                    await _context.Employees.DeleteManyAsync(_ => true);
                    await _context.Projects.DeleteManyAsync(_ => true);
                }
                else
                {
                    var deptCount = await _context.Departments.CountDocumentsAsync(_ => true);
                    if (deptCount > 0)
                    {
                        _logger.LogInformation("Cơ sở dữ liệu MongoDB đã có dữ liệu, bỏ qua bước tạo dữ liệu mẫu.");
                        return;
                    }
                }

                _logger.LogInformation("Khởi tạo dữ liệu mẫu phong phú cho 3 Collections (departments, employees, projects)...");

                // 1. Seed Departments
                var itDept = new Department
                {
                    Code = "PB01",
                    Name = "Phòng Công Nghệ Thông Tin",
                    Description = "Phụ trách nghiên cứu, phát triển phần mềm, hạ tầng Cloud và an toàn thông tin doanh nghiệp.",
                    Budget = 850000000m,
                    EstablishedDate = new DateTime(2020, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ContactEmail = "it-dept@hrmvietnam.com",
                    ContactPhone = "02838990101",
                    Location = new DepartmentLocation
                    {
                        Building = "Tòa nhà Innovation Hub",
                        Floor = "Tầng 8",
                        RoomNumber = "P.801 - Phòng Nghiên Cứu",
                        City = "TP. Hồ Chí Minh"
                    }
                };

                var hrDept = new Department
                {
                    Code = "PB02",
                    Name = "Phòng Nhân Sự & Văn Hóa",
                    Description = "Chịu trách nhiệm tuyển dụng nhân tài, đào tạo nội bộ, quản lý phúc lợi và xây dựng văn hóa công ty.",
                    Budget = 320000000m,
                    EstablishedDate = new DateTime(2020, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ContactEmail = "hr-dept@hrmvietnam.com",
                    ContactPhone = "02838990102",
                    Location = new DepartmentLocation
                    {
                        Building = "Tòa nhà Innovation Hub",
                        Floor = "Tầng 4",
                        RoomNumber = "P.402",
                        City = "TP. Hồ Chí Minh"
                    }
                };

                var mktDept = new Department
                {
                    Code = "PB03",
                    Name = "Phòng Marketing & Truyền Thông",
                    Description = "Quản lý chiến lược thương hiệu, quảng bá sản phẩm số và chăm sóc trải nghiệm đối tác khách hàng.",
                    Budget = 450000000m,
                    EstablishedDate = new DateTime(2021, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ContactEmail = "marketing@hrmvietnam.com",
                    ContactPhone = "02838990103",
                    Location = new DepartmentLocation
                    {
                        Building = "Tòa nhà Innovation Hub",
                        Floor = "Tầng 6",
                        RoomNumber = "P.605 - Sáng Tạo",
                        City = "TP. Hồ Chí Minh"
                    }
                };

                var financeDept = new Department
                {
                    Code = "PB04",
                    Name = "Phòng Tài Chính - Kế Toán",
                    Description = "Lập kế hoạch tài chính, quản trị dòng tiền, kiểm toán nội bộ và hạch toán thuế theo quy chuẩn.",
                    Budget = 280000000m,
                    EstablishedDate = new DateTime(2020, 3, 20, 0, 0, 0, DateTimeKind.Utc),
                    Status = "Active",
                    ContactEmail = "finance@hrmvietnam.com",
                    ContactPhone = "02838990104",
                    Location = new DepartmentLocation
                    {
                        Building = "Tòa nhà Innovation Hub",
                        Floor = "Tầng 3",
                        RoomNumber = "P.301",
                        City = "TP. Hồ Chí Minh"
                    }
                };

                await _context.Departments.InsertManyAsync(new[] { itDept, hrDept, mktDept, financeDept });

                // 2. Seed Employees
                var employees = new List<Employee>
                {
                    new Employee
                    {
                        EmployeeCode = "NV001",
                        FullName = "Phạm Thanh Huy",
                        Email = "huy.pham@hrmvietnam.com",
                        PhoneNumber = "0901230304",
                        Position = "Lead Software Architect",
                        DepartmentId = itDept.Id!,
                        DepartmentName = itDept.Name,
                        Salary = 45000000m,
                        HireDate = new DateTime(2022, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "128 Nguyễn Trãi",
                            Ward = "Phường 3",
                            District = "Quận 5",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Phạm Văn An",
                            Relationship = "Phụ huynh",
                            PhoneNumber = "0912345678"
                        },
                        Skills = new List<string> { "C#", "ASP.NET Core", "MongoDB", "Microservices", "Docker", "Architecture" },
                        Certifications = new List<Certification>
                        {
                            new Certification { Name = "AWS Solutions Architect Professional", IssuedBy = "Amazon Web Services", Year = 2024 },
                            new Certification { Name = "MongoDB Certified Developer Associate", IssuedBy = "MongoDB Inc.", Year = 2023 }
                        }
                    },
                    new Employee
                    {
                        EmployeeCode = "NV002",
                        FullName = "Trần Thị Mai Phương",
                        Email = "phuong.tran@hrmvietnam.com",
                        PhoneNumber = "0903456789",
                        Position = "Senior Backend Developer",
                        DepartmentId = itDept.Id!,
                        DepartmentName = itDept.Name,
                        Salary = 32000000m,
                        HireDate = new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "45 Lê Văn Sỹ",
                            Ward = "Phường 13",
                            District = "Quận Phú Nhuận",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Trần Văn Bình",
                            Relationship = "Anh trai",
                            PhoneNumber = "0908765432"
                        },
                        Skills = new List<string> { "C#", "ASP.NET Core", "MongoDB", "Redis", "RESTful API" },
                        Certifications = new List<Certification>
                        {
                            new Certification { Name = "Microsoft Certified: Azure Developer Associate", IssuedBy = "Microsoft", Year = 2023 }
                        }
                    },
                    new Employee
                    {
                        EmployeeCode = "NV003",
                        FullName = "Nguyễn Hoàng Long",
                        Email = "long.nguyen@hrmvietnam.com",
                        PhoneNumber = "0938112233",
                        Position = "Frontend Specialist",
                        DepartmentId = itDept.Id!,
                        DepartmentName = itDept.Name,
                        Salary = 26000000m,
                        HireDate = new DateTime(2023, 2, 10, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "72 Điện Biên Phủ",
                            Ward = "Phường 15",
                            District = "Quận Bình Thạnh",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Nguyễn Thị Hoa",
                            Relationship = "Mẹ",
                            PhoneNumber = "0977889900"
                        },
                        Skills = new List<string> { "JavaScript", "TypeScript", "React", "Vue.js", "Bootstrap", "TailwindCSS" },
                        Certifications = new List<Certification>()
                    },
                    new Employee
                    {
                        EmployeeCode = "NV004",
                        FullName = "Võ Minh Trí",
                        Email = "tri.vo@hrmvietnam.com",
                        PhoneNumber = "0945998877",
                        Position = "DevOps & Cloud Engineer",
                        DepartmentId = itDept.Id!,
                        DepartmentName = itDept.Name,
                        Salary = 35000000m,
                        HireDate = new DateTime(2022, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "19 Hoàng Hoa Thám",
                            Ward = "Phường 6",
                            District = "Quận Bình Thạnh",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Võ Quốc Cường",
                            Relationship = "Bố",
                            PhoneNumber = "0911223344"
                        },
                        Skills = new List<string> { "Docker", "Kubernetes", "Linux", "CI/CD", "AWS", "MongoDB" },
                        Certifications = new List<Certification>
                        {
                            new Certification { Name = "Certified Kubernetes Administrator (CKA)", IssuedBy = "CNCF", Year = 2024 }
                        }
                    },
                    new Employee
                    {
                        EmployeeCode = "NV005",
                        FullName = "Lê Thị Bích Ngọc",
                        Email = "ngoc.le@hrmvietnam.com",
                        PhoneNumber = "0918776655",
                        Position = "Trưởng Phòng Nhân Sự",
                        DepartmentId = hrDept.Id!,
                        DepartmentName = hrDept.Name,
                        Salary = 30000000m,
                        HireDate = new DateTime(2021, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "15 Phan Xích Long",
                            Ward = "Phường 2",
                            District = "Quận Phú Nhuận",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Lê Văn Hùng",
                            Relationship = "Chồng",
                            PhoneNumber = "0988776655"
                        },
                        Skills = new List<string> { "HR Management", "Luật Lao Động", "Talent Acquisition", "Đào tạo nội bộ" },
                        Certifications = new List<Certification>
                        {
                            new Certification { Name = "SHRM Certified Professional (SHRM-CP)", IssuedBy = "SHRM", Year = 2022 }
                        }
                    },
                    new Employee
                    {
                        EmployeeCode = "NV006",
                        FullName = "Đỗ Hải Đăng",
                        Email = "dang.do@hrmvietnam.com",
                        PhoneNumber = "0982334455",
                        Position = "Chuyên viên Tuyển dụng",
                        DepartmentId = hrDept.Id!,
                        DepartmentName = hrDept.Name,
                        Salary = 18000000m,
                        HireDate = new DateTime(2023, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "200 Cộng Hòa",
                            Ward = "Phường 12",
                            District = "Quận Tân Bình",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Đỗ Quốc Việt",
                            Relationship = "Bố",
                            PhoneNumber = "0909090909"
                        },
                        Skills = new List<string> { "Talent Acquisition", "Headhunting", "HR Interview", "LinkedIn Recruiter" },
                        Certifications = new List<Certification>()
                    },
                    new Employee
                    {
                        EmployeeCode = "NV007",
                        FullName = "Hoàng Kim Oanh",
                        Email = "oanh.hoang@hrmvietnam.com",
                        PhoneNumber = "0976554433",
                        Position = "Marketing Brand Manager",
                        DepartmentId = mktDept.Id!,
                        DepartmentName = mktDept.Name,
                        Salary = 28000000m,
                        HireDate = new DateTime(2022, 4, 15, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "88 Nguyễn Thị Minh Khai",
                            Ward = "Phường 6",
                            District = "Quận 3",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Hoàng Thanh Sơn",
                            Relationship = "Anh trai",
                            PhoneNumber = "0934567890"
                        },
                        Skills = new List<string> { "Brand Marketing", "SEO/SEM", "Content Strategy", "Digital Ads", "Event Management" },
                        Certifications = new List<Certification>
                        {
                            new Certification { Name = "Google Ads Search Certified", IssuedBy = "Google", Year = 2023 }
                        }
                    },
                    new Employee
                    {
                        EmployeeCode = "NV008",
                        FullName = "Bùi Anh Tuấn",
                        Email = "tuan.bui@hrmvietnam.com",
                        PhoneNumber = "0967889911",
                        Position = "Content & Creative Creator",
                        DepartmentId = mktDept.Id!,
                        DepartmentName = mktDept.Name,
                        Salary = 17500000m,
                        HireDate = new DateTime(2023, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "35 Võ Văn Tần",
                            Ward = "Phường 6",
                            District = "Quận 3",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Bùi Văn Nam",
                            Relationship = "Phụ huynh",
                            PhoneNumber = "0944556677"
                        },
                        Skills = new List<string> { "Copywriting", "Photoshop", "Premiere", "Social Media", "Canva" },
                        Certifications = new List<Certification>()
                    },
                    new Employee
                    {
                        EmployeeCode = "NV009",
                        FullName = "Trịnh Thu Thảo",
                        Email = "thao.trinh@hrmvietnam.com",
                        PhoneNumber = "0933221100",
                        Position = "Kế Toán Trưởng",
                        DepartmentId = financeDept.Id!,
                        DepartmentName = financeDept.Name,
                        Salary = 33000000m,
                        HireDate = new DateTime(2021, 5, 10, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Active",
                        Address = new Address
                        {
                            Street = "50 Sư Vạn Hạnh",
                            Ward = "Phường 12",
                            District = "Quận 10",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Trịnh Quang Hưng",
                            Relationship = "Bố",
                            PhoneNumber = "0912987654"
                        },
                        Skills = new List<string> { "Tài chính doanh nghiệp", "Kế toán thuế", "Báo cáo tài chính", "SAP ERP", "Excel Advanced" },
                        Certifications = new List<Certification>
                        {
                            new Certification { Name = "Chứng chỉ Kế toán trưởng Quốc gia", IssuedBy = "Bộ Tài Chính", Year = 2021 }
                        }
                    },
                    new Employee
                    {
                        EmployeeCode = "NV010",
                        FullName = "Vũ Đình Quân",
                        Email = "quan.vu@hrmvietnam.com",
                        PhoneNumber = "0919228833",
                        Position = "Thực tập sinh Backend",
                        DepartmentId = itDept.Id!,
                        DepartmentName = itDept.Name,
                        Salary = 9000000m,
                        HireDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                        Status = "Probation",
                        Address = new Address
                        {
                            Street = "142 Tô Hiến Thành",
                            Ward = "Phường 14",
                            District = "Quận 10",
                            City = "TP. Hồ Chí Minh"
                        },
                        EmergencyContact = new EmergencyContact
                        {
                            FullName = "Vũ Đình Khải",
                            Relationship = "Anh trai",
                            PhoneNumber = "0977665544"
                        },
                        Skills = new List<string> { "C#", "ASP.NET Core", "SQL", "MongoDB", "Git" },
                        Certifications = new List<Certification>()
                    }
                };

                await _context.Employees.InsertManyAsync(employees);

                // 3. Seed Projects
                var p1 = new Project
                {
                    Code = "DA01",
                    Name = "Hệ thống HRM Doanh Nghiệp Cloud",
                    Description = "Xây dựng giải pháp HRM hiện đại tích hợp NoSQL MongoDB, thiết kế theo Repository Pattern và Aggregation Pipelines.",
                    DepartmentId = itDept.Id!,
                    DepartmentName = itDept.Name,
                    StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Budget = 500000000m,
                    Status = "Active",
                    Members = new List<ProjectMember>
                    {
                        new ProjectMember { EmployeeId = employees[0].Id!, EmployeeCode = employees[0].EmployeeCode, FullName = employees[0].FullName, Role = "Project Manager / Architect", AllocatedHoursPerWeek = 40 },
                        new ProjectMember { EmployeeId = employees[1].Id!, EmployeeCode = employees[1].EmployeeCode, FullName = employees[1].FullName, Role = "Backend Core Lead", AllocatedHoursPerWeek = 40 },
                        new ProjectMember { EmployeeId = employees[2].Id!, EmployeeCode = employees[2].EmployeeCode, FullName = employees[2].FullName, Role = "Frontend UI/UX", AllocatedHoursPerWeek = 30 },
                        new ProjectMember { EmployeeId = employees[4].Id!, EmployeeCode = employees[4].EmployeeCode, FullName = employees[4].FullName, Role = "HR Consultant", AllocatedHoursPerWeek = 10 }
                    },
                    Milestones = new List<ProjectMilestone>
                    {
                        new ProjectMilestone { Title = "Phân tích yêu cầu & Thiết kế 3 Collections MongoDB", DueDate = DateTime.UtcNow.AddMonths(-2), IsCompleted = true },
                        new ProjectMilestone { Title = "Xây dựng Repository Pattern & Aggregation Pipelines", DueDate = DateTime.UtcNow.AddDays(-10), IsCompleted = true },
                        new ProjectMilestone { Title = "Hoàn thiện Dashboard thống kê tương tác Chart.js", DueDate = DateTime.UtcNow.AddDays(15), IsCompleted = true },
                        new ProjectMilestone { Title = "Nghiệm thu & Đóng gói triển khai", DueDate = DateTime.UtcNow.AddMonths(2), IsCompleted = false }
                    }
                };

                var p2 = new Project
                {
                    Code = "DA02",
                    Name = "Nâng cấp Hạ Tầng Cloud & CI/CD",
                    Description = "Triển khai containerization với Docker, thiết lập monitoring và cluster MongoDB an toàn dữ liệu.",
                    DepartmentId = itDept.Id!,
                    DepartmentName = itDept.Name,
                    StartDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2024, 8, 30, 0, 0, 0, DateTimeKind.Utc),
                    Budget = 250000000m,
                    Status = "Active",
                    Members = new List<ProjectMember>
                    {
                        new ProjectMember { EmployeeId = employees[3].Id!, EmployeeCode = employees[3].EmployeeCode, FullName = employees[3].FullName, Role = "DevOps Lead", AllocatedHoursPerWeek = 40 },
                        new ProjectMember { EmployeeId = employees[0].Id!, EmployeeCode = employees[0].EmployeeCode, FullName = employees[0].FullName, Role = "Advisor", AllocatedHoursPerWeek = 10 }
                    },
                    Milestones = new List<ProjectMilestone>
                    {
                        new ProjectMilestone { Title = "Thiết lập Pipeline GitHub Actions & Docker", DueDate = DateTime.UtcNow.AddMonths(-1), IsCompleted = true },
                        new ProjectMilestone { Title = "Setup Prometheus & Grafana Monitoring", DueDate = DateTime.UtcNow.AddDays(20), IsCompleted = false }
                    }
                };

                var p3 = new Project
                {
                    Code = "DA03",
                    Name = "Chiến Dịch Tuyển Dụng Lập Trình Viên 2026",
                    Description = "Tìm kiếm và chọn lọc 20 kỹ sư phần mềm tài năng cho các dự án mở rộng toàn cầu.",
                    DepartmentId = hrDept.Id!,
                    DepartmentName = hrDept.Name,
                    StartDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2024, 5, 31, 0, 0, 0, DateTimeKind.Utc),
                    Budget = 120000000m,
                    Status = "Completed",
                    Members = new List<ProjectMember>
                    {
                        new ProjectMember { EmployeeId = employees[4].Id!, EmployeeCode = employees[4].EmployeeCode, FullName = employees[4].FullName, Role = "Lead Organizer", AllocatedHoursPerWeek = 30 },
                        new ProjectMember { EmployeeId = employees[5].Id!, EmployeeCode = employees[5].EmployeeCode, FullName = employees[5].FullName, Role = "Talent Hunter", AllocatedHoursPerWeek = 40 },
                        new ProjectMember { EmployeeId = employees[6].Id!, EmployeeCode = employees[6].EmployeeCode, FullName = employees[6].FullName, Role = "Media Coordinator", AllocatedHoursPerWeek = 15 }
                    },
                    Milestones = new List<ProjectMilestone>
                    {
                        new ProjectMilestone { Title = "Truyền thông sự kiện Job Fair", DueDate = DateTime.UtcNow.AddMonths(-3), IsCompleted = true },
                        new ProjectMilestone { Title = "Phỏng vấn và ký kết hợp đồng", DueDate = DateTime.UtcNow.AddMonths(-1), IsCompleted = true }
                    }
                };

                var p4 = new Project
                {
                    Code = "DA04",
                    Name = "Cổng Tự Động Quyết Toán Thuế & Lương",
                    Description = "Tích hợp cổng tính toán lương thưởng và xuất file quyết toán thuế tự động cho cơ quan quản lý.",
                    DepartmentId = financeDept.Id!,
                    DepartmentName = financeDept.Name,
                    StartDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(2024, 11, 30, 0, 0, 0, DateTimeKind.Utc),
                    Budget = 180000000m,
                    Status = "Planning",
                    Members = new List<ProjectMember>
                    {
                        new ProjectMember { EmployeeId = employees[8].Id!, EmployeeCode = employees[8].EmployeeCode, FullName = employees[8].FullName, Role = "Project Sponsor / Financial Advisor", AllocatedHoursPerWeek = 20 },
                        new ProjectMember { EmployeeId = employees[1].Id!, EmployeeCode = employees[1].EmployeeCode, FullName = employees[1].FullName, Role = "Integration Dev", AllocatedHoursPerWeek = 20 }
                    },
                    Milestones = new List<ProjectMilestone>
                    {
                        new ProjectMilestone { Title = "Hoàn thành đặc tả nghiệp vụ kế toán", DueDate = DateTime.UtcNow.AddDays(30), IsCompleted = false }
                    }
                };

                await _context.Projects.InsertManyAsync(new[] { p1, p2, p3, p4 });

                // Cập nhật lại ProjectIds cho các nhân viên
                await _context.Employees.UpdateOneAsync(e => e.Id == employees[0].Id, Builders<Employee>.Update.Set(e => e.ProjectIds, new List<string> { p1.Id!, p2.Id! }));
                await _context.Employees.UpdateOneAsync(e => e.Id == employees[1].Id, Builders<Employee>.Update.Set(e => e.ProjectIds, new List<string> { p1.Id!, p4.Id! }));
                await _context.Employees.UpdateOneAsync(e => e.Id == employees[2].Id, Builders<Employee>.Update.Set(e => e.ProjectIds, new List<string> { p1.Id! }));
                await _context.Employees.UpdateOneAsync(e => e.Id == employees[3].Id, Builders<Employee>.Update.Set(e => e.ProjectIds, new List<string> { p2.Id! }));
                await _context.Employees.UpdateOneAsync(e => e.Id == employees[4].Id, Builders<Employee>.Update.Set(e => e.ProjectIds, new List<string> { p1.Id!, p3.Id! }));
                await _context.Employees.UpdateOneAsync(e => e.Id == employees[5].Id, Builders<Employee>.Update.Set(e => e.ProjectIds, new List<string> { p3.Id! }));
                await _context.Employees.UpdateOneAsync(e => e.Id == employees[6].Id, Builders<Employee>.Update.Set(e => e.ProjectIds, new List<string> { p3.Id! }));
                await _context.Employees.UpdateOneAsync(e => e.Id == employees[8].Id, Builders<Employee>.Update.Set(e => e.ProjectIds, new List<string> { p4.Id! }));

                _logger.LogInformation("Nạp dữ liệu mẫu hoàn tất: 4 phòng ban, 10 nhân sự đầy đủ mảng & embedded doc, 4 dự án!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong quá trình nạp dữ liệu mẫu");
            }
        }
    }
}
