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
public class CreateFood : ItemCreationSpellBase
{
    private const string SpellName = "Create Food";

    public CreateFood(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings)
        : base(randomManager, wiznet, itemManager, settings)
    {
    }

    protected override void Invoke()
    {
        var mushroom = ItemManager.AddItem(Guid.NewGuid(), Settings.MushroomBlueprintId, Caster.Room) as IItemFood;
        if (mushroom == null)
        {
            Caster.Send("The spell fizzles and dies.");
            Wiznet.Log($"SpellCreateFood: cannot create item from blueprint {Settings.MushroomBlueprintId}.", WiznetFlags.Bugs, AdminLevels.Implementor);
            return;
        }
        mushroom.SetHours(Level / 2, Level);
        Caster.Act(ActOptions.ToAll, "{0} suddenly appears.", mushroom);
    }
}
