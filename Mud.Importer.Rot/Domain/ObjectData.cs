namespace Mud.Importer.Rot.Domain;

internal class ObjectData
{
    public int VNum { get; set; }
    public string Name { get; set; } = default!;
    public string ShortDescr { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Material { get; set; } = default!;
    public string ItemType { get; set; } = default!;
    public long ExtraFlags { get; set; }
    public long WearFlags { get; set; }
    public object[] Values { get; set; } = new object[5];
    public int Level { get; set; }
    public int Weight { get; set; }
    public long Cost { get; set; }
    public char Condition { get; set; }
    public List<ObjectAffect> Affects { get; set; } = [];
    public Dictionary<string, string> ExtraDescr { get; set; } = [];
    public string Clan { get; set; } = default!;
    public string Guild { get; set; } = default!;
}

internal class ObjectAffect
{
    public const int WhereToObject = 1;
    public const int WhereToAffects = 2;
    public const int WhereToImmune = 3;
    public const int WhereToResist = 4;
    public const int WhereToVuln = 5;
    public const int WhereToShields = 6;

    public int Where { get; set; }
    public int Level { get; set; }
    public int Location { get; set; }
    public int Modifier { get; set; }
    public long BitVector { get; set; }
}
