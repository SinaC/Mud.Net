namespace Mud.Repository.Filesystem.Domain;

public class CharacterShieldFlagsAffectData : AffectDataBase
{
    public int Operator { get; set; } // Add and Or are identical

    public string Modifier { get; set; }
}
