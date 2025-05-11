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

        [HttpPost]
        public async Task<IActionResult> FireAndForgetJob()
        {
            _jobService.Execute(new FireAndForgotJob
            {
                Id=Guid.NewGuid(),
                Name="fireandForgot"
            });
            return Ok();
        }
    }
}
