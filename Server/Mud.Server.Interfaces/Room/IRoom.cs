using Mud.Blueprints;
using Mud.Blueprints.Character;
using Mud.Blueprints.Room;
using Mud.Domain;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Room;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Interfaces.Room;

public interface IRoom : IEntity, IContainer
{
    void Initialize(Guid guid, RoomBlueprint blueprint, IArea area);

    RoomBlueprint Blueprint { get; }

    IEnumerable<ExtraDescription> ExtraDescriptions { get; }

    IRoomFlags BaseRoomFlags { get; }
    IRoomFlags RoomFlags { get; }

    IArea Area { get; }

    IEnumerable<ICharacter> People { get; }
    IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }
    IEnumerable<IPlayableCharacter> PlayableCharacters { get; }
    IEnumerable<(INonPlayableCharacter character, TBlueprint blueprint)> GetNonPlayableCharacters<TBlueprint>()
        where TBlueprint : CharacterBlueprintBase;

    Sizes? MaxSize { get; }
    int BaseHealRate { get; }
    int HealRate { get; }
    int BaseResourceRate { get; }
    int ResourceRate { get; }
    int Light { get; }
    SectorTypes SectorType { get; }
    bool IsPrivate { get; }
    bool IsDark { get; }

    IExit[] Exits { get; } // fixed length

    IExit? this[ExitDirections direction] { get; }
    IRoom? GetRoom(ExitDirections direction);

    bool Enter(ICharacter character);
    bool Leave(ICharacter character);

    void IncreaseLight();
    void DecreaseLight();

    StringBuilder Append(StringBuilder sb, ICharacter viewer);
    StringBuilder AppendExits(StringBuilder sb, ICharacter viewer, bool compact);

    (IExit? exit, ExitDirections exitDirection) VerboseFindDoor(ICharacter character, ICommandParameter parameter);

    // Affects
    void ApplyAffect(IRoomFlagsAffect affect);
    void ApplyAffect(IRoomHealRateAffect affect);
    void ApplyAffect(IRoomResourceRateAffect affect);
}
