using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings.Interfaces;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Creation)]
public class CreateSpring : ItemCreationSpellBase
{
    private const string SpellName = "Create Spring";

    public CreateSpring(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings)
        : base(randomManager, wiznet, itemManager, settings)
    {
    }

    protected override void Invoke()
    {
        var fountain = ItemManager.AddItem(Guid.NewGuid(), Settings.SpringBlueprintId, Caster.Room) as IItemFountain;
        if (fountain == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Wiznet($"SpellCreateFood: cannot create item from blueprint {Settings.SpringBlueprintId}.", WiznetFlags.Bugs, AdminLevels.Implementor);
            return;
        }
        int duration = Level;
        fountain.SetTimer(TimeSpan.FromMinutes(duration));
        Caster.Act(ActOptions.ToAll, "{0} flows from the ground.", fountain);
    }
}
