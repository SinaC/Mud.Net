namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItemScroll : IItem
    {
        int SpellLevel { get; }
        string FirstSpell { get; }
        string SecondSpell { get; }
        string ThirdSpell { get; }
        string FourthSpell { get; }
    }
}
