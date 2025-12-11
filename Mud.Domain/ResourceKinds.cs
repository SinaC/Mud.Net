namespace Mud.Domain;

public enum ResourceKinds // must starts at 0 and no hole (is used as index in array)
{
    Mana        = 0,
    Psy         = 1,
    Energy      = 2, // max 100, increase at rate 10 energy/second
    Rage        = 3, // max 100, deplete 1 rage/second when OOC
    Combo       = 4, // max 5, increase and decrease when using specific abilities
}
