// Util.cs
// 
// Author:
//        JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
// 
// Copyright (c) 2023 NomLib

namespace NomLib.Misc;

public static class Util
{
    public static string ToCamelCase(this string str) =>
        string.IsNullOrEmpty(str) || str.Length < 2
            ? str?.ToLowerInvariant()
            : char.ToLowerInvariant(str[0]) + str.Substring(1);

    public static string GetCppType(this string fullQualifiedType)
    {
        if (fullQualifiedType == Config.StdIntType)
        {
            return "int64_t";
        }

        return fullQualifiedType == Config.StdFloatType ? "double" : "void*";
    }
    
    public static string GetDefaultReturnValue(this string fullQualifiedType)
    {
        if (fullQualifiedType == Config.StdIntType)
        {
            return "0";
        }

        return fullQualifiedType == Config.StdFloatType ? "0.0" : "nullptr";
    }
}