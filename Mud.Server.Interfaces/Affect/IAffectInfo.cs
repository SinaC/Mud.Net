using System;

namespace Mud.Server.Interfaces.Affect
{
    public interface IAffectInfo
    {
        string Name { get; }
        Type AffectDataType { get; }

        Type AffectType { get; }
    }
}
