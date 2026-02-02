using Mud.Common;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.PlayableCharacter.Information;

[PlayableCharacterCommand("itemaffects", "Information")]
[Alias("itemauras")]
[Alias("iaffects")]
[Alias("iauras")]
[Help(
@"This command is used to show all the skills and spells affecting your
character from worn items. At low levels, only the spell name will be
displayed, at higher levels the effects and duration of the spell will
also be shown.")]
public class ItemAffects : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        // TODO: various information depending on level
        StringBuilder sb = new();
        var found = false;
        foreach (var equipedItem in Actor.Equipments.Where(x => x.Item != null).Select(x => x.Item!))
        {
            // if equiped item has at least one aura with at least one character affect, display it
            if (equipedItem.Auras.Any(x => x.Affects.OfType<ICharacterAffect>().Any()))
            {
                sb.AppendFormatLine("%c%You are affected by following auras from {0}:%x%", equipedItem.DisplayName);
                // Auras
                foreach (var aura in equipedItem.Auras.Where(x => Actor.ImmortalMode.IsSet("Holylight") || !x.AuraFlags.IsSet("Hidden")).OrderBy(x => x.AuraFlags.IsSet("Permanent") ? int.MaxValue : x.PulseLeft))
                    aura.Append<ICharacterAffect>(sb);
                found = true;
            }
        }
        if (!found)
            sb.AppendLine("%c%You are not affected by any spells from items.%x%");

        Actor.Page(sb);
    }
}
