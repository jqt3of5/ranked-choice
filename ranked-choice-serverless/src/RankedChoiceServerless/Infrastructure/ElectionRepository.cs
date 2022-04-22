using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;

namespace RankedChoiceServices.Entities
{
    public class ElectionRepository : EntityRepository
    {
        public ElectionRepository()
        {
            var client = new AmazonDynamoDBClient();
            ElectionTable = Table.LoadTable(client, "ElectionData");
            Context = new DynamoDBContext(client);
        }
        
        private Table ElectionTable { get; }
        private DynamoDBContext Context{ get; }

        public async void Save(IElection election)
        {
            if (election is IEntity<IElectionEvent> entity)
            {
                foreach (var entityEvent in entity.Events)
                {
                    var doc = Context.ToDocument(entityEvent);
                    doc["Type"] = entityEvent.GetType().Name;
                    await ElectionTable.UpdateItemAsync(doc);
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
            var filter = new ScanFilter();
            filter.AddCondition("ElectionId", ScanOperator.Equal, electionId);
            if (ElectionTable.Scan(filter) is { } search)
            {
                var events = new List<IElectionEvent>();
                foreach (var match in search.Matches)
                {
                    var e = Context.FromDocument<SaveCandidatesEvent>(match);
                    events.Add(e);
                }
            }
            
            return null;
        }
    }
}