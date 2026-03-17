using ImageCampus.ToolBox.Cast;
using ImageCampus.ToolBox.Services;
using System;
using System.Reflection;

namespace ImageCampus.ToolBox.Blueprints
{
	public sealed class BlueprintBinder : IService
	{
		public bool IsPersistance => true;

		private BlueprintRegistry BlueprintRegistry => ServiceProvider.Instance.GetService<BlueprintRegistry>();
		private BlueprintFieldsCache BlueprintFieldsCache => ServiceProvider.Instance.GetService<BlueprintFieldsCache>();

		public BlueprintBinder() { }

		public void Apply(ref object instance, string blueprintTable, string blueprintID)
		{
			Type instanceType = instance.GetType();

			do
			{
				foreach (FieldInfo fieldInfo in BlueprintFieldsCache.GetFields(instanceType))
				{
					(bool hasAttribute, BlueprintParameterAttribute attribute) blueprintParameter =
						BlueprintFieldsCache.GetBlueprintParameterAttribute(fieldInfo);

					if (blueprintParameter.hasAttribute)
					{
						object castedValue;
						try
						{
							castedValue = StringCast.Convert(
							   BlueprintRegistry.BlueprintDatas[blueprintTable]
							   [blueprintID, blueprintParameter.attribute.ParameterHeader], fieldInfo.FieldType);
						}
						catch (InvalidCastException exception)
						{

							throw new DataMisalignedException(exception.Message);
						}

						fieldInfo.SetValue(instance, castedValue);
					}
				}
				instanceType = instanceType.BaseType;

			} while (instanceType != typeof(object));

		}


	}
}