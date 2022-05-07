using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;

namespace RankedChoiceServices.Entities
{
    public class VoteRepository 
    {
        public VoteRepository()
        {
            var client = new AmazonDynamoDBClient();
            VoteTable = Table.LoadTable(client, "VoteTable");
            Context = new DynamoDBContext(client);
        }
        
        private Table VoteTable { get; }
        private DynamoDBContext Context{ get; }
        
        public async Task<VoteEntity> GetForUser(string userId, string electionId)
        {
            var filter = new ScanFilter();
            filter.AddCondition(nameof(VoteEntity.IVoteEntityEvent.ElectionId), ScanOperator.Equal, electionId);
            filter.AddCondition(nameof(VoteEntity.IVoteEntityEvent.UserId), ScanOperator.Equal, userId);

            var events = new List<VoteEntity.IVoteEntityEvent>();
            if (VoteTable.Scan(filter) is { } search)
            {
                foreach (var match in await search.GetNextSetAsync())
                {
                    VoteEntity.IVoteEntityEvent e;
                    switch (match["EventType"])
                    {
                        case nameof(VoteEntity.SaveVoteEvent):
                            e = Context.FromDocument<VoteEntity.SaveVoteEvent>(match);
                            break;
                        case nameof(VoteEntity.SubmitVoteEvent):
                            e = Context.FromDocument<VoteEntity.SubmitVoteEvent>(match);
                            break;
                        default:
                            LambdaLogger.Log($"Type {match["Type"]} does exist");
                            continue;
                    }
                    events.Add(e);
                }
            }
            
            return new VoteEntity(userId, electionId, events);
        }

        public async Task SaveForUser(string userId, string electionId, IVoteEntity vote)
        {
            if (vote is IEntity<VoteEntity.IVoteEntityEvent> entity)
            {
                foreach (var entityEvent in entity.Events)
                {
                    Document doc;
                    switch (entityEvent)
                    {
                        case VoteEntity.SaveVoteEvent e :
                            doc = Context.ToDocument(e);
                            break;
                        case VoteEntity.SubmitVoteEvent e:
                            doc = Context.ToDocument(e);
                            break;
                        default:
                            throw new ArgumentException($"entityEvent of type: {entityEvent.GetType().Name} not supported");
                    }
                    doc.Add("EventType", entityEvent.GetType().Name);
                    await VoteTable.UpdateItemAsync(doc);
                }    
            }
        }
        
    }
}