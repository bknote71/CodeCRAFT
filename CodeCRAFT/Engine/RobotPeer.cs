using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Engine
{
    public class RobotPeer
    {
        static int WIDTH = 10;
        static int HEIGHT = 10;

        int _id;
        Client _client;
        Battle _battle;

        RobotProxy _robotProxy;
        RobotSpec _spec;
        RobotState _state;

        public int Id => _id;
        public Client Client => _client;
        public Battle Battle
        {
            get => _battle;
            set => _battle = value;
        }

        public string Name => _spec.Name;
        public string Username => _client.Username;
        public int SpecIndex => _client.CurrentSpecIndex;

        // Robot Status (Value)

        public double Hp { get; internal set; }
        public double Energy { get; internal set; }
        public double Velocity { get; internal set; }

        public double X { get; internal set; }
        public double Y { get; internal set; }
        public double BodyHeading { get; internal set; }
        public double GunHeading { get; internal set; }
        public double RadarHeading { get; internal set; }
        public double LastBodyHeading { get; internal set; }
        public double LastGunHeading { get; internal set; }
        public double LastRadarHeading { get; internal set; }
        public double GunHeat {  get; internal set; }

        bool _scan;
        bool _turnedRadarWithGun;
        Arc2D _scanArc;

        public Arc2D ScanArc => _scanArc;
        public BoundingRectangle BoundingBox { get; internal set; }
        
        // TODO: Atomic
        public bool _isRunning = false;
        public bool _isSleeping = false;
        public bool _isHalting = false;
        bool _isExecFinishedAndDisabled = false;

        object _sleepingLock = new object();

        public bool IsRunning
        {
            get => _isRunning;
            set => _isRunning = value;
        }

        public bool IsSleeping  
        {
            get => _isSleeping;
            set => _isSleeping = value;
        }

        public bool IsHalting
        {
            get => _isHalting;
            set => _isHalting = value;
        }

        public bool IsDead => _state == RobotState.DEAD;

        public RobotPeer(Client client, int id)
        {
            this._id = id;
            this._client = client;
            this.BoundingBox = new BoundingRectangle();
            this._scanArc = new Arc2D();
            this._state = RobotState.ACTIVE;
        }

        public RobotPeer(Client client, int id, Battle battle, RobotSpec spec)
            : this(client, id)
        {
            Init(battle, spec);
        }

        public void Init(Battle battle, RobotSpec spec)
        {
            this._battle = battle;
            this._spec = spec;
            this._robotProxy = new RobotProxy(spec);
        }

        public ExecResults Execute(ExecCommands commands)
        {
            // TODO: Validate Commands
            if (!_isExecFinishedAndDisabled)
                commands.Set(new ExecCommands(commands, true));
            else
            {
                try
                {
                    Thread.Sleep(100);

                } catch (ThreadInterruptedException e)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }

            if (IsDead)
                _isExecFinishedAndDisabled = true;

            WaitForUpdate();

            return new ExecResults
            {
                // TODO: 실행 결과
            };
        }

        public void StartBattle()
        {
            if (_battle == null)
                return;

            InitializeRobotPeer();
        }


        public void PerformLoadCommands()
        {
            throw new NotImplementedException();
        }

        public void PerformMove(List<RobotPeer> robotPeers)
        {
            throw new NotImplementedException();
        }

        public void PerformScan(List<RobotPeer> robotPeers)
        {
            throw new NotImplementedException();
        }

        public void UpdateAfterCollision()
        {
            throw new NotImplementedException();
        }

        public void PublishStatus(long currentTime)
        {
            throw new NotImplementedException();
        }

        public void WaitWakeup()
        {
            lock (_sleepingLock)
            {
                if (!_isSleeping)
                    return;
                
                Monitor.PulseAll(_sleepingLock); // 작업을 마쳤어!
                Monitor.Wait(_sleepingLock); // _isSleeping 상태를 적절히 업데이트했으면 깨워줘
            }
        }

        void WaitForUpdate()
        {
            lock (_sleepingLock)
            {
                _isSleeping = true;
                Monitor.PulseAll(_sleepingLock); // _isSleeping 값이 변경되었음을 (_isSleeping의 특정 상태를 대기 중인) 조건변수들에게 알리기 위함

                try
                {
                    Monitor.Wait(_sleepingLock); // 작업을 마치고 나서 깨워줘~
                }
                catch (ThreadInterruptedException e)
                {
                    Thread.CurrentThread.Interrupt();
                }

                _isSleeping = false;
                Monitor.PulseAll(_sleepingLock); // _isSleeping 상태를 적절히 업데이트했어~
            }
        }

        public void Reset(Battle battle, RobotSpec spec)
        {
            Dead();
            Clean();
            Init(battle, spec);
        }

        public void Clean()
        {
            _client = null;
        }

        // Private

        void InitializeRobotPeer()
        {
            Random random = new Random();
            double rndX = random.NextDouble();
            double rndY = random.NextDouble();

            X = WIDTH + rndX * (1); // TODO
            Y = HEIGHT + rndY * (1);

            GunHeading = RadarHeading  = BodyHeading = 2 * Math.PI * random.NextDouble();
            UpdateBoundingBox();

            _state = RobotState.ACTIVE; // TODO: Atomic

            Velocity = 0;
            Hp = 5;
            Energy = 100;
            GunHeat = 3;

            _isExecFinishedAndDisabled = false;
            _scan = false;

            _scanArc.AngleStart = 0;
            _scanArc.AngleExtent = 0;
            _scanArc.Frame = (-100, -100, 1, 1);

            // TODO: Add events, bulletUpdates, ExecCommands
        }

        void UpdateBoundingBox()
        {

        }

        void Dead()
        {
            throw new NotImplementedException();
        }
    }
}
