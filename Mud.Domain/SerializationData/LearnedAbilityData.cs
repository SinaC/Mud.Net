namespace Mud.Domain.SerializationData;

public class LearnedAbilityData
{
    public required string Name { get; set; }

    public required int Level { get; set; } // level at which ability can be learned

    public required int Learned { get; set; } // practice percentage, 0 means not learned, 100 mean fully learned

    public required int Rating { get; set; } // how difficult is it to improve/gain/practice
}
