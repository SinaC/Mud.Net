namespace Mud.Repository.Filesystem.Domain;

public class LearnedAbilityData
{
    public string Name { get; set; }

    public int ResourceKind { get; set; }

    public int CostAmount { get; set; }

    public int CostAmountOperator { get; set; }

    public int Level { get; set; } // level at which ability can be learned

    public int Learned { get; set; } // practice percentage, 0 means not learned, 100 mean fully learned

    public int Rating { get; set; } // how difficult is it to improve/gain/practice
}
