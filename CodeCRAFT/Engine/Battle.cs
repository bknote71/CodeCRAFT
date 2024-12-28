using Google.Protobuf;
using Server;
using Timer = System.Timers.Timer;


namespace Server.Engine
{
    public class Battle
    {
        Timer _updateTimer;
        long _currentTime;

        Dictionary<int, RobotPeer> _robots = new Dictionary<int, RobotPeer>();
        List<RobotPeer> _deadRobots = new List<RobotPeer>();
        List<BulletPeer> _bullets = new List<BulletPeer>();

        public Battle()
        {
            int interval = 1000 / 60;
            _updateTimer = new Timer(interval);
            _updateTimer.Elapsed += (sender, e) => Update();
            _updateTimer.AutoReset = true;
            _updateTimer.Start();
        }

        // Public

        public void EnterBattle(RobotPeer robotPeer)
        {
            var client = robotPeer.Client;
            if (client == null)
                return;

            _robots.Add(robotPeer.Id, robotPeer);
            robotPeer.Battle = this;
            robotPeer.StartBattle();

            // Enter Packet
            var message = new SEnterBattle
            {
                RobotId = robotPeer.Id,
                RobotName = robotPeer.Name,
                SpecIndex = robotPeer.SpecIndex,
                Username = client.Username
            };

            client.send(message);
        }

        public void LeaveBattle(RobotPeer robotPeer)
        {
            Client client = null;
            if (robotPeer == null || (client = robotPeer.Client) == null)
                return;

            _robots.Remove(robotPeer.Id);
            robotPeer.Clean();

            // TODO: Leave Packet
        }

        public void ChangeRobot(RobotPeer robotPeer, RobotSpec spec)
        {
            if (robotPeer == null)
                return;

            robotPeer.Reset(this, spec);
            robotPeer.StartBattle();
        }

        public void HandleChat(RobotPeer robotPeer, string chat)
        {
            if (robotPeer == null)
                return;

            var message = new SChat
            {
                // TODO
            };
            Broadcast(message);
        }

        public void RegisterBullet(BulletPeer bulletPeer)
        {
            _bullets.Add(bulletPeer);
        }

        public void RegisterAsDeadRobot(RobotPeer robotPeer)
        {
            _deadRobots.Add(robotPeer);
        }

        public void Dispose()
        {
            _updateTimer.Dispose();
        }

        // Private

        void Update()
        {
            UpdateInfo updateInfo = new UpdateInfo { T = ++_currentTime };

            FlushJob();

            LoadCommandsFromRobots(ref updateInfo);
            UpdateBullets(ref updateInfo);
            UpdateRobots(ref updateInfo);

            PublishStatuses();
            WakeupRobots();

            Broadcast(new SUpdate { Update = updateInfo });
        }

        void Broadcast(IMessage msg)
        {
            foreach (var robot in _robots.Values)
            {
                robot.Client.send(msg);
            }
        }

        void LoadCommandsFromRobots(ref UpdateInfo updateInfo)
        {
            foreach (var robot in _robots.Values)
            {
                robot.PerformLoadCommands();
            }
        }

        void UpdateBullets(ref UpdateInfo updateInfo)
        {
            foreach (var bullet in GetBulletsAtRandom())
            {
                bullet.Update();
                if (bullet.State == BulletState.INACTIVE)
                    _bullets.Remove(bullet);
                else
                {
                    var bulletInfo = new UpdateInfo.Types.BulletInfo
                    {
                        Id = bullet.Id,
                        X = bullet.X,
                        Y = bullet.Y
                    };
                    updateInfo.Bullets.Add(bulletInfo);
                }
            }
        }

        void UpdateRobots(ref UpdateInfo updateInfo)
        {
            foreach (var robotPeer in GetRobotsAtRandom())
            {
                robotPeer.PerformMove(GetRobotsAtRandom());
                var message = new UpdateInfo.Types.RobotInfo
                {
                    Id = robotPeer.Id,
                    Name = robotPeer.Name,
                    Username = robotPeer.Username,
                    X = robotPeer.X,
                    Y = robotPeer.Y,
                    BodyHeading = robotPeer.BodyHeading,
                    GunHeading = robotPeer.GunHeading,
                    RadarHeading = robotPeer.RadarHeading,
                    Hp = robotPeer.Hp,
                    Dead = robotPeer.IsDead
                };
                updateInfo.Robots.Add(message);
            }

            foreach (var robotPeer in _robots.Values)
            {
                robotPeer.UpdateAfterCollision();
            }

            foreach (var robotPeer in GetRobotsAtRandom())
            {
                robotPeer.PerformScan(GetRobotsAtRandom());
                Arc2D arc = robotPeer.ScanArc;
                var message = new UpdateInfo.Types.ScanInfo
                {
                    Id = robotPeer.Id,
                    Name = robotPeer.Name,
                    RobotX = robotPeer.X,
                    RobotY = robotPeer.Y,
                    AngleStart = arc.AngleStart,
                    AngleExtent = arc.AngleExtent,
                    X = arc.X,
                    Y = arc.Y,
                    Width = arc.Width,
                    Height = arc.Height,
                };
                updateInfo.Scans.Add(message);
            }
        }

        void PublishStatuses()
        {
            foreach (var robotPeer in _robots.Values)
            {
                robotPeer.PublishStatus(_currentTime);
            }
        }

        void WakeupRobots()
        {
            foreach (var robotPeer in _robots.Values)
            {
                if (!robotPeer.IsRunning)
                    continue;

                robotPeer.WaitWakeup();
            }
        }

        void FlushJob()
        {
            // TODO
        }

        // Helper
        List<RobotPeer> GetRobotsAtRandom()
        {
            List<RobotPeer> robots = _robots.Values.ToList();
            robots.Shuffle();
            return robots;
        }
    
        List<BulletPeer> GetBulletsAtRandom()
        {
            List<BulletPeer> bullets = _bullets.ToList();
            bullets.Shuffle();
            return bullets;
        }

        List<RobotPeer> GetDeathRobotsAtRandom()
        {
            // TODO
            return _robots.Values.ToList();
        }
    }
}
