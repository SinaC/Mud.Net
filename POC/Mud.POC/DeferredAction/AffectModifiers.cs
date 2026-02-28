namespace Mud.POC.DeferredAction;

public class AffectModifiers
{
    public int HitRollBonus { get; set; }
    public int DamageRollBonus { get; set; }
    public int ArmorClassBonus { get; set; }

    public int StrengthBonus { get; set; }
    public int DexterityBonus { get; set; }
    public int ConstitutionBonus { get; set; }

    public int SaveBonus { get; set; }

    public AffectModifiers Clone()
    {
        return (AffectModifiers)MemberwiseClone();
    }

    public void Add(AffectModifiers other)
    {
        HitRollBonus += other.HitRollBonus;
        DamageRollBonus += other.DamageRollBonus;
        ArmorClassBonus += other.ArmorClassBonus;
        StrengthBonus += other.StrengthBonus;
        DexterityBonus += other.DexterityBonus;
        ConstitutionBonus += other.ConstitutionBonus;
        SaveBonus += other.SaveBonus;
    }
}
