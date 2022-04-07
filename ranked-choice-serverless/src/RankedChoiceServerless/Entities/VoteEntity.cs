using System.Collections.Generic;

namespace RankedChoiceServices.Entities
{
   
    public class VoteEntity : IVoteEntity
    {
        public string UserId { get; set; }
        public string ElectionId { get; set; }

        public IReadOnlyList<Candidate> Candidates { get; set; }

        private bool _submitted;

        public bool Submitted
        {
            get => _submitted;
            set
            {
                //Cannot set to false if true
                if (!_submitted)
                {
                    _submitted = value;
                }
            }
        }

        public VoteEntity(string userId, string electionId)
        {
            UserId = userId;
            ElectionId = electionId;
            Candidates = new List<Candidate>();
        }
    }
}