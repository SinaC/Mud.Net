namespace Mud.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class JsonBaseTypeAttribute : Attribute
{
    public Type BaseType { get; }
    public string? Discriminator { get; }

    public JsonBaseTypeAttribute(Type baseType)
    {
        BaseType = baseType;
    }

    public JsonBaseTypeAttribute(Type baseType, string discriminator)
        : this(baseType)
    {
        {
            Discriminator = discriminator;
        }
    }
}
