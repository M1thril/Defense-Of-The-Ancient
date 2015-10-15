using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TowerDef.Forms
{
    public partial class Form_About : Form
    {
        public Form_About()
        {
            InitializeComponent();
        }

        private void Form_About_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form_About_Load(object sender, EventArgs e)
        {
            
        }

        private void Form_About_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Image about = Image.FromFile("..//..//images//About.png");
            g.DrawImage(about, 0, 0);
        }
        
    }
}