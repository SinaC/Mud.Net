using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Item;

public interface IItemCorpse : IItemCanContain
{
    void Initialize(Guid guid, ItemCorpseBlueprint blueprint, IContainer container);
    void Initialize(Guid guid, ItemCorpseBlueprint blueprint, ItemCorpseData itemCorpseData, IContainer container);
    void Initialize(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim);
    void Initialize(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim, ICharacter killer);

    bool IsPlayableCharacterCorpse { get; }
}
