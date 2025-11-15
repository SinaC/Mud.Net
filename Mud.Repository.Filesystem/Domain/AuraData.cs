namespace Mud.Repository.Filesystem.Domain;

public class AuraData
{
    public string AbilityName { get; set; }

    public int Level { get; set; }

    public int PulseLeft { get; set; }

    public int AuraFlags { get; set; }

    public AffectDataBase[] Affects { get; set; }
}
