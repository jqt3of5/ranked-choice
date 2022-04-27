using System;
using System.Collections.Generic;
using System.Linq;
using HelloWorld.Data;

namespace RankedChoiceServices.Entities
{
    public interface IElectionEvent : IEntityEvent
    {
        public string ElectionId { get; }
    }
    
    public record CreateElectionEvent(string ElectionId, string EventId, DateTime EventTime) : IElectionEvent;
    public record SaveCandidatesEvent(string ElectionId, string EventId, DateTime EventTime, Candidate[] Candidates) : IElectionEvent;
    public record SaveSettingsEvent(string ElectionId, string EventId, DateTime EventTime,(bool uniqueIdPerUser, string electionName) Settings) : IElectionEvent;
    public record SaveUserEmailsEvent(string ElectionId, string EventId, DateTime EventTime, string [] Emails) : IElectionEvent;
    public record SubmitVoteEvent(string ElectionId, string EventId, DateTime EventTime, Vote Vote) : IElectionEvent;
    public record CreateEntityEvent(string ElectionId, string EventId, DateTime EventTime, string OwnerUserId) : IElectionEvent;
    public record StartEntityEvent(string ElectionId, string EventId, DateTime EventTime) : IElectionEvent;
    public record RestartEntityEvent(string ElectionId, string EventId, DateTime EventTime) : IElectionEvent;
    public record EndEntityEvent(string ElectionId, string EventId, DateTime EventTime) : IElectionEvent;
    
    public class ElectionEntity : IElection, IEntity<IElectionEvent>
    {
        public Stack<IElectionEvent> Events { get; }
        
        public string ElectionId { get; private set; }
        public string OwnerUserId { get; private set; }

        private List<IReadOnlyList<Candidate>> _history = new();
        public IReadOnlyList<IReadOnlyList<Candidate>> History => _history; 
        
        private List<Candidate> _candidates = new();
        public IReadOnlyList<Candidate> Candidates => _candidates;

        private List<Vote> _votes = new();
        public IReadOnlyList<Vote> Votes => _votes;

        private List<User> _users = new();
        public IReadOnlyList<User> Users => _users;

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

        public ElectionEntity(string electionId, string ownerUserId)
        {
            ElectionId = electionId;
            OwnerUserId = ownerUserId;
            Events = new Stack<IElectionEvent>();

            Dispatch(new CreateEntityEvent(electionId, EntityId.Generate() ,DateTime.Now, ownerUserId));
        }
        public ElectionEntity(string electionId, IReadOnlyList<IElectionEvent> events)
        {
            ElectionId = electionId;
            OwnerUserId = string.Empty;
            Events = new Stack<IElectionEvent>(events);
            if (!events.Any())
            {
                throw new ArgumentException("events cannot be empty");
            }
            foreach (var @event in Events)
            {
                Reduce(@event);
            }
        }

        public bool Reduce(IEntityEvent entityEvent)
        {
            switch (entityEvent)
            {
                case CreateElectionEvent e:
                    return true;
                case SaveCandidatesEvent e:
                    if (State != ElectionState.New)
                    {
                        return false;
                    }
                    _candidates.Clear(); 
                    _candidates.AddRange(e.Candidates); 
                    return true;
                case SaveSettingsEvent e:
                    if (State != ElectionState.New)
                    {
                        return false;
                    }
                    UniqueIdsPerUser = e.Settings.uniqueIdPerUser;
                    
                    return true;
                case SaveUserEmailsEvent e:
                    //Can't add users to a finished election
                    if (State == ElectionState.Finished)
                    {
                        return false;
                    } 
                    _users.Clear();
                    _users.AddRange(e.Emails.Select(e => new User(e)));

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
                case CreateEntityEvent e:
                    if (!string.IsNullOrEmpty(OwnerUserId))
                    {
                        return false;
                    }
                    OwnerUserId = e.OwnerUserId;
                    State = ElectionState.New;
                    return true;
                case StartEntityEvent e:
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
                case EndEntityEvent e:
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
                case RestartEntityEvent e:
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
            return Dispatch(new SaveCandidatesEvent(ElectionId, EntityId.Generate(), DateTime.Now, candidates.ToArray()));
        }
        public bool StartElection()
        {
            return Dispatch(new StartEntityEvent(ElectionId, EntityId.Generate(), DateTime.Now));
        }

        public bool StopElection()
        {
            return Dispatch(new EndEntityEvent(ElectionId, EntityId.Generate(), DateTime.Now));
        }
        
        public bool RestartElection()
        {
            return Dispatch(new RestartEntityEvent(ElectionId, EntityId.Generate(), DateTime.Now));
        }

        public bool SetUserEmails(string [] emails)
        {
            return Dispatch(new SaveUserEmailsEvent(ElectionId, EntityId.Generate(), DateTime.Now, emails));
        }

        public bool AddVote(Vote vote)
        {
            return Dispatch(new SubmitVoteEvent(ElectionId, EntityId.Generate(), DateTime.Now, vote));
        }

        public bool SaveSettings(bool uniqueIdsPerUser, string electionName)
        {
            return Dispatch(new SaveSettingsEvent(ElectionId, EntityId.Generate(), DateTime.Now, (uniqueIdsPerUser, electionName)));
        }
    }
}