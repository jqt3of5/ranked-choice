using System;
using System.Collections.Generic;
using System.Text;

namespace RankedChoiceServices.Entities
{
    public static class EntityId
    {
        public static string Generate()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var ticks = DateTime.Now.Ticks;
            var time = Convert.ToBase64String(BitConverter.GetBytes(ticks));

            var sb = new StringBuilder();
            sb.Append(time);
            for (int i = 0; i < 10; ++i)
            {
                sb.Append(chars[new Random().Next() % chars.Length]);
            }

            return sb.ToString();
        }
    }
   
    public interface IEntityEvent
    {
        DateTime EventTime { get; }
        string EventId { get; }
    }

    public interface IEntity<T> where T : IEntityEvent
    {
        public Stack<T> Events { get; } 
    }
}