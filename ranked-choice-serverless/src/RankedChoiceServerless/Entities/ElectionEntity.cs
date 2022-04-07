using System;
using System.Collections.Generic;
using System.Linq;

namespace RankedChoiceServices.Entities
{
    public class ElectionEntity : IElection
    {
        public string ElectionId
        {
            get;
            set;
        }

        private List<IReadOnlyList<Candidate>> _history = new();
        public IReadOnlyList<IReadOnlyList<Candidate>> History => _history; 
        
        private List<Candidate> _candidates = new();
        public IReadOnlyList<Candidate> Candidates
        {
            get => _candidates;
            set
            {
                if (State != IElection.ElectionState.New)
                {
                    return;
                }
                _candidates.Clear(); 
                _candidates.AddRange(value);
            }
        }

        private List<Vote> _votes = new();
        public IReadOnlyList<Vote> Votes => _votes;

        private List<User> _users = new();
        public IReadOnlyList<User> Users => _users;

        public IEnumerable<string> UniqueElectionIds =>
            UniqueIdsPerUser ? Users.Select(u => u.userId) : new List<string>();

        public bool UniqueIdsPerUser
        {
            get; set;
        }

        public IElection.ElectionState State
        {
            get;
            private set;
        }

        public ElectionEntity(string electionId)
        {
            ElectionId = electionId;
            UniqueIdsPerUser = false;
        }

        public bool StartElection()
        {
            switch (State)
            {
                case IElection.ElectionState.New:
                    State = IElection.ElectionState.Started;
                    return true;
                case IElection.ElectionState.Started:
                case IElection.ElectionState.Finished:
                    return false;
            }

            return false;
        }

        public bool StopElection()
        {
            switch (State)
            {
                case IElection.ElectionState.New:
                    return false;
                case IElection.ElectionState.Started:
                    State = IElection.ElectionState.Finished;
                    return true;
                case IElection.ElectionState.Finished:
                    return false;
            }

            return false;
        }
        
        public bool RestartElection()
        {
            switch (State)
            {
                case IElection.ElectionState.New:
                case IElection.ElectionState.Started:
                    return false;
                case IElection.ElectionState.Finished:
                    State = IElection.ElectionState.Started;
                    return true;
            }
            
            _history.Add(CalculateResults().ToList());
            _votes.Clear();

            return false;
        }

        public bool SetUserEmails(string[] emails)
        {
            //Can't add users to a finished election
            if (State == IElection.ElectionState.Finished)
            {
                return false;
            }

            //Do a diff, and find the added emails
            var added = emails.Where(e => _users.All(u => u.email != e));
            
            //concat the new ones onto thelist
            var users = _users
                .Where(u => emails.Any(e => e == u.email))
                .Concat(added.Select(e => new User(e, new Guid().ToString())));
            
            _users.AddRange(users);
            return true;
        }

        public bool AddVote(Vote vote)
        {
            //Finished and New elections can't have votes added to them
            if (State != IElection.ElectionState.Started)
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
        
        public IEnumerable<Candidate> CalculateResults()
        {
            if (_candidates.Count() <= 1)
            {
                return _candidates;
            }

            Dictionary<string, int> counts = _candidates.ToDictionary(c => c.candidateId, c => 0);
            for (int round = 0; round < _candidates.Count; ++round)
            {
                var votes = _votes
                //Some votes might not have picked all candidates
                    .Where(v => v.candidates.Count() > round)
                //Group them by their selected candidate for this round
                    .GroupBy(v => v.candidates[round].candidateId)
                    .ToList();

                votes.Sort((a, b) =>
                {
                    return a.Count() - b.Count();
                });
                
            }

            //TODO: this isn't right
            return _candidates;
        }
    }
}