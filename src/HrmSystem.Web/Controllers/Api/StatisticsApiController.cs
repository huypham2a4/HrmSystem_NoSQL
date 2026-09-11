using System;
using System.Threading.Tasks;
using HrmSystem.Core.Interfaces;
using HrmSystem.Infrastructure;
using HrmSystem.Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace HrmSystem.Web.Controllers.Api
{
    [ApiController]
    [Route("api/stats")]
    public class StatisticsApiController : ControllerBase
    {
        private readonly IStatisticsRepository _statsRepo;
        private readonly IDatabaseProviderInfo _providerInfo;
        private readonly MongoIndexInitializer? _indexInitializer;

        public StatisticsApiController(
            IStatisticsRepository statsRepo,
            IDatabaseProviderInfo providerInfo,
            IServiceProvider serviceProvider)
        {
            _statsRepo = statsRepo;
            _providerInfo = providerInfo;
            _indexInitializer = (MongoIndexInitializer?)serviceProvider.GetService(typeof(MongoIndexInitializer));
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _statsRepo.GetDashboardOverviewAsync();
            return Ok(new
            {
                provider = _providerInfo.CurrentProvider,
                database = _providerInfo.DatabaseName,
                data = overview
            });
        }

        [HttpGet("department-salary")]
        public async Task<IActionResult> GetDepartmentSalaryStats()
        {
            var stats = await _statsRepo.GetDepartmentStatsAsync();
            return Ok(stats);
        }

        [HttpGet("skills")]
        public async Task<IActionResult> GetSkillStats([FromQuery] int limit = 10)
        {
            var stats = await _statsRepo.GetSkillDistributionAsync(limit);
            return Ok(stats);
        }

        [HttpGet("project-status")]
        public async Task<IActionResult> GetProjectStatusStats()
        {
            var stats = await _statsRepo.GetProjectStatusStatsAsync();
            return Ok(stats);
        }

        [HttpGet("positions")]
        public async Task<IActionResult> GetPositionStats()
        {
            var stats = await _statsRepo.GetSalaryByPositionStatsAsync();
            return Ok(stats);
        }

        [HttpGet("indexes")]
        public async Task<IActionResult> GetIndexes()
        {
            if (_indexInitializer == null)
            {
                return Ok(new { message = "Chỉ khả dụng trên MongoDB Provider" });
            }

            var indexes = await _indexInitializer.GetAllIndexDetailsAsync();
            return Ok(indexes);
        }
    }
}
