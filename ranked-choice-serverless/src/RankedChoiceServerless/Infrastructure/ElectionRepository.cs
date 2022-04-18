using System.Collections.Generic;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;

namespace RankedChoiceServices.Entities
{
    public class ElectionRepository
    {
        public async void Save(IElection election)
        {
            if (election is IEntity<IElectionEvent> entity)
            {
                var client = new AmazonDynamoDBClient();
                var table = Table.LoadTable(client, "ElectionData");

                //TODO: When saving to dynamo db.... my events are poly morphic so I have a few options
                //TODO: Does the sdk support that already? through the object persistence model?
                //TODO: Should I store the object as json with type discriminators?
                //TODO: Should I just store each different object in the table (with type discriminators) and do some custom deserialization?
                foreach (var entityEvent in entity.Events)
                {
                    var doc = new Document();
                    doc["Id"] = entityEvent.EventId;
                    doc["ElectionId"] = entityEvent.ElectionId;
                    doc["Time"] = entityEvent.EventTime.Ticks;
                    doc["payload"] = JsonConvert.SerializeObject(entityEvent);
                    await table.UpdateItemAsync(doc);
                }
            }
        }

        public bool Exists(string electionId)
        {
            return _elections.ContainsKey(electionId);
        }

        public IElection Create(string electionId, string ownerUserId)
        {
            var election = new ElectionEntity(electionId, ownerUserId);
            _elections[electionId] = election;
            return election;
        }

        public IElection? GetByUniqueUserId(string uniqueId)
        {
            //TODO: How should this work?
            // if (_usersIndex.TryGetValue(uniqueId, out var election2))
            // {
                // return election2;
            // }

            return null;
        }
        public IElection? Get(string electionId)
        {
            if (_elections.TryGetValue(electionId, out var election))
            {
                return election;
            }
            
            return null;
        }
    }
}