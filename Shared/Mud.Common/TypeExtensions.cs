using System.Reflection;

namespace Mud.Common;

public static class TypeExtensions
{
    // search MethodName(parameterTypes[0], parameterTypes[1], ...)
    public static MethodInfo? SearchMethod(this Type type, string methodName, params Type[] parameterTypes)
    {
        var initializeMethods = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.Name == methodName && x.GetParameters().Length == parameterTypes.Length)
            .ToArray();
        if (initializeMethods.Length == 0)
            return null;
        if (parameterTypes.Length == 0)
        {
            // don't use SingleOrDefault to avoid raising exception
            if (initializeMethods.Length == 1)  // should only one parameterless method with a specific name
                return initializeMethods[0];
            return null;
        }
        var validInitializeMethods = new List<MethodInfo>();
        foreach (var initializeMethod in initializeMethods)
        {
            // check method parameters
            var methodParameters = initializeMethod.GetParameters();
            var isValid = true;
            for(var i = 0; i < parameterTypes.Length; i++)
            {
                if (!parameterTypes[i].IsAssignableTo(methodParameters[i].ParameterType))
                {
                    isValid = false;
                    break;
                }
            }
            if (isValid)
                validInitializeMethods.Add(initializeMethod);
        }
        if (validInitializeMethods.Count == 1)
            return validInitializeMethods[0];
        return null;
    }

    // search MethodName<genericType[0], genericType[1], ...>(parameterTypes[0], parameterTypes[1], ...)
    public static MethodInfo? SearchMethod(this Type type, string methodName, Type[] genericArgumentTypes, Type[] parameterTypes)
    {
        var initializeMethods = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            .Where(x => x.Name == methodName && x.IsGenericMethod == true && x.GetGenericArguments().Length == genericArgumentTypes.Length && x.GetParameters().Length == parameterTypes.Length)
            .ToArray();
        if (initializeMethods.Length == 0)
            return null;
        var validInitializeMethods = new List<MethodInfo>();
        foreach (var initializeMethod in initializeMethods)
        {
            var isValid = true;
            // check generic arguments
            var genericArguments = initializeMethod.GetGenericArguments();
            for (var i = 0; i < genericArguments.Length; i++)
            {
                if (genericArgumentTypes[i] != genericArguments[i].BaseType)
                {
                    isValid = false;
                    break;
                }
            }
            if (isValid)
            {
                // check method parameters
                var methodParameters = initializeMethod.GetParameters();
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    if (!parameterTypes[i].IsAssignableTo(methodParameters[i].ParameterType.ContainsGenericParameters ? methodParameters[i].ParameterType.BaseType : methodParameters[i].ParameterType))
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            if (isValid)
                validInitializeMethods.Add(initializeMethod);
        }
        if (validInitializeMethods.Count == 1)
            return validInitializeMethods[0].MakeGenericMethod(genericArgumentTypes);
        return null;
    }
}
