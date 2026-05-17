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
using UnityEngine.AI;
using MutliplayerView.Game.Mapping;
using MultiplayerView;

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
        private GameObject bulletPrefab;
        private GameObject bananaPrefab;
        private GameObject oilPrefab;
        private Camera camera;

        private MethodInfo registerEntityMethodView;
        private MethodInfo setEntityIdMethod;

        public NetworkFactoryView(GameObject usersPrefab, GameObject bulletPrefab, GameObject bananaPrefab, GameObject oilPrefab, Camera camera)
        {
            this.usersPrefab = usersPrefab;
            this.oilPrefab = oilPrefab;
            this.bulletPrefab = bulletPrefab;
            this.bananaPrefab = bananaPrefab;
            this.camera = camera;

            componentAssigner = new Dictionary<Type, ComponentAssigner>()
            {
                {typeof(Player), PlayerComponents},
                {typeof(ChasingBullet), ChasingBulletComponents},
                {typeof(Banana), BananaComponenets},
                {typeof(Oil), OilComponenets}
            };

            prefabsOfTypes = new Dictionary<Type, GameObject>()
            {
                { typeof(Player), this.usersPrefab},
                { typeof(ChasingBullet), this.bulletPrefab},
                { typeof(Banana), this.bananaPrefab},
                { typeof(Oil), this.oilPrefab}
            };

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
            string entityName = entityType.Name + ". Owner: " + entityCreated.ownerNetworkID + ". ObjectID: " + entityCreated.objectNetworkID + ".";

            ViewComponent viewComponent = BaseSceneView.AddSceneComponent(ViewArchitectureMap.ViewOf(entityType), entityName, null, prefabsOfTypes.ContainsKey(entityType) ? prefabsOfTypes[entityType] : null);

            setEntityIdMethod.Invoke(viewComponent, new object[] { entityCreated.ownerNetworkID, entityCreated.objectNetworkID });

            registerEntityMethodView.Invoke(EntityRegistryView, new object[] { viewComponent });

            viewComponent.transform.position = BaseSceneView.CoordinateToWorld(entityCreated.coordinate);

            if (componentAssigner.TryGetValue(entityType, out ComponentAssigner assigner))
                assigner(viewComponent);

            viewComponent.Init();
            viewComponent.LateInit();

            Type openEventType = typeof(EntityViewCreatedEvent<>);
            Type closedEventType = openEventType.MakeGenericType(entityType);

            MethodInfo raiseMethod = typeof(EventBus).GetMethod("Raise")
                .MakeGenericMethod(closedEventType);

            object[] eventParams = new object[] { entityCreated.ownerNetworkID, entityCreated.objectNetworkID };
            raiseMethod.Invoke(EventBus, new object[] { eventParams });
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

        private void ChasingBulletComponents(ViewComponent viewComponent)
        {
            GameObject gameObject = viewComponent.gameObject;

            BoxCollider boxCollider = gameObject.GetComponentInChildren<BoxCollider>();
            boxCollider.isTrigger = true;

            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();

            renderer.material.color = GameClient.MyID == (viewComponent as EntityView).OwnerNetworkID
                ? Color.green : Color.red;

            Vector3 spawnPos = gameObject.transform.position;

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                gameObject.transform.position = hit.position + Vector3.up * (gameObject.transform.localScale.y * 0.5f);
                NavMeshAgent navMesh = gameObject.AddComponent<NavMeshAgent>();
                navMesh.stoppingDistance = 0.1f;
                navMesh.speed = 25.0f;
                navMesh.autoBraking = false;
                navMesh.enabled = true;
            }
            else
            {
                Debug.LogError("Could not find a NavMesh near the spawn position! Bullet cannot be created.");
            }
        }

        private void BananaComponenets(ViewComponent viewComponent)
        {
            GameObject gameObject = viewComponent.gameObject;

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        private void OilComponenets(ViewComponent viewComponent)
        {
            GameObject gameObject = viewComponent.gameObject;

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<EntityCreatedEvent<Entity>>(OnEntityCreated);
        }
    }
}
