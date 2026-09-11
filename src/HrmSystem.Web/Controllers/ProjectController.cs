using System;
using System.Linq;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;
using HrmSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HrmSystem.Web.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly IEmployeeRepository _empRepo;

        public ProjectController(
            IProjectRepository projectRepo,
            IDepartmentRepository deptRepo,
            IEmployeeRepository empRepo)
        {
            _projectRepo = projectRepo;
            _deptRepo = deptRepo;
            _empRepo = empRepo;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _projectRepo.GetAllAsync();
            return View(projects);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return NotFound();

            var allEmployees = await _empRepo.GetAllAsync();
            ViewBag.AvailableEmployees = allEmployees.Where(e => !project.Members.Any(m => m.EmployeeId == e.Id)).ToList();

            return View(project);
        }

        public async Task<IActionResult> Create()
        {
            var depts = await _deptRepo.GetAllAsync();
            ViewBag.DepartmentList = new SelectList(depts, "Id", "Name");
            return View(new Project());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            var existing = await _projectRepo.GetByCodeAsync(project.Code);
            if (existing != null)
            {
                ModelState.AddModelError("Code", "Mã dự án đã tồn tại (Unique Index).");
            }

            if (!ModelState.IsValid)
            {
                var depts = await _deptRepo.GetAllAsync();
                ViewBag.DepartmentList = new SelectList(depts, "Id", "Name", project.DepartmentId);
                return View(project);
            }

            if (!string.IsNullOrEmpty(project.DepartmentId))
            {
                var dept = await _deptRepo.GetByIdAsync(project.DepartmentId);
                if (dept != null)
                {
                    project.DepartmentName = dept.Name;
                }
            }

            await _projectRepo.CreateAsync(project);
            TempData["SuccessMessage"] = $"Đã tạo dự án {project.Name} ({project.Code}) thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var project = await _projectRepo.GetByIdAsync(id);
            if (project == null) return NotFound();

            var depts = await _deptRepo.GetAllAsync();
            ViewBag.DepartmentList = new SelectList(depts, "Id", "Name", project.DepartmentId);

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Project project)
        {
            if (id != project.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                var depts = await _deptRepo.GetAllAsync();
                ViewBag.DepartmentList = new SelectList(depts, "Id", "Name", project.DepartmentId);
                return View(project);
            }

            if (!string.IsNullOrEmpty(project.DepartmentId))
            {
                var dept = await _deptRepo.GetByIdAsync(project.DepartmentId);
                if (dept != null)
                {
                    project.DepartmentName = dept.Name;
                }
            }

            // Bảo toàn danh sách Members và Milestones cũ nếu không chỉnh sửa trên form cơ bản
            var old = await _projectRepo.GetByIdAsync(id);
            if (old != null)
            {
                project.Members = old.Members;
                project.Milestones = old.Milestones;
            }

            await _projectRepo.UpdateAsync(project);
            TempData["SuccessMessage"] = $"Đã cập nhật dự án {project.Name}!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var project = await _projectRepo.GetByIdAsync(id);
            if (project != null)
            {
                await _projectRepo.DeleteAsync(id);
                TempData["SuccessMessage"] = $"Đã xóa dự án {project.Name}.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Thêm thành viên vào dự án (Array embedded document push)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(string projectId, string employeeId, string role, int hours)
        {
            var emp = await _empRepo.GetByIdAsync(employeeId);
            if (emp == null) return NotFound();

            var member = new ProjectMember
            {
                EmployeeId = emp.Id!,
                EmployeeCode = emp.EmployeeCode,
                FullName = emp.FullName,
                Role = role,
                AllocatedHoursPerWeek = hours > 0 ? hours : 40,
                JoinedDate = DateTime.UtcNow
            };

            await _projectRepo.AddMemberAsync(projectId, member);
            await _empRepo.AssignProjectAsync(employeeId, projectId);

            TempData["SuccessMessage"] = $"Đã phân công {emp.FullName} vào dự án với vai trò {role}!";
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // POST: Xóa thành viên khỏi dự án (Array embedded document pull)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(string projectId, string employeeId)
        {
            await _projectRepo.RemoveMemberAsync(projectId, employeeId);
            await _empRepo.UnassignProjectAsync(employeeId, projectId);

            TempData["SuccessMessage"] = "Đã loại thành viên khỏi dự án.";
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // POST: Thêm Milestone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMilestone(string projectId, string title, DateTime dueDate)
        {
            var milestone = new ProjectMilestone
            {
                Title = title,
                DueDate = dueDate,
                IsCompleted = false
            };

            await _projectRepo.AddMilestoneAsync(projectId, milestone);
            TempData["SuccessMessage"] = $"Đã thêm mốc sự kiện '{title}'!";
            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // POST: Đổi trạng thái hoàn thành Milestone
        [HttpPost]
        public async Task<IActionResult> ToggleMilestone(string projectId, string title, bool isCompleted)
        {
            var success = await _projectRepo.ToggleMilestoneStatusAsync(projectId, title, isCompleted);
            return Json(new { success });
        }
    }
}
