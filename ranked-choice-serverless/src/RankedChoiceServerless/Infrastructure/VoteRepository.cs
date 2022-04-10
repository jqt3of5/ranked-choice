using System.Collections.Generic;

namespace RankedChoiceServices.Entities
{
    public class VoteRepository
    {
        private Dictionary<string, VoteEntity> _dictionary = new();

        public bool Exists(string userId, string electionId)
        {
            return _dictionary.ContainsKey(userId + electionId);
        }

        public VoteEntity Create(string userId, string electionId)
        {
            var entity = new VoteEntity(userId, electionId, new VoteEntity.IVoteEntityEvent[] {});
            
            _dictionary[userId + electionId] = entity;
            
            return entity;
        }
        public VoteEntity? GetForUser(string userId, string electionId)
        {
            if (_dictionary.TryGetValue(userId + electionId, out var entity))
            {
                return entity;
            }

            return null;
        }

      
        public bool SaveForUser(string userId, string electionId, VoteEntity entity)
        {
            _dictionary[userId + electionId] = entity;
            return true;
        }
        
    }
}