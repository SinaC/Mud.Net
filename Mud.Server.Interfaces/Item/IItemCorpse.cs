namespace Mud.Server.Interfaces.Item;

public interface IItemCorpse : IItemCanContain
{
    bool IsPlayableCharacterCorpse { get; }
}
