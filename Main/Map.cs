using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Threading;
namespace TowerDef
{
    /// <summary>
    /// 单元格
    /// </summary>
    public class MapCell
    {
        private bool FBlocking = false;
        private Tower FTower = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MapCell()
        {
        }

        /// <summary>
        /// 添加障碍
        /// </summary>
        public void Block()
        {
            FBlocking = true;
        }

        /// <summary>
        /// 移除障碍
        /// </summary>
        public void Unblock()
        {
            FBlocking = false;
        }

        /// <summary>
        /// 是否被阻挡
        /// </summary>
        public bool Blocking
        {
            get
            {
                return FBlocking;
            }
        }

        /// <summary>
        /// 单元格上的塔
        /// </summary>
        public Tower Tower
        {
            get
            {
                return FTower;
            }
            set
            {
                FTower = value;
            }
        }
    }

    /// <summary>
    /// 地图
    /// </summary>
    public class Map
    {
        private int FCols = 0;
        private int FRows = 0;
        private MapCell[][] FCells;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Cols">行数</param>
        /// <param name="Rows">列数</param>
        public Map(int aCols, int aRows)
        {
            FCols = aCols;
            FRows = aRows;
            FCells=new MapCell[FCols][];
            for (int i = 0; i < FCols; i++)
            {
                FCells[i] = new MapCell[FRows];
                for (int j = 0; j < FRows; j++)
                {
                    FCells[i][j] = new MapCell();
                }
            }
            
        }

        /// <summary>
        /// 获得单元格
        /// </summary>
        /// <param name="Col">行</param>
        /// <param name="Row">列</param>
        /// <returns>指定行列的单元格对象</returns>
        public MapCell Cells(int Col, int Row)
        {
            return FCells[Col][Row];
        }

        /// <summary>
        /// 行数
        /// </summary>
        public int Cols
        {
            get
            {
                return FCols;
            }
        }

        /// <summary>
        /// 列数
        /// </summary>
        public int Rows
        {
            get
            {
                return FRows;
            }
        }
    }

    /// <summary>
    /// 寻路器
    /// </summary>
    class Guider
    {
        /// <summary>
        /// 单元格方向
        /// </summary>
        public class CellDirection
        {
            private Direction[] FDirections=new Direction[4];
            private int FCount = 0;
            private bool FLocking = true;
            private bool FBinding = false;

            /// <summary>
            /// 构造函数
            /// </summary>
            public CellDirection()
            {
            }

            /// <summary>
            /// 清除方向
            /// </summary>
            public void ClearDirections()
            {
                if (!FLocking)
                {
                    FCount = 0;
                }
            }

            /// <summary>
            /// 添加方向
            /// </summary>
            /// <param name="aDirection">方向</param>
            public void AddDirection(Direction aDirection)
            {
                if (!FLocking)
                {
                    for (int i = 0; i < FCount; i++)
                    {
                        if (FDirections[i] == aDirection)
                        {
                            return;
                        }
                    }
                    FDirections[FCount++] = aDirection;
                }
            }

            /// <summary>
            /// 锁
            /// </summary>
            public void Lock()
            {
                FLocking = true;
            }

            /// <summary>
            /// 解锁
            /// </summary>
            public void Unlock()
            {
                FLocking = false;
            }

            /// <summary>
            /// 添加绑定
            /// </summary>
            public void Bind()
            {
                FBinding = true;
            }

            /// <summary>
            /// 移除绑定
            /// </summary>
            public void Unbind()
            {
                FBinding = false;
            }

            /// <summary>
            /// 方向数量
            /// </summary>
            public int Count
            {
                get
                {
                    return FCount;
                }
            }

            /// <summary>
            /// 单元格方向
            /// </summary>
            /// <param name="Index">方向索引</param>
            /// <returns>指定索引的方向</returns>
            public Direction this[int Index]
            {
                get
                {
                    return FDirections[Index];
                }
            }

            /// <summary>
            /// 锁状态
            /// </summary>
            public bool Locking
            {
                get
                {
                    return FLocking;
                }
            }

            /// <summary>
            /// 是否被绑定
            /// </summary>
            public bool Binding
            {
                get
                {
                    return FBinding;
                }
            }
        }

