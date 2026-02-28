using Mud.Blueprints.Area;
using Mud.Blueprints.Character;
using Mud.Blueprints.Item;
using Mud.Blueprints.MobProgram;
using Mud.Blueprints.Room;

namespace Mud.Importer;

public interface IImporter
{
    public IReadOnlyCollection<AreaBlueprint> Areas { get; }
    public IReadOnlyCollection<RoomBlueprint> Rooms { get; }
    public IReadOnlyCollection<ItemBlueprintBase> Items { get; }
    public IReadOnlyCollection<CharacterBlueprintBase> Characters { get; }
    public IReadOnlyCollection<MobProgramBlueprint> MobPrograms { get; }

    void ImportByList(string path, string areaLst);
    void Import(string path, params string[] filenames);
    void Import(string path, IEnumerable<string> filenames);
}
