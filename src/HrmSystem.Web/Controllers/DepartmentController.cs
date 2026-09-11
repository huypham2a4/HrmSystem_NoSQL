using System;
using System.Threading.Tasks;
using HrmSystem.Core.Entities;
using HrmSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HrmSystem.Web.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _deptRepo;
        private readonly IEmployeeRepository _empRepo;
        private readonly IProjectRepository _projectRepo;

        public DepartmentController(
            IDepartmentRepository deptRepo,
            IEmployeeRepository empRepo,
            IProjectRepository projectRepo)
        {
            _deptRepo = deptRepo;
            _empRepo = empRepo;
            _projectRepo = projectRepo;
        }

        public async Task<IActionResult> Index()
        {
            var depts = await _deptRepo.GetAllAsync();
            return View(depts);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();

            ViewBag.Employees = await _empRepo.GetByDepartmentAsync(id);
            ViewBag.Projects = await _projectRepo.GetByDepartmentAsync(id);

            return View(dept);
        }

        public IActionResult Create()
        {
            return View(new Department());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            var existing = await _deptRepo.GetByCodeAsync(department.Code);
            if (existing != null)
            {
                ModelState.AddModelError("Code", "Mã phòng ban đã tồn tại (Unique Index).");
            }

            if (!ModelState.IsValid)
            {
                return View(department);
            }

            await _deptRepo.CreateAsync(department);
            TempData["SuccessMessage"] = $"Đã thêm mới phòng ban {department.Name} ({department.Code})!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept == null) return NotFound();

            return View(dept);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Department department)
        {
            if (id != department.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(department);
            }

            await _deptRepo.UpdateAsync(department);
            TempData["SuccessMessage"] = $"Đã cập nhật thông tin phòng ban {department.Name}!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var dept = await _deptRepo.GetByIdAsync(id);
            if (dept != null)
            {
                var employees = await _empRepo.GetByDepartmentAsync(id);
                if (employees.Count > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa phòng ban {dept.Name} vì đang có {employees.Count} nhân viên thuộc phòng này!";
                    return RedirectToAction(nameof(Index));
                }

                await _deptRepo.DeleteAsync(id);
                TempData["SuccessMessage"] = $"Đã xóa phòng ban {dept.Name}.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