        private CellDirection[][] FCells = null;
        private Map FMap = null;
        private Point[] FDestinations = null;
        private Direction[] FDirections = new Direction[]
        {
            Direction.Left,
            Direction.Up,
            Direction.Right,
            Direction.Down
        };

        /// <summary>
        /// 根据方向获得前进向量
        /// </summary>
        /// <param name="aDirection">方向</param>
        /// <returns>向量</returns>
        //private Point GetVector(Direction aDirection)
        //{
        //    switch (aDirection)
        //    {
        //        case Direction.Left:
        //            return new Point(-1, 0);
        //        case Direction.Up:
        //            return new Point(0, -1);
        //        case Direction.Right:
        //            return new Point(1, 0);
        //        case Direction.Down:
        //            return new Point(0, 1);
        //        default:
        //            return new Point(0, 0);
        //    }
        //}

        /// <summary>
        /// 获得单元格
        /// </summary>
        /// <param name="Col">行</param>
        /// <param name="Row">列</param>
        /// <returns>获得指定位置的单元格对象</returns>
        public CellDirection Cells(int Col, int Row)
        {
            return FCells[Col][Row];
        }

        
    }

    /// <summary>
    /// 游戏对象
    /// </summary>
    public class Game
    {
        public delegate void Delegate_MonsterPass(Game aGame, Monster aMonster);
        public delegate void Delegate_MonsterKilled(Game aGame, Monster aMonster);

        private enum MessageStatus
        {
            Lighting,
            Darking,
            Disabled
        }

        private const int _CellSize = 32;
        private const int _MapCols = 20;
        private const int _MapRows = 15;
        private const int _ScreenWidth = _MapCols * _CellSize;
        private const int _ScreenHeight = _MapRows * _CellSize;

        private Pen FCellPen = new Pen(Color.FromArgb(0, 0, 0));
        private Pen FLifePen = new Pen(Color.FromArgb(0, 255, 0));
        private Pen FLifeBackPen = new Pen(Color.FromArgb(255, 0, 0));
        private SolidBrush FCellBrush = new SolidBrush(Color.FromArgb(255, 196, 0));
        private SolidBrush FBlockBrush = new SolidBrush(Color.FromArgb(0, 192, 255));
        private SolidBrush FWallBrush = new SolidBrush(Color.FromArgb(192, 0, 0));
        private SolidBrush FBornBrush = new SolidBrush(Color.FromArgb(0, 192, 96));
        private SolidBrush FBackBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
        private SolidBrush FMessageBrush = new SolidBrush(Color.FromArgb(0, 0, 0));
        private Font FMessageFont = new Font("微软雅黑", 72, FontStyle.Regular, GraphicsUnit.Pixel);
        private Bitmap FBuffer = null;
        private Graphics FPainter = null;
        //private Random FRandom = new Random();

        private Map FMap = null;
        private LinkedList<Monster>[] FMonsters = null;
        private ClipPainter FClipPainter = new ClipPainter();

        private string FMessage = "";
        private MessageStatus FMessageStatus = MessageStatus.Disabled;
        private int FMessageAlpha = 0;
        private const byte AlphaPositiveStep = 25;
        private const byte AlphaNegativeStep = 6;
        /// <summary>
        /// 绘制游戏背景
        /// </summary>
        private void Draw_DrawBackground(Game_Clip gc)
        {
            switch (gc) 
            { 
                case Game_Clip.Zero_Clip:
                    Image map01=Image.FromFile("..//..//images//map01.png");
                    FPainter.DrawImage(map01, 0, 0);
                break;
                case Game_Clip.One_Clip:
                    Image map02 = Image.FromFile("..//..//images//map02.png");
                    FPainter.DrawImage(map02, 0, 0);
                break;
                case Game_Clip.Two_Clip:
                Image map03 = Image.FromFile("..//..//images//map03.png");
                FPainter.DrawImage(map03, 0, 0);
                break;
            }
            
        }

        /// <summary>
        /// 绘制游戏开始logo
        /// </summary>
        //public void DrawLogo()
        //{
        //    Image logo = Image.FromFile("..//..//images//logo.png");
        //    FPainter.DrawImage(logo, 0, 0);
        //}
        
