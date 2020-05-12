namespace Mud.Server.Item
{
    public interface IItemCastSpellsNoRecharge : IItem
    {
        int SpellLevel { get; }
        IAbility FirstSpell { get; }
        IAbility SecondSpell { get; }
        IAbility ThirdSpell { get; }
        IAbility FourthSpell { get; }
    }
}
