using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZooArchitect.View.Mapping
{
    public static class ViewArchitectureMap
    {
        private static Dictionary<Type, Type> architectureToViewTypes;
        private static Dictionary<Type, Type> viewToArchitectureTypes;

        public static void Init()
        {
            architectureToViewTypes = new Dictionary<Type, Type>();
            viewToArchitectureTypes = new Dictionary<Type, Type>();

            foreach (Type viewType in Assembly.GetCallingAssembly().GetTypes())
            {
                List<ViewOfAttribute> attributes = new List<ViewOfAttribute>(viewType.GetCustomAttributes<ViewOfAttribute>());
                if (attributes.Count >= 1)
                {
                    Type architectureType = attributes[0].architectureType;

                    viewToArchitectureTypes.Add(viewType, architectureType);
                    architectureToViewTypes.Add(architectureType, viewType);
                }
            }
        }

        public static Type ViewOf(Type architectureType) => architectureToViewTypes[architectureType];
        public static Type ArchitectureOf(Type viewType) => viewToArchitectureTypes[viewType];
    }

}
