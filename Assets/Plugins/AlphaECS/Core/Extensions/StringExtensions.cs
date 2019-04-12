using System;

public static partial class StringExtensions
{
	public static Type GetTypeWithAssembly(this string typeName)
	{
        return TypeUtilities.GetTypeWithAssembly(typeName);
	}

	public static Type TryGetConvertedType(this string typeName)
	{
        return TypeUtilities.TryGetConvertedType(typeName);
	}
}
