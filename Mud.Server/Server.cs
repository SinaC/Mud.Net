using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mud.Logger;
using Mud.Network;

namespace Mud.Server
{
    public class ServerSleepUntilDelayElapsed
    {
        private class PlayerClient
        {
            public IClient Client { get; set; }
            public IPlayer Player { get; set; }
        }

        private readonly List<PlayerClient> _playerClients;

        private readonly INetworkServer _networkServer;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _gameLoopTask;

        public ServerSleepUntilDelayElapsed()
        {
            _playerClients = new List<PlayerClient>();
        }

        public ServerSleepUntilDelayElapsed(INetworkServer networkServer)
            :this()
        {
            _networkServer = networkServer;
            _networkServer.NewClientConnected += NetworkServerOnNewClientConnected;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _gameLoopTask = Task.Factory.StartNew(GameLoopTask, _cancellationTokenSource.Token);
            
        }

        public void Stop()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                _gameLoopTask.Wait(2000, _cancellationTokenSource.Token);
            }
            catch (AggregateException ex)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Aggregate exception while stopping. Exception: {0}", ex.Flatten());
            }
        }

        // TODO: remove
        // TEST PURPOSE
        public void AddClient(IClient client, string name)
        {
            IPlayer player = new Player.Player(client, Guid.NewGuid(), name);
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = player
            };
            _playerClients.Add(playerClient);

            player.Send("Let's go");
        }

        #region INetworkServer events handler
        
        private void NetworkServerOnNewClientConnected(IClient client)
        {
            IPlayer player = new Player.Player(client, Guid.NewGuid());
            PlayerClient playerClient = new PlayerClient
            {
                Client = client,
                Player = player
            };
            _playerClients.Add(playerClient);

            player.Send("Why don't you login or tell us the name you wish to be known by?");
        }

        #endregion

        private void ProcessInput()
        {
            // Read one command from each client and process it
            foreach (PlayerClient client in _playerClients) // TODO: first connected player will be processed before other, try a randomize
            {
                string data = client.Client.ReadData();
                if (!String.IsNullOrWhiteSpace(data))
                    client.Player.ProcessCommand(data);
            }
        }

        private void DoPulse()
        {
            // TODO:  (see handler.c)
            //Log.Default.WriteLine(LogLevels.Debug, "PULSE: {0:HH:mm:ss.ffffff}", DateTime.Now);
        }

        private void ProcessOutput()
        {
            // NOP: Actually, outputs are sent directly to client  TODO ???
        }

        private void GameLoopTask()
        {
            // TODO:  (see comm.C:881)
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop GameLoopTask requested");
                        break;
                    }

                    ProcessInput();

                    DoPulse();

                    ProcessOutput();

                    sw.Stop();
                    long elapsedMs = sw.ElapsedMilliseconds; // in milliseconds
                    long elapsedTick = sw.ElapsedTicks; // 1 tick = 1 second/Stopwatch.Frequency
                    long elapsedNs = sw.Elapsed.Ticks; // 1 tick = 1 nanosecond
                    if (elapsedMs < 250)
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "Elapsed {0}Ms {1}Ticks {2}Ns", elapsedMs, elapsedTick, elapsedNs);
                        Thread.Sleep(250 - (int) elapsedMs);
                    }
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Error, "!!! No sleep for GameLoopTask. Elapsed {0}", elapsedMs);
                        Thread.Sleep(1);
                    }
                    sw.Restart();
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameLoopTask exception. Exception: {0}", ex);
            }

            Log.Default.WriteLine(LogLevels.Info, "GameLoopTask stopped");
        }
    }
}
