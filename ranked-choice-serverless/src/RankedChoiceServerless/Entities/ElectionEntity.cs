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

    public class CreateElectionEvent : IElectionEvent
    {
        public string ElectionId { get; set;}
        public string EventId { get; set; }
        public DateTime EventTime { get; set;}
        public string OwnerUserId { get; set;}
    }

    public class SaveCandidatesEvent : IElectionEvent
    {
        public string ElectionId{ get; set;}
        public string EventId{ get; set;}
        public DateTime EventTime{ get; set;}
        public Candidate[] Candidates { get; set;}
    }

    public class SaveSettingsEvent : IElectionEvent
    {
        public class ElectionSettings
        {
            public bool UniqueIdPerUser { get; set; }
            public string ElectionName{ get; set; }
            
        }
        public string ElectionId { get; set;}
        public string EventId { get; set;}
        public DateTime EventTime { get; set;}
        public ElectionSettings Settings { get; set;}
    }

    public class SaveUserEmailsEvent : IElectionEvent
    {
        public string ElectionId{ get; set;}
        public string EventId{ get; set;}
        public DateTime EventTime{ get; set;}
        public string[] Emails { get; set;}
    }

    public class SubmitVoteEvent : IElectionEvent
    {
        public string ElectionId { get; set;}
        public string EventId { get; set;}
        public DateTime EventTime { get; set;}
        public Vote Vote { get; set;}
    }

    public class StartElectionEvent : IElectionEvent
    {
        public string ElectionId { get; set;}
        public string EventId { get; set;}
        public DateTime EventTime { get; set;}
    }

    public class RestartElectionEvent : IElectionEvent
    {
        public string ElectionId { get; set;}
        public string EventId { get; set;}
        public DateTime EventTime { get; set;}
    }

    public class EndElectionEvent : IElectionEvent
    {
        public string ElectionId { get; set;} 
        public string EventId { get; set;}
        public DateTime EventTime { get; set;}
    }

    public class ElectionEntity : IElection, IEntity<IElectionEvent>
    {
        public Stack<IElectionEvent> Events { get; }
        
        public string ElectionId { get; private set; }
        
        public string ElectionName { get; private set; }
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
            Events = new Stack<IElectionEvent>();
            Dispatch(new CreateElectionEvent{ ElectionId = electionId, EventId = EntityId.Generate(), EventTime = DateTime.Now, OwnerUserId = ownerUserId});
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
                    UniqueIdsPerUser = e.Settings.UniqueIdPerUser;
                    ElectionName = e.Settings.ElectionName;
                    
                    return true;
                case SaveUserEmailsEvent e:
                    //Can't add users to a finished election
                    if (State == ElectionState.Finished)
                    {
                        return false;
                    } 
                    _users.Clear();
                    _users.AddRange(e.Emails.Select(e => new User{email = e}));

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

                    //Invalid candidate id
                    if (e.Vote.candidates.Any(c => Candidates.All(can => can.candidateId != c.candidateId)))
                    {
                        return false;
                    }

                    _votes.Add(e.Vote);
                    return true;
                case CreateElectionEvent e:
                    if (Events.Any())
                    {
                        return false;
                    }

                    ElectionId = e.ElectionId;
                    OwnerUserId = e.OwnerUserId;
                    State = ElectionState.New;
                    return true;
                case StartElectionEvent e:
                    switch (State)
                    {
                        case ElectionState.New:
                        case ElectionState.Reset:
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
                        case ElectionState.Reset:
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
                        case ElectionState.Reset:
                        case ElectionState.Started:
                            return false;
                        case ElectionState.Finished:
                            State = ElectionState.Reset;
                            _votes.Clear();
                            return true;
                    }
                    return false;
            }

            return false;
        }

        bool Dispatch(IElectionEvent e)
        {
            if (Reduce(e))
            {
                Events.Push(e);
                return true;
            }

            return false;
        }

        public bool SaveCandidates(IEnumerable<Candidate> candidates)
        {
            return Dispatch(new SaveCandidatesEvent{ElectionId = ElectionId, EventId = EntityId.Generate(), EventTime = DateTime.Now, Candidates = candidates.ToArray()});
        }
        public bool StartElection()
        {
            return Dispatch(new StartElectionEvent{ElectionId = ElectionId, EventId = EntityId.Generate(), EventTime = DateTime.Now});
        }

        public bool StopElection()
        {
            return Dispatch(new EndElectionEvent{ElectionId = ElectionId, EventId = EntityId.Generate(), EventTime = DateTime.Now});
        }
        
        public bool RestartElection()
        {
            return Dispatch(new RestartElectionEvent{ElectionId = ElectionId, EventId = EntityId.Generate(), EventTime = DateTime.Now});
        }

        public bool SetUserEmails(string [] emails)
        {
            return Dispatch(new SaveUserEmailsEvent{ElectionId = ElectionId, EventId = EntityId.Generate(), EventTime = DateTime.Now, Emails = emails});
        }

        public bool AddVote(Vote vote)
        {
            return Dispatch(new SubmitVoteEvent{ElectionId = ElectionId, EventId = EntityId.Generate(), EventTime = DateTime.Now, Vote = vote});
        }

        public bool SaveSettings(bool uniqueIdsPerUser, string electionName)
        {
            return Dispatch(new SaveSettingsEvent{ElectionId = ElectionId, EventId = EntityId.Generate(), EventTime = DateTime.Now, Settings = new SaveSettingsEvent.ElectionSettings{UniqueIdPerUser = uniqueIdsPerUser, ElectionName = electionName}});
        }
    }
}