using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CreateRose : ItemCreationSpellBase
    {
        public override int Id => 20;
        public override string Name => "Create Rose";

        public CreateRose(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings) 
            : base(randomManager, wiznet, itemManager, settings)
        {
        }

        public override void Action(ICharacter caster, int level)
        {
            caster.Send("Not Yet Implemented");
            //TODO: add rose blueprint
        }
    }
}
