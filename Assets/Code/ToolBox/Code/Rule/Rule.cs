using ImageCampus.ToolBox.Blueprints;
using ImageCampus.ToolBox.Cast;
using ImageCampus.ToolBox.Dataflow;
using ImageCampus.ToolBox.Services;
using System;

namespace ImageCampus.ToolBox.Rules
{
	public sealed class Rule : IInitable
	{
		private const string ACCESS_TOKENS_SEPARATOR = " - ";
		private const string SUB_ACCESS_TOKENS_SEPARATOR = " / ";
		private const char OPEN_INSTANCE_REFERENCE_KEY = '{';
		private const char CLOSE_INSTANCE_RFERENCE_KEY = '}';
		private const char OPEN_SERVICE_REFERENCE_KEY = '[';
		private const char CLOSE_SERVICE_REFERENCE_KEY = ']';
		private const char OPEN_BLUEPRINT_REFERENCE_KEY = '(';
		private const char CLOSE_BLUEPRINT_REFERENCE_KEY = ')';
		private const char OPEN_SUB_ACCESS_TOKEN_REFERENCE_KEY = '<';
		private const char CLOSE_SUB_ACCESS_TOKEN_REFERENCE_KEY = '>';

		private RuleEvaluator RuleEvaluator => ServiceProvider.Instance.GetService<RuleEvaluator>();
		private BlueprintRegistry BlueprintRegistry => ServiceProvider.Instance.GetService<BlueprintRegistry>();
		private BlueprintFieldsCache BlueprintFieldsCache => ServiceProvider.Instance.GetService<BlueprintFieldsCache>();

		[BlueprintParameter("Value A")] private string valueA;
		[BlueprintParameter("Value B")] private string valueB;
		[BlueprintParameter("Operator")] private string operatorKey;

		private delegate int PointerToValue(object instance = null);

		private PointerToValue valueOfA;
		private PointerToValue valueOfB;

		public Rule() { }

		public void Init()
		{
			valueOfA = CreatePointer(valueA);
			valueOfB = CreatePointer(valueB);
		}

		private PointerToValue CreatePointer(string dataPath)
		{
			string[] accessTokens = dataPath.Split(ACCESS_TOKENS_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

			if (IsInstanceReference(accessTokens[0]))
				return (instance) => GetFromInstance(accessTokens[1], instance);

			if (IsServiceReference(accessTokens[0]))
				return (instance) => GetFromService(accessTokens, instance);

			if (IsBlueprintReference(accessTokens[1]))
				return (instance) => StringCast.Convert<int>(BlueprintRegistry[accessTokens[0], Convert.ToString(instance), accessTokens[2]]);

			return (instance) => StringCast.Convert<int>(BlueprintRegistry[accessTokens[0], accessTokens[1], accessTokens[2]]);

			bool IsInstanceReference(string headerToken)
			{
				return headerToken[0] == OPEN_INSTANCE_REFERENCE_KEY &&
					headerToken[^1] == CLOSE_INSTANCE_RFERENCE_KEY;
			}

			bool IsServiceReference(string headerToken)
			{
				return headerToken[0] == OPEN_SERVICE_REFERENCE_KEY &&
					headerToken[^1] == CLOSE_SERVICE_REFERENCE_KEY;
			}

			bool IsBlueprintReference(string blueprintIdToken)
			{
				return blueprintIdToken[0] == OPEN_BLUEPRINT_REFERENCE_KEY &&
					blueprintIdToken[^1] == CLOSE_BLUEPRINT_REFERENCE_KEY;
			}

			int GetFromInstance(string parameterKey, object instance)
			{
				return (int)BlueprintFieldsCache[instance.GetType(), parameterKey].GetValue(instance);
			}

			int GetFromService(string[] accessTokens, object instance)
			{
				string serviceReference = accessTokens[0]
					.Replace(Convert.ToString(OPEN_SERVICE_REFERENCE_KEY), string.Empty)
					.Replace(Convert.ToString(CLOSE_SERVICE_REFERENCE_KEY), string.Empty);

				if (accessTokens[1][0] == OPEN_SUB_ACCESS_TOKEN_REFERENCE_KEY &&
					accessTokens[1][^1] == CLOSE_SUB_ACCESS_TOKEN_REFERENCE_KEY)
				{
					accessTokens[1] = accessTokens[1]
						.Replace(Convert.ToString(OPEN_SUB_ACCESS_TOKEN_REFERENCE_KEY), string.Empty)
						.Replace(Convert.ToString(CLOSE_SUB_ACCESS_TOKEN_REFERENCE_KEY), string.Empty);
					string[] subAccessTokens = accessTokens[1].Split(SUB_ACCESS_TOKENS_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
					accessTokens[1] = BlueprintRegistry[subAccessTokens[0], Convert.ToString(instance), subAccessTokens[2]];
				}

				return Convert.ToInt32(ServiceProvider.Instance.GetDataService(serviceReference).GetDataValue(accessTokens));
			}
		}

		public void LateInit() { }

		public bool Evaluate(object instanceOfA = null, object instanceOfB = null)
		{
			return RuleEvaluator.Evaluate(operatorKey, valueOfA.Invoke(instanceOfA), valueOfB.Invoke(instanceOfB));
		}
	}
}