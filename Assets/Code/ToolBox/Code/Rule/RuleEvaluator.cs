using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ImageCampus.ToolBox.Rules
{
    public sealed class RuleEvaluator : IService
    {
        public bool IsPersistance => true;

        private Dictionary<string, RuleOperation> operations;

        public RuleEvaluator()
        {
            operations = new Dictionary<string, RuleOperation>();

            LoadAssembly(Assembly.GetExecutingAssembly());
            LoadAssembly(Assembly.GetCallingAssembly());
        }

        public void LoadAssembly(Assembly assembly) 
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsAbstract)
                {
                    if (typeof(RuleOperation).IsAssignableFrom(type))
                    {
                        RuleOperatorAttribute operatorAttribute = type.GetCustomAttribute<RuleOperatorAttribute>();
                        if (operatorAttribute != null)
                            operations.Add(operatorAttribute.operatorKey, Activator.CreateInstance(type) as RuleOperation);
                    }
                }
            }
        }

        public bool Evaluate(string operatorKey, int a, int b)
        {
            return operations[operatorKey].Evaluate(a, b);
        }
    }
}
