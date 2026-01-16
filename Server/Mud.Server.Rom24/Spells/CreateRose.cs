using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[Syntax("cast [spell]")]
[Help(
@"A romantic spell that creates a fragrant red rose, with utterly no game
use whatsoever.")]
[OneLineHelp("creates a beautiful red rose")]
public class CreateRose : ItemCreationSpellBase
{
    private const string SpellName = "Create Rose";

    private int RoseBlueprintId { get; }

    public CreateRose(ILogger<CreateRose> logger, IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, IOptions<Rom24Options> options) 
        : base(logger, randomManager, wiznet, itemManager)
    {
        RoseBlueprintId = options.Value.SpellBlueprintIds.Rose;
    }

    protected override void Invoke()
    {
        var rose = ItemManager.AddItem<IItemTrash>(Guid.NewGuid(), RoseBlueprintId, $"SpellCreateRose[{Caster.DebugName}]", Caster);
        if (rose == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Log($"SpellCreateRose: cannot create item from blueprint {RoseBlueprintId}.", WiznetFlags.Bugs, AdminLevels.Implementor);
            return;
        }
        int duration = Level;
        rose.SetTimer(TimeSpan.FromMinutes(duration));
        Caster.Act(ActOptions.ToRoom, "{0:N} has created a beautiful %R%red rose%x%.", Caster);
        Caster.Send("You create a beautiful %R%red rose%x%.");
    }
}
