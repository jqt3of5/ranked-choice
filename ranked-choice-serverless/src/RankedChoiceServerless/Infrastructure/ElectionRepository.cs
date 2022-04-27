﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace RankedChoiceServices.Entities
{
    public class ElectionRepository : EntityRepository
    {
        public ElectionRepository()
        {
            var client = new AmazonDynamoDBClient();
            ElectionTable = Table.LoadTable(client, "ElectionTable");
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
                    try
                    {
                        Document doc;
                        switch (entityEvent)
                        {
                            case CreateElectionEvent e:
                                doc = Context.ToDocument(e);
                                break;
                            case SaveCandidatesEvent e:
                                doc = Context.ToDocument(e);
                                break;
                            case SaveSettingsEvent e:
                                doc = Context.ToDocument(e);
                                break;
                            case SaveUserEmailsEvent e:
                                doc = Context.ToDocument(e);
                                break;
                            case SubmitVoteEvent e:
                                doc = Context.ToDocument(e);
                                break;
                            case StartElectionEvent e:
                                doc = Context.ToDocument(e);
                                break;
                            case RestartElectionEvent e:
                                doc = Context.ToDocument(e);
                                break;
                            case EndElectionEvent e:
                                doc = Context.ToDocument(e);
                                break;
                            default:
                                throw new ArgumentException(
                                    $"entityEvent of type: {entityEvent.GetType().Name} not supported");
                        }
                        
                        doc.Add("EventType", entityEvent.GetType().Name);
                        await ElectionTable.UpdateItemAsync(doc);
                    }
                    catch (Exception e)
                    {
                        LambdaLogger.Log(e.ToString()); 
                    }
                }
            }
        }

        public bool Exists(string electionId)
        {
            var filter = new ScanFilter();
            filter.AddCondition("ElectionId", ScanOperator.Equal, electionId);
            if (ElectionTable.Scan(filter) is { } search)
            {
                return search.Matches.Any();
            }

            return false;
        }

        public IElection Create(string electionId, string ownerUserId)
        {
            return new ElectionEntity(electionId, ownerUserId);
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
            filter.AddCondition(nameof(IElectionEvent.ElectionId), ScanOperator.Equal, electionId);
            if (ElectionTable.Scan(filter) is { } search)
            {
                LambdaLogger.Log($"{search.Count}");
                var events = new List<IElectionEvent>();
                foreach (var match in search.Matches)
                {
                    IElectionEvent e;
                    switch (match["EventType"])
                    { 
                       case nameof(CreateElectionEvent):
                           e = Context.FromDocument<CreateElectionEvent>(match);
                           break;
                       case nameof(SaveCandidatesEvent):
                           e = Context.FromDocument<SaveCandidatesEvent>(match);
                           break;
                       case nameof(SaveSettingsEvent):
                           e = Context.FromDocument<SaveSettingsEvent>(match);
                           break;
                       case nameof(SaveUserEmailsEvent):
                           e = Context.FromDocument<SaveUserEmailsEvent>(match);
                           break;
                       case nameof(SubmitVoteEvent):
                           e = Context.FromDocument<SubmitVoteEvent>(match);
                           break;
                       case nameof(StartElectionEvent):
                           e = Context.FromDocument<StartElectionEvent>(match);
                           break;
                       case nameof(RestartElectionEvent):
                           e = Context.FromDocument<RestartElectionEvent>(match);
                           break;
                       case nameof(EndElectionEvent):
                           e = Context.FromDocument<EndElectionEvent>(match);
                           break;
                       default:
                           LambdaLogger.Log($"Type {match["Type"]} does exist");
                           continue;
                    }
                   events.Add(e);
                }

                if (events.Any())
                {
                    return new ElectionEntity(electionId, events);
                }
            }
            
            return null;
        }
    }
}