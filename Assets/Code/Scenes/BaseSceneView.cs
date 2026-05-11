using Assets.MultiplayerArchitecture.Code.Entities;
using MultiplayerArchitecture.Code.Scenes;
using MultiplayerView;
using System;
using UnityEngine;

namespace Assets.Code.Scenes
{
    internal abstract class BaseSceneView : BaseScene
    {
        public abstract void Init(params object[] parameters);

        public static ViewComponent AddSceneComponent(Type viewComponentType, string name, Transform parent = null, GameObject prefab = null)
        {
            if (!typeof(ViewComponent).IsAssignableFrom(viewComponentType))
                throw new InvalidOperationException();

            GameObject newSceneObject = prefab == null ? new GameObject() : UnityEngine.Object.Instantiate(prefab);
            newSceneObject.name = name;

            if (parent != null)
                newSceneObject.transform.parent = parent;

            ViewComponent viewComponent = newSceneObject.AddComponent(viewComponentType) as ViewComponent;
            Container container = GetContainer(viewComponent);

            if (container != null)
                container.Register(newSceneObject);

            return viewComponent;
        }

        public Vector3 CoordinateToWorld(Coordinate coordinate)
        {
            return new Vector3(coordinate.x, coordinate.y, coordinate.z);
        }

        public static Container GetContainer(ViewComponent component)
        {
            Transform parent = component.transform.parent;

            while (parent != null)
            {
                if (parent.gameObject.TryGetComponent(out Container container))
                    return container;
                else
                    parent = parent.parent;
            }

            return null;
        }
    }
}
