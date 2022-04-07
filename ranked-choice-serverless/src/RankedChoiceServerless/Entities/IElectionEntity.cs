using System.Collections.Generic;
using HelloWorld.Data;

namespace RankedChoiceServices.Entities
{
    public record Candidate(string value, string candidateId);

    public record User(string email, string userId);

    public record Vote(string userId, Candidate [] candidates);

    public interface IElection
    {
        public string ElectionId { get; }
        
        //TODO: Doesn't include metadata, like dates, and users, etc. 
        public IReadOnlyList<IReadOnlyList<Candidate>> History { get; }
        public IReadOnlyList<Candidate> Candidates { get; set;  }
        public IReadOnlyList<User> Users { get; }
        public IReadOnlyList<Vote> Votes { get; }
        public IEnumerable<string> UniqueElectionIds { get; }
        public bool UniqueIdsPerUser { get; set; }
        
        public ElectionState State { get; }

        public bool AddVote(Vote vote);
        public bool StartElection();
        public bool StopElection();
        public bool RestartElection();
        
        public bool SetUserEmails(string[] emails);
        public IEnumerable<Candidate> CalculateResults();
    }
}