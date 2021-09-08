using System;
using System.Collections.Generic;
using System.Text;

namespace AlintaCodingTest.Services.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public string EntityName { get; set; }
        public int EntityId { get; set; }

        public EntityNotFoundException()
        {
        }

        public EntityNotFoundException(string entityName,
            int entityId)
            : base($"{entityName} with ID {entityId} could not be found.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public EntityNotFoundException(string entityName,
            int entityId,
            string message)
            : base(message)
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}