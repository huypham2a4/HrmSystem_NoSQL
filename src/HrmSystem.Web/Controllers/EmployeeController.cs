using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;
using HrmSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HrmSystem.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IStatisticsRepository _statsRepo;

        public EmployeeController(
            IEmployeeRepository employeeRepo,
            IDepartmentRepository deptRepo,
            IProjectRepository projectRepo,
            IStatisticsRepository statsRepo)
        {
            _employeeRepo = employeeRepo;
            _deptRepo = deptRepo;
            _projectRepo = projectRepo;
            _statsRepo = statsRepo;
        }

        // Danh sách nhân viên kèm tìm kiếm toàn văn, lọc theo phòng ban, kỹ năng, trạng thái
        public async Task<IActionResult> Index(string? keyword, string? departmentId, string? skill, string? status)
        {
            var employees = await _employeeRepo.SearchAsync(keyword, departmentId, skill, status);
            var departments = await _deptRepo.GetAllAsync();
            var skillStats = await _statsRepo.GetSkillDistributionAsync(15);

            ViewBag.Keyword = keyword;
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SelectedSkill = skill;
            ViewBag.SelectedStatus = status;
            ViewBag.Departments = departments;
            ViewBag.PopularSkills = skillStats.Select(s => s.Skill).ToList();

            return View(employees);
        }

        // Xem hồ sơ chi tiết nhân viên (Profile view với Embedded Documents & Mảng Skills, Dự án)
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null) return NotFound();

            // Lấy danh sách các dự án nhân viên tham gia
            var allProjects = await _projectRepo.GetAllAsync();
            var assignedProjects = allProjects.Where(p =>
                (employee.ProjectIds != null && employee.ProjectIds.Contains(p.Id ?? "")) ||
                p.Members.Any(m => m.EmployeeId == employee.Id)
            ).ToList();

            ViewBag.AssignedProjects = assignedProjects;
            return View(employee);
        }

        // GET: Thêm mới nhân viên
        public async Task<IActionResult> Create()
        {
            var departments = await _deptRepo.GetAllAsync();
            ViewBag.DepartmentList = new SelectList(departments, "Id", "Name");
            return View(new Employee());
        }

        // POST: Thêm mới nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee, string? skillsInput)
        {
            // Kiểm tra mã nhân viên trùng
            var existingByCode = await _employeeRepo.GetByCodeAsync(employee.EmployeeCode);
            if (existingByCode != null)
            {
                ModelState.AddModelError("EmployeeCode", "Mã nhân viên này đã tồn tại trong hệ thống (Unique Index).");
            }

            if (!ModelState.IsValid)
            {
                var departments = await _deptRepo.GetAllAsync();
                ViewBag.DepartmentList = new SelectList(departments, "Id", "Name", employee.DepartmentId);
                return View(employee);
            }

            // Gán tên phòng ban
            if (!string.IsNullOrEmpty(employee.DepartmentId))
            {
                var dept = await _deptRepo.GetByIdAsync(employee.DepartmentId);
                if (dept != null)
                {
                    employee.DepartmentName = dept.Name;
                }
            }

            // Xử lý danh sách kỹ năng từ chuỗi phân tách
            if (!string.IsNullOrWhiteSpace(skillsInput))
            {
                employee.Skills = skillsInput
                    .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            await _employeeRepo.CreateAsync(employee);
            TempData["SuccessMessage"] = $"Đã thêm mới nhân viên {employee.FullName} ({employee.EmployeeCode}) thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Chỉnh sửa nhân viên
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var employee = await _employeeRepo.GetByIdAsync(id);
            if (employee == null) return NotFound();

            var departments = await _deptRepo.GetAllAsync();
            ViewBag.DepartmentList = new SelectList(departments, "Id", "Name", employee.DepartmentId);
            ViewBag.SkillsCommaSeparated = string.Join(", ", employee.Skills);

            return View(employee);
        }

        // POST: Chỉnh sửa nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Employee employee, string? skillsInput)
        {
            if (id != employee.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                var departments = await _deptRepo.GetAllAsync();
                ViewBag.DepartmentList = new SelectList(departments, "Id", "Name", employee.DepartmentId);
                ViewBag.SkillsCommaSeparated = skillsInput;
                return View(employee);
            }

            // Gán tên phòng ban
            if (!string.IsNullOrEmpty(employee.DepartmentId))
            {
                var dept = await _deptRepo.GetByIdAsync(employee.DepartmentId);
                if (dept != null)
                {
                    employee.DepartmentName = dept.Name;
                }
            }

            // Xử lý danh sách kỹ năng
            if (!string.IsNullOrWhiteSpace(skillsInput))
            {
                employee.Skills = skillsInput
                    .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            else
            {
                employee.Skills = new List<string>();
            }

            // Giữ lại danh sách ProjectIds và Certifications cũ nếu form không sửa
            var oldEmp = await _employeeRepo.GetByIdAsync(id);
            if (oldEmp != null)
            {
                employee.ProjectIds = oldEmp.ProjectIds;
                if (employee.Certifications == null || employee.Certifications.Count == 0)
                {
                    employee.Certifications = oldEmp.Certifications;
                }
            }

            await _employeeRepo.UpdateAsync(employee);
            TempData["SuccessMessage"] = $"Đã cập nhật thông tin nhân viên {employee.FullName} thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Xóa nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var emp = await _employeeRepo.GetByIdAsync(id);
            if (emp != null)
            {
                await _employeeRepo.DeleteAsync(id);
                TempData["SuccessMessage"] = $"Đã xóa nhân viên {emp.FullName} khỏi hệ thống.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Ajax endpoint: Thêm nhanh kỹ năng vào mảng Skills bằng toán tử $addToSet
        [HttpPost]
        public async Task<IActionResult> QuickAddSkill(string employeeId, string skill)
        {
            if (string.IsNullOrWhiteSpace(employeeId) || string.IsNullOrWhiteSpace(skill))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var success = await _employeeRepo.AddSkillAsync(employeeId, skill.Trim());
            return Json(new { success, message = success ? $"Đã thêm kỹ năng '{skill}'" : "Không thể thêm kỹ năng" });
        }

        // Ajax endpoint: Xóa nhanh kỹ năng khỏi mảng Skills bằng toán tử $pull
        [HttpPost]
        public async Task<IActionResult> QuickRemoveSkill(string employeeId, string skill)
        {
            if (string.IsNullOrWhiteSpace(employeeId) || string.IsNullOrWhiteSpace(skill))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var success = await _employeeRepo.RemoveSkillAsync(employeeId, skill.Trim());
            return Json(new { success, message = success ? $"Đã xóa kỹ năng '{skill}'" : "Không thể xóa kỹ năng" });
        }
    }
}
