using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using System;

namespace Mud.Server.Tests.Mocking
{
    public class AuraManagerMock : IAuraManager
    {
        public IAura AddAura(IEntity target, string abilityName, IEntity source, int level, TimeSpan duration, AuraFlags flags, bool recompute, params IAffect[] affects)
        {
            throw new NotImplementedException();
        }
    }
}
