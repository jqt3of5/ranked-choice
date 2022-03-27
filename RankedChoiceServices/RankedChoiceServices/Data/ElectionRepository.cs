using System;
using System.Collections.Generic;

namespace RankedChoiceServices.Data
{
    public record Candidate(string electionId, string candidateId, string value);

    public record Election(string electionId, IEnumerable<Candidate> candidates);
    
    public class ElectionRepository
    {
        private Dictionary<string, Election> _elections = new();
        
        public void SaveElection(string electionId, Election election)
        {
            _elections[electionId] = election;
        }

        public bool ElectionExists(string electionId)
        {
            return _elections.ContainsKey(electionId);
        }
        
        public Election? GetElection(string electionId)
        {
            if (_elections.TryGetValue(electionId, out var election))
            {
                return election;
            }

            return null;
        }
    }
}