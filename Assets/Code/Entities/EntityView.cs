using UnityEngine;

namespace Assets.Code.Entities
{
    internal class EntityView : MonoBehaviour
    {
        public uint ArchitectureID { get; private set; }

        static public string SetIDName => nameof(SetID);

        private void SetID(uint architectureID)
        {
            ArchitectureID = architectureID;
        }
    }
}