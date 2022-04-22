using System;
using System.Collections.Generic;

namespace RankedChoiceServices.Entities
{
    public abstract class EntityRepository
    {
        protected static List<Type> EventTypes { get; } = new List<Type>();

        public void Register<T>() where T : new()
        {
           EventTypes.Add(typeof(T));
        }
        public void Deregister<T>() where T : new()
        {
           EventTypes.Remove(typeof(T));
        }
    }
}