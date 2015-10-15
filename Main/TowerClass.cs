using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Media;

namespace TowerDef
{
    /// <summary>
    /// 射击炮
    /// </summary>
    public abstract class ShootingTower : Tower
    {
        private class Shot
        {
            public PointF Position;
            public PointF Velocity;
            public bool Enabled = false;
            public bool Tracking = false;
            public Monster Destination = null;
        }

        private int[] FLevelMoney = null;
        private int[] FLevelUpTime = null;
        private int FLevel = 0;
        private Shot[] FShots;
        private static Pen FPen = new Pen(Color.FromArgb(0, 0, 0));
        private bool FUpdating = false;
        private ProgressClip FProgress = null;
        protected Game FGame = null;
        /// <summary>
        /// 绘制塔
        /// </summary>
        /// <param name="Pos">位置</param>
        /// <param name="Painter">画板</param>
        public abstract void DrawTower(PointF Pos, Graphics Painter);
        /// <summary>
        /// 绘制炮弹
        /// </summary>
        /// <param name="Pos">位置</param>
        /// <param name="Painter">画板</param>
        protected abstract void DrawShot(PointF Pos, Graphics Painter);
        /// <summary>
        /// 获得炮弹速度
        /// </summary>
        /// <param name="Level">等级</param>
        /// <returns>速度</returns>
        protected abstract float GetVelocity(int Level);
        /// <summary>
        /// 获得射程
        /// </summary>
        /// <param name="Level">等级</param>
        /// <returns>射程</returns>
        protected abstract float GetRange(int Level);
        /// <summary>
        /// 检查炮弹是否跟踪
        /// </summary>
        /// <param name="Level">等级</param>
        /// <returns>是否跟踪</returns>
        protected abstract bool GetTracking(int Level);
        /// <summary>
        /// 获得同时发射的最大炮弹数量
        /// </summary>
        /// <param name="Level">等级</param>
        /// <returns>炮弹数量</returns>
        protected abstract int GetMaxShot(int Level);
        /// <summary>
        /// 击中处理
        /// </summary>
        /// <param name="Level">等级</param>
        /// <param name="m">被击中怪物</param>
        /// <param name="Monsters">所有怪物</param>
        protected virtual void Hit(int Level, PointF Pos, Monster m, LinkedList<Monster>[] Monsters)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="LevelMoney">升级必备金钱设置</param>
        /// <param name="LevelMoney">升级时间</param>
        public ShootingTower(Game aGame, int[] LevelMoney, int[] LevelUpTime)
        {
            FLevelMoney = LevelMoney;
            FLevelUpTime = LevelUpTime;
            FGame = aGame;
            int MaxShot = GetMaxShot(FLevelMoney.Length);
            FShots = new Shot[MaxShot];
            for (int i = 0; i < MaxShot; i++)
            {
                FShots[i] = new Shot();
            }
        }

        public override void DrawTower(Graphics Painter)
        {
            DrawTower(Position, Painter);
            PointF LeftTop = new PointF(Position.X - 14, Position.Y + 12);
            for (int i = 0; i < FLevel; i++)
            {
                Painter.DrawRectangle(FPen, LeftTop.X + i * 4, LeftTop.Y, 2, 2);
            }
        }
        Audio aa = new Audio();
        int t = 0;
        int count = 0;
        public override void DrawFloating(Graphics Painter)
        {
            foreach (Shot s in FShots)
            {
                if (s.Enabled)
                {
                    if (t == count)
                    {
                        //Thread t1 = new Thread(new ThreadStart(Play_Shoot1));
                        //t1.Start();
                        t++;
                    }
                    DrawShot(s.Position, Painter);
                    count++;
                }
            }
        }
        private void Play_Shoot1()
        {
            //SoundPlayer ss = new SoundPlayer();
            //ss.SoundLocation = "..//..//Sound//Shoot.wav";
            //ss.Load();
            //ss.PlaySync();

            //aa.Close();
            //aa.Play("Sound/Shoot.wav");
        }
        public override void Tracking(Monster aMonster, ref PointF Vector)
        {
        }

