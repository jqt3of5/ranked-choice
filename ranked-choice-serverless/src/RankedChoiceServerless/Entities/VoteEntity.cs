using System;
using System.Collections.Generic;

namespace RankedChoiceServices.Entities
{
   
    public class VoteEntity : IVoteEntity
    {
        public interface IVoteEntityEvent
        {
            DateTime EventTime { get; }
        }

        public record SaveVoteEvent(DateTime EventTime, Candidate [] Candidates) : IVoteEntityEvent;
        public record SubmitVoteEvent(DateTime EventTime) : IVoteEntityEvent;
        
        public string UserId { get; private set; }
        public string ElectionId { get; private set; }
        public IReadOnlyList<Candidate> Candidates { get; private set; }

        private bool _submitted;
        public bool Submitted
        {
            get => _submitted;
        }

        public Stack<IVoteEntityEvent> _events;
        public VoteEntity(string userId, string electionId, IEnumerable<IVoteEntityEvent> events)
        {
            UserId = userId;
            ElectionId = electionId;
            Candidates = new List<Candidate>();
            _events = new Stack<IVoteEntityEvent>(events);
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

        bool Dispatch(IVoteEntityEvent @event)
        {
           _events.Push(@event);
           return Reduce(@event);
        }

        public bool SubmitVote()
        {
            return Dispatch(new SubmitVoteEvent(DateTime.Now));
        }

        public bool SaveVote(Candidate[] candidates)
        {
            return Dispatch(new SaveVoteEvent(DateTime.Now, candidates));
        }
    }
}