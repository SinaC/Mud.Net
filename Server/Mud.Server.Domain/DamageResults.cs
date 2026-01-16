namespace Mud.Server.Domain;

public enum DamageResults
{
    AlreadyDead, // target was already dead
    NotInSameRoom, // target was not in the same room
    Safe, // target is safe
    NoDamage, // damage has been reduced to 0
    Killed, // target has been killed by damage
    Done, // normal damage
}
