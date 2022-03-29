using System;
using System.Collections.Generic;

namespace RankedChoiceServices.Data
{
    public record Candidate(string electionId, string candidateId, string value);

    public record ElectionSettings(string electionId, string []voterEmails, bool uniqueUrl, string[] uniqueElectionIds);
    public enum ElectionState {New, Started, Finished}
    public record Election(string electionId, ElectionState state, ElectionSettings settings,  IEnumerable<Candidate> candidates);
    
    public class ElectionRepository
    {
        private Dictionary<string, Election> _elections = new();
        
        public void Save(string electionId, Election election)
        {
            _elections[electionId] = election;
        }

        public bool Exists(string electionId)
        {
            return _elections.ContainsKey(electionId);
        }
        
        public Election? Get(string electionId)
        {
            if (_elections.TryGetValue(electionId, out var election))
            {
                return election;
            }

            return null;
        }
    }
}