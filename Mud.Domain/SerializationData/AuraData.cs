namespace Mud.Domain.SerializationData;

public class AuraData
{
    public required string AbilityName { get; set; }

    public required int Level { get; set; }

    public required int PulseLeft { get; set; }

    public required AuraFlags AuraFlags { get; set; }

    public required AffectDataBase[] Affects { get; set; }
}
