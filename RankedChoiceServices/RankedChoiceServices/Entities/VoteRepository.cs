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

        public bool UserVoteSubmitted(string userId, string electionId)
        {
            return GetVote(userId, electionId)?.submitted ?? false;
        }
        public void SaveUserVote(string userId, string electionId, UserVote vote)
        {
            _userVotes[(userId, electionId)] = vote;
        }

        public void SubmitVote(string userId, string electionId)
        {
            _userVotes[(userId, electionId)].submitted = true;
        }

        public UserVote GetVote(string userId, string electionId)
        {
            if (!_userVotes.ContainsKey((userId, electionId)))
            {
                _userVotes[(userId, electionId)] = new UserVote(electionId, userId, new Candidate[] { });
            }
            return _userVotes[(userId, electionId)];
        }
        
    }
}