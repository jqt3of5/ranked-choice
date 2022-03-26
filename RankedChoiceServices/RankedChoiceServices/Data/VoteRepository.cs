using System.Collections.Generic;
using System.Linq;

namespace RankedChoiceServices.Data
{
    public record UserVote(string electionId, string userId, Candidate[] choices)
    {
        public bool submitted
        {
            get;
            set;
        }
    }
    public class VoteRepository
    {
        private Dictionary<(string, string), UserVote> _userVotes = new ();
        
        public void SaveUserVote(string userId, string electionId, IEnumerable<Candidate> rankedChoices)
        {
            _userVotes[(userId, electionId)] = new UserVote(electionId, userId, rankedChoices.ToArray());
        }

        public void SubmitVote(string userId, string electionId)
        {
            _userVotes[(userId, electionId)].submitted = true;
        }

        public UserVote GetVote(string userId, string electionId)
        {
            return _userVotes[(userId, electionId)];
        }
        
    }
}