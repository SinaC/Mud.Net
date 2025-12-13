namespace Mud.POC.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class PolymorphismAttribute : Attribute
{
    public Type BaseType { get; }
    public string? Discriminator { get; }

    public PolymorphismAttribute(Type baseType)
    {
        BaseType = baseType;
    }

    public PolymorphismAttribute(Type baseType, string discriminator)
        : this(baseType) {
        {
            Discriminator = discriminator;
        }
    }
}