using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HOKM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Image img = pictureBox9.BackgroundImage;
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox9.BackgroundImage = img;

            img = pictureBox10.BackgroundImage;
            img.RotateFlip(RotateFlipType.Rotate270FlipNone);
            pictureBox10.BackgroundImage = img;

            img = pictureBox11.BackgroundImage;
            img.RotateFlip(RotateFlipType.Rotate270FlipNone);
            pictureBox11.BackgroundImage = img;

            img = pictureBox12.BackgroundImage;
            img.RotateFlip(RotateFlipType.Rotate270FlipNone);
            pictureBox12.BackgroundImage = img;

            img = pictureBox13.BackgroundImage;
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox13.BackgroundImage = img;

            img = pictureBox14.BackgroundImage;
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox14.BackgroundImage = img;

            img = pictureBox15.BackgroundImage;
            img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            pictureBox15.BackgroundImage = img;

            img = pictureBox16.BackgroundImage;
            img.RotateFlip(RotateFlipType.Rotate270FlipNone);
            pictureBox16.BackgroundImage = img;

        }
    }
}
