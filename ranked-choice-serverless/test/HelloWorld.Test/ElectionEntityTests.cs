using System.Linq;
using HelloWorld.Data;
using RankedChoiceServices.Entities;
using Xunit;

namespace HelloWorld.Tests
{
    public class ElectionEntityTests
    {
        [Fact]
        void TestCreateElection()
        {
            var entity = new ElectionEntity("test", "owner");
            
            Assert.Single(entity.Events);
            Assert.IsType<CreateElectionEvent>(entity.Events.First());
        }
        
        [Fact]
        void TestStartElection()
        {
            var entity = new ElectionEntity("test", "owner");

            var result = entity.StartElection();
            
            Assert.True(result);
            Assert.IsType<StartElectionEvent>(entity.Events.First());
            Assert.Equal(ElectionState.Started ,entity.State);
        } 
        
        [Fact]
        void TestEndElection()
        {
            var entity = new ElectionEntity("test", "owner");

            var result = entity.StartElection();
            Assert.True(result);
            
            result = entity.StopElection();
            Assert.True(result);
            Assert.Equal(ElectionState.Finished,entity.State);
            Assert.IsType<EndElectionEvent>(entity.Events.First());
        }  
        
        [Fact]
        void TestRestartElection()
        {
            var entity = new ElectionEntity("test", "owner");

            var result = entity.StartElection();
            Assert.True(result);
            
            result = entity.StopElection();
            Assert.True(result);
            
            result = entity.RestartElection();
            Assert.True(result);
            Assert.Equal(ElectionState.Reset,entity.State);
            Assert.IsType<RestartElectionEvent>(entity.Events.First());
        }   
        
        [Fact]
        void TestBadStateTransition()
        {
            var entity = new ElectionEntity("test", "owner");

            var result = entity.StopElection();
            Assert.False(result);
        }   
        
        [Fact]
        void TestAddCandidates()
        {
            var entity = new ElectionEntity("test", "owner");
            entity.SaveCandidates(new[] { new Candidate(){candidateId = "1", value = "A"}, new Candidate(){candidateId = "2", value = "B"} });
            
            Assert.NotEmpty(entity.Candidates);
        }    
        
        [Fact]
        void TestAddCandidatesWrongState()
        {
            var entity = new ElectionEntity("test", "owner");
            entity.StartElection();
            var result = entity.SaveCandidates(new[] { new Candidate(){candidateId = "1", value = "A"}, new Candidate(){candidateId = "2", value = "B"} });
            
            Assert.False(result);
        }    
        
        [Fact]
        void TestAddVote()
        {
            var entity = new ElectionEntity("test", "owner");
            entity.SaveCandidates(new[] { new Candidate(){candidateId = "1", value = "A"}, new Candidate(){candidateId = "2", value = "B"} });
            entity.StartElection();
            
            var result = entity.AddVote(new Vote(){candidates = new []{entity.Candidates.First()}, userId = "voter"});
            
            Assert.True(result);
        }     
        
        [Fact]
        void TestAddVoteInvalidCandidate()
        {
            var entity = new ElectionEntity("test", "owner");
            entity.SaveCandidates(new[] { new Candidate(){candidateId = "1", value = "A"}, new Candidate(){candidateId = "2", value = "B"} });
            entity.StartElection();
            
            var result = entity.AddVote(new Vote(){candidates = new []{new Candidate(){candidateId = "Z", value = "A"}}, userId = "voter"});
            
            Assert.False(result);
        }     
        
        [Fact]
        void TestLoadEvents()
        {
            var entity = new ElectionEntity("test", "owner");
            entity.SaveCandidates(new[] { new Candidate(){candidateId = "1", value = "A"}, new Candidate(){candidateId = "2", value = "B"} });
            entity.StartElection();
            var result = entity.AddVote(new Vote(){candidates = new []{entity.Candidates.First()}, userId = "voter"});
            entity.StopElection();

            var e = new ElectionEntity("test1", entity.Events.ToList());
            
            Assert.Equal(ElectionState.Finished,e.State);
            Assert.Equal(5, e.Events.Count);
            Assert.Equal(2, e.Candidates.Count);
            
        }    
    }
}