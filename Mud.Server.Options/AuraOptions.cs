namespace Mud.Server.Options
{
    public class AuraOptions
    {
        public const string SectionName = "Aura.Settings";

        public required bool RemovePeriodicAurasInNotInSameRoom { get; init; }
    }
}
