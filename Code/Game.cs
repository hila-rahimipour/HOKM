using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HOKM.Code
{
    public class Game
    {

        public static EventWaitHandle waitHandle = new AutoResetEvent(false);

        public static void main(Form1 root)
        {
            root.UpdateStrong('C');

            root.ShowTurn(0, 1, "4D", "3C", "AS");
            waitHandle.WaitOne();
            ScreenBlink(root, 'g');
            root.UpdatePoints(1, 0);

            root.ShowTurn(0, 10, "5D", "AC", "JS");
            waitHandle.WaitOne();
            ScreenBlink(root, 'r');
            root.UpdatePoints(1, 1);

        }

        public static void ScreenBlink(Form1 root, char color)
        {
            if (color == 'g')
                root.BackColor = System.Drawing.Color.LightGreen;
            if (color == 'r')
                root.BackColor = System.Drawing.Color.OrangeRed;
            Thread.Sleep(100);
            root.BackColor = System.Drawing.Color.Empty;
        }
    }
}
