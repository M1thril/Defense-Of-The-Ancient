using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Threading;
using System.Media;

namespace TowerDef
{
    public partial class Form_Main : Form
    {
        enum GameStatus
        {
            Logo,
            Logo2,
            Ready,
            Playing,
            Finished
        }

        private Bitmap FBuffer = null;
        private Graphics FPainter = null;
        private static Brush FBackground = new SolidBrush(Color.FromArgb(0, 0, 0));
        private Game FGame = new Game();
        private Graphics FScreen = null;
        private static Font FMoneyFont = new Font("微软雅黑", 12, FontStyle.Bold, GraphicsUnit.Pixel);
        private static Font FMoneyLabelFont = new Font("微软雅黑", 12, GraphicsUnit.Pixel);
        private static Font FMenuFont = new Font("微软雅黑", 20,FontStyle.Bold, GraphicsUnit.Pixel);
        private static Pen FFocusPen = new Pen(Color.FromArgb(255, 0, 128));
        private static Pen FHoverPen = new Pen(Color.FromArgb(255, 128, 0));
        private static Brush FYesBrush = new SolidBrush(Color.FromArgb(0, 255, 0));
        private static Brush FNoBrush = new SolidBrush(Color.FromArgb(255, 0, 0));
        private static Brush FSellBrush = new SolidBrush(Color.FromArgb(255, 128, 0));
        private static Pen FBuildPen = new Pen(Color.FromArgb(0, 0, 0),1);                                //攻击范围描边
        private static Brush FBuildBrush = new SolidBrush(Color.FromArgb(60, 50, 255, 255));              //攻击范围填充
        private int FMoney = 0;
        private int FKilled = 0;
        //private int FChannel = 0;
        private int FLife = 0;
        private int FFactoryIndex = 0;
        private int FFocusItem = 0;
        private int FHoverItem = 0;
        private int FBornCount = 0;
        private Point FCursor = new Point(0, 0);
        private TowerFactory[] FFactories = new TowerFactory[]{
            MultiTower.Factory,
            SpeedTower.Factory,
            RangeTower.Factory,
            TrapTower.Factory
        };
        private string[] FTextMenu = new string[] { "卖塔", "重来", "暂停", "退出","关于" };
        private GameStatus FStatus = GameStatus.Logo;       //当前游戏状态
        private Game_Clip Glip = Game_Clip.Zero_Clip;       //当前关卡
        public int LogoStatus = 0;
        int st = 0;                     //logo音乐播放次数

        Image logo = Image.FromFile("..//..//images//logo.png");
        Image logo2 = Image.FromFile("..//..//images//logo2.png");

        SoundPlayer ss = new SoundPlayer();

        Audio au = new Audio();
        //SoundPlayer Yes = new SoundPlayer();
        //SoundPlayer Menu = new SoundPlayer();

        private void NewGame()
        {
            FMoney = 3000;
            FKilled = 0;
            //FChannel = 2;
            FLife = 10;
            FGame.Clear();
            RefreshCaption();
            FGame.PutMessage("婆娑树海");
        }
        private void ReStart() 
        {
            FMoney = 3000;
            FKilled = 0;
            //FChannel = 2;
            FLife = 10;
            FStatus = GameStatus.Ready;
            FGame.Clear();
            RefreshCaption();
            FGame.PutMessage("     请建造防御塔\r\n   阻挡敌人的进攻！");
        }
        private void FinishGame(bool Win)
        {
            if (Win)
            {
                FStatus = GameStatus.Finished;
                FGame.PutMessage("你赢了");
            }
            else
            {
                FStatus = GameStatus.Finished;
                FGame.PutMessage("你死了");
            }
        }

        private void PauseGame()
        {
            tmGame.Enabled = false;
            tmEnemy.Enabled = false;
            //tmTemp.Enabled = true;
            FTextMenu[2] = "继续";
            RefreshScreen();
        }

        private void ContinueGame()
        {
            tmGame.Enabled = true;
            tmEnemy.Enabled = true;
            //tmTemp.Enabled = false;
            FTextMenu[2] = "暂停";
            RefreshScreen();
        }

        private void AddMoneyClip(PointF Pos, int Money)
        {
            string Text = Money > 0 ? "+" + Money.ToString() : Money.ToString();
            Color TextColor = Money > 0 ? Color.Orange : Color.Red;
            FGame.AddClip(new TextClip(Form_Main.FMoneyFont, TextColor, Text, Pos, new PointF(0, -0.4f), true, 50));
        }

