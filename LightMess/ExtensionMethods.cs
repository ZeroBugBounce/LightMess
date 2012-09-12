using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroBugBounce.LightMess
{
	public static class ExtensionMethods
	{
		public static Boolean InheritsFrom(this Type type, Type fromType)
		{
			return type.AllBaseTypes().Where(t => t == fromType).Any();
		}

		public static IEnumerable<Type> AllBaseTypes(this Type type)
		{
			if(type.BaseType != typeof(Object))
			{
				yield return type.BaseType;

				foreach (var baseType in AllBaseTypes(type.BaseType))
				{
					yield return baseType;
				}

				if (type.BaseType.IsGenericType)
				{
					var baseGeneric = type.BaseType.GetGenericTypeDefinition();

					yield return baseGeneric;

					foreach (var genericBaseType in AllBaseTypes(baseGeneric))
					{
						yield return genericBaseType;
					}
				}
			}
		}
	}
}
