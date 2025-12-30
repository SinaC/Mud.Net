namespace Mud.Domain;

public enum CostAmountOperators
{
    Fixed               = 1, // fixed amount
    PercentageCurrent   = 2, // percentage of current resource
    PercentageMax       = 3, // percentage of max resource
    All                 = 4, // use every point resource
    AllWithMin          = 5, // use every point resource and must have at least min
}
