using Mud.Domain;
using Mud.Flags.Interfaces;

namespace Mud.POC.Tests.Serialization;

public abstract class CharacterData
{
    public string Name { get; set; } = null!;

    public Sex Sex { get; set; }

    public Dictionary<ResourceKinds, int> CurrentResources { get; set; } = null!;

    public string CharacterFlags { get; set; } = null!;

    public AffectDataBase[] AffectDatas { get; set; } = null!;
}
