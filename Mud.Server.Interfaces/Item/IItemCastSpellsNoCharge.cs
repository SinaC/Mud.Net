namespace Mud.Server.Interfaces.Item
{
    public interface IItemCastSpellsNoCharge : IItem
    {
        int SpellLevel { get; }
        string FirstSpellName { get; }
        string SecondSpellName { get; }
        string ThirdSpellName { get; }
        string FourthSpellName { get; }
    }
}