        private void RefreshCaption()
        {
            Text = "遗迹守卫战     " +
                "[金钱:" + FMoney.ToString() + "]" +
                "[生命:" + FLife.ToString() + "]" +
                "[杀敌:" + FKilled.ToString() + "]";
        }

        private void Draw(PointF Pos, Brush TextBrush, string Text, Font TextFont)      //绘制浮动数字
        {
            SizeF Size = FPainter.MeasureString(Text, TextFont);
            Pos.X -= Size.Width / 2;
            Pos.Y -= Size.Height / 2;
            FPainter.DrawString(Text, TextFont, TextBrush, Pos);
        }

        private void Draw(PointF Pos, int Money)
        {
            if (Money < int.MaxValue)
            {
                Draw(Pos, (Money <= FMoney ? FYesBrush : FNoBrush), Money.ToString(), FMoneyLabelFont);
            }
        }

        private void Draw(PointF Pos, string Text)
        {
            Draw(Pos, FSellBrush, Text.ToString(), FMenuFont);
        }

        private void Draw(PointF Pos, TowerFactory Factory)
        {
            Factory.DrawTower(Pos, FPainter);
        }

        private int GetSell(int X, int Y)                   //获取所在格塔可买金钱值
        {
            if (FGame.IsEditableCell(FCursor.X, FCursor.Y))
            {
                Tower aTower = FGame.GetTower(FCursor.X, FCursor.Y);
                if (aTower != null)
                {
                    for (int i = 0; i < FFactories.Length; i++)
                    {
                        if (FFactories[i].IsMine(aTower))
                        {
                            ShootingTower sTower = (ShootingTower)aTower;
                            int Money = FFactories[i].GetMoney();
                            for (int j = 0; j < sTower.Level; j++)
                            {
                                Money += FFactories[i].GetLevelUpMoney(j);
                            }
                            return Money * 3 / 5;
                        }
                    }
                }
            }
            return -1;
        }

