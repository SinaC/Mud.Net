namespace Mud.Importer.Mystery;

public class AreaData
{
    public int VNum { get; set; } // Virtual number (unique number)
    public string FileName { get; set; } = default!; // Filename
    public string Name { get; set; } = default!; // Name
    public string Credits { get; set; } = default!; // Credits
    public int MinVNum { get; set; } // Mobiles/Objects/Rooms virtual number range
    public int MaxVNum { get; set; }
    public string Builders { get; set; } = default!; // Builders
    public long Flags { get; set; } // Flags
    public int Security { get; set; } // Security
}
