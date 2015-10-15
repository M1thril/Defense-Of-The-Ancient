using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TowerDef
{
    public abstract class MonsterBase : Monster
    {
        private float FSpeed = 0;
        private int FMoney = 0;
        public Image pic;
        protected override void Walk(Direction aDirection)
        {
            PointF p = Position;
            switch (aDirection)
            {
                case Direction.Left:
                    p.X = p.X - FSpeed;
                    pic = Image.FromFile("..//..//images//left.png");
                    break;
                case Direction.Right:
                    pic = Image.FromFile("..//..//images//right.png");
                    p.X = p.X + FSpeed;
                    break;
                case Direction.Up:
                    pic = Image.FromFile("..//..//images//up.png");
                    p.Y = p.Y - FSpeed;
                    break;
                case Direction.Down:
                    pic = Image.FromFile("..//..//images//down.png");
                    p.Y = p.Y + FSpeed;
                    break;
            }
            Position = p;
        }

        public MonsterBase(int aLife, int aDefense, float aSpeed, int aMoney)
            : base(aLife, aDefense, 24)
        {
            FSpeed = aSpeed;
            FMoney = aMoney;
        }

        public float Speed
        {
            get
            {
                return FSpeed;
            }
        }

        public int Money
        {
            get
            {
                return FMoney;
            }
        }
    }

    public class LeveledMonster : MonsterBase
    {
        private int FLevel;
        public LeveledMonster(int Level)
            : base(
                50 * Level,                     //生命值
                (Level / 25) * (Level / 25),    //防御力
                3,                 //速度      2 + Level / 50
                4 * Level                       //金钱
            )
        {
            FLevel = Level;
        }

        public override void Draw(Graphics Painter)
        {
            Painter.DrawImage(pic, Position.X - 44, Position.Y - 25);
        }
    }
}