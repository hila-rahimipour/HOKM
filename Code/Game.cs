using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HOKM.Screens;
using System.Net.Sockets;

namespace HOKM.Code
{
    public class Game
    {

        public static EventWaitHandle waitHandle = new AutoResetEvent(false);

        public static void TurnAnim(GameScreen root, int first, int index, string friend, string enemy1, string enemy2)
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

        public static void ScreenBlink(GameScreen root, char color)
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
