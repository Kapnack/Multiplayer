using Assets.MultiplayerArchitecture.Code.Entities.Events;
using Assets.MultiplayerArchitecture.Code.Network;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

namespace Assets.Code.Entities
{
    internal class EntityFactoryView : IInitable, IDisposable
    {
        EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        GameClient GameClient => ServiceProvider.Instance.GetService<GameClient>();
        EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();

        private GameObject usersPrefab;
        private Camera camera;

        public EntityFactoryView(GameObject usersPrefab, Camera camera)
        {
            this.usersPrefab = usersPrefab;
            this.camera = camera;
        }

        public void Init()
        {
            EventBus.Subscribe<EntityCreated>(OnEntityCreated);
        }

        public void LateInit()
        {

        }

        private void OnEntityCreated(in EntityCreated entityCreated)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(usersPrefab);

            gameObject.AddComponent<EntityView>();

            EntityView entityView = gameObject.GetComponent<EntityView>();

            EntityRegistryView.OnEntityCreated(entityCreated.objectID, entityView);

            gameObject.AddComponent<Rigidbody>();

            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();

            if (GameClient.MyID == entityCreated.networkClientID)
            {
                renderer.material.color = Color.green;
                gameObject.AddComponent<PlayerController>();
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
            EventBus.Unsubscribe<EntityCreated>(OnEntityCreated);
        }
    }
}