        /// <summary>
        /// 绘制游戏画面
        /// </summary>
        private void Draw_DrawScreen(Game_Clip gc)
        {
            /*绘制背景*/
            Draw_DrawBackground(gc);
            /*绘制塔*/
            for (int i = 1; i < _MapCols - 1; i++)
            {
                for (int j = 1; j < _MapRows - 1; j++)
                {
                    Tower aTower = FMap.Cells(i, j).Tower;
                    if (aTower != null)
                    {
                        aTower.DrawTower(FPainter);
                    }
                }
            }
            /*绘制怪物*/
            foreach (LinkedList<Monster> List in FMonsters)
            {
                foreach (Monster m in List)
                {
                    m.Draw(FPainter);
                }
            }
            
            /*绘制炮弹*/
            for (int i = 1; i < _MapCols - 1; i++)
            {
                for (int j = 1; j < _MapRows - 1; j++)
                {
                    Tower aTower = FMap.Cells(i, j).Tower;
                    
                    if (aTower != null)
                    {
                        aTower.DrawFloating(FPainter);
                    }
                }
                
            }
            /*绘制血条*/
            foreach (LinkedList<Monster> List in FMonsters)
            {
                foreach (Monster m in List)
                {
                    float Left = -10;
                    float Right = 10;
                    float Life = (Right - Left) * m.Life / m.MaxLife + Left;
                    Painter.DrawLine(FLifeBackPen, m.Position.X + Left, m.Position.Y - m.Radius - 2, m.Position.X + Right, m.Position.Y - m.Radius - 2);
                    Painter.DrawLine(FLifeBackPen, m.Position.X + Left, m.Position.Y - m.Radius - 1, m.Position.X + Right, m.Position.Y - m.Radius - 1);
                    Painter.DrawLine(FLifePen, m.Position.X + Left, m.Position.Y - m.Radius - 2, m.Position.X + Life, m.Position.Y - m.Radius - 2);
                    Painter.DrawLine(FLifePen, m.Position.X + Left, m.Position.Y - m.Radius - 1, m.Position.X + Life, m.Position.Y - m.Radius - 1);
                }
            }
            /*绘制剪辑*/
            FClipPainter.Draw(FPainter);
            /*绘制文字消息*/
            if (FMessageStatus != MessageStatus.Disabled)
            {
                Color BrushColor = FMessageBrush.Color;
                FMessageBrush.Color = Color.FromArgb(FMessageAlpha, Color.Black);
                SizeF MessageSize = FPainter.MeasureString(FMessage, FMessageFont);
                float X = (_ScreenWidth - MessageSize.Width) / 2;
                float Y = (_ScreenHeight - MessageSize.Height) / 2;
                FPainter.DrawString(FMessage, FMessageFont, FMessageBrush, new PointF(X, Y));
            }
        }

