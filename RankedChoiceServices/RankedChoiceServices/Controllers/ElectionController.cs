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

        [HttpPost("{electionId}/vote")]
        public bool SubmitVote(string electionId, [FromBody]string [] candidateIds, [FromHeader(Name = "userId")] string userId)
        {
            var election = _repo.Get(electionId);
            if (election == null)
            {
                _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                Response.StatusCode = 404;
                return false;
            }

            List<Candidate> candidates = new List<Candidate>();
            foreach (var candidateId in candidateIds)
            {
                var candidate = election.Candidates.FirstOrDefault(c => c.candidateId == candidateId);
                if (candidate == null)
                {
                    //That id is not valid, fail!
                    _logger.Log(LogLevel.Warning, "candidate with Id {candidateId} does not exist", candidateId);
                    Response.StatusCode = 400;
                    return false;
                }
                
                candidates.Add(candidate);
            }
            
            election.AddVote(new Vote(userId, candidates.ToArray()));

            return true;
        }
        
        [HttpPost("{electionId}/start")]
        public bool StartElection(string electionId)
        {
            var election = _repo.Get(electionId);
            if (election == null)
            {
                _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                Response.StatusCode = 404;
                return false;
            } 
            return election.StartElection();
        }
        
        [HttpPost("{electionId}/end")]
        public bool EndElection(string electionId)
        {
            var election = _repo.Get(electionId);
            if (election == null)
            {
                _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                Response.StatusCode = 404;
                return false;
            }
            return election.StopElection();
        }
        
        [HttpPost("{electionId}/restart")]
        public bool RestartElection(string electionId)
        {
            var election = _repo.Get(electionId);
            if (election == null)
            {
                _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                Response.StatusCode = 404;
                return false;
            }
            return election.RestartElection();
        }
        
        [HttpGet("{electionId}/results")]
        public ElectionDTO? GetElectionResults(string electionId)
        {
            var election = _repo.Get(electionId);
            if (election == null)
            {
                Response.StatusCode = 404;
                _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                return null;
            }
            return new ElectionDTO(electionId,
                election.CalculateResults().Select(c => new CandidateDTO(c.value, c.candidateId)).ToArray());
        }
        
        [HttpGet("{electionId}/settings")]
        public ElectionSettingsDTO? GetSettings(string electionId)
        {
            var election = _repo.Get(electionId);
            if (election == null)
            {
                Response.StatusCode = 404;
                _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                return null;
            } 
            return new ElectionSettingsDTO(electionId, election.UniqueIdsPerUser, 
                election.UniqueElectionIds.ToArray(), election.Users.Select(u => u.email).ToArray(), election.State);
        }
        
        [HttpGet("{electionId}/candidates")]
        public ElectionDTO? GetCandidates(string electionId)
        {
            var election = _repo.Get(electionId);
            if (election == null)
            {
                election = _repo.GetByUniqueUserId(electionId);
                if (election == null)
                {
                    Response.StatusCode = 404;
                    _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                    return null;
                }

                if (!election.UniqueIdsPerUser)
                {
                    Response.StatusCode = 404;
                    _logger.Log(LogLevel.Warning, "Attempting to get election with Id {election.ElectionId} by unique Id {uniqueId} failed, because UniqueIdesPerUser is not enabled", election.ElectionId, electionId);
                    return null;
                }
            } 
            
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
                _repo.Create(electionId);
                _logger.Log(LogLevel.Information, "New election created with Id {electionId}", electionId);
            }
            
            var election = _repo.Get(electionId);
            if (election == null)
            {
                _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                Response.StatusCode = 404;
                return false;
            }
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
                _repo.Create(electionId);
            }
            
            _logger.Log(LogLevel.Information, "Election with Id {electionId} Saved", electionId);

            var election = _repo.Get(electionId);
            if (election == null)
            {
                _logger.Log(LogLevel.Warning, "Election with Id {electionId} does not exist", electionId);
                Response.StatusCode = 404;
                return false;
            }
            var candidates = dto.candidates.Select(c => new Candidate(c.value, c.candidateId)).ToArray();
            election.Candidates = candidates;
            
            _repo.Save(electionId, election);
            
            return true;
        }
      
    }
}