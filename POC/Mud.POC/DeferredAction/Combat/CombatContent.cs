using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class CombatContext
{
    public Mob Attacker { get; }
    public Mob Defender { get; }

    public bool IsHit { get; set; }
    public bool IsCritical { get; set; }

    public int HitRoll { get; set; }
    public int ArmorClass { get; set; }

    public int BaseDamage { get; set; }
    public int ModifiedDamage { get; set; }
    public int FinalDamage { get; set; }

    public DamageType DamageType { get; set; } = DamageType.Physical;

    public Skill? Spell { get; set; }
    public bool IsSpell { get; set; }

    public SaveType? SaveType { get; set; }

    public bool IsBackstab { get; set; }

    public CombatContext(Mob attacker, Mob defender)
    {
        Attacker = attacker;
        Defender = defender;
    }
}
