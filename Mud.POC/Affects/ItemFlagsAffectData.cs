namespace Mud.POC.Affects
{
    public class ItemFlagsAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical

        public ItemFlags Modifier { get; set; }
    }
}
