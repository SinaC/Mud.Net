namespace Mud.Domain;

public enum ResourceKinds // must starts at 0 and no hole (is used as index in array)
{
    HitPoints   = 0,
    MovePoints  = 1,
    Mana        = 2,
    Psy         = 3,
    Energy      = 4, // max 100, increase at rate 10 energy/second
    Rage        = 5, // max 100, deplete 1 rage/second when OOC
    Combo       = 6, // max 5, increase and decrease when using specific abilities
}
