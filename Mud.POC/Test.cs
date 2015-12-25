using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC
{
    public interface IActor
    {
        bool ProcessCommand(string commandLine);
        void Send(string message);
    }

    // TODO: command processor simply split command+parameters

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
    }

    public interface IPlayer : IActor
    {
        Guid Id { get; }
        string Name { get; }

        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }
    }


    public class Player : IPlayer
    {
        public bool ProcessCommand(string commandLine)
        {
            throw new NotImplementedException();
        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime LastCommandTimestamp { get; private set; }
        public string LastCommand { get; private set; }

        [Command]
        protected virtual bool Tell(string parameters)
        {
            return true;
        }

        [Command]
        protected virtual bool Impersonate(string parameters)
        {
            return true;
        }

        [Command]
        protected virtual bool Test(string parameters)
        {
            return true;
        }
    }

    public interface IEntity : IActor
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        bool Impersonable { get; }
    }

    public interface ICharacter : IEntity
    {
    }

    public class Character : ICharacter
    {
        public bool ProcessCommand(string commandLine)
        {
            throw new NotImplementedException();
        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool Impersonable { get; private set; }

        [Command]
        protected virtual bool Look(string parameters)
        {
            return true;
        }

        [Command]
        protected virtual bool Order(string parameters)
        {
            return true;
        }

        [Command]
        protected virtual bool Kill(string parameters)
        {
            return true;
        }

        [Command]
        protected virtual bool Test(string parameters)
        {
            return true;
        }
    }
}
