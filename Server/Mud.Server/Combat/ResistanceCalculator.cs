using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;

namespace Mud.Server.Combat;

[Export(typeof(IResistanceCalculator)), Shared]
public class ResistanceCalculator : IResistanceCalculator
{
    private ILogger<ResistanceCalculator> Logger { get; }

    public ResistanceCalculator(ILogger<ResistanceCalculator> logger)
    {
        Logger = logger;
    }

    public ResistanceLevels CheckResistance(ICharacter victim, SchoolTypes damageType)
    {
        string irvFlags;
        // Generic resistance
        var defaultResistance = ResistanceLevels.Normal;
        if (damageType <= SchoolTypes.Slash) // Physical
        {
            if (victim.Immunities.IsSet("Weapon"))
                defaultResistance = ResistanceLevels.Immune;
            else if (victim.Resistances.IsSet("Weapon"))
                defaultResistance = ResistanceLevels.Resistant;
            else if (victim.Vulnerabilities.IsSet("Weapon"))
                defaultResistance = ResistanceLevels.Vulnerable;
        }
        else // Magic
        {
            if (victim.Immunities.IsSet("Magic"))
                defaultResistance = ResistanceLevels.Immune;
            else if (victim.Resistances.IsSet("Magic"))
                defaultResistance = ResistanceLevels.Resistant;
            else if (victim.Vulnerabilities.IsSet("Magic"))
                defaultResistance = ResistanceLevels.Vulnerable;
        }
        // check specific damage
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
        // if immune to input damage -> immune
        // if resistant to input damage
        //      if default immune -> immune
        //      else -> resistant
        //  if vulnerable to input damage
        //      if default immune -> resistant
        //      if default resistant -> normal
        //      if default vulnerable -> vulnerable
        if (victim.Immunities.IsSet(irvFlags))
            return ResistanceLevels.Immune;
        if (victim.Resistances.IsSet(irvFlags))
        {
            if (defaultResistance == ResistanceLevels.Immune)
                return ResistanceLevels.Immune;
            return ResistanceLevels.Resistant;
        }
        if (victim.Vulnerabilities.IsSet(irvFlags))
        {
            if (defaultResistance == ResistanceLevels.Immune)
                return ResistanceLevels.Resistant;
            else if (defaultResistance == ResistanceLevels.Resistant)
                return ResistanceLevels.Normal;
            else
                return ResistanceLevels.Vulnerable;
        }
        return defaultResistance;
    }
}
