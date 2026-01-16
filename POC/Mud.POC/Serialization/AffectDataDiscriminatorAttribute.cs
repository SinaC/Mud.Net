namespace Mud.POC.Serialization;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AffectDataDiscriminatorAttribute : Attribute
{
    public string? Discriminator { get; }

    public AffectDataDiscriminatorAttribute()
    {
    }

    public AffectDataDiscriminatorAttribute(string discriminator)
    {
        Discriminator = discriminator;
    }
}
