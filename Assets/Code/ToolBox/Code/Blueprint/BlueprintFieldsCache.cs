using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ImageCampus.ToolBox.Blueprints
{
	public sealed class BlueprintFieldsCache : IService
	{
		public bool IsPersistance => true;

		private Dictionary<Type, List<FieldInfo>> fieldsInType;
		private Dictionary<FieldInfo, (bool hasAttribute, BlueprintParameterAttribute attribute)> attributeInFields;
		private Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoFromTypeAndParameterKey;

		public BlueprintFieldsCache()
		{
			fieldsInType = new Dictionary<Type, List<FieldInfo>>();
			attributeInFields = new Dictionary<FieldInfo, (bool hasAttribute, BlueprintParameterAttribute attribute)>();
			fieldInfoFromTypeAndParameterKey = new Dictionary<Type, Dictionary<string, FieldInfo>>();
		}

		public FieldInfo this[Type type, string blueprintParameterKey]
		{
			get
			{
				if (!fieldInfoFromTypeAndParameterKey.ContainsKey(type))
				{
					fieldInfoFromTypeAndParameterKey.Add(type, new Dictionary<string, FieldInfo>());

					foreach (FieldInfo fieldInfo in GetFields(type))
					{
						(bool hasAttriburte, BlueprintParameterAttribute blueprintParameterAttribute) parameter =
							GetBlueprintParameterAttribute(fieldInfo);

						if (parameter.hasAttriburte)
						{
							if (!fieldInfoFromTypeAndParameterKey[type].ContainsKey(parameter.blueprintParameterAttribute.ParameterHeader))
								fieldInfoFromTypeAndParameterKey[type].Add(parameter.blueprintParameterAttribute.ParameterHeader, fieldInfo);
						}
					}
				}

				return fieldInfoFromTypeAndParameterKey[type][blueprintParameterKey];
			}
		}

		public List<FieldInfo> GetFields(Type type)
		{
			if (!fieldsInType.ContainsKey(type))
			{
				fieldsInType.Add(type, new List<FieldInfo>());
				Type currentType = type;

				while (currentType != typeof(object))
				{
					fieldsInType[type].AddRange(type.GetFields(
						BindingFlags.Public | BindingFlags.NonPublic |
						BindingFlags.Instance | BindingFlags.DeclaredOnly));

					currentType = currentType.BaseType;
				}
			}

			return fieldsInType[type];
		}

		public (bool hasAttribute, BlueprintParameterAttribute attribute)
			GetBlueprintParameterAttribute(FieldInfo fieldInfo)
		{
			if (!attributeInFields.ContainsKey(fieldInfo))
			{
				bool contains = false;
				foreach (Attribute attribute in fieldInfo.GetCustomAttributes())
				{
					if (attribute is BlueprintParameterAttribute)
					{
						attributeInFields.Add(fieldInfo, (true, attribute as BlueprintParameterAttribute));
						contains = true;
						break;
					}
				}

				if (!contains)
					attributeInFields.Add(fieldInfo, (false, null));
			}
			return attributeInFields[fieldInfo];
		}
	}
}