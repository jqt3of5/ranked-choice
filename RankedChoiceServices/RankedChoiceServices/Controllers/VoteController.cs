using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RankedChoiceServices.Data;

namespace RankedChoiceServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VoteController
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
        public UserVote SaveVote(string electionId, [FromBody] Candidate[] choices)
        {
            _voteRepository.SaveUserVote("1234", electionId, choices);

            return _voteRepository.GetVote("1234", electionId);
        }
        
        [HttpPost("{electionId}/vote")]
        public void SubmitVote(string electionId)
        {
           _voteRepository.SubmitVote("1234", electionId); 
        }
    }
}