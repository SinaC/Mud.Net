using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Abilities2.Helpers
{
    public static class FindHelpers
    {
        public static TEntity FindByName<TEntity>(IEnumerable<TEntity> entities, CommandParameter parameter)
            where TEntity : IEntity
             => entities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value));
    }
}
