using System.Collections.Generic;

namespace RankedChoiceServices.Entities
{
    public class ElectionRepository
    {
        private Dictionary<string, ElectionEntity> _elections = new();
        
        public void Save(string electionId, IElection election)
        {
            //TODO: We shouldn't make this assumption. But there is a lot more work to be done here any way
            _elections[electionId] = election as ElectionEntity;
        }

        public bool Exists(string electionId)
        {
            return _elections.ContainsKey(electionId);
        }
        
        public IElection Get(string electionId)
        {
            if (!_elections.TryGetValue(electionId, out var election))
            {
                election = new ElectionEntity(electionId);
                _elections[electionId] = election;
            }
            return election;
        }
    }
}