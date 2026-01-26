namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterAggroModifierAffect : IAffect
{
    int MultiplierInPercent { get; }
}
