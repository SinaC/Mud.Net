﻿using Mud.Network.Interfaces;
using Mud.Server.Interfaces;
using System;
using System.Collections.Generic;

namespace Mud.Server.Tests.Mocking
{
    // TODO

    internal class ServerMock : IServer
    {
        public DateTime CurrentTime { get; private set; }

        public void Initialize(List<INetworkServer> networkServers)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Dump()
        {
            throw new NotImplementedException();
        }
    }
}
