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
        public IEnumerable<Candidate> GetElection(string electionId)
        {
            if (_repo.ElectionExists(electionId))
            {
                return _repo.GetElection(electionId);
            }

            return Array.Empty<Candidate>();
        }
        
        [HttpPost("{electionId}")]
        public bool SaveElection(string electionId, [FromBody] Candidate[] candidates)
        {
            _repo.SaveElection(electionId,candidates);
            return true;
        }
      
    }
}