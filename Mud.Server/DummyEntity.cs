using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server
{
    public class DummyEntity : IEntity
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public IClient ImpersonatedBy { get; set; }

        public bool Impersonable
        {
            get { return true; }
        }
    }
}
