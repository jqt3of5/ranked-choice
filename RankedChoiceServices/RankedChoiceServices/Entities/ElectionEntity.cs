using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace RankedChoiceServices.Entities
{
    public record Candidate(string value, string candidateId);

    public record User(string email, string userId);

    public record Vote(string userId, Candidate [] candidates);

    public interface IElection
    {
        //TODO: Doesn't include metadata, like dates, and users, etc. 
        public IReadOnlyList<IReadOnlyList<Candidate>> History { get; }
        public IReadOnlyList<Candidate> Candidates { get; set;  }
        public IReadOnlyList<User> Users { get; }
        public IReadOnlyList<Vote> Votes { get; }
        public IEnumerable<string> UniqueElectionIds { get; }
        public bool UniqueIdsPerUser { get; set; }
        
        public enum ElectionState
        {
            New, Started, Finished
        }
        public ElectionState State { get; }

        public bool StartElection();
        public bool StopElection();
        public bool RestartElection();
        
        public bool SetUserEmails(string[] emails);
        public IEnumerable<Candidate> CalculateResults();
    }
    
    public class ElectionEntity : IElection
    {
        private string ElectionId
        {
            get;
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
            _users.Clear();
            
            var users = emails.Select(e => new User(e, new Guid().ToString()));
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
            //TODO: Do this 
            return _candidates;
        }
    }
}