using System;
using Mud.DataStructures.Trie;
using Mud.Server.Blueprints;
using Mud.Server.Input;

namespace Mud.Server.Object
{
    public class Object : EntityBase, IObject
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> ObjectCommands;

        static Object()
        {
            ObjectCommands = CommandHelpers.GetCommands(typeof (Object));
        }

        public Object(Guid guid, string name, IContainer container)
            : base(guid, name)
        {
            ContainedInto = container;
            container.Put(this);
        }

        #region IObject

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return ObjectCommands; }
        }

        #endregion

        public IContainer ContainedInto { get; private set; }

        public ObjectBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public bool ChangeContainer(IContainer container)
        {
            // TODO: check if can be put in a container
            ContainedInto = container;
            return true;
        }

        #endregion
    }
}