        private void DrawToolBox()
        {
            int CellSize = FGame.CellSize;
            int HalfSize = CellSize / 2;
            //bool MouseOnUpdating = false;
            int MouseTowerIndex = -1;
            int MouseTowerLevel = -1;
            if (FGame.IsEditableCell(FCursor.X, FCursor.Y))
            {
                Tower aTower = FGame.GetTower(FCursor.X, FCursor.Y);
                if (aTower != null)
                {
                    for (int i = 0; i < FFactories.Length; i++)
                    {
                        if (FFactories[i].IsMine(aTower))
                        {
                            MouseTowerIndex = i;
                            break;
                        }
                    }
                    if (MouseTowerIndex >= 0)
                    {
                        ShootingTower sTower = (ShootingTower)aTower;
                        if (sTower.Level < sTower.MaxLevel)
                        {
                            MouseTowerLevel = sTower.Level;
                        }
                    }
                }
            }
            ////////////////绘制菜单背景//////////////////////////////////////
            Image menu = Image.FromFile("..//..//images//Menu.png");
            Rectangle rect=new Rectangle(640,0,191,480);
            ImageAttributes imA=new ImageAttributes();
            FPainter.DrawImage(menu,rect,0,0,191,480,GraphicsUnit.Pixel,imA);
            ///////////////循环绘制等级表示（0,1,2,3,4）///////////////////////
            for (int i = 0; i < 6; i++)
            {
                Draw(new PointF(FGame.ScreenWidth + HalfSize + CellSize * (i + 1), HalfSize), i.ToString());
            }
            ////////////////遍历绘制各种塔的选项ITEM////////////////////
            for (int i = 0; i < FFactories.Length; i++)
            {
                TowerFactory Factory = FFactories[i];
                /////////////塔的图标///////////////////////
                Draw(new PointF(FGame.ScreenWidth + HalfSize, HalfSize + CellSize * (i + 1)), Factory);
                ////////////初始金钱////////////////////////
                Draw(new PointF(FGame.ScreenWidth + HalfSize + CellSize, HalfSize + CellSize * (i + 1)), Factory.GetMoney());

                for (int j = 0; j < Factory.GetMaxLevel(); j++)
                {
                    /////////数字分割线/////////
                    //FPainter.DrawRectangle(FFocusPen, FGame.ScreenWidth + CellSize * (j + 1), CellSize * (i + 1), CellSize, CellSize);
                    ////////各级升级金钱///////
                    Draw(new PointF(FGame.ScreenWidth + HalfSize + CellSize * (j + 2), HalfSize + CellSize * (i + 1)), Factory.GetLevelUpMoney(j));
                }
            }
            //////////////////卖塔事件->菜单文字动态变化////////////////////
            if (GetSell(FCursor.X, FCursor.Y) == -1 || FFocusItem != FFactories.Length)
            {
                FTextMenu[0] = "卖塔";
            }
            else
            {
                FTextMenu[0] = "可卖" + GetSell(FCursor.X, FCursor.Y).ToString()+"块";
            }
            ////////////////绘制菜单选项ITEM//////////////////////////
            for (int i = 0; i < FTextMenu.Length; i++)
            {
                Draw(new PointF(FGame.ScreenWidth + (FBuffer.Width - FGame.ScreenWidth) / 2, (FFactories.Length + i + 1) * CellSize + HalfSize), FTextMenu[i]);
            }
            ////////////选中菜单项绘制方框/////////////
            FPainter.DrawRectangle(FFocusPen, FGame.ScreenWidth, CellSize * (FFocusItem + 1), FBuffer.Width - FGame.ScreenWidth - 1, CellSize);
            ////////////鼠标在选项上方绘制方框/////////
            FPainter.DrawRectangle(FHoverPen, FGame.ScreenWidth, CellSize * (FHoverItem + 1), FBuffer.Width - FGame.ScreenWidth - 1, CellSize);
        }
        /// <summary>
        /// 刷新游戏画面
        /// </summary>
        private void RefreshScreen()
        {
            int CellSize=FGame.CellSize;
            int HalfSize=CellSize/2;
            //////绘制游戏画面//////
            FPainter.DrawImage(FGame.Buffer, 0, 0);
            ////绘制炮塔攻击范围///
            if (FGame.IsEditableCell(FCursor.X, FCursor.Y))
            {
                int Col = FCursor.X / CellSize;
                int Row = FCursor.Y / CellSize;
                if (FFocusItem == FFactories.Length)               //鼠标焦点所在塔序号==菜单选中塔序号
                {
                    Tower aTower = FGame.GetTower(FCursor.X, FCursor.Y);
                    if (aTower != null)
                    {
                        Draw(new PointF(HalfSize + Col * CellSize, HalfSize + Row * CellSize), "¥");
                    }
                }
                else
                {
                    Tower aTower = FGame.GetTower(FCursor.X, FCursor.Y);
                    if (aTower != null)
                    {
                        Draw(new PointF(HalfSize + Col * CellSize, HalfSize + Row * CellSize), "↑");
                    }
                    else
                    {
                        float cenX = HalfSize + Col * CellSize;
                        float cenY = HalfSize + Row * CellSize;
                        float r = FFactories[FFactoryIndex].GetRange();         //获取塔射程范围
                        FPainter.FillEllipse(FBuildBrush, cenX - r, cenY - r, r * 2, r * 2);
                        FPainter.DrawEllipse(FBuildPen, cenX - r, cenY - r, r * 2, r * 2);
                        FPainter.DrawRectangle(FBuildPen, cenX - HalfSize, cenY - HalfSize, CellSize, CellSize);
                    }
                }
            }
            ////选项栏////
            FPainter.FillRectangle(FBackground, FGame.ScreenWidth, 0, FBuffer.Width - FGame.ScreenWidth, FBuffer.Height);
            DrawToolBox();
            ////将缓冲区图像绘制ww
            FScreen.DrawImage(FBuffer, 0, 0);               
        }

        private void OnMonsterPass(Game aGame, Monster aMonster)        //怪物通过事件处理
        {
            FLife--;
            if (FLife == 0)
            {
                FinishGame(false);
                FBornCount = 0;
            }
            RefreshCaption();
        }

        private void OnMonsterKilled(Game aGame, Monster aMonster)      //击杀怪物事件处理
        {
            MonsterBase m = (MonsterBase)aMonster;
            AddMoneyClip(aMonster.Position, m.Money);
            FMoney += m.Money;
            FKilled += 1;
            RefreshCaption();
        }
        //TODO
        public Form_Main()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            InitializeComponent();
            FBuffer = new Bitmap(FGame.ScreenWidth + FGame.CellSize * 6, FGame.ScreenHeight);//定义图像缓冲区长宽
            ClientSize = new Size(FBuffer.Width, FBuffer.Height);               //定义窗口实际工作区长宽
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            FPainter = Graphics.FromImage(FBuffer);
            FScreen = Graphics.FromHwnd(this.Handle);
            FGame.OnMonsterPass += new Game.Delegate_MonsterPass(OnMonsterPass);
            FGame.OnMonsterKilled += new Game.Delegate_MonsterKilled(OnMonsterKilled);
            ss.SoundLocation = "..//..//Sound//Freljord.wav";
            ss.Load();
            //Yes.SoundLocation = "..//..//Sound//Yes.wav";
            //Yes.Load();
            //Menu.SoundLocation = "..//..//Sound//Menu.wav";
            //Menu.Load();
            NewGame();
        }

