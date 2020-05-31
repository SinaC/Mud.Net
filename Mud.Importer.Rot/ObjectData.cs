using System.Collections.Generic;

namespace Mud.Importer.Rot
{
    public class ObjectData
    {
        public int VNum { get; set; }
        public string Name { get; set; }
        public string ShortDescr { get; set; }
        public string Description { get; set; }
        public string Material { get; set; }
        public string ItemType { get; set; }
        public long ExtraFlags { get; set; }
        public long WearFlags { get; set; }
        public object[] Values { get; set; }
        public int Level { get; set; }
        public int Weight { get; set; }
        public long Cost { get; set; }
        public char Condition { get; set; }
        public List<ObjectAffect> Affects { get; set; }
        public Dictionary<string,string> ExtraDescr { get; set; }
        public string Clan { get; set; }
        public string Guild { get; set; }

        public ObjectData()
        {
            Values = new object[5];
            Affects = new List<ObjectAffect>();
            ExtraDescr = new Dictionary<string, string>();
        }
    }

    public class ObjectAffect
    {
        public const int WhereToObject = 1;
        public const int WhereToAffects = 2;
        public const int WhereToImmune = 3;
        public const int WhereToResist = 4;
        public const int WhereToVuln = 5;
        public const int WhereToShields = 6;

        public int Where { get; set; }
        public int Type { get; set; }
        public int Level { get; set; }
        public int Duration { get; set; }
        public int Location { get; set; }
        public int Modifier { get; set; }
        public long BitVector { get; set; }
    }
}
