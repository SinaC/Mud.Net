namespace Mud.Server.Options
{
    public class AuraOptions
    {
        public const string SectionName = "Aura.Settings";

        public required bool RemovePeriodicAurasIfNotInSameRoom { get; init; }
    }
}
