﻿using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Buff, PulseWaitTime = 24)]
    public class HolyWord : NoTargetSpellBase
    {
        public const string SpellName = "Holy Word";

        private IAuraManager AuraManager { get; }

        public HolyWord(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            Caster.Act(ActOptions.ToRoom, "{0:N} utters a word of divine power!");
            Caster.Send("You utter a word of divine power.");

            foreach (ICharacter victim in Caster.Room.People)
            {
                if ((Caster.IsGood && victim.IsGood)
                    || (Caster.IsNeutral && victim.IsNeutral)
                    || (Caster.IsEvil && victim.IsEvil))
                {
                    victim.Send("You feel full more powerful.");
                    FrenzyEffect frenzyEffect = new FrenzyEffect(AuraManager);
                    frenzyEffect.Apply(victim, Caster, Frenzy.SpellName, Level, 0);
                    BlessEffect blessEffect = new BlessEffect(AuraManager);
                    blessEffect.Apply(victim, Caster, Bless.SpellName, Level, 0);
                }
                else if ((Caster.IsGood && victim.IsEvil)
                         || (Caster.IsEvil && victim.IsGood))
                {
                    if (!victim.SavesSpell(Level, SchoolTypes.Holy))
                    {
                        victim.Send("You are struck down!");
                        int damage = RandomManager.Dice(Level, 6);
                        var damageResult = victim.AbilityDamage(Caster, damage, SchoolTypes.Holy, "divine wrath", true);
                        if (damageResult != DamageResults.Killed)
                        {
                            CurseEffect curseEffect = new CurseEffect(AuraManager);
                            curseEffect.Apply(victim, Caster, Curse.SpellName, Level, 0);
                        }
                    }

                    else if (Caster.IsNeutral)
                    {
                        if (!victim.SavesSpell(Level, SchoolTypes.Holy))
                        {
                            victim.Send("You are struck down!");
                            int damage = RandomManager.Dice(Level, 4);
                            var damageResult = victim.AbilityDamage(Caster, damage, SchoolTypes.Holy, "divine wrath", true);
                            if (damageResult != DamageResults.Killed)
                            {
                                CurseEffect curseEffect = new CurseEffect(AuraManager);
                                curseEffect.Apply(victim, Caster, Curse.SpellName, Level, 0);
                            }
                        }
                    }
                }
            }

            Caster.Send("You feel drained.");
            Caster.UpdateMovePoints(-Caster.MovePoints); // set to 0
            Caster.UpdateHitPoints(-Caster.HitPoints / 2);
        }
    }
}
