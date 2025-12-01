using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24
{
    [Export(typeof(IDamageModifierManager)), Shared]
    public class DamageModifierManager : IDamageModifierManager
    {
        private ILogger<DamageModifierManager> Logger { get; }

        public DamageModifierManager(ILogger<DamageModifierManager> logger)
        {
            Logger = logger;
        }

        public ResistanceLevels ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, ref int damage)
        {
            if (damage > 1 && this is IPlayableCharacter pcVictim && pcVictim[Conditions.Drunk] > 10)
                damage -= damage / 10;
            if (damage > 1 && victim.ShieldFlags.IsSet("Sanctuary"))
                damage /= 2;
            if (damage > 1
                && ((victim.ShieldFlags.IsSet("ProtectEvil") && source.IsEvil)
                    || (victim.ShieldFlags.IsSet("ProtectGood") && source.IsGood)))
                damage -= damage / 4;
            ResistanceLevels resistanceLevel = CheckResistance(victim, damageType);
            switch (resistanceLevel)
            {
                case ResistanceLevels.Immune:
                    damage = 0;
                    break;
                case ResistanceLevels.Resistant:
                    damage -= damage / 3;
                    break;
                case ResistanceLevels.Vulnerable:
                    damage += damage / 2;
                    break;
            }
            return resistanceLevel ;
        }


        public ResistanceLevels CheckResistance(ICharacter victim, SchoolTypes damageType)
        {
            string irvFlags;
            // Generic resistance
            ResistanceLevels defaultResistance = ResistanceLevels.Normal;
            if (damageType <= SchoolTypes.Slash) // Physical
            {
                if (victim.Immunities.IsSet("Weapon"))
                    defaultResistance = ResistanceLevels.Immune;
                else if (victim.Resistances.IsSet("Weapon"))
                    defaultResistance = ResistanceLevels.Resistant;
                else if (victim.Vulnerabilities.IsSet("Weapon"))
                    defaultResistance = ResistanceLevels.Normal;
            }
            else // Magic
            {
                if (victim.Immunities.IsSet("Magic"))
                    defaultResistance = ResistanceLevels.Immune;
                else if (victim.Resistances.IsSet("Magic"))
                    defaultResistance = ResistanceLevels.Resistant;
                else if (victim.Vulnerabilities.IsSet("Magic"))
                    defaultResistance = ResistanceLevels.Normal;
            }
            switch (damageType)
            {
                case SchoolTypes.None:
                    return ResistanceLevels.None; // no Resistance
                case SchoolTypes.Bash:
                case SchoolTypes.Pierce:
                case SchoolTypes.Slash:
                    irvFlags = "Weapon";
                    break;
                case SchoolTypes.Fire:
                    irvFlags = "Fire";
                    break;
                case SchoolTypes.Cold:
                    irvFlags = "Cold";
                    break;
                case SchoolTypes.Lightning:
                    irvFlags = "Lightning";
                    break;
                case SchoolTypes.Acid:
                    irvFlags = "Acid";
                    break;
                case SchoolTypes.Poison:
                    irvFlags = "Poison";
                    break;
                case SchoolTypes.Negative:
                    irvFlags = "Negative";
                    break;
                case SchoolTypes.Holy:
                    irvFlags = "Holy";
                    break;
                case SchoolTypes.Energy:
                    irvFlags = "Energy";
                    break;
                case SchoolTypes.Mental:
                    irvFlags = "Mental";
                    break;
                case SchoolTypes.Disease:
                    irvFlags = "Disease";
                    break;
                case SchoolTypes.Drowning:
                    irvFlags = "Drowning";
                    break;
                case SchoolTypes.Light:
                    irvFlags = "Light";
                    break;
                case SchoolTypes.Other: // no specific IRV
                    return defaultResistance;
                case SchoolTypes.Harm: // no specific IRV
                    return defaultResistance;
                case SchoolTypes.Charm:
                    irvFlags = "Charm";
                    break;
                case SchoolTypes.Sound:
                    irvFlags = "Sound";
                    break;
                default:
                    Logger.LogError("CharacterBase.CheckResistance: Unknown {schoolType} {damageType}", nameof(SchoolTypes), damageType);
                    return defaultResistance;
            }
            // Following code has been reworked because Rom24 was testing on currently computed resistance (imm) instead of defaultResistance (def)
            ResistanceLevels resistance = ResistanceLevels.None;
            if (victim.Immunities.IsSet(irvFlags))
                resistance = ResistanceLevels.Immune;
            else if (victim.Resistances.IsSet(irvFlags) && defaultResistance != ResistanceLevels.Immune)
                resistance = ResistanceLevels.Resistant;
            else if (victim.Vulnerabilities.IsSet(irvFlags))
            {
                if (defaultResistance == ResistanceLevels.Immune)
                    resistance = ResistanceLevels.Resistant;
                else if (defaultResistance == ResistanceLevels.Resistant)
                    resistance = ResistanceLevels.Normal;
                else
                    resistance = ResistanceLevels.Vulnerable;
            }
            // if no specific resistance found, return generic one
            if (resistance == ResistanceLevels.None)
                return defaultResistance;
            // else, return specific resistance
            return resistance;
        }
    }
}
