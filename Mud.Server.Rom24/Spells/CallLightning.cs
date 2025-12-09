using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Collections.ObjectModel;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.DamageArea)]
[Syntax("cast [spell]")]
[Help(
@"This spell works only out of doors, and only when the weather is bad.
It calls down lightning bolts from Mota.")]
[OneLineHelp("summons a huge bolt from the heavens, if the weather is right")]
public class CallLightning : NoTargetSpellBase
{
    private const string SpellName = "Call Lightning";

    private ITimeManager TimeManager { get; }

    public CallLightning(ILogger<CallLightning> logger, IRandomManager randomManager, ITimeManager timeManager)
        : base(logger, randomManager)
    {
        TimeManager = timeManager;
    }

    public override string? Setup(ISpellActionInput spellActionInput)
    {
        var baseSetup = base.Setup(spellActionInput);
        if (baseSetup != null)
            return baseSetup;

        if (Caster.Room.RoomFlags.IsSet("Indoors"))
            return "You must be out of doors.";
        if (TimeManager.SkyState < SkyStates.Raining)
            return "You need bad weather.";
        return null;
    }

    protected override void Invoke()
    {
        var npcCaster = Caster as INonPlayableCharacter;
        int damage = RandomManager.Dice(Level / 2, 8);
        Caster.Send("Mota's lightning strikes your foes!");
        Caster.Act(ActOptions.ToRoom, "{0:N} calls Mota's lightning to strike {0:s} foes!", Caster);
        var clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.Where(x => x != Caster).ToList()); // clone because damage could kill and remove character from list
        foreach (var victim in clone)
        {
            var npcVictim = victim as INonPlayableCharacter;
            if (npcCaster != null ? npcVictim == null : npcVictim != null) // NPC on PC and PC on NPC
            {
                if (victim.SavesSpell(Level, SchoolTypes.Lightning))
                    victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Lightning, "lightning bolt", true);
                else
                    victim.AbilityDamage(Caster, damage, SchoolTypes.Lightning, "lightning bolt", true);
            }
        }
        // Inform in area about it
        foreach (var character in Caster.Room.Area.Characters.Where(x => x.Position > Positions.Sleeping && !x.Room.RoomFlags.IsSet("Indoors")))
            character.Send("Lightning flashes in the sky.");
    }
}
