﻿using System;
using System.Collections.Generic;

namespace RankedChoiceServices.Entities
{
    public class VoteEntity : IVoteEntity
    {
        public interface IVoteEntityEvent : IEntityEvent
        {
            DateTime EventTime { get; }
            string ElectionId { get; }
            string UserId { get; }
        }

        public class SaveVoteEvent : IVoteEntityEvent
        {
            public string EventId { get; set; }
            public string ElectionId { get; set; }
            public string UserId { get; set; }
            public DateTime EventTime { get; set; }
            public Candidate[] Candidates { get; set; }
        }

        public class SubmitVoteEvent : IVoteEntityEvent
        { 
            public string EventId { get; set; }
            public string ElectionId { get; set; }
            public string UserId { get; set; }
            public DateTime EventTime { get; set; } 
        } 
        
        public string UserId { get; private set; }
        public string ElectionId { get; private set; }
        public IReadOnlyList<Candidate> Candidates { get; private set; }

        private bool _submitted;
        public bool Submitted
        {
            get => _submitted;
        }
   
        public Stack<IVoteEntityEvent> Events { get; }

        public VoteEntity(string userId, string electionId)
        {
            Candidates = new List<Candidate>();
            Events = new Stack<IVoteEntityEvent>();
            UserId = userId;
            ElectionId = electionId;
        }
        public VoteEntity(string userId, string electionId, IEnumerable<IVoteEntityEvent> events) 
            : this(userId, electionId)
        {
            Events = new Stack<IVoteEntityEvent>(events);
            
            foreach (var voteEntityEvent in events)
            {
                Reduce(voteEntityEvent);
            }
        }

        bool Reduce(IVoteEntityEvent @event)
        {
            switch (@event)
            {
                case SaveVoteEvent e:
                    if (Submitted)
                    {
                        return false;
                    }

                    Candidates = e.Candidates;
                    return true;
                case SubmitVoteEvent e:
                    if (Submitted)
                    {
                        return false;
                    }
                    _submitted = true;
                    return true;
            }

            return false;
        }

        bool Dispatch(IVoteEntityEvent e)
        {
            if (Reduce(e))
            {
                Events.Push(e);
                return true;
            }

            return false;
        }

        public bool SubmitVote()
        {
            return Dispatch(new SubmitVoteEvent{EventTime = DateTime.Now, EventId = EntityId.Generate(), ElectionId = ElectionId, UserId = UserId});
        }

        public bool SaveVote(Candidate[] candidates)
        {
            return Dispatch(new SaveVoteEvent{EventTime = DateTime.Now, Candidates = candidates, EventId = EntityId.Generate(), ElectionId = ElectionId, UserId = UserId});
        }
    }
}