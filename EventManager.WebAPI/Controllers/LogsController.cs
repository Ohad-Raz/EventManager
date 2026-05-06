using EventManager.DAL.Models;
using EventManager.DAL.Repositories;
using EventManager.WebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        private readonly ILogRepository _logRepository;

        public LogsController(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        /// <summary>
        /// Returns the total number of logs.
        /// Endpoint: GET /api/Logs/count
        /// </summary>
        [HttpGet("count")]
        public ActionResult<int> Count()
        {
            try
            {
                // 1. count all logs
                int result = _logRepository.CountLogs();

                // 2. return count
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns the last N logs ordered from newest to oldest.
        /// Endpoint: GET /api/Logs/get/{n}
        /// </summary>
        [HttpGet("get/{n}")]
        public ActionResult<ICollection<LogDto>> Get(int n)
        {
            try
            {
                // 1. validate input
                if (n < 1)
                    return BadRequest("N must be at least 1.");

                // 2. load last n logs ordered by newest first
                List<Log> logs = _logRepository.GetLatestLogs(n);

                // 3. map entities to DTOs
                List<LogDto> result = logs
                    .Select(x => new LogDto
                    {
                        Id = x.Id,
                        Timestamp = x.Timestamp,
                        Level = x.Level,
                        Message = x.Message,
                        ErrorText = x.ErrorText
                    })
                    .ToList();

                // 4. return DTO list
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}