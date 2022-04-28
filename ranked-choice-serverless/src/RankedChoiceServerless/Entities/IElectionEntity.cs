using System.Collections.Generic;
using HelloWorld.Data;

namespace RankedChoiceServices.Entities
{
    public class Candidate
    {
        public string value { get; set; }
        public string candidateId { get; set; }
    }

    public class User
    {
        public string email { get; set; }
    }

    public class Vote
    {
        public string userId { get; set; }
        public Candidate [] candidates { get; set; }
    }

    public interface IElection
    {
        public string ElectionId { get; }
        
        public string ElectionName { get; }
        
        //Query
        //TODO: Doesn't include metadata, like dates, and users, etc. 
        public IReadOnlyList<IReadOnlyList<Candidate>> History { get; }
        public IReadOnlyList<Candidate> Candidates { get; }
        public IReadOnlyList<User> Users { get; }
        public IReadOnlyList<Vote> Votes { get; }
        public bool UniqueIdsPerUser { get; }
        
        public ElectionState State { get; }
        public IEnumerable<Candidate> CalculateResults();

        //Commands
        public bool SaveCandidates(IEnumerable<Candidate> candidates);
        public bool AddVote(Vote vote);
        public bool StartElection();
        public bool StopElection();
        public bool RestartElection();
        public bool SaveSettings(bool uniqueIdsPerUser, string electionName);
        public bool SetUserEmails(string [] email);
    }
}