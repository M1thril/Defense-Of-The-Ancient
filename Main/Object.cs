using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TowerDef
{
    /// <summary>
    /// 方向
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// 左
        /// </summary>
        Left,
        /// <summary>
        /// 上
        /// </summary>
        Up,
        /// <summary>
        /// 右
        /// </summary>
        Right,
        /// <summary>
        /// 下
        /// </summary>
        Down
    }

    public enum Game_Clip 
    { 
        Zero_Clip,
        One_Clip,
        Two_Clip
    }
    /// <summary>
    /// 塔
    /// </summary>
    public abstract class Tower
    {
        private PointF FPosition = new PointF(0, 0);
        private int FSellMoney = 0;

        /// <summary>
        /// 位置
        /// </summary>
        public PointF Position
        {
            get
            {
                return FPosition;
            }
            set
            {
                FPosition = value;
            }
        }

        /// <summary>
        /// 卖出可得金钱
        /// </summary>
        public int SellMoney
        {
            get
            {
                return FSellMoney;
            }
            set
            {
                FSellMoney = value;
            }
        }
        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="Painter">画板</param>
        public abstract void DrawTower(Graphics Painter);
        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="Painter">画板</param>
        public abstract void DrawFloating(Graphics Painter);
        /// <summary>
        /// 被跟踪的怪物正在行动
        /// </summary>
        /// <param name="aMonster">怪物</param>
        /// <param name="Vector">速度</param>
        public abstract void Tracking(Monster aMonster, ref PointF Vector);
        /// <summary>
        /// 执行动作
        /// </summary>
        public abstract void Run(LinkedList<Monster>[] Monsters);
        /// <summary>
        /// 升级
        /// </summary>
        /// <param name="Money">现有金钱</param>
        /// <returns>是否成功</returns>
        public abstract bool LevelUp(ref int Money);
        /// <summary>
        /// 获得升级所需要的金钱
        /// </summary>
        /// <returns>是否可以继续升级</returns>
        public abstract int LevelUpMoney();
    }

    /// <summary>
    /// 怪物
    /// </summary>
    public abstract class Monster
    {
        private class TrackingRecord
        {
            public Tower TrackingTower = null;
            public int RemainSteps = 0;
        }

        private PointF FPosition = new PointF(0, 0);
        private LinkedList<TrackingRecord> FTrackingTowers = new LinkedList<TrackingRecord>();
        private int FMaxLife = 0;
        private int FLife = 0;
        private int FDefense = 0;
        private float FSize = 0;
        private Direction FDirection = Direction.Left;
        private Direction FOriginalDirection = Direction.Left;
        private Point FOriginalCell;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="aLife">生命值</param>
        /// <param name="aDefense">防御力</param>
        /// <param name="aMoney">金钱</param>
        /// <param name="aSize">尺寸</param>
        public Monster(int aLife, int aDefense, float aSize)
        {
            FMaxLife = aLife;
            FLife = aLife;
            FDefense = aDefense;
            if (aSize <= 28)
            {
                FSize = aSize;
            }
            else
            {
                FSize = 28;
            }
        }

        /// <summary>
        /// 追踪
        /// </summary>
        /// <param name="TrackingTower">追踪塔</param>
        /// <param name="Steps">步数</param>
        public void Track(Tower TrackingTower, int Steps)
        {
            foreach (TrackingRecord r in FTrackingTowers)
            {
                if (r.TrackingTower == TrackingTower)
                {
                    r.RemainSteps += Steps;
                    return;
                }
            }
            TrackingRecord tr = new TrackingRecord();
            tr.TrackingTower = TrackingTower;
            tr.RemainSteps = Steps;
            FTrackingTowers.AddLast(tr);
        }

        /// <summary>
        /// 停止追踪
        /// </summary>
        /// <param name="TrackingTower">追踪塔</param>
        public void Untrack(Tower TrackingTower)
        {
            LinkedListNode<TrackingRecord> Current = FTrackingTowers.First;
            while (Current != null)
            {
                LinkedListNode<TrackingRecord> Next = Current.Next;
                if (Current.Value.TrackingTower == TrackingTower)
                {
                    FTrackingTowers.Remove(Current);
                }
                Current = Next;
            }
        }

        /// <summary>
        /// 查询指定的塔追踪的剩余步数
        /// </summary>
        /// <param name="TrackingTower">追踪塔</param>
        /// <returns>剩余步数</returns>
        //public int TraskSteps(Tower TrackingTower)
        //{
        //    foreach (TrackingRecord r in FTrackingTowers)
        //    {
        //        if (r.TrackingTower == TrackingTower)
        //        {
        //            return r.RemainSteps;
        //        }
        //    }
        //    return 0;
        //}

        /// <summary>
        /// 受到攻击
        /// </summary>
        /// <param name="LifeTaken">失去的生命值</param>
        public void Hurt(int LifeTaken)
        {
            int Delta = LifeTaken - FDefense;
            if (Delta < 1)
            {
                Delta = 1;
            }
            FLife -= Delta;
            if (FLife < 0)
            {
                FLife = 0;
            }
        }

        /// <summary>
        /// 执行动作
        /// </summary>
        /// <param name="aDirection"></param>
        public void Run(Direction aDirection)
        {
            //LinkedListNode<TrackingRecord> Current = FTrackingTowers.First;
            //while (Current != null)
            //{
            //    LinkedListNode<TrackingRecord> Next = Current.Next;
            //    if (Current.Value.RemainSteps-- == 0)
            //    {
            //        FTrackingTowers.Remove(Current);
            //    }
            //    Current = Next;
            //}
            FDirection = aDirection;
            Walk(FDirection);
            
        }

        /// <summary>
        /// 位置
        /// </summary>
        public PointF Position
        {
            get
            {
                return FPosition;
            }
            set
            {
                PointF Vector = new PointF(value.X - FPosition.X, value.Y - FPosition.Y);
                foreach (TrackingRecord r in FTrackingTowers)
                {
                    r.TrackingTower.Tracking(this, ref Vector);
                }
                FPosition.X += Vector.X;
                FPosition.Y += Vector.Y;
            }
        }

        /// <summary>
        /// 当前方向
        /// </summary>
        public Direction Direction
        {
            get
            {
                return FDirection;
            }
        }

        /// <summary>
        /// 初始方向
        /// </summary>
        public Direction OriginalDirection
        {
            get
            {
                return FOriginalDirection;
            }
            set
            {
                FDirection = value;
                FOriginalDirection = value;
            }
        }

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxLife
        {
            get
            {
                return FMaxLife;
            }
        }

        /// <summary>
        /// 生命值
        /// </summary>
        public int Life
        {
            get
            {
                return FLife;
            }
            set
            {
                FLife = value;
            }
        }

        /// <summary>
        /// 防御值
        /// </summary>
        public int Defense
        {
            get
            {
                return FDefense;
            }
        }

        /// <summary>
        /// 半径
        /// </summary>
        public float Radius
        {
            get
            {
                return FSize / 2;
            }
        }

        /// <summary>
        /// 原来所在单元
        /// </summary>
        public Point OriginalCell
        {
            get
            {
                return FOriginalCell;
            }
            set
            {
                FOriginalCell = value;
            }
        }

        /// <summary>
        /// 行走
        /// </summary>
        /// <param name="aDirection">方向</param>
        protected abstract void Walk(Direction aDirection);
        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="Painter">画板</param>
        public abstract void Draw(Graphics Painter);
    }

    /// <summary>
    /// 剪辑
    /// </summary>
    public abstract class Clip
    {
        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="Painter">画板</param>
        /// <returns>true则删除</returns>
        public abstract bool Draw(Graphics Painter);
    }

    /// <summary>
    /// 剪辑容器
    /// </summary>
    public class ClipPainter
    {
        private LinkedList<Clip> FClips = new LinkedList<Clip>();

        /// <summary>
        /// 添加剪辑
        /// </summary>
        /// <param name="aClip">剪辑</param>
        public void AddClip(Clip aClip)
        {
            FClips.AddLast(aClip);
        }

        /// <summary>
        /// 绘制剪辑
        /// </summary>
        /// <param name="Painter">画板</param>
        public void Draw(Graphics Painter)
        {
            LinkedListNode<Clip> Current = FClips.First;
            while (Current != null)
            {
                LinkedListNode<Clip> Next = Current.Next;
                if (Current.Value.Draw(Painter))
                {
                    FClips.Remove(Current);
                }
                Current = Next;
            }
        }

        /// <summary>
        /// 清除所有剪辑
        /// </summary>
        public void Clear()
        {
            FClips.Clear();
        }
    }

    /// <summary>
    /// 文字剪辑
    /// </summary>
    public class TextClip : Clip
    {
        private Font FFont = null;
        private SolidBrush FBrush = new SolidBrush(Color.Black);
        private string FText = "";
        private int FLife = 0;
        private int FMaxLife = 0;
        private PointF FPosition;
        private PointF FVelocity;
        private bool FCenter = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="aFont">字体</param>
        /// <param name="aColor">颜色</param>
        /// <param name="aText">文字</param>
        /// <param name="aPosition">位置</param>
        /// <param name="aVelocity">速度</param>
        /// <param name="aCenter">是否居中</param>
        /// <param name="aLife">生命值</param>
        public TextClip(Font aFont, Color aColor, string aText, PointF aPosition, PointF aVelocity, bool aCenter, int aLife)
        {
            FFont = aFont;
            FBrush.Color = aColor;
            FText = aText;
            FPosition = aPosition;
            FVelocity = aVelocity;
            FCenter = aCenter;
            FLife = aLife;
            FMaxLife = aLife;
        }

        public override bool Draw(Graphics Painter)
        {
            FBrush.Color = Color.FromArgb(255 * FLife / FMaxLife, FBrush.Color);
            PointF Pos = FPosition;
            if (FCenter)
            {
                SizeF Size = Painter.MeasureString(FText, FFont);
                Pos.X -= Size.Width / 2;
                Pos.Y -= Size.Height / 2;
            }
            Painter.DrawString(FText, FFont, FBrush, FPosition);
            FPosition.X += FVelocity.X;
            FPosition.Y += FVelocity.Y;
            FLife--;
            return FLife == 0;
        }
    }

    /// <summary>
    /// 进度条剪辑
    /// </summary>
    public class ProgressClip : Clip
    {
        private static Pen FPen = new Pen(Color.FromArgb(255, 0, 0));
        private static Brush FBackground = new SolidBrush(Color.FromArgb(128, 255, 0, 0));
        private static Brush FProgress = new SolidBrush(Color.FromArgb(255, 0, 0));
        private int FLife = 0;
        private int FMaxLife = 0;
        private PointF FPosition;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Life">生命值</param>
        public ProgressClip(int Life, PointF Pos)
        {
            FLife = Life;
            FMaxLife = Life;
            FPosition = Pos;
        }

        public override bool Draw(Graphics Painter)
        {
            int Left = (int)FPosition.X - 16;
            int Top = (int)FPosition.Y - 16;
            int pLeft = Left + 5;
            int pTop = Top + 14;
            int pWidth = 22;
            int pHeight = 4;
            int pLen = pWidth * (FMaxLife - FLife) / FMaxLife;          //实时对应长度
            Painter.FillRectangle(FBackground, Left, Top, 32, 32);      //红色升级背景
            //Painter.DrawRectangle(FPen, Left, Top, 32, 32);           //描边
            Painter.FillRectangle(FProgress, pLeft, pTop, pLen, pHeight);//升级进度条
            Painter.DrawRectangle(FPen, pLeft, pTop, pWidth, pHeight);  //描边
            FLife--;
            return FLife == 0;
        }

        /// <summary>
        /// 是否结束
        /// </summary>
        public bool Finished
        {
            get
            {
                return FLife == 0;
            }
        }
    }

    /// <summary>
    /// 范围剪辑
    /// </summary>
    public class RangeClip : Clip
    {
        private static Pen FPen = new Pen(Color.Orange,3);          //绘制爆炸范围
        private static Brush FBrush = new SolidBrush(Color.FromArgb(8, 128, 255, 0));
        private int FLife = 0;
        private int FMaxLife = 0;
        private float FRadius = 0;
        private PointF FPosition;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Life">生命值</param>
        /// <param name="Radius">最大半径</param>
        /// <param name="Pos">位置</param>
        public RangeClip(int aLife, float aRadius, PointF aPos)
        {
            FLife = aLife;
            FMaxLife = aLife;
            FRadius = aRadius;
            FPosition = aPos;
        }

        public override bool Draw(Graphics Painter)         //绘制爆炸范围
        {
            float r = Radius;
            float d = r * 2;
            Painter.FillEllipse(FBrush, FPosition.X - r, FPosition.Y - r, d, d);
            Painter.DrawEllipse(FPen, FPosition.X - r, FPosition.Y - r, d, d);
            FLife--;
            return FLife == 0;
        }

        /// <summary>
        /// 是否结束
        /// </summary>
        public bool Finished
        {
            get
            {
                return FLife == 0;
            }
        }

        /// <summary>
        /// 半径
        /// </summary>
        public float Radius
        {
            get
            {
                return FRadius * (FMaxLife - FLife) / FMaxLife;
            }
        }

        public PointF Position
        {
            get
            {
                return FPosition;
            }
        }
    }
}