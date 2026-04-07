using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assets.Code.Architecture.Code.Entities
{
    internal class EntityRegistry : IService, IDisposable
    {

        public bool IsPersistance => false;

        internal string RegisterMethodName => nameof(Register);

        private Dictionary<uint, Entity> entities;
        private Dictionary<Type, List<uint>> entitiesIDsByType;

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

        public void Dispose()
        {
            
        }
    }
}
