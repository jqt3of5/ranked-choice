using System.Collections.Generic;

namespace RankedChoiceServices.Entities
{
    public interface IVoteEntity
    {
        public string UserId { get; }
        public string ElectionId { get; }
        public IReadOnlyList<Candidate> Candidates { get; set; } 
        public bool Submitted { get; set; }
    }
}