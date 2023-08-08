using System;

namespace EventSourcing
{
    public class AggregateDoesNotExistException : Exception
    {
        private readonly string _aggregateName;

        public AggregateDoesNotExistException(string aggregateName)
        {
            _aggregateName = aggregateName;
        }

        public override string ToString()
        {
            return $"Aggregate {_aggregateName} does not exist";
        }
    }
}
