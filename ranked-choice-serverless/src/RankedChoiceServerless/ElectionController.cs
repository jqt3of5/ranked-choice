using System;
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
        public Task<APIGatewayProxyResponse> CreateElection(APIGatewayProxyRequest apiProxyEvent,
            ILambdaContext context)
        {
            
            LambdaLogger.Log("Starting Lambda");
            var userId = apiProxyEvent.Headers["userId"];
            LambdaLogger.Log("User Id " + userId);
            var repo = new ElectionRepository();
            var electionId = Guid.NewGuid().ToString();
            LambdaLogger.Log("electionId " + electionId);

            try
            {
                var election = repo.Create(electionId, userId);
                LambdaLogger.Log( "election "+ election.ElectionId);

                repo.Save(election);
                return Task.FromResult(new ElectionResponse(string.Empty, true, electionId).toResponse());
            }
            catch (Exception e)
            {
                LambdaLogger.Log(e.ToString());
            }

            return Task.FromResult(new ElectionResponse("Failed to create a new election due to exception", false, null).toResponse());
        }

        public async Task<APIGatewayProxyResponse> SubmitVote(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            string[] candidateIds = JsonConvert.DeserializeObject<string[]>(apiProxyEvent.Body);
            var userId = apiProxyEvent.Headers["userId"];
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            var repo = new ElectionRepository();
            var election = await repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return new ElectionResponse($"Election with Id {electionId} does not exist", false,null).toResponse(404);
            }

            List<Candidate> candidates = new List<Candidate>();
            foreach (var candidateId in candidateIds)
            {
                var candidate = election.Candidates.FirstOrDefault(c => c.candidateId == candidateId);
                if (candidate == null)
                {
                    //That id is not valid, fail!
                    LambdaLogger.Log($"candidate with Id {candidateId} does not exist");
                    return new ElectionResponse($"candidate with Id {candidateId} does not exist", false,null).toResponse(400);
                }
                
                candidates.Add(candidate);
            }
            
            election.AddVote(new Vote{userId = userId, candidates = candidates.ToArray()});

            repo.Save(election);
            
            return new ElectionResponse(string.Empty, true, null).toResponse();
        }
        
        public async Task<APIGatewayProxyResponse> StartElection(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            var repo = new ElectionRepository();
            var election = await repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return new ElectionResponse($"Election with Id {electionId} does not exist", false, null).toResponse(404);
            } 
            var result = election.StartElection();
            repo.Save(election);
            
            return  new ElectionResponse(string.Empty, false, result).toResponse();
        }
        
        public async Task<APIGatewayProxyResponse> EndElection(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var repo = new ElectionRepository();
            var election = await repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return new ElectionResponse($"Election with Id {electionId} does not exist", false, null).toResponse(404);
            }
            var result = election.StopElection();
            repo.Save(election);
            
            return new ElectionResponse(string.Empty, true, result).toResponse();
        }
        
        public async Task<APIGatewayProxyResponse> RestartElection(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var repo = new ElectionRepository();
            var election = await repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return new ElectionResponse($"Election with Id {electionId} does not exist", false, null).toResponse(404);
            }
            var result = election.RestartElection();
            repo.Save(election);
            
            return new ElectionResponse(string.Empty, true, result).toResponse();
        }
        
        public async Task<APIGatewayProxyResponse> GetElectionResults(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var repo = new ElectionRepository();
            var election = await repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return new ElectionResponse($"Election with Id {electionId} does not exist", false, null).toResponse(404);
            }
            var result = new ElectionDTO(electionId, 
                election.CalculateResults().Select(c => new CandidateDTO(c.value, c.candidateId)).ToArray());
            
            return new ElectionResponse(string.Empty, true, result).toResponse();
        }
        
        public async Task<APIGatewayProxyResponse> GetSettings(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var repo = new ElectionRepository();
            var election = await repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return new ElectionResponse($"Election with Id {electionId} does not exist", false, null).toResponse(404);
            } 
            var result = new ElectionSettingsDTO(electionId, election.UniqueIdsPerUser, election.Users.Select(u => u.email).ToArray(), election.ElectionName, election.State);
            
            return new ElectionResponse(string.Empty, true, result).toResponse();
        }
        
        public async Task<APIGatewayProxyResponse> GetCandidates(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            LambdaLogger.Log($"Getting candidates for electionId: {electionId}");
            
            var repo = new ElectionRepository();
            var election = await repo.Get(electionId);
            if (election == null)
            {
                election = repo.GetByUniqueUserId(electionId);
                if (election == null)
                {
                    LambdaLogger.Log($"Election with Id {electionId} does not exist");
                    return new ElectionResponse($"Election with Id {electionId} does not exist", false, null).toResponse(404);
                }

                if (!election.UniqueIdsPerUser)
                {
                    LambdaLogger.Log($"Attempting to get election with Id {election.ElectionId} by unique Id {electionId} failed, because UniqueIdesPerUser is not enabled");
                    return new ElectionResponse($"Attempting to get election with Id {election.ElectionId} by unique Id {electionId} failed, because UniqueIdesPerUser is not enabled", false, null).toResponse(404);
                }
            } 
            
            var result = new ElectionDTO(electionId, 
                election.Candidates.Select(c => 
                    new CandidateDTO(c.value, c.candidateId)).ToArray()
                );
            return new ElectionResponse(string.Empty, true, result).toResponse();
        }

        public async Task<APIGatewayProxyResponse> SaveSettings(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var userId = apiProxyEvent.Headers["userId"];
            var settings = JsonConvert.DeserializeObject<ElectionSettingsDTO>(apiProxyEvent.Body); 
            
            var repo = new ElectionRepository();
            if (electionId != settings.electionId)
            {
                LambdaLogger.Log($"ElectionId mismatch {electionId} {settings.electionId}");
                return new ElectionResponse($"ElectionId mismatch {electionId} {settings.electionId}", false, null).toResponse(400);
            }
            
            if (!await repo.Exists(electionId))
            {
                repo.Create(electionId, userId);
                LambdaLogger.Log($"New election created with Id {electionId}");
            }
            
            var election = await repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return new ElectionResponse($"Election with Id {electionId} does not exist", false, null).toResponse(404);
            }
            
            election.SetUserEmails(settings.userEmails);
            election.SaveSettings(settings.uniqueIdsPerUser, settings.electionName);

            repo.Save(election);
            return new ElectionResponse(string.Empty, true, null).toResponse();
        }
        
        public async Task<APIGatewayProxyResponse> SaveCandidates(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var electionId = apiProxyEvent.PathParameters["electionId"];
            var userId = apiProxyEvent.Headers["userId"];
            var dto = JsonConvert.DeserializeObject<ElectionDTO>(apiProxyEvent.Body); 
            
            var repo = new ElectionRepository();
            if (electionId != dto.electionId)
            {
                LambdaLogger.Log($"ElectionId mismatch {electionId} {dto.electionId}");
                return new ElectionResponse($"ElectionId mismatch {electionId} {dto.electionId}", false, null).toResponse(400);
            }

            if (!await repo.Exists(electionId))
            {
                LambdaLogger.Log($"New election created with Id {electionId}");
                repo.Create(electionId, userId);
            }
            
            var election = await repo.Get(electionId);
            if (election == null)
            {
                LambdaLogger.Log($"Election with Id {electionId} does not exist");
                return new ElectionResponse($"Election with Id {electionId} does not exist", false, null).toResponse(404);
            }
            var candidates = dto.candidates.Select(c => new Candidate{value = c.value, candidateId = c.candidateId}).ToArray();
            
            election.SaveCandidates(candidates);
            
            repo.Save(election);
            
            return new ElectionResponse(string.Empty, true, null).toResponse();
        }
    }
}
