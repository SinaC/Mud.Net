namespace Mud.Server.Interfaces
{
    public interface ISanityCheck
    {
        bool PerformSanityChecks(); // return true if a fatal sanity check failed
    }
}