        private void FormMain_Paint(object sender, PaintEventArgs e)
        {
            RefreshScreen();
        }

        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            int X = e.X;
            int Y = e.Y;
            int Item = Y / FGame.CellSize - 1;
            switch (FStatus)
            {
                case GameStatus.Logo:
                    break;
                case GameStatus.Logo2:
                    break;
                case GameStatus.Ready:
                    {
                        
                        if (X < FGame.ScreenWidth)
                        {
                            if (tmGame.Enabled)
                            {
                                if (X >= 0 && X < FGame.ScreenWidth && Y >= 0 && Y < FGame.ScreenHeight)
                                {
                                    FCursor = new Point(X, Y);
                                }
                                if (FGame.IsEditableCell(X, Y))
                                {
                                    ShootingTower aTower = (ShootingTower)FGame.GetTower(X, Y);
                                    if (FFocusItem == FFactories.Length)
                                    {
                                        int Money = GetSell(X, Y);          //获取该格塔的价值
                                        if (Money == -1)
                                        {
                                            FGame.PutMessage("未选中");
                                        }
                                        else
                                        {
                                            FGame.SetTower(X, Y, null);
                                            FMoney += Money;
                                            RefreshCaption();
                                        }
                                    }
                                    else
                                    {
                                        if (aTower != null)
                                        {
                                            if (aTower.Updating)
                                            {
                                                FGame.PutMessage("升级中...");
                                            }
                                            else if (aTower.Level == aTower.MaxLevel)
                                            {
                                                FGame.PutMessage("顶级！");
                                            }
                                            else if (aTower.LevelUpMoney() > FMoney)
                                            {
                                                FGame.PutMessage("金钱不足");
                                            }
                                            else
                                            {
                                                AddMoneyClip(new PointF(X, Y), -aTower.LevelUpMoney());
                                                aTower.LevelUp(ref FMoney);
                                                RefreshCaption();
                                            }
                                        }
                                        else
                                        {
                                            if (FFactories[FFactoryIndex].GetMoney() > FMoney)
                                            {
                                                FGame.PutMessage("金钱不足");
                                            }
                                            else
                                            {
                                                AddMoneyClip(new PointF(X, Y), -FFactories[FFactoryIndex].GetMoney());
                                                FGame.SetTower(X, Y, FFactories[FFactoryIndex].GetTower(ref FMoney, FGame));
                                                RefreshCaption();
                                                au.Close();
                                                au.Play("..//..//Sound//Yes.wav");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FGame.PutMessage("违章建筑！");
                                }
                            }
                        }
                        else
                        {
                            if (Item >= 0 && Item < FFactories.Length)
                            {
                                FFocusItem = Item;
                                FFactoryIndex = Item;
                            }
                            ///////////////////菜单选项执行/////////////////
                            else if (Item == FFactories.Length)             //卖塔
                            {
                                au.Close();
                                au.Play("..//..//sound//menu.wav");
                                FFocusItem = Item;
                            }
                            else if (Item == FFactories.Length + 1)         //重来
                            {
                                au.Close();
                                au.Play("..//..//sound//menu.wav");
                                //ContinueGame();
                                //NewGame();
                                ReStart();
                            }
                            else if (Item == FFactories.Length + 2)         //暂停or继续
                            {
                                au.Close();
                                au.Play("..//..//sound//menu.wav");
                                if (tmGame.Enabled)
                                {
                                    PauseGame();
                                }
                                else
                                {
                                    ContinueGame();
                                }
                            }
                            else if (Item == FFactories.Length + 3)         //退出
                            {
                                Application.Exit();
                            }
                            else if (Item == FFactories.Length + 4)         //关于
                            {
                                au.Close();
                                au.Play("..//..//sound//menu.wav");
                                PauseGame();
                                (new TowerDef.Forms.Form_About()).ShowDialog();
                                ContinueGame();
                            }
                        }
                    }
                    break;
                case GameStatus.Playing:
                    {
                        if (X < FGame.ScreenWidth)
                        {
                            if (tmGame.Enabled)
                            {
                                if (X >= 0 && X < FGame.ScreenWidth && Y >= 0 && Y < FGame.ScreenHeight)
                                {
                                    FCursor = new Point(X, Y);
                                }
                                if (FGame.IsEditableCell(X, Y))
                                {
                                    ShootingTower aTower = (ShootingTower)FGame.GetTower(X, Y);
                                    if (FFocusItem == FFactories.Length)
                                    {
                                        int Money = GetSell(X, Y);          //获取该格塔的价值
                                        if (Money == -1)
                                        {
                                            FGame.PutMessage("未选中");
                                        }
                                        else
                                        {
                                            FGame.SetTower(X, Y, null);
                                            FMoney += Money;
                                            RefreshCaption();
                                        }
                                    }
                                    else
                                    {
                                        if (aTower != null)
                                        {
                                            if (aTower.Updating)
                                            {
                                                FGame.PutMessage("升级中...");
                                            }
                                            else if (aTower.Level == aTower.MaxLevel)
                                            {
                                                FGame.PutMessage("顶级！");
                                            }
                                            else if (aTower.LevelUpMoney() > FMoney)
                                            {
                                                FGame.PutMessage("金钱不足");
                                            }
                                            else
                                            {
                                                AddMoneyClip(new PointF(X, Y), -aTower.LevelUpMoney());
                                                aTower.LevelUp(ref FMoney);
                                                RefreshCaption();
                                            }
                                        }
                                        else
                                        {
                                            if (FFactories[FFactoryIndex].GetMoney() > FMoney)
                                            {
                                                FGame.PutMessage("金钱不足");
                                            }
                                            else
                                            {
                                                AddMoneyClip(new PointF(X, Y), -FFactories[FFactoryIndex].GetMoney());
                                                FGame.SetTower(X, Y, FFactories[FFactoryIndex].GetTower(ref FMoney, FGame));
                                                au.Close();
                                                au.Play("..//..//sound//Yes.wav");
                                                RefreshCaption();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FGame.PutMessage("违章建筑！");
                                }
                            }
                        }
                        else
                        {
                            if (Item >= 0 && Item < FFactories.Length)
                            {
                                FFocusItem = Item;
                                FFactoryIndex = Item;
                            }
                            ///////////////////菜单选项执行/////////////////
                            else if (Item == FFactories.Length)             //卖塔
                            {
                                au.Close();
                                au.Play("..//..//sound//menu.mp3");
                                FFocusItem = Item;
                            }
                            else if (Item == FFactories.Length + 1)         //重来
                            {
                                au.Close();
                                au.Play("..//..//sound//menu.mp3");
                                ContinueGame();
                                ReStart();
                            }
                            else if (Item == FFactories.Length + 2)         //暂停or继续
                            {
                                au.Close();
                                au.Play("..//..//sound//menu.mp3");
                                if (tmGame.Enabled)
                                {
                                    PauseGame();
                                }
                                else
                                {
                                    ContinueGame();
                                }
                            }
                            else if (Item == FFactories.Length + 3)         //退出
                            {
                                Application.Exit();
                            }
                            else if (Item == FFactories.Length + 4)         //关于
                            {
                                au.Close();
                                au.Play("..//..//sound//menu.mp3");
                                PauseGame();
                                (new TowerDef.Forms.Form_About()).ShowDialog();
                                ContinueGame();
                            }
                        }
                    }
                    break;
                case GameStatus.Finished:
                    {
                            
                            if (Item >= 0 && Item < FFactories.Length)
                            {
                                FFocusItem = Item;
                                FFactoryIndex = Item;
                            }
                            ///////////////////菜单选项执行/////////////////
                            else if (Item == FFactories.Length)             //卖塔
                            {
                                //au.Close();
                                //au.Play("..//..//sound//menu.mp3");
                                FFocusItem = Item;
                            }
                            else if (Item == FFactories.Length + 1)         //重来
                            {
                                //au.Close();
                                //au.Play("..//..//sound//menu.mp3");
                                ContinueGame();
                                ReStart();
                            }
                            else if (Item == FFactories.Length + 2)         //暂停or继续
                            {
                                //au.Close();
                                //au.Play("..//..//sound//menu.mp3");
                                if (tmGame.Enabled)
                                {
                                    PauseGame();
                                }
                                else
                                {
                                    ContinueGame();
                                }
                            }
                            else if (Item == FFactories.Length + 3)         //退出
                            {
                                Application.Exit();
                            }
                            else if (Item == FFactories.Length + 4)         //关于
                            {
                                //au.Close();
                                //au.Play("..//..//sound//menu.mp3");
                                PauseGame();
                                (new TowerDef.Forms.Form_About()).ShowDialog();
                                ContinueGame();
                            }
                    }
                    break;
            }
        }
        
        private void tmGame_Tick(object sender, EventArgs e)            //游戏计时器运转方法
        {
            Graphics g = this.CreateGraphics();
            if (FStatus == GameStatus.Logo) {                           //Logo状态
                
                if (LogoStatus == 0)
                {
                    g.DrawImage(logo, 0, 0);
                    Thread.Sleep(3000);
                    LogoStatus++;
                    return;
                }
                else
                {
                    FStatus = GameStatus.Logo2;
                    return;
                }
            }
            if (FStatus == GameStatus.Logo2) {
                if(st==0)
                    StatusChange();
                st++;
                g.DrawImage(logo2, 0, 0);
                return;
            }
            if (FStatus == GameStatus.Ready) {
                st = 0;
                if (st == 0)
                    StatusChange();
                st++;
            }
            if (FStatus == GameStatus.Playing)                          //游戏状态
            {
                FGame.Action(Glip);
            }
            FGame.Run();
            FGame.Draw(Glip);
            RefreshScreen();
            System.Threading.Thread.Sleep(10);
        }
        private void tmEnemy_Tick(object sender, EventArgs e)           //怪物计时器运转方法
        {
            if (FStatus == GameStatus.Playing)
            {
                switch (Glip) 
                { 
                    case Game_Clip.Zero_Clip:
                        if (FBornCount == 20)
                        {
                            if (FGame.AliveMonsterCount == 0)
                            {
                                //FinishGame(true);
                                ReStart();
                                FBornCount = 0;
                                Thread.Sleep(1000);
                                FGame.PutMessage("积寒之地");
                                Glip = Game_Clip.One_Clip;
                            }
                        }
                        else
                        {
                            FGame.SetMonster1(new LeveledMonster(1));/*生成小怪1*/
                            FBornCount++;
                            RefreshCaption();
                            System.Threading.Thread.Sleep(10);
                        }
                    break;
                    case Game_Clip.One_Clip:
                    if (FBornCount == 30)
                    {
                        if (FGame.AliveMonsterCount == 0)
                        {
                            //FinishGame(true);
                            ReStart();
                            FBornCount = 0;
                            Thread.Sleep(1000);
                            FGame.PutMessage("深海墓地");
                            Glip = Game_Clip.Two_Clip;
                        }
                    }
                    else
                    {
                        FGame.SetMonster2(new LeveledMonster(1));/*生成小怪2*/
                        FBornCount++;
                        RefreshCaption();
                        System.Threading.Thread.Sleep(10);
                    }
                    break;
                    case Game_Clip.Two_Clip:
                        if (FBornCount == 40)
                        {
                            if (FGame.AliveMonsterCount == 0)
                            {
                                FinishGame(true);
                            }
                        }
                        else
                        {
                            FGame.SetMonster3(new LeveledMonster(1));/*生成小怪3*/
                            FBornCount++;
                            RefreshCaption();
                            System.Threading.Thread.Sleep(10);
                        }
                    break;
                }
            }
        }
        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                switch (FStatus)
                {
                    case GameStatus.Logo:
                    break;
                    case GameStatus.Logo2:
                    FStatus = GameStatus.Ready;
                    break;
                    case GameStatus.Ready:
                        FStatus = GameStatus.Playing;
                        FGame.PutMessage("开始");
                        break;
                    case GameStatus.Finished:
                        NewGame();
                        break;
                }
            }
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X >= 0 && e.Y >= 0 && e.Y < FGame.ScreenHeight)
            {
                if (e.X < FGame.ScreenWidth)
                {
                    FCursor = new Point(e.X, e.Y);
                }
                else
                {
                    int Item = e.Y / FGame.CellSize - 1;
                    if (Item >= 0 && Item < FFactories.Length+FTextMenu.Length)
                    {
                        FHoverItem = Item;
                    }
                }
            }
        }
        
        public void StatusChange() {
            switch (FStatus)
            {
                case GameStatus.Logo2:
                    Play_Logo_Music();
                    break;
                case GameStatus.Ready:
                    ss.Stop();
                    break;
            }
        }
        public void Play_Logo_Music() 
        {
            ss.Play();
        }
    }
}
