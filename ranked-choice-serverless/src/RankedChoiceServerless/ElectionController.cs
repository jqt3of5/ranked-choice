using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using System.ComponentModel;
using HelloWorld.Data;
using RankedChoiceServices.Entities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit{}
}

namespace RankedChoiceServerless
{
    public static class ResponseExtensions 
    {
        public static APIGatewayProxyResponse toResponse(this object obj, int statusCode = 200)
        {
            return new APIGatewayProxyResponse() { Body = JsonConvert.SerializeObject(obj) , StatusCode = statusCode};
        }     
    }
    public class ElectionController
    {
        
        public Task<APIGatewayProxyResponse> SubmitVote(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            string[] candidateIds = JsonConvert.DeserializeObject<string[]>(apiProxyEvent.Body);
            var userId = apiProxyEvent.Headers["userId"];
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            var repo = new ElectionRepository();
            var election = repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return Task.FromResult(false.toResponse(404));
            }

            List<Candidate> candidates = new List<Candidate>();
            foreach (var candidateId in candidateIds)
            {
                var candidate = election.Candidates.FirstOrDefault(c => c.candidateId == candidateId);
                if (candidate == null)
                {
                    //That id is not valid, fail!
                    LambdaLogger.Log($"candidate with Id {candidateId} does not exist");
                    return Task.FromResult(false.toResponse(400));
                }
                
                candidates.Add(candidate);
            }
            
            election.AddVote(new Vote(userId, candidates.ToArray()));

            repo.Save(electionId, election);
            
            return Task.FromResult(true.toResponse());
        }
        
        public Task<APIGatewayProxyResponse> StartElection(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            var repo = new ElectionRepository();
            var election = repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return Task.FromResult(false.toResponse(404));
            } 
            var result = election.StartElection();
            repo.Save(electionId, election);
            
            return Task.FromResult(result.toResponse());
        }
        
        public Task<APIGatewayProxyResponse> EndElection(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var repo = new ElectionRepository();
            var election = repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return Task.FromResult(false.toResponse(404));
            }
            var result = election.StopElection();
            repo.Save(electionId, election);
            
            return Task.FromResult(result.toResponse());
        }
        
        public Task<APIGatewayProxyResponse> RestartElection(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var repo = new ElectionRepository();
            var election = repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return Task.FromResult(false.toResponse(404));
            }
            var result = election.RestartElection();
            repo.Save(electionId, election);
            
            return Task.FromResult(result.toResponse());
        }
        
        public Task<APIGatewayProxyResponse> GetElectionResults(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var repo = new ElectionRepository();
            var election = repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return Task.FromResult("".toResponse(404));
            }
            var result = new ElectionDTO(electionId, 
                election.CalculateResults().Select(c => new CandidateDTO(c.value, c.candidateId)).ToArray());
            
            return Task.FromResult(result.toResponse());
        }
        
        public Task<APIGatewayProxyResponse> GetSettings(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var repo = new ElectionRepository();
            var election = repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return Task.FromResult("".toResponse(404));
            } 
            var result = new ElectionSettingsDTO(electionId, election.UniqueIdsPerUser, 
                election.UniqueElectionIds.ToArray(), election.Users.Select(u => u.email).ToArray(), election.State);
            
            return Task.FromResult(result.toResponse());
        }
        
        public Task<APIGatewayProxyResponse> GetCandidates(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            var repo = new ElectionRepository();
            var election = repo.Get(electionId);
            if (election == null)
            {
                election = repo.GetByUniqueUserId(electionId);
                if (election == null)
                {
                    LambdaLogger.Log($"Election with Id {electionId} does not exist");
                    return Task.FromResult("".toResponse(404));
                }

                if (!election.UniqueIdsPerUser)
                {
                    LambdaLogger.Log($"Attempting to get election with Id {election.ElectionId} by unique Id {electionId} failed, because UniqueIdesPerUser is not enabled");
                    return Task.FromResult("".toResponse(404));
                }
            } 
            
            var result = new ElectionDTO(electionId, 
                election.Candidates.Select(c => 
                    new CandidateDTO(c.value, c.candidateId)).ToArray()
                );
            return Task.FromResult(result.toResponse());
        }

        public Task<APIGatewayProxyResponse> SaveSettings(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var settings = JsonConvert.DeserializeObject<ElectionSettingsDTO>(apiProxyEvent.Body); 
            
            var repo = new ElectionRepository();
            if (electionId != settings.electionId)
            {
                LambdaLogger.Log($"ElectionId mismatch {electionId} {settings.electionId}");
                return Task.FromResult(false.toResponse(400));
            }
            
            if (!repo.Exists(electionId))
            {
                repo.Create(electionId);
                LambdaLogger.Log($"New election created with Id {electionId}");
            }
            
            var election = repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return Task.FromResult(false.toResponse(404));
            }
            election.SetUserEmails(settings.userEmails);
            election.SaveSettings(settings.uniqueIdsPerUser, "");

            repo.Save(electionId, election);
            return Task.FromResult(true.toResponse());
        }
        
        public Task<APIGatewayProxyResponse> SaveCandidates(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var dto = JsonConvert.DeserializeObject<ElectionDTO>(apiProxyEvent.Body); 
            
            var repo = new ElectionRepository();
            if (electionId != dto.electionId)
            {
                LambdaLogger.Log($"ElectionId mismatch {electionId} {dto.electionId}");
                return Task.FromResult(false.toResponse(400));
            }

            if (!repo.Exists(electionId))
            {
                LambdaLogger.Log($"New election created with Id {electionId}");
                repo.Create(electionId);
            }
            
            LambdaLogger.Log($"Election with Id {electionId} Saved");

            var election = repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return Task.FromResult(false.toResponse(404));
            }
            var candidates = dto.candidates.Select(c => new Candidate(c.value, c.candidateId)).ToArray();
            
            election.SaveCandidates(candidates);
            
            repo.Save(electionId, election);
            
            return Task.FromResult(true.toResponse());
        }
    }
}
