using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RankedChoiceServices.Data;

namespace RankedChoiceServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VoteController : ControllerBase
    {
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
        public UserVote SaveCandidateSelection(string electionId, [FromBody] UserVote vote)
        {
            if (vote.electionId != electionId)
            {
                HttpContext.Response.StatusCode = 400;
                return vote;
            }
            
            //TODO: Make sure the user is correct too
            
            
            //If the user has already submitted their vote, do nothing but return to them their old vote. 
            if (_voteRepository.UserVoteSubmitted(vote.userId, vote.electionId))
            {
                return _voteRepository.GetVote(vote.userId, vote.electionId);
            }
            
            //This endpoint cannot be used to submit a vote
            vote.submitted = false;
            
            if (_electionRepository.Get(electionId) is {} election)
            {
                //We want to add a little security, this prevents users from sending invalid candidates. 
                var candidates = vote.choices 
                    .Select(candidate => election.candidates.FirstOrDefault(c => c.candidateId == candidate.candidateId))
                    .Where(candidate => candidate != null)
                    //Removes the nullable because we just filtered
                    .Cast<Candidate>()
                    .ToArray();
            
                //TODO: Deduplicate vote list?
                
                _voteRepository.SaveUserVote(vote.userId, vote.electionId, new UserVote(vote.electionId, vote.userId, candidates));

                _logger.Log(LogLevel.Trace, "Vote saved for user {userId} for election {electionId}", vote.userId, vote.electionId);
                return _voteRepository.GetVote(vote.userId, vote.electionId); 
            }

            _logger.Log(LogLevel.Trace, "Vote note saved for user {userId} electionId {electionId} not found", vote.userId, vote.electionId);
            HttpContext.Response.StatusCode = 404;
            return null;
        }
        
        [HttpPost("{electionId}/vote")]
        public void SubmitVote(string electionId, [FromBody] UserVote vote)
        {
            _logger.Log(LogLevel.Trace, "Vote submitted for user {userId} for election {electionId}", vote.userId, vote.electionId);
           _voteRepository.SubmitVote(vote.userId, vote.electionId); 
        }

        [HttpGet("{electionId}/{userId}")]
        public UserVote GetVote(string electionId, string userId)
        {
            return _voteRepository.GetVote(userId, electionId);
        }
    }
}