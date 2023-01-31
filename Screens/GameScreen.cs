using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using HOKM.Code;


namespace HOKM.Screens
{
    public partial class GameScreen : UserControl
    {
        /// <summary>
        /// A class dedicated to the client-side GUI during the game.
        /// Written by Matan Weinman.
        /// </summary>

        // For hovering above the cards:
        int[] hoverCounters = new int[13];
        // A list of cards to grow (hovered upon)
        List<int> toGrow = new List<int>();

        // Arrays of all the cards: (PictureBoxes)
        PictureBox[] cards = null;
        PictureBox[] friend = null;
        PictureBox[] enemy1 = null;
        PictureBox[] enemy2 = null;
        PictureBox[][] players = null;

        // Points to which cards of each type will arrive:
        Point myPoint = new Point(380, 290);
        Point friendPoint = new Point(380, 170);
        Point enemy1Point = new Point(460, 230);
        Point enemy2Point = new Point(300, 230);

        // For moving the cards:
        PictureBox moving = null;
        int moveCount = 1;
        Point movingSrc = new Point(-1, -1);
        Point movingDst = new Point(-1, -1);

        List<PictureBox> toMove = new List<PictureBox>();
        List<PictureBox> toDiscard = new List<PictureBox>();


        public GameScreen()
        {
            InitializeComponent();

            cards = new PictureBox[] { card00, card01, card02, card03, card04, card05, card06, card07, card08, card09, card10, card11, card12 };
            friend = new PictureBox[] { friend00, friend01, friend02, friend03, friend04, friend05, friend06, friend07, friend08, friend09, friend10, friend11, friend12 };
            enemy1 = new PictureBox[] { enemy100, enemy101, enemy102, enemy103, enemy104, enemy105, enemy106, enemy107, enemy108, enemy109, enemy110, enemy111, enemy112 };
            enemy2 = new PictureBox[] { enemy200, enemy201, enemy202, enemy203, enemy204, enemy205, enemy206, enemy207, enemy208, enemy209, enemy210, enemy211, enemy212 };
            players = new PictureBox[][] { cards, enemy2, friend, enemy1 };
        }

        private void GameScreen_Load(object sender, EventArgs e)
        {
            HoverTimer.Start();
            MoveTimer.Start();
        }

        public void ShowTurn(int first, int myAction, string friendAction, string enemy1Action, string enemy2Action)
        {
            /// Animating a turn in the game.
            /// first- Who plays first (relative to me).
            /// myAction- index of my played card.
            /// friendAction, enemy1Action, enemy2Action- names of cards played by the other players.

            int nextCard = 0;
            while (players[1][nextCard] == null)
                nextCard++;
            Console.WriteLine(nextCard);

            string[] actions = new string[] { "", enemy2Action, friendAction, enemy1Action };

            for (int i = first; i < first + 4; i++)
            {
                // Not me:
                if (players[i % 4] != cards)
                {
                    Console.WriteLine(actions[i % 4]);
                    // Revealing the card:
                    players[i % 4][nextCard].BackgroundImage = (Bitmap)Properties.Resources.ResourceManager.GetObject(actions[i % 4]);
                    toMove.Add(players[i % 4][nextCard]);
                    players[i % 4][nextCard] = null;
                }
                else
                {
                    toMove.Add(cards[myAction]);
                    cards[myAction] = null;
                }
            }
        }

        public void UpdatePoints(int myTeam, int enemyTeam)
        {
            /// Updating the points, as received from the server.

            label2.Text = "SCORE:\n" +
                "Your team: " + myTeam + "\n" +
                "Enemy team: " + enemyTeam;
        }

        public void UpdateStrong(char strong)
        {
            /// Receives the strong suit, and updates it.

            switch (strong)
            {
                case 'C':
                    label1.Text = "Strong: ♣";
                    break;
                case 'S':
                    label1.Text = "Strong: ♠";
                    break;
                case 'H':
                    label1.Text = "Strong: ♥";
                    break;
                case 'D':
                    label1.Text = "Strong: ♦";
                    break;
            }
        }

        private void CardGrow(object sender, EventArgs e)
        {
            /// Makes a card grow in size when hovered upon.

            for (int i = 0; i < 13; i++)
            {
                if (toGrow.Contains(i))
                {
                    if (hoverCounters[i] <= 5)
                    {
                        hoverCounters[i]++;
                        cards[i].Size = new Size(cards[i].Size.Width + 3, cards[i].Size.Height + 3);
                    }
                    else
                        toGrow.Remove(i);
                }
            }
        }

        private void cardHover(object sender, EventArgs e)
        {
            /// Delegate- of "MouseEnter". Grows the card.
            PictureBox card = (PictureBox)sender;
            string name = card.Name;
            toGrow.Add(int.Parse(name.Substring(name.Length - 2, 2)));
        }

        private void cardLeave(object sender, EventArgs e)
        {
            /// Delegate- of "MouseLeave". Shrinks the card.
            PictureBox card = (PictureBox)sender;
            string name = card.Name;
            int num = int.Parse(name.Substring(name.Length - 2, 2));
            toGrow.Remove(num);
            cards[num].Size = new Size(cards[num].Size.Width - 3 * hoverCounters[num], cards[num].Size.Height - 3 * hoverCounters[num]);
            hoverCounters[num] = 0;
        }

        private void MoveAnimation(object sender, EventArgs e)
        {
            /// Animates a card when moving to its destination.

            // A card is currently on the move:
            if (moving != null)
            {
                if (moveCount == 25)
                {
                    moveCount = 1;
                    toDiscard.Add(moving);
                    moving = null;
                    return;
                }
                moving.Location = new Point(movingSrc.X + (movingDst.X - movingSrc.X) * moveCount / 25,
                    movingSrc.Y + (movingDst.Y - movingSrc.Y) * moveCount / 25);
                moveCount++;
            }
            // There are new cards to be moved:
            else if (toMove.Count != 0)
            {
                PictureBox card = toMove.ElementAt(0);
                toMove.RemoveAt(0);
                movingSrc = card.Location;
                moving = card;
                if (card.Name.StartsWith("card"))
                    movingDst = myPoint;
                else if (card.Name.StartsWith("friend"))
                    movingDst = friendPoint;
                else if (card.Name.StartsWith("enemy1"))
                    movingDst = enemy1Point;
                else if (card.Name.StartsWith("enemy2"))
                    movingDst = enemy2Point;
            }
            // Deletes all cards that were moved:
            else if (toDiscard.Count != 0)
            {
                Thread.Sleep(1000);
                foreach (var card in toDiscard)
                    card.Dispose();
                toDiscard.Clear();
                // Signals the 'Game' Thread to keep playing.
                Program.waitHandle.Set();
            }
        }
    }
}
