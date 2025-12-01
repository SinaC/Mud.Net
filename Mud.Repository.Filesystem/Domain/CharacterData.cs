using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.Domain;

[XmlInclude(typeof(PlayableCharacterData))]
[XmlInclude(typeof(PetData))]
public abstract class CharacterData
{
    public string Name { get; set; }

    public string Race { get; set; }

    public string Class { get; set; }

    public int Level { get; set; }

    public int Sex { get; set; }

    public int Size { get; set; }

    public int HitPoints { get; set; }

    public int MovePoints { get; set; }

    public PairData<int, int>[] CurrentResources { get; set; }

    public PairData<int, int>[] MaxResources { get; set; }

    public EquippedItemData[] Equipments { get; set; }

    public ItemData[] Inventory { get; set; }

    public AuraData[] Auras { get; set; }

    public string CharacterFlags { get; set; }

    public string Immunities { get; set; }

    public string Resistances { get; set; }

    public string Vulnerabilities { get; set; }
    public string ShieldFlags { get; set; }

    public PairData<int,int>[] Attributes { get; set; }
}
