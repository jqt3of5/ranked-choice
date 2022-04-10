﻿using System;
using System.Collections.Generic;
using System.Linq;
using HelloWorld.Data;

namespace RankedChoiceServices.Entities
{
    public class ElectionEntity : IElection
    {
        public interface IElectionEvent
        {
            DateTime EventTime { get; }
        }
        
        public record SaveCandidatesEvent(DateTime EventTime, Candidate[] Candidates) : IElectionEvent;
        public record SaveSettingsEvent(DateTime EventTime,(bool uniqueIdPerUser, string electionName) Settings) : IElectionEvent;
        public record SaveUserEmailsEvent(DateTime EventTime, (string email, string userId) [] Users) : IElectionEvent;
        public record SubmitVoteEvent(DateTime EventTime, Vote Vote) : IElectionEvent;

        public record CreateElectionEvent(DateTime EventTime, string OwnerUserId) : IElectionEvent;
        public record StartElectionEvent(DateTime EventTime) : IElectionEvent;
        public record RestartElectionEvent(DateTime EventTime) : IElectionEvent;
        public record EndElectionEvent(DateTime EventTime) : IElectionEvent;
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
        private IElection _electionImplementation;
        public IReadOnlyList<User> Users => _users;

        public IEnumerable<string> UniqueElectionIds => _users.Select(u => u.userId);

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
            OwnerUserId = string.Empty;
            Events = new Stack<IElectionEvent>();

            Dispatch(new CreateElectionEvent(DateTime.Now, ownerUserId));
        }
        public ElectionEntity(string electionId, IEnumerable<IElectionEvent> events)
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

                    //We need to generate guids for new users, but not existing ones.
                    //Do a diff, and find the added emails
                    var added = e.Users.Where(e => _users.All(u => u.email != e.email)).Select(e => new User(e.email, e.userId)).ToList();
            
                    //concat the new ones onto the list, and maintain a specific order since we have parallel arrays
                    var users = _users
                        .Where(u => e.Users.Any(e => e.email == u.email))
                        .Concat(added);
            
                    _users.Clear();
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
                case CreateElectionEvent e:
                    if (!string.IsNullOrEmpty(OwnerUserId))
                    {
                        return false;
                    }
                    OwnerUserId = e.OwnerUserId;
                    State = ElectionState.New;
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
            return Dispatch(new SaveCandidatesEvent(DateTime.Now, candidates.ToArray()));
        }
        public bool StartElection()
        {
            return Dispatch(new StartElectionEvent(DateTime.Now));
        }

        public bool StopElection()
        {
            return Dispatch(new EndElectionEvent(DateTime.Now));
        }
        
        public bool RestartElection()
        {
            return Dispatch(new RestartElectionEvent(DateTime.Now));
        }

        public bool SetUserEmails((string email, string userId) [] users)
        {
            return Dispatch(new SaveUserEmailsEvent(DateTime.Now, users));
        }

        public bool AddVote(Vote vote)
        {
            return Dispatch(new SubmitVoteEvent(DateTime.Now, vote));
        }

        public bool SaveSettings(bool uniqueIdsPerUser, string electionName)
        {
            return Dispatch(new SaveSettingsEvent(DateTime.Now, (uniqueIdsPerUser, electionName)));
        }
    }
}