        /// <summary>
        /// 检查坐标是否越界
        /// </summary>
        /// <param name="Col">行</param>
        /// <param name="Row">列</param>
        /// <returns>指定单元格是否边界</returns>
        private bool Logic_IsBorder(int Col, int Row)
        {
            return (Col == 0 || Col == _MapCols - 1 || Row == 0 || Row == _MapRows - 1);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Game()
        {
            FBuffer = new Bitmap(_ScreenWidth, _ScreenHeight);
            FPainter = Graphics.FromImage(FBuffer);
            FMap = new Map(_MapCols, _MapRows);

            FMonsters = new LinkedList<Monster>[1];
            FMonsters[0] = new LinkedList<Monster>();
        }

        /// <summary>
        /// 游戏动作
        /// </summary>
        /// <param name="Channel">出口数量</param>
        public void Action(Game_Clip gc)
        {
            /*运行塔*/
            for (int i = 1; i < _MapCols - 1; i++)
            {
                for (int j = 1; j < _MapRows - 1; j++)
                {
                    Tower aTower = FMap.Cells(i, j).Tower;
                    if (aTower != null)
                    {
                        aTower.Run(FMonsters);
                    }
                }
            }
            /*运行怪物*/
            switch(gc)
            {
                case Game_Clip.Zero_Clip:
                    LinkedList<Monster> List1 = FMonsters[0];
                    LinkedListNode<Monster> Current1 = List1.First;

                    while (Current1 != null)
                    {
                        LinkedListNode<Monster> Next = Current1.Next;
                        Direction CurrentDirection;
                        if (Current1.Value.Life == 0)
                        {
                            if (OnMonsterKilled != null)
                            {
                                OnMonsterKilled(this, Current1.Value);
                            }
                            List1.Remove(Current1);
                        }
                        else
                        {
                            CurrentDirection = Current1.Value.Direction;       //哈哈哈哈哈哈哈哈哈
                            {
                                int X = (int)Current1.Value.Position.X;
                                int Y = (int)Current1.Value.Position.Y;
                                int Col = X / _CellSize;
                                int Row = Y / _CellSize;
                                //////怪物转向控制（呃
                                if (X >= 112 && Col == 3)
                                {
                                    CurrentDirection = Direction.Down;
                                }
                                if (Row == 11 && Col == 3)
                                {
                                    CurrentDirection = Direction.Right;
                                }
                                if (X >= 272 && Row == 11)
                                {
                                    CurrentDirection = Direction.Up;
                                }
                                if (Row == 1 && Col == 8)
                                {
                                    CurrentDirection = Direction.Right;
                                }
                                if (Row == 1 && X >= 400)
                                {
                                    CurrentDirection = Direction.Down;
                                }
                                if (Row == 11 && X >= 400)
                                {
                                    CurrentDirection = Direction.Right;
                                }
                                if (Row == 11 && X >= 560)
                                {
                                    CurrentDirection = Direction.Up;
                                }
                            }
                            /*行走*/
                            Current1.Value.Run(CurrentDirection);
                            /*判断怪物是否通过场地*/
                            {
                                int X = (int)Current1.Value.Position.X;
                                int Y = (int)Current1.Value.Position.Y;
                                int Col = X / _CellSize;
                                int Row = Y / _CellSize;
                                if (Row == 4 && Col == 17)
                                {
                                    if (OnMonsterPass != null)
                                    {
                                        OnMonsterPass(this, Current1.Value);
                                    }
                                    Current1.Value.Life = 0;
                                    List1.Remove(Current1);
                                }
                            }
                        }
                        Current1 = Next;
                    }
                break;

                case Game_Clip.One_Clip:
                {
                    LinkedList<Monster> List2 = FMonsters[0];
                    LinkedListNode<Monster> Current2 = List2.First;

                    while (Current2 != null)
                    {
                        LinkedListNode<Monster> Next = Current2.Next;
                        Direction CurrentDirection;
                        if (Current2.Value.Life == 0)
                        {
                            if (OnMonsterKilled != null)
                            {
                                OnMonsterKilled(this, Current2.Value);
                            }
                            List2.Remove(Current2);
                        }
                        else
                        {
                            CurrentDirection = Current2.Value.Direction;       //哈哈哈哈哈哈哈哈哈
                            {
                                int X = (int)Current2.Value.Position.X;
                                int Y = (int)Current2.Value.Position.Y;
                                int Col = X / _CellSize;
                                int Row = Y / _CellSize;
                                //////怪物转向控制（呃
                                if (X >= 336 && Row == 2)
                                {
                                    CurrentDirection = Direction.Down;
                                }
                                else if (X >= 336 && X<520 && Row == 8)
                                {
                                    CurrentDirection = Direction.Left;
                                }
                                else if (X <= 85 && Row == 8)
                                {
                                    CurrentDirection = Direction.Down;
                                }
                                else if (X <= 85 && Row == 12)
                                {
                                    CurrentDirection = Direction.Right;
                                }
                                else if (X >= 522 && Row == 12)
                                {
                                    CurrentDirection = Direction.Up;
                                }
                            }
                            /*行走*/
                            Current2.Value.Run(CurrentDirection);
                            /*判断怪物是否通过场地*/
                            {
                                int X = (int)Current2.Value.Position.X;
                                int Y = (int)Current2.Value.Position.Y;
                                int Col = X / _CellSize;
                                int Row = Y / _CellSize;
                                if (Row == 2 && Col == 16)
                                {
                                    if (OnMonsterPass != null)
                                    {
                                        OnMonsterPass(this, Current2.Value);
                                    }
                                    Current2.Value.Life = 0;
                                    List2.Remove(Current2);
                                }
                            }
                        }
                        Current2 = Next;
                    }
                }
                break;

                case Game_Clip.Two_Clip:
                {
                    LinkedList<Monster> List3 = FMonsters[0];
                    LinkedListNode<Monster> Current3 = List3.First;

                    while (Current3 != null)
                    {
                        LinkedListNode<Monster> Next = Current3.Next;
                        Direction CurrentDirection;
                        if (Current3.Value.Life == 0)
                        {
                            if (OnMonsterKilled != null)
                            {
                                OnMonsterKilled(this, Current3.Value);
                            }
                            List3.Remove(Current3);
                        }
                        else
                        {
                            CurrentDirection = Current3.Value.Direction;       //哈哈哈哈哈哈哈哈哈
                            {
                                int X = (int)Current3.Value.Position.X;
                                int Y = (int)Current3.Value.Position.Y;
                                int Col = X / _CellSize;
                                int Row = Y / _CellSize;
                                //////怪物转向控制（呃
                                if (X <= 148 && Row == 3)
                                {
                                    CurrentDirection = Direction.Down;
                                }
                                if (X <= 148 && Row == 9)
                                {
                                    CurrentDirection = Direction.Right;
                                }
                                if (X >= 208 && X<600 && Row == 9)
                                {
                                    CurrentDirection = Direction.Down;
                                }
                                if (X >= 208 && Row == 12)
                                {
                                    CurrentDirection = Direction.Right;
                                }
                                if (X >= 612 && Row == 12)
                                {
                                    CurrentDirection = Direction.Up;
                                }
                                if (X >= 612 && Row == 8)
                                {
                                    CurrentDirection = Direction.Left;
                                }
                                if (X <= 465 && X>400 && Row == 8)
                                {
                                    CurrentDirection = Direction.Up;
                                }
                            }
                            /*行走*/
                            Current3.Value.Run(CurrentDirection);
                            /*判断怪物是否通过场地*/
                            {
                                int X = (int)Current3.Value.Position.X;
                                int Y = (int)Current3.Value.Position.Y;
                                int Col = X / _CellSize;
                                int Row = Y / _CellSize;
                                if (Row == 7 && Col == 14)
                                {
                                    if (OnMonsterPass != null)
                                    {
                                        OnMonsterPass(this, Current3.Value);
                                    }
                                    Current3.Value.Life = 0;
                                    List3.Remove(Current3);
                                }
                            }
                        }
                        Current3 = Next;
                    }
                }
                break;
            }
            
        }

        /// <summary>
        /// 游戏循环
        /// </summary>
        public void Run()
        {
            /*处理文字消息*/
            switch (FMessageStatus)
            {
                case MessageStatus.Lighting:
                    FMessageAlpha += AlphaPositiveStep;
                    if (FMessageAlpha >= 255)
                    {
                        FMessageAlpha = 255;
                        FMessageStatus = MessageStatus.Darking;
                    }
                    break;
                case MessageStatus.Darking:
                    FMessageAlpha -= AlphaNegativeStep;
                    if (FMessageAlpha <= 0)
                    {
                        FMessageAlpha = 0;
                        FMessageStatus = MessageStatus.Disabled;
                    }
                    break;
            }
        }

        /// <summary>
        /// 清除游戏
        /// </summary>
        public void Clear()
        {
            for (int i = 1; i < _MapCols - 1 ; i++)
            {
                for (int j = 1; j < _MapRows - 1; j++)
                {
                    FMap.Cells(i, j).Tower = null;
                    FMap.Cells(i, j).Unblock();
                }
            }
            foreach (LinkedList<Monster> l in FMonsters)
            {
                l.Clear();
            }
            FClipPainter.Clear();
        }

        /// <summary>
        /// 绘制游戏画面
        /// </summary>
        public void Draw(Game_Clip gg)
        {
            //if (ttt == 0)
            //{
            //    DrawLogo();
            //    Thread.Sleep(1000);
            //    ttt++;
            //}
            Draw_DrawScreen(gg);
        }

        /// <summary>
        /// 放置文字消息
        /// </summary>
        /// <param name="Message">文字消息</param>
        public void PutMessage(string Message)
        {
            FMessage = Message;
            FMessageStatus = MessageStatus.Lighting;
            FMessageAlpha = 0;
        }
        /// <summary>
        /// 检查单元格是否可编辑
        /// </summary>
        /// <param name="Col">横坐标</param>
        /// <param name="Row">纵坐标</param>
        /// <returns>是否可编辑</returns>
        public bool IsEditableCell(int X, int Y)
        {
            int Col = X / _CellSize;
            int Row = Y / _CellSize;
            return !Logic_IsBorder(Col, Row);
        }

        /// <summary>
        /// 获得单元格上的塔
        /// </summary>
        /// <param name="Col">横坐标</param>
        /// <param name="Row">纵坐标</param>
        /// <returns>塔</returns>
        public Tower GetTower(int X, int Y)
        {
            int Col = X / _CellSize;
            int Row = Y / _CellSize;
            return FMap.Cells(Col, Row).Tower;
        }

        /// <summary>
        /// 设置单元格上的塔
        /// </summary>
        /// <param name="Col">横坐标</param>
        /// <param name="Row">纵坐标</param>
        /// <param name="aTower">塔</param>
        public void SetTower(int X, int Y, Tower aTower)
        {
            int Col = X / _CellSize;
            int Row = Y / _CellSize;
            if (aTower != null)
            {
                aTower.Position = new PointF(Col * _CellSize + _CellSize / 2, Row * _CellSize + _CellSize / 2);
            }
            FMap.Cells(Col, Row).Tower = aTower;
        }

        /// <summary>
        /// 创建怪物(地图1)
        /// </summary>
        /// <param name="Channel">出口</param>
        /// <param name="aMonster">怪物</param>
        public void SetMonster1(Monster aMonster)
        {
            aMonster.OriginalDirection = Direction.Right;
            aMonster.Position = new PointF(32, 96);          //初始小怪生成位置
            FMonsters[0].AddLast(aMonster);                 //马克！待会回来看
        }

        /// <summary>
        /// 创建怪物(地图2)
        /// </summary>
        /// <param name="Channel">出口</param>
        /// <param name="aMonster">怪物</param>
        public void SetMonster2(Monster aMonster)
        {
            aMonster.OriginalDirection = Direction.Right;
            aMonster.Position = new PointF(32, 64);          //初始小怪生成位置
            FMonsters[0].AddLast(aMonster);                 //马克！待会回来看
        }

        /// <summary>
        /// 创建怪物(地图3)
        /// </summary>
        /// <param name="Channel">出口</param>
        /// <param name="aMonster">怪物</param>
        public void SetMonster3(Monster aMonster)
        {
            aMonster.OriginalDirection = Direction.Left;
            aMonster.Position = new PointF(640, 96);          //初始小怪生成位置
            FMonsters[0].AddLast(aMonster);                 //马克！待会回来看
        }
        /// <summary>
        /// 添加剪辑
        /// </summary>
        /// <param name="aClip">剪辑</param>
        public void AddClip(Clip aClip)
        {
            FClipPainter.AddClip(aClip);
        }

        /// <summary>
        /// 屏幕宽度
        /// </summary>
        public int ScreenWidth
        {
            get
            {
                return _ScreenWidth;
            }
        }

        /// <summary>
        /// 屏幕高度
        /// </summary>
        public int ScreenHeight
        {
            get
            {
                return _ScreenHeight;
            }
        }

        /// <summary>
        /// 方块尺寸
        /// </summary>
        public int CellSize
        {
            get
            {
                return _CellSize;
            }
        }

        /// <summary>
        /// 图像缓冲区
        /// </summary>
        public Bitmap Buffer
        {
            get
            {
                return FBuffer;
            }
        }

        /// <summary>
        /// 画板
        /// </summary>
        public Graphics Painter
        {
            get
            {
                return FPainter;
            }
        }

        /// <summary>
        /// 现有怪物数量
        /// </summary>
        public int AliveMonsterCount
        {
            get
            {
                int Count = 0;
                foreach (LinkedList<Monster> List in FMonsters)
                {
                    Count += List.Count;
                }
                return Count;
            }
        }

        public event Delegate_MonsterKilled OnMonsterKilled;
        public event Delegate_MonsterPass OnMonsterPass;
    }
}
