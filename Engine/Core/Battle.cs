using Timer = System.Timers.Timer;


namespace Engine.Core
{
    public class Battle
    {
        Timer _updateTimer;

        public Battle()
        {
            int interval = 1000 / 60;
            _updateTimer = new Timer(interval);
            _updateTimer.Elapsed += (sender, e) => update();
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
            

        }
        void update()
        {
            
            FlushJob();
            loadCommandsFromRobots(ref updateMsg);
        }

        void FlushJob()
        {
            // TODO
        }



        void Dispose()
        {
            _updateTimer.Dispose();
        }
    }

    
}
