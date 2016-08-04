using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.NewMud.Behaviors
{
    public abstract class Behavior : IDisposable
    {
        public long Id { get; set; }

        public Thing Parent { get; set; }

        public virtual void OnAddBehavior()
        {
        }

        public virtual void OnRemoveBehavior()
        {
        }

        #region IDisposable

        public virtual void Dispose()
        {
            Parent = null;
        }

        #endregion
    }
}
