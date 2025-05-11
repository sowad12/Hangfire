using Main.Models.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Scheduler.Services;

namespace Main.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BackgroundJobController : ControllerBase
    {
        private readonly IJobService _jobService;
        public BackgroundJobController(IJobService jobService)
        {
            _jobService= jobService;
        }

        [HttpPost("fire-and-forget")]
        public async Task<IActionResult> FireAndForgetJob()
        {
            var JobId=_jobService.FireAndForgotService(new FireAndForgotJob
            {
                Id=Guid.NewGuid(),
                Name="fireandForgot"
            });
            return Ok(JobId);
        }

        [HttpPost("delayed")]
        public async Task<IActionResult> DelayedJob()
        {
            var JobId = _jobService.DelayedJobsService(new DelayedJob
            {
                Id = Guid.NewGuid(),
                Name = "delayed"
            }, TimeSpan.FromMinutes(1));
            return Ok(JobId);
        }
    }
}
