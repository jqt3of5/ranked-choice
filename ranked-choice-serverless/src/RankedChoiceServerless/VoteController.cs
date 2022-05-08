using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HelloWorld.Data;
using Newtonsoft.Json;
using RankedChoiceServerless;
using RankedChoiceServices.Entities;

namespace RankedChoiceServerless
{
    public class VoteController
    {
        public async Task<APIGatewayProxyResponse> SaveCandidates(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            //TODO: Doesn't respect election status
            var candidateIds= JsonConvert.DeserializeObject<string[]>(apiProxyEvent.Body);
            var userId = apiProxyEvent.Headers["userId"];
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            try
            { 
                var electionRepository = new ElectionRepository();
                var election = await electionRepository.Get(electionId);
                if (election == null)
                {
                    LambdaLogger.Log(
                        $"Election with Id {electionId} doesn't exist, or user with id {userId} isn't allowed to vote in this election");
                    return new VoteResponse($"Election with Id {electionId} doesn't exist, or user with id {userId} isn't allowed to vote in this election",
                            false, null)
                        .toResponse(404);
                }

                var voteRepository = new VoteRepository();
                var entity = await voteRepository.GetForUser(userId, electionId);
                if (entity.Submitted)
                {
                    LambdaLogger.Log($"Vote for user {userId} for election {electionId} not saved because it has already been submitted");
                    return new VoteResponse($"Vote for user {userId} for election {electionId} not saved because it has already been submitted",
                        false, null).toResponse(400);
                }
                
                //validate the candidates submitted
                List<Candidate> candidates = new List<Candidate>();
                foreach (var candidateId in candidateIds)
                {
                    //Find this candidate in the main election list. We don't really trust the values sent from the client
                    var candidate = election.Candidates.FirstOrDefault(c => c.candidateId == candidateId);
                    if (candidate == null)
                    {
                        //That id is not valid, fail!
                        LambdaLogger.Log($"candidate with Id {candidateId} does not exist");
                        return new VoteResponse($"candidate with Id {candidateId} does not exist", false,null).toResponse(400);
                    }
                    
                    candidates.Add(candidate);
                }

                if (!entity.SaveVote(candidates.ToArray()))
                {
                    LambdaLogger.Log($"Something went wrong saving the vote to the entity");
                    return new VoteResponse($"Something went wrong saving the vote to the entity", false,null).toResponse(400);
                }
           
                await voteRepository.SaveForUser(userId, electionId, entity);
                return new VoteResponse(string.Empty, true, null).toResponse();
            }
            catch (Exception e)
            {
                LambdaLogger.Log($"Exception while saving candidates for user {userId} and election {electionId}\n {e.ToString()}");
                return new VoteResponse("An exception has occurred.Please view the logs", false, null).toResponse(500);
            }
        }
        
        public async Task<APIGatewayProxyResponse> GetCandidates(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var userId = apiProxyEvent.Headers["userId"];
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            try
            {
                var electionRepository = new ElectionRepository();
                var election = await electionRepository.Get(electionId);
                if (election == null)
                {
                    LambdaLogger.Log(
                        $"Election with Id {electionId} doesn't exist, or user with id {userId} isn't allowed to vote in this election");
                    return new VoteResponse($"Election with Id {electionId} doesn't exist, or user with id {userId} isn't allowed to vote in this election",
                            false, null)
                        .toResponse(404);
                }
                
                var voteRepository = new VoteRepository();
                
                var entity = await voteRepository.GetForUser(userId, electionId);
                var candidates = election.Candidates
                    .Where(c => entity.Candidates.Any(d=> d.candidateId == c.candidateId))
                    .Select(e => new CandidateDTO(e.value, e.candidateId))
                    .ToArray();
                var result = new VoteDTO(entity.Submitted, candidates);
                return new VoteResponse(string.Empty, true, result).toResponse();
            }
            catch (Exception e)
            {
                LambdaLogger.Log($"Exception while saving candidates for user {userId} and election {electionId}\n {e.ToString()}");
                return new VoteResponse("An exception has occurred.Please view the logs", false, null).toResponse(500);
            }
        }

        public async Task<APIGatewayProxyResponse> SubmitVote(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var userId = apiProxyEvent.Headers["userId"];
            var electionId = apiProxyEvent.PathParameters["electionId"];

            //TODO: Unique id per user system needs to be implemented
            try
            {
                var electionRepository = new ElectionRepository();
                var election = await electionRepository.Get(electionId);
                if (election == null)
                {
                    LambdaLogger.Log(
                        $"Election with Id {electionId} doesn't exist, or user with id {userId} isn't allowed to vote in this election");
                    return new VoteResponse($"Election with Id {electionId} doesn't exist, or user with id {userId} isn't allowed to vote in this election",
                            false, null)
                        .toResponse(404);
                }
                
                var voteRepository = new VoteRepository();
                var entity = await voteRepository.GetForUser(userId, electionId);

                if (entity.Submitted)
                {
                    LambdaLogger.Log($"Vote for user {userId} for election {electionId} not saved because it has already been submitted");
                    return new VoteResponse(
                        $"Vote for user {userId} for election {electionId} not saved because it has already been submitted",
                        false, null).toResponse(400);
                }
                
                entity.SubmitVote();
                
                var vote = new Vote() { userId = userId, candidates = entity.Candidates.ToArray() };
                election.AddVote(vote);

                await electionRepository.Save(election);
                await voteRepository.SaveForUser(userId, electionId, entity);
                return new VoteResponse(string.Empty, true, null).toResponse();
            }
            catch (Exception e)
            {
                LambdaLogger.Log($"Exception while saving candidates for user {userId} and election {electionId}\n {e.ToString()}");
                return new VoteResponse("An exception has occurred.Please view the logs", false, null).toResponse(500);
            }
        }
    }
}