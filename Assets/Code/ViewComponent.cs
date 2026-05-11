using UnityEngine;

namespace Assets.Code
{
    public class ViewComponent : MonoBehaviour
    {
        public virtual void Init() { }

        public virtual void Init(params object[] parameters)
        {
            Init();
        }

        public virtual void LateInit() { }

        public virtual void Tick(float deltaTime) { }

        public virtual void Dispose() { }
    }
}
