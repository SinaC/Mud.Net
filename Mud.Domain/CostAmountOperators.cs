namespace Mud.Domain;

public enum CostAmountOperators
{
    None        = 0,
    Fixed       = 1,
    Percentage  = 2,
    All         = 3, // use every point resource
    AllWithMin  = 4, // use every point resource and must have at least min
}
