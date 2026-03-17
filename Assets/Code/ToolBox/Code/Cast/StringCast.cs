using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ImageCampus.ToolBox.Cast
{
    public static class StringCast
    {
        private static readonly Dictionary<Type, MethodInfo> addMethods = new Dictionary<Type, MethodInfo>();
        private static readonly Dictionary<Type, Type[]> dictionaryTypes = new Dictionary<Type, Type[]>();
        private static readonly Dictionary<Type, Type> elementTypes = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, bool> isGenericCollections = new Dictionary<Type, bool>();
        private static readonly Dictionary<Type, bool> isDictionary = new Dictionary<Type, bool>();

        public static T Convert<T>(string value) 
        {
            return (T)Convert(value, typeof(T));
        }

        public static object Convert(string value, Type targetType)
        {
            try
            {
                if (targetType == typeof(string))
                    return value;

                if (targetType == typeof(bool))
                {
                    if (string.Equals(value.ToLower(), "true"))
                        return true;

                    if (string.Equals(value.ToLower(), "false"))
                        return false;
                }

                Type nullableType = Nullable.GetUnderlyingType(targetType);
                if (nullableType != null)
                {
                    if (string.IsNullOrEmpty(value))
                        return null;

                    return Convert(value, nullableType);
                }

                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value, true);

                if (IsDictionary(targetType))
                    return ConvertDictionary(value, targetType);

                if (targetType.IsArray)
                    return ConvertArray(value, targetType);

                if (IsGenericCollection(targetType))
                    return ConvertGenericCollection(value, targetType);

                return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new InvalidCastException($"Failed to convert {value} into {targetType.Name}");
            }
            
        }

        private static object ConvertDictionary(string value, Type dictionaryType)
        {
            object dictionary = Activator.CreateInstance(dictionaryType);

            Type[] keyValueTypes = GetDictionaryTypes(dictionaryType);
            Type keyType = keyValueTypes[0];
            Type valueType = keyValueTypes[1];

            MethodInfo addMethod = GetAddMethod(dictionaryType);
            string[] entries = SplitDictionaryEntries(value);

            for (int i = 0; i < entries.Length; i++)
            {
                string entry = entries[i].Trim('[', ']');
                string[] keyValuePair = entry.Split(';', 2);

                object keyEntry = Convert(keyValuePair[0], keyType);
                object valueEntry = Convert(keyValuePair[1], valueType);

                addMethod.Invoke(dictionary, new[] { keyEntry, valueEntry });
            }

            return dictionary;
        }

        private static bool IsDictionary(Type type)
        {
            if (!isDictionary.TryGetValue(type, out bool result))
            {
                result = type.IsGenericType &&
                         type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

                isDictionary.Add(type, result);
            }

            return result;
        }

        private static Type[] GetDictionaryTypes(Type type)
        {
            if (!dictionaryTypes.TryGetValue(type, out Type[] types))
            {
                types = type.GetGenericArguments();
                dictionaryTypes.Add(type, types);
            }

            return types;
        }

        private static object ConvertArray(string value, Type arrayType)
        {
            Type elementType = GetElementType(arrayType);
            string[] tokens = SplitCollection(value);

            Array array = Array.CreateInstance(elementType, tokens.Length);
            for (int i = 0; i < tokens.Length; i++)
            {
                array.SetValue(Convert(tokens[i], elementType), i);
            }

            return array;
        }

        private static object ConvertGenericCollection(string value, Type genericCollectionType)
        {
            Type elementType = GetElementType(genericCollectionType);
            MethodInfo addMethod = GetAddMethod(genericCollectionType);

            object collection = Activator.CreateInstance(genericCollectionType);
            string[] tokens = SplitCollection(value);

            for (int i = 0; i < tokens.Length; i++)
            {
                object elementOfCollection = Convert(tokens[i], elementType);
                addMethod.Invoke(collection, new[] { elementOfCollection });
            }

            return collection;
        }

        private static bool IsGenericCollection(Type type)
        {
            if (!isGenericCollections.TryGetValue(type, out bool result))
            {
                result = type.IsGenericType &&
                         typeof(IEnumerable).IsAssignableFrom(type) &&
                         type.GetMethod("Add") != null;

                isGenericCollections.Add(type, result);
            }

            return result;
        }

        private static Type GetElementType(Type type)
        {
            if (!elementTypes.TryGetValue(type, out Type elementType))
            {
                if (type.IsArray)
                    elementType = type.GetElementType();
                else
                    elementType = type.GetGenericArguments()[0];

                elementTypes.Add(type, elementType);
            }

            return elementType;
        }

        private static MethodInfo GetAddMethod(Type type)
        {
            if (!addMethods.TryGetValue(type, out MethodInfo method))
            {
                method = type.GetMethod("Add");
                if (method == null)
                    throw new InvalidOperationException($"Type {type} has no Add method");

                addMethods.Add(type, method);
            }

            return method;
        }

        private static string[] SplitCollection(string value)
        {
            return value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        private static string[] SplitDictionaryEntries(string value)
        {
            return value.Split("],", StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
