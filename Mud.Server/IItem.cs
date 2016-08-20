using Mud.Server.Blueprints;
using Mud.Server.Blueprints.Item;

namespace Mud.Server
{
    public interface IItem : IEntity
    {
        IContainer ContainedInto { get; }

        ItemBlueprintBase Blueprint { get; }

        int DecayPulseLeft { get; } // 0: means no decay

        int Weight { get; }
        int Cost { get; }

        bool ChangeContainer(IContainer container);

        void DecreaseDecayPulseLeft();
    }
}
