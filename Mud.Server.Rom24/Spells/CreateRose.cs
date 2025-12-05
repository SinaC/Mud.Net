using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation)]
[Syntax("cast [spell]")]
[Help(
@"A romantic spell that creates a fragrant red rose, with utterly no game
use whatsoever.")]
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
        var rose = ItemManager.AddItem<IItemTrash>(Guid.NewGuid(), RoseBlueprintId, Caster);
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
