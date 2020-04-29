namespace Mud.Domain
{
    public class AuraData
    {
        public int AbilitiId { get; set; }

        // TODO: source

        public AuraModifiers Modifier { get; set; }

        public int Amount { get; set; }

        public AmountOperators AmountOperator { get; set; }

        public int Level { get; set; }

        public int PulseLeft { get; set; }
    }
}
