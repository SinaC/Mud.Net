using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces;

public interface ILootManager
{
    void GenerateLoots(IItemCorpse? corpse, ICharacter victim, IEnumerable<IPlayableCharacter> playableCharactersImpactedByKill);
}
