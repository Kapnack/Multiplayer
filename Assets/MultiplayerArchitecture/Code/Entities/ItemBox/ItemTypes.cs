using MultiplayerArchitecture;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assets.MultiplayerArchitecture.Code.Entities.ItemBox
{
    public static class ItemTypes
    {
        private static List<Type> powerUps;

        public static Type GetItem(int index) => powerUps[index];
        public static int Count => powerUps.Count;

        public static void Init()
        {
            powerUps = new List<Type>();

            foreach (Type classType in typeof(ItemTypes).Assembly.GetTypes())
            {
                List<ItemAttribute> attributes = new List<ItemAttribute>(classType.GetCustomAttributes<ItemAttribute>());

                if (attributes.Count >= 1)
                    powerUps.Add(classType);
            }
        }
    }
}
