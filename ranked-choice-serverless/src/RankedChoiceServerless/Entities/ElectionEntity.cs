using System;
using System.Collections.Generic;
using System.Linq;
using HelloWorld.Data;

namespace RankedChoiceServices.Entities
{
    public class ElectionEntity : IElection
    {
        public interface IElectionEvent
        {
            string ElectionId { get; }
        }
        public record SaveCandidatesEvent(string ElectionId, Candidate[] Candidates) : IElectionEvent;
        public record SaveSettingsEvent(string ElectionId, (bool uniqueIdPerUser, string electionName) settings) : IElectionEvent;
        public record SaveUserEmailsEvent(string ElectionId, string[] Emails) : IElectionEvent;
        public record SubmitVoteEvent(string ElectionId, Vote Vote) : IElectionEvent;
        public record StartElectionEvent(string ElectionId) : IElectionEvent;
        public record RestartElectionEvent(string ElectionId) : IElectionEvent;
        public record EndElectionEvent(string ElectionId) : IElectionEvent;
        public Stack<IElectionEvent> Events { get; }
        
        
        public string ElectionId { get; private set; }

        private List<IReadOnlyList<Candidate>> _history = new();
        public IReadOnlyList<IReadOnlyList<Candidate>> History => _history; 
        
        private List<Candidate> _candidates = new();
        public IReadOnlyList<Candidate> Candidates => _candidates;

        private List<Vote> _votes = new();
        public IReadOnlyList<Vote> Votes => _votes;

        private List<User> _users = new();
        public IReadOnlyList<User> Users => _users;

        public IEnumerable<string> UniqueElectionIds =>
            UniqueIdsPerUser ? Users.Select(u => u.userId) : new List<string>();

        public bool UniqueIdsPerUser
        {
            get;
            private set;
        }

        public ElectionState State
        {
            get;
            private set;
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

        public ElectionEntity(string electionId, IEnumerable<IElectionEvent> events)
        {
            ElectionId = electionId;
            Events = new Stack<IElectionEvent>(events);
            foreach (var @event in Events)
            {
                Reduce(@event);
            }
        }

        public bool Reduce(IElectionEvent electionEvent)
        {
            switch (electionEvent)
            {
                case SaveCandidatesEvent e:
                    if (State != ElectionState.New)
                    {
                        return false;
                    }
                    _candidates.Clear(); 
                    _candidates.AddRange(e.Candidates); 
                    return true;
                case SaveSettingsEvent e:
                    UniqueIdsPerUser = e.settings.uniqueIdPerUser;
                    
                    return true;
                case SaveUserEmailsEvent e:
                    //Can't add users to a finished election
                    if (State == ElectionState.Finished)
                    {
                        return false;
                    }

                    //Do a diff, and find the added emails
                    var added = e.Emails.Where(e => _users.All(u => u.email != e));
            
                    //concat the new ones onto thelist
                    var users = _users
                        .Where(u => e.Emails.Any(e => e == u.email))
                        .Concat(added.Select(e => new User(e, new Guid().ToString())));
            
                    _users.AddRange(users);
                    return true;
                case SubmitVoteEvent e:
                    //Finished and New elections can't have votes added to them
                    if (State != ElectionState.Started)
                    {
                        return false;
                    }
                    if (Votes.Any(v => v.userId == e.Vote.userId))
                    {
                        return false;
                    }

                    _votes.Add(e.Vote);
                    return true;
                case StartElectionEvent e:
                    switch (State)
                    {
                        case ElectionState.New:
                            State = ElectionState.Started;
                            return true;
                        case ElectionState.Started:
                        case ElectionState.Finished:
                            return false;
                    }

                    return false;
                case EndElectionEvent e:
                    switch (State)
                    {
                        case ElectionState.New:
                            return false;
                        case ElectionState.Started:
                            State = ElectionState.Finished;
                            _history.Add(CalculateResults().ToList());
                            return true;
                        case ElectionState.Finished:
                            return false;
                    }
                    return false;
                case RestartElectionEvent e:
                    switch (State)
                    {
                        case ElectionState.New:
                        case ElectionState.Started:
                            return false;
                        case ElectionState.Finished:
                            State = ElectionState.Started;
                            _votes.Clear();
                            return true;
                    }
                    return false;
            }

            return false;
        }

        bool Dispatch(IElectionEvent e)
        {
            Events.Push(e);
            return Reduce(e);
        }

        public bool SaveCandidates(IEnumerable<Candidate> candidates)
        {
            return Dispatch(new SaveCandidatesEvent(ElectionId, candidates.ToArray()));
        }
        public bool StartElection()
        {
            return Dispatch(new StartElectionEvent(ElectionId));
        }

        public bool StopElection()
        {
            return Dispatch(new EndElectionEvent(ElectionId));
        }
        
        public bool RestartElection()
        {
            return Dispatch(new RestartElectionEvent(ElectionId));
        }

        public bool SetUserEmails(string[] emails)
        {
            return Dispatch(new SaveUserEmailsEvent(ElectionId, emails));
        }

        public bool AddVote(Vote vote)
        {
            return Dispatch(new SubmitVoteEvent(ElectionId, vote));
        }

        public bool SaveSettings(bool uniqueIdsPerUser, string electionName)
        {
            return Dispatch(new SaveSettingsEvent(ElectionId, (uniqueIdsPerUser, electionName)));
        }
    }
}