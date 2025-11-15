namespace Mud.Repository.Filesystem.Domain;

public class CharacterIRVAffectData : AffectDataBase
{
    public int Location { get; set; }

    public int Operator { get; set; } // Add and Or are identical

    public string Modifier { get; set; }
}
