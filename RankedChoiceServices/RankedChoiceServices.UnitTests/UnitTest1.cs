using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RankedChoiceServices.Entities;

namespace RankedChoiceServices.UnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FirstRoundMajority()
        {
            const string canididateChar = "abcdefghijklmnopqrstuvqxyz";
            const string userChar = "1234567890!@#$%^&*()";
            
            var entity = new ElectionEntity("qwerty");
            entity.Candidates = Enumerable.Range(0, 10)
                .Select(i => new Candidate(canididateChar[i].ToString(), canididateChar[i].ToString())).ToList();

            entity.SetUserEmails(Enumerable.Range(0, 10)
                .Select(i => userChar[i].ToString()).ToArray());

            foreach (var user in entity.Users)
            {
                entity.AddVote(new Vote(user.userId, entity.Candidates.ToArray()));
            }

            var results = entity.CalculateResults();
            Assert.That(results, Is.Not.Null.And.Not.Empty);
            Assert.That(results.First(), Is.EqualTo(entity.Candidates.First()));
        }
    }
}