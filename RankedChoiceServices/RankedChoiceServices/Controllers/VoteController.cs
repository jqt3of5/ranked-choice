using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RankedChoiceServices.Entities;

namespace RankedChoiceServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VoteController : ControllerBase
    {
        public record VoteDTO(bool submitted, ElectionController.CandidateDTO[] candidates);
        
        private readonly ILogger<VoteController> _logger;
        private readonly ElectionRepository _electionRepository;
        private readonly VoteRepository _voteRepository;

        public VoteController(ILogger<VoteController> logger, ElectionRepository electionRepository, VoteRepository voteRepository)
        {
            _logger = logger;
            _electionRepository = electionRepository;
            _voteRepository = voteRepository;
        }
        
        [HttpPost("{electionId}")]
        public bool SaveCandidates(string electionId, [FromBody] VoteDTO vote, [FromHeader(Name = "userId")] string userId)
        {
            var entity = _voteRepository.GetForUser(userId, electionId);
            if (entity == null)
            {
                _logger.Log(LogLevel.Warning, "Vote for user {userId} for election {electionId}", userId, electionId);
                entity = _voteRepository.Create(userId, electionId);
            }

            if (entity.Submitted == true)
            {
                return false;
            }

            entity.Candidates = vote.candidates.Select(c => new Candidate(c.value, c.candidateId)).ToList();
             
            _voteRepository.SaveForUser(userId, electionId, entity);

            return true;
        }
        
        [HttpGet("{electionId}")]
        public VoteDTO? GetCandidates(string electionId,  [FromHeader(Name = "userId")] string userId)
        {
            var entity = _voteRepository.GetForUser(userId, electionId);
            if (entity == null)
            {
                _logger.Log(LogLevel.Warning, "Vote for user {userId} for election {electionId}", userId, electionId);
                entity = _voteRepository.Create(userId, electionId);
            }

            return new VoteDTO(entity.Submitted, entity.Candidates.Select(e => new ElectionController.CandidateDTO(e.value, e.candidateId)).ToArray());
        }

        [HttpGet("{electionId}/submit")]
        public bool SubmitVote(string electionId, [FromHeader(Name = "userId")]string userId)
        {
            var entity = _voteRepository.GetForUser(userId, electionId);
            if (entity == null)
            {
                _logger.Log(LogLevel.Warning, "Vote for user {userId} for election {electionId}", userId, electionId);
                Response.StatusCode = 404;
                return false;
            }

            entity.Submitted = true;

            _voteRepository.SaveForUser(userId, electionId, entity);

            return true;
        }
    }
}