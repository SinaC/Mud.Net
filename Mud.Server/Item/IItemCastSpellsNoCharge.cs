namespace Mud.Server.Item
{
    public interface IItemCastSpellsNoCharge : IItem
    {
        int SpellLevel { get; }
        IAbility FirstSpell { get; }
        IAbility SecondSpell { get; }
        IAbility ThirdSpell { get; }
        IAbility FourthSpell { get; }
    }
}
