using Assets.Code.Architecture.Code.Entities;
using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

namespace Assets.Code
{
    public class EntityView : MonoBehaviour
    {
        protected EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

        protected uint archiectureEntitiyID;
        public uint ArchitectureEnitityID => archiectureEntitiyID;
        protected Entity ArchitectureEntity => EntityRegistry.GetAs<Entity>(archiectureEntitiyID);

        public static string SetIdMethodName => nameof(SetId);

        private void SetId(uint ID)
        {
            archiectureEntitiyID = ID;
        }
    }
}
