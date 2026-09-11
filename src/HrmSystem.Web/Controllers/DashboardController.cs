using System;
using System.Threading.Tasks;
using HrmSystem.Core.Interfaces;
using HrmSystem.Infrastructure;
using HrmSystem.Infrastructure.Configuration;
using HrmSystem.Infrastructure.Seeder;
using Microsoft.AspNetCore.Mvc;

namespace HrmSystem.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IStatisticsRepository _statsRepo;
        private readonly IDatabaseProviderInfo _providerInfo;
        private readonly MongoIndexInitializer? _indexInitializer;
        private readonly MongoDbDataSeeder? _seeder;

        public DashboardController(
            IStatisticsRepository statsRepo,
            IDatabaseProviderInfo providerInfo,
            IServiceProvider serviceProvider)
        {
            _statsRepo = statsRepo;
            _providerInfo = providerInfo;
            _indexInitializer = (MongoIndexInitializer?)serviceProvider.GetService(typeof(MongoIndexInitializer));
            _seeder = (MongoDbDataSeeder?)serviceProvider.GetService(typeof(MongoDbDataSeeder));
        }

        public async Task<IActionResult> Index()
        {
            var overview = await _statsRepo.GetDashboardOverviewAsync();
            ViewBag.ProviderInfo = _providerInfo;

            if (_indexInitializer != null)
            {
                ViewBag.Indexes = await _indexInitializer.GetAllIndexDetailsAsync();
            }

            return View(overview);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReSeedData()
        {
            if (_seeder != null)
            {
                await _seeder.SeedAsync(force: true);
                TempData["SuccessMessage"] = "Đã khởi tạo và làm mới dữ liệu mẫu thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
