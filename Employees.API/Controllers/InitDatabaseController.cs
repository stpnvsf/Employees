using Employees.Application;
using Microsoft.AspNetCore.Mvc;

namespace Employees.API.Controllers
{
    [Route("/init")]
    [ApiController]
    public sealed class InitDatabaseController : Controller
    {
        private readonly IInitDb _initDb;

        public InitDatabaseController(IInitDb initDb)
        {
            _initDb = initDb;
        }

        [HttpPost]
        public async Task<ActionResult> Init()
        {
            await _initDb.Init();

            return Ok();
        }

    }
}
