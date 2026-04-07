using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assets.Code.Architecture.Code.Entities
{
    public class EntityRegistry : IService, IDisposable
    {
        public bool IsPersistance => false;

        internal string RegisterMethodName => nameof(Register);

        private Dictionary<uint, Entity> entities;
        private Dictionary<Type, List<uint>> entitiesIDsByType;

        public Entity this[uint ID] => entities[ID]; 

        public EntityRegistry()
        {
            entities = new Dictionary<uint, Entity>();
            entitiesIDsByType = new Dictionary<Type, List<uint>>();
        }

        private void Register(Entity entity)
        {
            entities.Add(entity.ID, entity);
            Type currentEntityType = null;

            do
            {
                currentEntityType = currentEntityType == null ? entity.GetType() : currentEntityType.BaseType;

                if (!entitiesIDsByType.ContainsKey(currentEntityType))
                    entitiesIDsByType.Add(currentEntityType, new List<uint>());

                entitiesIDsByType[currentEntityType].Add(entity.ID);

            } while (currentEntityType != typeof(Entity));
        }

        public EntityType GetAs<EntityType>(uint ID) where EntityType : Entity
        {
            if (ID == Entity.UNASSIGNED_ENTITY_ID)
            {
                throw new NullReferenceException("Entity id 0 represents a null entity");
            }

            if (!entities.ContainsKey(ID))
            {
                throw new KeyNotFoundException(ID.ToString());
            }

            if (entities[ID] is not EntityType)
            {
                throw new InvalidCastException($"An attempt was made to obtain a type {entities[ID].GetType().Name}"
                                             + $"entity as type {typeof(EntityType).Name} from the EntityRegistry");
            }

            return entities[ID] as EntityType;
        }

        public void Dispose()
        {
            
        }
    }
}
