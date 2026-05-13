using Assets.Code.Scenes;
using Assets.MultiplayerArchitecture.Code.Entities;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ZooArchitect.View.Mapping;

namespace Assets.Code.Entities
{
    internal class NetworkFactoryView : IInitable, IDisposable
    {
        private delegate void ComponentAssigner(ViewComponent viewComponent);

        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        NetworkRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<NetworkRegistryView>();
        NetworkRegistry EntityRegistry => ServiceProvider.Instance.GetService<NetworkRegistry>();
        BaseSceneView BaseSceneView => ServiceProvider.Instance.GetService<BaseSceneView>();

        private Dictionary<Type, ComponentAssigner> componentAssigner;
        private Dictionary<Type, GameObject> prefabsOfTypes;

        private GameObject usersPrefab;
        private Camera camera;

        private MethodInfo registerEntityMethodView;
        private MethodInfo registerEntityMethod;
        private MethodInfo setEntityIdMethod;

        public NetworkFactoryView(GameObject usersPrefab, Camera camera)
        {
            this.usersPrefab = usersPrefab;
            this.camera = camera;

            componentAssigner = new Dictionary<Type, ComponentAssigner>()
            {
                {typeof(Player), PlayerComponents}
            };

            prefabsOfTypes = new Dictionary<Type, GameObject>()
            {
                { typeof(Player), this.usersPrefab}
            };

            registerEntityMethod = EntityRegistry.GetType().GetMethod(EntityRegistry.RegisterMethodName, BindingFlags.NonPublic | BindingFlags.Instance);

            registerEntityMethodView = EntityRegistryView.GetType().GetMethod(EntityRegistryView.RegisterMethodName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            setEntityIdMethod = typeof(EntityView).GetMethod(EntityView.SetIDMethod,
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [Obsolete]
        public void Init()
        {
            EntityView[] preLoadedEntities = UnityEngine.Object.FindObjectsOfType<EntityView>();
            uint baseGameCurrentID = 0;

            Type architectureType;

            for (int i = 0; i < preLoadedEntities.Length; ++i)
            {
                architectureType = ViewArchitectureMap.ArchitectureOf(preLoadedEntities[i].GetType());

                SetUpPreloadedAssets(preLoadedEntities[i], architectureType, ++baseGameCurrentID);
            }

            void SetUpPreloadedAssets(EntityView view, Type entityType, uint currentObjectID)
            {
                const uint networkOwnerID = 0u;
                Coordinate coord = BaseSceneView.WorldToCoordinate(view.transform.position);

                Type openEventType = typeof(NetworkSpawnRequestAcceptedEvent<>);
                Type closedEventType = openEventType.MakeGenericType(entityType);

                MethodInfo raiseMethod = typeof(EventBus).GetMethod("Raise")
                    .MakeGenericMethod(closedEventType);

                object[] eventParams = new object[] { networkOwnerID, currentObjectID, coord };
                raiseMethod.Invoke(EventBus, new object[] { eventParams });

                setEntityIdMethod.Invoke(view, new object[] { networkOwnerID, currentObjectID });
                registerEntityMethodView.Invoke(EntityRegistryView, new object[] { view });
            }
        }

        public void LateInit()
        {
            EventBus.Subscribe<EntityCreatedEvent<Entity>>(OnEntityCreated);
        }

        private void OnEntityCreated(in EntityCreatedEvent<Entity> entityCreated)
        {
            Type entityType = EntityRegistry.Get<Entity>(entityCreated.ownerNetworkID, entityCreated.objectNetworkID).GetType();
            string entityName = entityType.Name + "Owner: " + entityCreated.ownerNetworkID + "ObjectID: " + entityCreated.objectNetworkID;

            ViewComponent viewComponent = BaseSceneView.AddSceneComponent(ViewArchitectureMap.ViewOf(entityType), entityName, null, prefabsOfTypes.ContainsKey(entityType) ? prefabsOfTypes[entityType] : null);

            setEntityIdMethod.Invoke(viewComponent, new object[] { entityCreated.ownerNetworkID, entityCreated.objectNetworkID });

            registerEntityMethodView.Invoke(EntityRegistryView, new object[] { viewComponent });

            viewComponent.transform.position = BaseSceneView.CoordinateToWorld(entityCreated.coordinate);

            if (componentAssigner.TryGetValue(entityType, out ComponentAssigner assigner))
                assigner(viewComponent);

            viewComponent.Init();
            viewComponent.LateInit();
        }

        private void PlayerComponents(ViewComponent viewComponent)
        {
            GameObject gameObject = viewComponent.gameObject;

            Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();

            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();

            if (GameClient.MyID == (viewComponent as EntityView).OwnerNetworkID)
            {
                renderer.material.color = Color.green;

                camera.transform.parent = gameObject.transform;
                camera.transform.localPosition = new Vector3(0, 5, -10);
                camera.transform.LookAt(gameObject.transform);
            }
            else
            {
                renderer.material.color = Color.red;
            }
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<EntityCreatedEvent<Entity>>(OnEntityCreated);
        }
    }
}
