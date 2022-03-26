using System.Collections.Generic;

namespace RankedChoiceServices.Data
{
    public record Candidate(string electionId, string candidateId, string value);
    public class ElectionRepository
    {
        private Dictionary<string, IEnumerable<Candidate>> _elections = new();
        
        public void SaveElection(string electionId, IEnumerable<Candidate> candidates)
        {
            _elections[electionId] = candidates;
        }

        public bool ElectionExists(string electionId)
        {
            return _elections.ContainsKey(electionId);
        }
        
        public IEnumerable<Candidate> GetElection(string electionId)
        {
            return _elections[electionId];
        }
    }
}