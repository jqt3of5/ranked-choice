using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using HelloWorld.Data;
using Newtonsoft.Json;
using RankedChoiceServerless;
using RankedChoiceServices.Entities;

namespace HelloWorld
{
    public class VoteController
    {
        public Task<APIGatewayProxyResponse> SaveCandidates(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var vote = JsonConvert.DeserializeObject<VoteDTO>(apiProxyEvent.Body);
            var userId = apiProxyEvent.Headers["userId"];
            var electionId = apiProxyEvent.PathParameters["electionId"];

            var voteRepository = new VoteRepository();
            
            var entity = voteRepository.GetForUser(userId, electionId);
            if (entity == null)
            {
                LambdaLogger.Log($"Vote for user {userId} for election {electionId}");
                entity = voteRepository.Create(userId, electionId);
            }

            if (entity.Submitted == true)
            {
                return Task.FromResult(false.toResponse(400));
            }

            entity.Candidates = vote.candidates.Select(c => new Candidate(c.value, c.candidateId)).ToList();
             
            voteRepository.SaveForUser(userId, electionId, entity);

            return Task.FromResult(true.toResponse());
        }
        
        public  Task<APIGatewayProxyResponse> GetCandidates(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var userId = apiProxyEvent.Headers["userId"];
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            var voteRepository = new VoteRepository();
            
            var entity = voteRepository.GetForUser(userId, electionId);
            if (entity == null)
            {
                LambdaLogger.Log($"Vote for user {userId} for election {electionId}");
                entity = voteRepository.Create(userId, electionId);
            }

            var result = new VoteDTO(entity.Submitted, entity.Candidates.Select(e => new CandidateDTO(e.value, e.candidateId)).ToArray());
            return Task.FromResult(result.toResponse());
        }

        public  Task<APIGatewayProxyResponse> SubmitVote(APIGatewayProxyRequest apiProxyEvent, ILambdaContext context)
        {
            var userId = apiProxyEvent.Headers["userId"];
            var electionId = apiProxyEvent.PathParameters["electionId"];
            
            var voteRepository = new VoteRepository();
            
            var entity = voteRepository.GetForUser(userId, electionId);
            if (entity == null)
            {
                LambdaLogger.Log($"Vote for user {userId} for election {electionId}");
                return Task.FromResult(false.toResponse(404));
            }

            entity.Submitted = true;

            voteRepository.SaveForUser(userId, electionId, entity);

            return Task.FromResult(true.toResponse());
        }
    }
}