using Assets.Code.Architecture.Code.Client;
using Assets.Code.Architecture.Code.Entities;
using Assets.Code.Architecture.Code.Entities.Events;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using System.Reflection;
using UnityEngine;

namespace Assets.Code
{
    public class EntityFactoryView : IDisposable
    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        private EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();
        private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
        private GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();


        private MethodInfo registerEntityMethod;
        private MethodInfo setEntityIdMethod;

        private GameObject userPrefabs;

        public EntityFactoryView(GameObject userPrefabs)
        {
            this.userPrefabs = userPrefabs;

            EventBus.Subscribe<EntityCreatedEvent<Entity>>(OnEntityCreated);

            registerEntityMethod = EntityRegistryView.GetType().GetMethod(EntityRegistryView.RegisterMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            setEntityIdMethod = typeof(EntityView).GetMethod(EntityView.SetIdMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void OnEntityCreated(in EntityCreatedEvent<Entity> entityCreatedEvent)
        {
            GameObject goGameObject = UnityEngine.Object.Instantiate(userPrefabs);

            Renderer renderer = goGameObject.GetComponent<Renderer>();

            if (GameClient.MyID == entityCreatedEvent.networkID)
                renderer.material.color = Color.green;
            else
                renderer.material.color = Color.red;

            setEntityIdMethod.Invoke(goGameObject, new object[] { entityCreatedEvent.entityID });

            registerEntityMethod.Invoke(EntityRegistryView, new object[] { goGameObject });
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<EntityCreatedEvent<Entity>>(OnEntityCreated);
        }
    }
}
