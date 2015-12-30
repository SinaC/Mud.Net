using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mud.Logger;

namespace Mud.Server
{
    public class ServerSleepUntilDelayElapsed
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _gameLoopTask;

        private void GameLoop()
        {
            // TODO:  (see comm.C:881)
            // process pending input from client
            // World.Update
            // process pending output to client
        }

        public ServerSleepUntilDelayElapsed()
        {
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

        private void DoPulse()
        {
            Log.Default.WriteLine(LogLevels.Debug, "PULSE: {0:HH:mm:ss.ffffff}", DateTime.Now);
        }

        private void GameLoopTask()
        {
            try
            {
                System.Diagnostics.Stopwatch sw = new Stopwatch();
                sw.Start();
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "Stop GameLoopTask requested");
                        break;
                    }

                    DoPulse();

                    sw.Stop();
                    long elapsedTicks = sw.ElapsedMilliseconds;
                    sw.Restart();
                    if (elapsedTicks < 250)
                        Thread.Sleep(250 - (int) elapsedTicks);
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Error, "!!! No sleep for GameLoopTask. Elapsed {0}", elapsedTicks);
                        Thread.Sleep(1);
                    }
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
