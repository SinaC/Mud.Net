namespace Mud.Importer.Rom
{
    public class AreaData
    {
        public int VNum { get; set; } // Virtual number (unique number)
        public string FileName { get; set; } // Filename
        public string Name { get; set; } // Name
        public string Credits { get; set; } // Credits
        public int MinVNum { get; set; } // Mobiles/Objects/Rooms virtual number range
        public int MaxVNum { get; set; }
        public string Builders { get; set; } // Builders
        public long Flags { get; set; } // Flags
        public int Security { get; set; } // Security
    }
}
