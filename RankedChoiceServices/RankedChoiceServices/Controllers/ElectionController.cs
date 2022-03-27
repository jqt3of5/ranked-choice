using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RankedChoiceServices.Data;

namespace RankedChoiceServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElectionController : ControllerBase
    {
        private readonly ILogger<ElectionController> _logger;
        private readonly ElectionRepository _repo;
        
        public ElectionController(ILogger<ElectionController> logger, ElectionRepository repository)
        {
            _logger = logger;
            _repo = repository;
        }

        [HttpGet("{electionId}")]
        public Election GetElection(string electionId)
        {
            if (_repo.ElectionExists(electionId))
            {
                return _repo.GetElection(electionId);
            }

            return null;
        }
        
        [HttpPost("{electionId}")]
        public bool SaveElection(string electionId, [FromBody] Election election)
        {
            if (electionId != election.electionId)
            {
                _logger.Log(LogLevel.Error, "ElectionId mismatch {electionId} {election.electionId}", electionId, election.electionId);
                HttpContext.Response.StatusCode = 400;
                return false;
            }

            if (!_repo.ElectionExists(electionId))
            {
                _logger.Log(LogLevel.Information, "New election created with Id {electionId}", electionId);
            }
            
            _logger.Log(LogLevel.Information, "Election with Id {electionId} Saved", electionId);
            _repo.SaveElection(electionId,election);
            return true;
        }
      
    }
}