using Mud.Domain;
using Mud.Server.Flags.Interfaces;

namespace Mud.POC.Tests.Serialization;

public abstract class CharacterData
{
    public string Name { get; set; }

    public Sex Sex { get; set; }

    public Dictionary<ResourceKinds, int> CurrentResources { get; set; }

    public ICharacterFlags CharacterFlags { get; set; }

    public AffectDataBase[] AffectDatas { get; set; }
}
