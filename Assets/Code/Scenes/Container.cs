using Assets.Code;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerView
{
    internal sealed class Container : ViewComponent
    {
        private Dictionary<int, GameObject> instancesPerId;

        public override void Init()
        {
            instancesPerId = new Dictionary<int, GameObject>();
        }

        public void Register(GameObject gameObject) 
        {
            instancesPerId.Add(gameObject.GetInstanceID(), gameObject);
        }

        public GameObject this[int instanceId] => instancesPerId[instanceId];
    }
}
