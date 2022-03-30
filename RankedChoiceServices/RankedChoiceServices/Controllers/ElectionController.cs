using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RankedChoiceServices.Entities;

namespace RankedChoiceServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElectionController : ControllerBase
    {
        public record CandidateDTO(string value, string candidateId);
        public record ElectionDTO(string electionId, CandidateDTO[] candidates);
        public record ElectionSettingsDTO(string electionId, bool uniqueIdsPerUser, 
            string [] uniqueIds, string [] userEmails, IElection.ElectionState state);
        
        private readonly ILogger<ElectionController> _logger;
        private readonly ElectionRepository _repo;
        
        public ElectionController(ILogger<ElectionController> logger, ElectionRepository repository)
        {
            _logger = logger;
            _repo = repository;
        }

        
        [HttpPost("{electionId}/start")]
        public bool StartElection(string electionId)
        {
            var election = _repo.Get(electionId);
            
            return election.StartElection();
        }
        
        [HttpPost("{electionId}/end")]
        public bool EndElection(string electionId)
        {
            var election = _repo.Get(electionId);

            return election.StopElection();
        }
        
        [HttpPost("{electionId}/restart")]
        public bool RestartElection(string electionId)
        {
            var election = _repo.Get(electionId);
            return election.RestartElection();
        }
        
        [HttpGet("{electionId}/results")]
        public ElectionDTO GetElectionResults(string electionId)
        {
            var election = _repo.Get(electionId);

            return new ElectionDTO(electionId,
                election.CalculateResults().Select(c => new CandidateDTO(c.value, c.candidateId)).ToArray());
        }
        
        [HttpGet("{electionId}/settings")]
        public ElectionSettingsDTO GetSettings(string electionId)
        {
            var election = _repo.Get(electionId);
            
            return new ElectionSettingsDTO(electionId, election.UniqueIdsPerUser, 
                election.UniqueElectionIds.ToArray(), election.Users.Select(u => u.email).ToArray(), election.State);
        }
        
        [HttpGet("{electionId}/candidates")]
        public ElectionDTO GetCandidates(string electionId)
        {
            var election = _repo.Get(electionId);
            
            return new ElectionDTO(electionId, 
                election.Candidates.Select(c => 
                    new CandidateDTO(c.value, c.candidateId)).ToArray()
                );
        }

        [HttpPost("{electionId}/settings")]
        public bool SaveSettings(string electionId, [FromBody] ElectionSettingsDTO settings)
        {
            if (electionId != settings.electionId)
            {
                _logger.Log(LogLevel.Error, "ElectionId mismatch {electionId} {election.electionId}", electionId, settings.electionId);
                HttpContext.Response.StatusCode = 400;
                return false;
            }
            
            if (!_repo.Exists(electionId))
            {
                _logger.Log(LogLevel.Information, "New election created with Id {electionId}", electionId);
            }
            
            var election = _repo.Get(electionId);

            election.SetUserEmails(settings.userEmails);
            election.UniqueIdsPerUser = settings.uniqueIdsPerUser;

            _repo.Save(electionId, election);
            return true;
        }
        
        [HttpPost("{electionId}/candidates")]
        public bool SaveCandidates(string electionId, [FromBody] ElectionDTO dto)
        {
            if (electionId != dto.electionId)
            {
                _logger.Log(LogLevel.Error, "ElectionId mismatch {electionId} {election.electionId}", electionId, dto.electionId);
                HttpContext.Response.StatusCode = 400;
                return false;
            }

            if (!_repo.Exists(electionId))
            {
                _logger.Log(LogLevel.Information, "New election created with Id {electionId}", electionId);
            }
            
            _logger.Log(LogLevel.Information, "Election with Id {electionId} Saved", electionId);

            var election = _repo.Get(electionId);

            var candidates = dto.candidates.Select(c => new Candidate(c.value, c.candidateId)).ToArray();
            election.Candidates = candidates;
            
            _repo.Save(electionId, election);
            
            return true;
        }
      
    }
}