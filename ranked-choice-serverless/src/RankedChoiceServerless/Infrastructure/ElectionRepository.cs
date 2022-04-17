using System.Collections.Generic;

namespace RankedChoiceServices.Entities
{
    public class ElectionRepository
    {
        private Dictionary<string, ElectionEntity> _elections = new();
        private Dictionary<string, ElectionEntity> _usersIndex = new();
        
        public void Save(string electionId, IElection election)
        {
            //TODO: We shouldn't make this assumption. But there is a lot more work to be done here any way
            if (election is ElectionEntity entity)
            {
                _elections[electionId] = entity;
            }
        }

        public bool Exists(string electionId)
        {
            return _elections.ContainsKey(electionId);
        }

        public IElection Create(string electionId, string ownerUserId)
        {
            var election = new ElectionEntity(electionId, ownerUserId);
            _elections[electionId] = election;
            return election;
        }

        public IElection? GetByUniqueUserId(string uniqueId)
        {
            //TODO: How should this work?
            if (_usersIndex.TryGetValue(uniqueId, out var election2))
            {
                return election2;
            }

            return null;
        }
        public IElection? Get(string electionId)
        {
            if (_elections.TryGetValue(electionId, out var election))
            {
                return election;
            }
            
            return null;
        }
    }
}