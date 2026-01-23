namespace Mud.Domain;

[Flags]
public enum ImmortalModeFlags
{
    None = 0x0000,
    NoDeath = 0x0001, // cannot die
    Holylight = 0x0002, // can see anything
    Infinite = 0x0004, // infinite resource/no max weight/never full/no CD/all resources
    PassThru = 0x0008, // can go anywhere, no key needed
    AlwaysSafe = 0x0010, // will always be safe
    Omniscient = 0x0020, // known every skills/spells/passives and never miss using/casting
    UberMode = NoDeath | Holylight | Infinite | PassThru | AlwaysSafe | Omniscient,
}