        public override void Run(LinkedList<Monster>[] Monsters)
        {
            /*正在升级*/
            if (FUpdating)
            {
                if (FProgress.Finished)
                {
                    FUpdating = false;
                    FProgress = null;
                }
            }
            /*处理炮弹*/
            for (int i = 0; i < GetMaxShot(FLevel); i++)
            {
                Shot s = FShots[i];
                /*如果炮弹有效*/
                if (s.Enabled)
                {
                    /*处理跟踪*/
                    if (s.Tracking)
                    {
                        if (s.Destination.Life != 0)
                        {
                            s.Velocity = new PointF(s.Destination.Position.X - s.Position.X, s.Destination.Position.Y - s.Position.Y);
                            float Length = (float)Math.Sqrt(Math.Pow(s.Velocity.X, 2) + Math.Pow(s.Velocity.Y, 2));
                            s.Velocity.X /= Length / GetVelocity(FLevel);
                            s.Velocity.Y /= Length / GetVelocity(FLevel);
                        }
                        else
                        {
                            s.Tracking = false;
                        }
                    }
                    /*炮弹行走*/
                    s.Position.X += s.Velocity.X*2;
                    s.Position.Y += s.Velocity.Y*2;
                    /*如果炮弹超出射程则禁用*/
                    if (Math.Pow(s.Position.X - Position.X, 2) + Math.Pow(s.Position.Y - Position.Y, 2) <= Math.Pow(GetRange(FLevel), 2))
                    {
                        foreach (LinkedList<Monster> List in Monsters)
                        {
                            foreach (Monster m in List)
                            {
                                /*如果炮弹击中则调用回调函数并禁用*/
                                if (Math.Pow(m.Position.X - s.Position.X, 2) + Math.Pow(m.Position.Y - s.Position.Y, 2) <= m.Radius * m.Radius)
                                {
                                    Hit(FLevel, s.Position, m, Monsters);
                                    s.Enabled = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        s.Enabled = false;
                    }
                }
                /*如果炮弹禁用并且没有正在升级*/
                if (!s.Enabled && !FUpdating)
                {
                    Monster mDest = null;
                    float mDist = float.MaxValue;
                    /*寻找距离最接近的怪物*/
                    foreach (LinkedList<Monster> List in Monsters)
                    {
                        foreach (Monster m in List)
                        {
                            float Distance=(float)(Math.Pow(m.Position.X - Position.X, 2) + Math.Pow(m.Position.Y - Position.Y, 2));
                            if (Distance <= Math.Pow(GetRange(FLevel), 2))
                            {
                                if (Distance < mDist)
                                {
                                    mDist = Distance;
                                    mDest = m;
                                }
                            }
                        }
                    }
                    /*如果找到则发射炮弹*/
                    if (mDest != null)
                    {
                        s.Velocity = new PointF(mDest.Position.X - Position.X, mDest.Position.Y - Position.Y);
                        float Length = (float)Math.Sqrt(Math.Pow(s.Velocity.X, 2) + Math.Pow(s.Velocity.Y, 2));
                        s.Velocity.X /= Length / GetVelocity(FLevel);
                        s.Velocity.Y /= Length / GetVelocity(FLevel);
                        s.Position = Position;
                        s.Enabled = true;
                        s.Tracking = GetTracking(FLevel);
                        s.Destination = mDest;
                        return;
                    }
                }
            }
        }

        public override bool LevelUp(ref int Money)
        {
            if (Money >= LevelUpMoney())
            {
                Money -= LevelUpMoney();
                FUpdating = true;
                /****绘制升级进度条****/
                FProgress = new ProgressClip(FLevelUpTime[FLevel], Position);
                FGame.AddClip(FProgress);
                /**********************/
                FLevel++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int LevelUpMoney(int Level)
        {
            return FLevelMoney[Level];
        }

        public override int LevelUpMoney()
        {
            if (FLevel == MaxLevel)
            {
                return int.MaxValue;
            }
            else
            {
                return FLevelMoney[FLevel];
            }
        }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level
        {
            get
            {
                return FLevel;
            }
        }

        /// <summary>
        /// 最高等级
        /// </summary>
        public int MaxLevel
        {
            get
            {
                return FLevelMoney.Length;
            }
        }

        /// <summary>
        /// 当前射程
        /// </summary>
        public float Range
        {
            get
            {
                return GetRange(FLevel);
            }
        }

        /// <summary>
        /// 正在升级
        /// </summary>
        public bool Updating
        {
            get
            {
                return FUpdating;
            }
        }
    }

    public abstract class TowerFactory
    {
        public abstract float GetRange();
        public abstract int GetMoney();
        public abstract int GetMaxLevel();
        public abstract int GetLevelUpMoney(int Level);
        public abstract void DrawTower(PointF Pos, Graphics Painter);
        public abstract bool IsMine(Tower aTower);
        protected abstract Tower GetTower(Game aGame);
        public Tower GetTower(ref int Money, Game aGame)
        {
            Money -= GetMoney();
            return GetTower(aGame);
        }
    }

    /// <summary>
    /// 多重攻击塔
    /// </summary>
    public class MultiTower : ShootingTower
    {
        private class _Factory : TowerFactory
        {
            private MultiTower FTower = new MultiTower(null);

            public override float GetRange()
            {
                return FTower.Range;
            }

            public override int GetMoney()
            {
                return 5;
            }

            public override int GetMaxLevel()
            {
                return FTower.MaxLevel;
            }

            public override int GetLevelUpMoney(int Level)
            {
                return FTower.LevelUpMoney(Level);
            }

            public override void DrawTower(PointF Pos, Graphics Painter)
            {
                FTower.DrawTower(Pos, Painter);
            }

            public override bool IsMine(Tower aTower)
            {
                return aTower is MultiTower;
            }

            protected override Tower GetTower(Game aGame)
            {
                return new MultiTower(aGame);
            }
        }
        /// <summary>
        /// 炮塔工厂
        /// </summary>
        public static TowerFactory Factory = new _Factory();

        private static Pen FPen = new Pen(Color.FromArgb(0, 0, 0));
        private static Brush FBrush = new SolidBrush(Color.FromArgb(255, 0, 128));

        public MultiTower(Game aGame)
            : base(aGame,
            new int[] { 16, 81, 256, 625, 1296 },
            new int[] { 15, 30, 45, 60, 75 })
        {
        }
        public override void DrawTower(PointF Pos, Graphics Painter)
        {
            Image tower = Image.FromFile("..//..//images//tower00.png");
            Painter.DrawImage(tower, Pos.X - 16, Pos.Y - 16);
            //Painter.DrawEllipse(new Pen(Color.Blue), Pos.X - 15, Pos.Y - 15, 30, 30);
            //Painter.DrawString("多", new Font("微软雅黑", 20, GraphicsUnit.Pixel), new SolidBrush(Color.Blue), new PointF(Pos.X - 14, Pos.Y - 14));
            
        }
        protected override void DrawShot(PointF Pos, Graphics Painter)
        {
            Painter.FillEllipse(new SolidBrush(Color.Yellow), Pos.X-2, Pos.Y-2, 10, 10);
        }
        protected override float GetVelocity(int Level)
        {
            return 3;
        }

        protected override float GetRange(int Level)
        {
            return 100 + Level * 10;
        }

        protected override bool GetTracking(int Level)
        {
            return true;
        }

        protected override int GetMaxShot(int Level)
        {
            return Level + 1;
        }

        protected override void Hit(int Level, PointF Pos, Monster m, LinkedList<Monster>[] Monsters)
        {
            m.Hurt((Level + 1) * (Level + 1));
        }
    }

    /// <summary>
    /// 速射塔
    /// </summary>
    public class SpeedTower : ShootingTower
    {
        private class _Factory : TowerFactory
        {
            private SpeedTower FTower = new SpeedTower(null);

            public override float GetRange()
            {
                return FTower.Range;
            }

            public override int GetMoney()
            {
                return 10;
            }

            public override int GetMaxLevel()
            {
                return FTower.MaxLevel;
            }

            public override int GetLevelUpMoney(int Level)
            {
                return FTower.LevelUpMoney(Level);
            }

            public override void DrawTower(PointF Pos, Graphics Painter)
            {
                FTower.DrawTower(Pos, Painter);
            }

            public override bool IsMine(Tower aTower)
            {
                return aTower is SpeedTower;
            }

            protected override Tower GetTower(Game aGame)
            {
                return new SpeedTower(aGame);
            }
        }
        /// <summary>
        /// 炮塔工厂
        /// </summary>
        public static TowerFactory Factory = new _Factory();

        private static Pen FPen = new Pen(Color.FromArgb(0, 0, 0));
        private static Brush FBrush = new SolidBrush(Color.FromArgb(255, 128, 0));

        public SpeedTower(Game aGame)
            : base(aGame,
            new int[] { 32, 162, 512, 1250, 2592 },
            new int[] { 20, 40, 60, 80, 100 })
        {
        }

        //protected void DrawCircle(PointF Pos, Graphics Painter)
        //{
        //    Painter.FillEllipse(FBrush, Pos.X - 3, Pos.Y - 3, 6, 6);
        //    Painter.DrawEllipse(FPen, Pos.X - 3, Pos.Y - 3, 6, 6);
        //}

        public override void DrawTower(PointF Pos, Graphics Painter)
        {
            Image tower = Image.FromFile("..//..//images//tower01.png");
            Painter.DrawImage(tower, Pos.X - 16, Pos.Y - 16);
            //Painter.DrawEllipse(new Pen(Color.Blue), Pos.X - 15, Pos.Y - 15, 30, 30);
            //Painter.DrawString("速",new Font("微软雅黑",20, GraphicsUnit.Pixel),new SolidBrush(Color.Blue),new PointF(Pos.X-14,Pos.Y-14));
        }

        protected override void DrawShot(PointF Pos, Graphics Painter)
        {
            Painter.FillEllipse(new SolidBrush(Color.Yellow), Pos.X - 2, Pos.Y - 2, 10, 10);
        }

        protected override float GetVelocity(int Level)
        {
            return 3 + Level;
        }

        protected override float GetRange(int Level)
        {
            return 100 + Level * 10;
        }

        protected override bool GetTracking(int Level)
        {
            return true;
        }

        protected override int GetMaxShot(int Level)
        {
            return 1;
        }

        protected override void Hit(int Level, PointF Pos, Monster m, LinkedList<Monster>[] Monsters)
        {
            m.Hurt(2 * (Level + 1) * (Level + 1));
        }
    }


    /// <summary>
    /// 群攻塔
    /// </summary>
    public class RangeTower : ShootingTower
    {
        private class _Factory : TowerFactory
        {
            private RangeTower FTower = new RangeTower(null);

            public override float GetRange()
            {
                return FTower.Range;
            }

            public override int GetMoney()
            {
                return 50;
            }

            public override int GetMaxLevel()
            {
                return FTower.MaxLevel;
            }

            public override int GetLevelUpMoney(int Level)
            {
                return FTower.LevelUpMoney(Level);
            }

            public override void DrawTower(PointF Pos, Graphics Painter)
            {
                FTower.DrawTower(Pos, Painter);
            }

            public override bool IsMine(Tower aTower)
            {
                return aTower is RangeTower;
            }

            protected override Tower GetTower(Game aGame)
            {
                return new RangeTower(aGame);
            }
        }
        /// <summary>
        /// 炮塔工厂
        /// </summary>
        public static TowerFactory Factory = new _Factory();

        private static Pen FPen = new Pen(Color.FromArgb(0, 0, 0));
        private static Brush FBrush = new SolidBrush(Color.FromArgb(128, 255, 0));
        private LinkedList<RangeClip> FRanges = new LinkedList<RangeClip>();

        public RangeTower(Game aGame)
            : base(aGame,
            new int[] { 150, 300, 600, 1200, 2000 },
            new int[] { 30, 60, 90, 120, 150 })
        {
        }
        public override void DrawTower(PointF Pos, Graphics Painter)
        {
            Image tower = Image.FromFile("..//..//images//tower02.png");
            Painter.DrawImage(tower, Pos.X - 16, Pos.Y - 16);
            //Painter.DrawEllipse(new Pen(Color.Blue), Pos.X - 15, Pos.Y - 15, 30, 30);
            //Painter.DrawString("群", new Font("微软雅黑", 20, GraphicsUnit.Pixel), new SolidBrush(Color.Blue), new PointF(Pos.X - 14, Pos.Y - 14));
        }

        protected override void DrawShot(PointF Pos, Graphics Painter)
        {
            Painter.FillEllipse(new SolidBrush(Color.Green), Pos.X - 2, Pos.Y - 2, 15, 15);
        }

        protected override float GetVelocity(int Level)
        {
            return 3 + Level / 2;
        }

        protected override float GetRange(int Level)
        {
            return 100 + Level * 10;
        }

        protected override bool GetTracking(int Level)
        {
            return true;
        }

        protected override int GetMaxShot(int Level)
        {
            return 1;
        }

        protected override void Hit(int Level, PointF Pos, Monster m, LinkedList<Monster>[] Monsters)
        {
            RangeClip aClip = new RangeClip(((int)GetRange(Level)) / 10, GetRange(Level) / 2, Pos);
            FRanges.AddLast(aClip);
            FGame.AddClip(aClip);
        }

        public override void Run(LinkedList<Monster>[] Monsters)
        {
            base.Run(Monsters);
            /*遍历所有辐射*/
            LinkedListNode<RangeClip> Current = FRanges.First;
            while (Current != null)
            {
                LinkedListNode<RangeClip> Next = Current.Next;
                /*辐射结束则删除辐射*/
                if (Current.Value.Finished)
                {
                    FRanges.Remove(Current);
                }
                else
                {
                    /*否则对辐射范围内的所有怪物造成伤害*/
                    PointF Pos = Current.Value.Position;
                    float Radius = Current.Value.Radius;
                    foreach (LinkedList<Monster> ms in Monsters)
                    {
                        foreach (Monster m in ms)
                        {
                            if (Math.Pow(Pos.X - m.Position.X, 2) + Math.Pow(Pos.Y - m.Position.Y, 2) <= Math.Pow(Radius + m.Radius, 2))
                            {
                                m.Hurt(m.Defense + Level + 1);
                            }
                        }
                    }
                }
                Current = Next;
            }
        }
    }


    /// <summary>
    /// 减速炮弹
    /// </summary>
    public class TrapTower : ShootingTower
    {
        private class _Factory : TowerFactory
        {
            private TrapTower FTower = new TrapTower(null);

            public override float GetRange()
            {
                return FTower.Range;
            }

            public override int GetMoney()
            {
                return 1000;
            }

            public override int GetMaxLevel()
            {
                return FTower.MaxLevel;
            }

            public override int GetLevelUpMoney(int Level)
            {
                return FTower.LevelUpMoney(Level);
            }

            public override void DrawTower(PointF Pos, Graphics Painter)
            {
                FTower.DrawTower(Pos, Painter);
            }

            public override bool IsMine(Tower aTower)
            {
                return aTower is TrapTower;
            }

            protected override Tower GetTower(Game aGame)
            {
                return new TrapTower(aGame);
            }
        }
        /// <summary>
        /// 炮塔工厂
        /// </summary>
        public static TowerFactory Factory = new _Factory();

        private static Pen FPen = new Pen(Color.FromArgb(0, 0, 0));
        private static Pen FLight = new Pen(Color.FromArgb(180,71, 194, 194), 7);
        private static Brush FBrush = new SolidBrush(Color.FromArgb(128, 128, 255));
        private LinkedList<Monster> FTracking = new LinkedList<Monster>();

        public TrapTower(Game aGame)
            : base(aGame,
            new int[] { 1000, 2000 },
            new int[] { 100, 200 })
        {
        }

        public override void DrawTower(PointF Pos, Graphics Painter)
        {
            Image tower = Image.FromFile("..//..//images//tower03.png");
            Painter.DrawImage(tower, Pos.X - 16, Pos.Y - 16);
            //Painter.DrawEllipse(new Pen(Color.Blue), Pos.X - 15, Pos.Y - 15, 30, 30);
            //Painter.DrawString("缓", new Font("微软雅黑", 20, GraphicsUnit.Pixel), new SolidBrush(Color.Blue), new PointF(Pos.X - 14, Pos.Y - 14));
        }
        Audio aa = new Audio();
        public override void DrawFloating(Graphics Painter)
        {
            base.DrawFloating(Painter);
            if (!Updating)
            {
                foreach (Monster m in FTracking)
                {
                    //Thread t1 = new Thread(new ThreadStart(Play_Shoot1));
                    //t1.Start();
                    Painter.DrawLine(FLight, Position, m.Position);
                }
            }
        }
        private void Play_Shoot1()
        {
            //SoundPlayer ss = new SoundPlayer();
            //ss.SoundLocation = "..//..//Sound//Shoot.wav";
            //ss.Load();
            //ss.PlaySync();

            //aa.Close();
            //aa.Play("Sound/Shoot.wav");
        }
        protected override void DrawShot(PointF Pos, Graphics Painter)
        {
        }

        protected override float GetVelocity(int Level)
        {
            return 0;
        }

        protected override float GetRange(int Level)        //获取范围
        {
            return 100 + Level * 20;
        }

        protected override bool GetTracking(int Level)
        {
            return true;
        }

        protected override int GetMaxShot(int Level)
        {
            return 0;
        }

        public override void Tracking(Monster aMonster, ref PointF Vector)
        {
            if (!Updating)
            {
                float Factor = 1;
                switch (Level)
                {
                    case 0:
                        Factor = 0.6f;
                        break;
                    case 1:
                        Factor = 0.4f;
                        break;
                    case 2:
                        Factor = 0.3f;
                        break;
                }
                Vector.X *= Factor;
                Vector.Y *= Factor;
            }
        }

        public override void Run(LinkedList<Monster>[] Monsters)
        {
            base.Run(Monsters);
            LinkedListNode<Monster> Current = FTracking.First;
            /*去掉超出射程的怪物的跟踪*/
            while (Current != null)
            {
                LinkedListNode<Monster> Next = Current.Next;
                if (Current.Value.Life == 0)
                {
                    Current.Value.Untrack(this);
                    FTracking.Remove(Current);
                }
                else if (Math.Pow(Current.Value.Position.X - Position.X, 2) + Math.Pow(Current.Value.Position.Y - Position.Y, 2) > Math.Pow(Range, 2))
                {
                    Current.Value.Untrack(this);
                    FTracking.Remove(Current);
                }
                Current = Next;
            }
            /*跟踪范围内的所有怪物*/
            foreach (LinkedList<Monster> List in Monsters)
            {
                foreach (Monster m in List)
                {
                    if (Math.Pow(m.Position.X - Position.X, 2) + Math.Pow(m.Position.Y - Position.Y, 2) <= Math.Pow(Range, 2))
                    {
                        /*查看跟踪范围内的怪物是否已经被跟踪*/
                        bool Found = false;
                        foreach (Monster m2 in FTracking)
                        {
                            if (m == m2)
                            {
                                Found = true;
                                break;
                            }
                        }
                        /*没被跟踪则添加跟踪*/
                        if (!Found)
                        {
                            m.Track(this, int.MaxValue);
                            FTracking.AddLast(m);
                        }
                    }
                }
            }
        }
    }
}