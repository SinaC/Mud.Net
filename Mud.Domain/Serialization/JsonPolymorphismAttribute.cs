namespace Mud.Domain.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class JsonPolymorphismAttribute : Attribute
{
    public Type BaseType { get; }
    public string? Discriminator { get; }

    public JsonPolymorphismAttribute(Type baseType)
    {
        BaseType = baseType;
    }

    public JsonPolymorphismAttribute(Type baseType, string discriminator)
        : this(baseType)
    {
        {
            Discriminator = discriminator;
        }
    }
}
