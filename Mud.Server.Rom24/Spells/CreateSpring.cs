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
@"This spell brings forth a magical spring from the ground, which has the
same properties as a fountain.")]
public class CreateSpring : ItemCreationSpellBase
{
    private const string SpellName = "Create Spring";

    private int SpringBlueprintId { get; }

    public CreateSpring(ILogger<CreateSpring> logger, IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, IOptions<Rom24Options> options)
        : base(logger, randomManager, wiznet, itemManager)
    {
        SpringBlueprintId = options.Value.BlueprintIds.Spring;
    }

    protected override void Invoke()
    {
        var fountain = ItemManager.AddItem(Guid.NewGuid(), SpringBlueprintId, Caster.Room) as IItemFountain;
        if (fountain == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Log($"SpellCreateFood: cannot create item from blueprint {SpringBlueprintId}.", WiznetFlags.Bugs, AdminLevels.Implementor);
            return;
        }
        int duration = Level;
        fountain.SetTimer(TimeSpan.FromMinutes(duration));
        Caster.Act(ActOptions.ToAll, "{0} flows from the ground.", fountain);
    }
}
