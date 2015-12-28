using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints;
using Mud.Server.Input;

namespace Mud.Server.Object
{
    public class Object : EntityBase, IObject
    {
        private static readonly Trie<MethodInfo> ObjectCommands;

        static Object()
        {
            ObjectCommands = CommandHelpers.GetCommands(typeof (Object));
        }

        public Object(Guid guid, string name) : base(guid, name)
        {
        }

        #region IActor

        public override IReadOnlyTrie<MethodInfo> Commands
        {
            get { return ObjectCommands; }
        }

        #endregion

        public ObjectBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor
    }
}
