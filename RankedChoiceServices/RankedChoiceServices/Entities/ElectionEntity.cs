using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace RankedChoiceServices.Entities
{
    public record Candidate(string value, string candidateId);

    public record User(string email, string userId);

    public record Vote(string userId, Candidate [] candidates); 
    
    public class ElectionEntity
    {
        private string ElectionId
        {
            get;
        }

        private List<Candidate> _candidates = new();
        public IReadOnlyList<Candidate> Candidates => _candidates;

        private List<Vote> _votes = new();
        public IReadOnlyList<Vote> Votes => _votes;

        private List<User> _users = new();
        private IReadOnlyList<User> Users => _users;

        public IEnumerable<string> UniqueElectionIds =>
            UniqueIdsPerUser ? Users.Select(u => u.userId) : new List<string>();

        public bool UniqueIdsPerUser
        {
            get; set;
        }

        public enum ElectionState
        {
            New, Started, Finished
        }

        public ElectionState State
        {
            get;
            private set;
        }

        public ElectionEntity(string electionId)
        {
            ElectionId = electionId;
            UniqueIdsPerUser = false;
        }


        public void StartElection()
        {
            State = ElectionState.Started;
        }

        public void StopElection()
        {
            State = ElectionState.Finished;
        }

        public bool AddUser(string email)
        {
            //Can't add users to a finished election
            if (State == ElectionState.Finished)
            {
                return false;
            }
            //Don't readd theuser twice
            if (_users.Any(u => u.email == email))
            {
                return false;
            }
            
            var user = new User(email, new Guid().ToString());

            _users.Add(user);
            return true;
        }

        public bool AddVote(Vote vote)
        {
            //Finished and New elections can't have votes added to them
            if (State != ElectionState.Started)
            {
                return false;
            }
            if (Votes.Any(v => v.userId == vote.userId))
            {
                return false;
            }
            
            _votes.Add(vote);
            return true;
        }

        public bool SetCandidates(Candidate[] candidates)
        {
            if (State != ElectionState.New)
            {
                return false;
            }
            
            _candidates.Clear();
            _candidates.AddRange(candidates);
            return true;
        }
        
        public IEnumerable<Candidate> CalculateResults()
        {
            Dictionary<string, int> candidateCounts = new Dictionary<string, int>();
            foreach (var candidate in _candidates)
            {
                candidateCounts[candidate.candidateId] = 0;
            }
            
            for (int i = 0; i < _candidates.Count; ++i)
            {
                foreach (var vote in _votes)
                {
                    //Voters don't have to vote for all candidates
                    if (i >= vote.candidates.Length)
                    {
                        continue;
                    }
                }
                
                //Any have a majority?
                foreach (var candidateCount in candidateCounts)
                {
                    if (candidateCount.Value > _candidates.Count / 2)
                    {
                        
                    }
                }
            }
        }
    }
}