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
@"This spell brings forth a magical spring from the ground, which has the
same properties as a fountain.")]
[OneLineHelp("calls forth a small but pure spring from the ground")]
public class CreateSpring : ItemCreationSpellBase
{
    private const string SpellName = "Create Spring";

    private int SpringBlueprintId { get; }

    public CreateSpring(ILogger<CreateSpring> logger, IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, IOptions<Rom24Options> options)
        : base(logger, randomManager, wiznet, itemManager)
    {
        SpringBlueprintId = options.Value.SpellBlueprintIds.Spring;
    }

    protected override void Invoke()
    {
        var fountain = ItemManager.AddItem<IItemFountain>(Guid.NewGuid(), SpringBlueprintId, Caster.Room);
        if (fountain == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Log($"SpellCreateSpring: cannot create item from blueprint {SpringBlueprintId}.", WiznetFlags.Bugs, AdminLevels.Implementor);
            return;
        }
        int duration = Level;
        fountain.SetTimer(TimeSpan.FromMinutes(duration));
        Caster.Act(ActOptions.ToAll, "{0} flows from the ground.", fountain);
    }
}
