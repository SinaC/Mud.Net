namespace Mud.POC.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DiscriminatorAttribute : Attribute
{
    public string? Discriminator { get; }

    public DiscriminatorAttribute()
    {
    }

    public DiscriminatorAttribute(string discriminator)
    {
        Discriminator = discriminator;
    }
}
