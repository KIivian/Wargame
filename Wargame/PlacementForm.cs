using System;
using System.Drawing;
using System.Windows.Forms;

namespace Wargame
{
    public partial class PlacementForm : Form
    {
        private MainForm Main;
        private Label InfoLabel;
        private Label MoneyLabel;
        private Point[] ShipIcoCenters;

        public int Money { get; private set; }

        private int? MousePos;
        private int Indent;
        public Ship SelectedShip { get; set; }

        public PlacementForm(MainForm mainForm)
        {
            Text = "Ship Placement";
            DoubleBuffered = true;
            Indent = 100;
            Main = mainForm;
            ShipIcoCenters = new Point[9];
            StartPosition = FormStartPosition.Manual;
            SetPosition();
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(255, 57, 82, 89);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitInfoLabel();
            InitMoneyLabel();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            CheckMouse();
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (MousePos.HasValue)
            {
                switch (MousePos)
                {
                    case 0:
                        SelectedShip = new Battleship(0);
                        break;
                    case 1:
                        SelectedShip = new Carrier(0);
                        break;
                    case 2:
                        SelectedShip = new HeavyCruiser(0);
                        break;
                    case 3:
                        SelectedShip = new Cruiser(0);
                        break;
                    case 4:
                        SelectedShip = new Fregate(0);
                        break;
                    case 5:
                        SelectedShip = new Gunship(0);
                        break;
                    case 6:
                        SelectedShip = new Destroyer(0);
                        break;
                    case 7:
                        SelectedShip = new Freighter(0);
                        break;
                    case 8:
                        SelectedShip = new Transport(0);
                        break;
                }
                SetInfoLabelText();
            }
            Invalidate();
        }
        private void CheckMouse()
        {
            var mousePos = PointToClient(MousePosition);
            var result = new int?();
            var minDistance = Main.HexRadius + 1;

            for (var i = 0; i < ShipIcoCenters.Length; i++)
            {
                var distance = Main.GetDistance(mousePos, ShipIcoCenters[i]);
                if (distance < minDistance)
                {
                    minDistance = (float)distance;
                    result = i;
                }
            }
            MousePos = (minDistance > Main.HexRadius) ? null : result;
        }

        private void SetIcoPositions()
        {
            for (var i = 0; i < ShipIcoCenters.Length; i++)
            {
                ShipIcoCenters[i].X = ((i % 3) * 3 + 2) * (int)Main.HalfHexWidth;
                ShipIcoCenters[i].Y = (int)((i / 3 * 2.5 + 1) * Main.HexRadius) + Indent;

            }
        }
        public void SetPosition()
        {
            Left = Main.Right - 5;
            Top = Main.Top;
            Size = new Size(10 * (int)Main.HalfHexWidth, Main.Height - 8);
            SetIcoPositions();
            Invalidate();
        }

        private void InitMoneyLabel()
        {
            MoneyLabel = new Label
            {
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft Sans Serif", 12),
                Size = new Size(this.Size.Width, 100),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(0, 0, 0, 0),
                ForeColor = Color.White
            };
            SetMoneyLabelText();
            Controls.Add(MoneyLabel);

        }
        private void InitInfoLabel()
        {
            InfoLabel = new Label
            {
                Size = new Size(Width - (2 * (int)Main.HalfHexWidth), Height - (int)(ShipIcoCenters[^1].Y - Main.HexRadius)),
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.FromArgb(0, 0, 0, 0),
                ForeColor = Color.White,
                Location = new Point((int)Main.HalfHexWidth, (int)(ShipIcoCenters[^1].Y + Main.HexRadius))
            };
            Controls.Add(InfoLabel);
        }

        private void SetInfoLabelText()
        {
            if (SelectedShip == null)
            {
                InfoLabel.Text = null;
                return;
            }
            switch (SelectedShip)
            {
                case Battleship _:
                    InfoLabel.Text = "Type: Battleship\n";
                    break;
                case Carrier _:
                    InfoLabel.Text = "Type: Carrier\n";
                    break;
                case HeavyCruiser _:
                    InfoLabel.Text = "Type: Heavy Cruiser\n";
                    break;
                case Cruiser _:
                    InfoLabel.Text = "Type: Cruiser\n";
                    break;
                case Fregate _:
                    InfoLabel.Text = "Type: Fregate\n";
                    break;
                case Gunship _:
                    InfoLabel.Text = "Type: Gunship\n";
                    break;
                case Destroyer _:
                    InfoLabel.Text = "Type: Destroyer\n";
                    break;
                case Freighter _:
                    InfoLabel.Text = "Type: Freighter\n";
                    break;
                case Transport _:
                    InfoLabel.Text = "Type: Transport\n";
                    break;
                case Fighter _:
                    InfoLabel.Text = "Type: Fighter\n";
                    break;
                case DoubleFighter _:
                    InfoLabel.Text = "Type: DoubleFighter\n";
                    break;

            }
            InfoLabel.Text +=
               "Price: " + SelectedShip.Price + "\n\n" +
               "Shields: " + SelectedShip.Shields + "\n" +
               "Speed: " + SelectedShip.Speed + "\n" +
               "Evasion: " + SelectedShip.Evasion + "\n" +
               "Cannons: ";

            if (SelectedShip.Armament.Length > 0)
            {
                foreach (var gun in SelectedShip.Armament)
                {
                    InfoLabel.Text += "d" + gun.Power + " r" + gun.Range + "; ";
                }
            }
            else
            {
                InfoLabel.Text += "None";
            }
            InfoLabel.Text += "\n\n";

            switch (SelectedShip)
            {
                case Battleship _:
                    InfoLabel.Text += "Очень большой корабль с мощным вооружением и дефлектором, применяемый в масштабных космических боях.";
                    return;

                case Carrier _:
                    InfoLabel.Text += "Огромный корабль, немногим меньше космостанции. По сути скорее база для управления меньшими кораблями, чем самостоятельная боевая единица." + "\n"
                      + "Два раза (по одному разу за ход) Авианосец может вместо своего хода выпустить до шести истребителей на соседние гексы." + "\n";
                    return;

                case HeavyCruiser _:
                    InfoLabel.Text += "Большой корабль с тяжелым вооружением и дополнительными системами на борту, который может как стать основой флотилии, так и выполнять вспомогательные задачи.";
                    return;

                case Cruiser _:
                    InfoLabel.Text += "Крупный корабль, оптимизированный для выполнения многих задач." + "\n"
                        + "Благодаря простоте производства, надежности и удобству эксплуатации в годы Мандалорской войны был самым массовым кораблем ВС ГР.";
                    return;

                case Fregate _:
                    InfoLabel.Text += "Средний по размерам корабль, предшественник крейсера, уже не закупаемый регулярными флотами государств, но все еще используемый." + "\n"
                        + "Благодаря дешевизне, надежности и универсальности производился в огромных количествах (часть которых предсказуемо в конце концов осела в руках пиратов).";
                    return;

                case Gunship _:
                    InfoLabel.Text += "Средний по размерам корабль, на котором скорость и живучесть принесена в жертву огневой мощи." + "\n"
                        + "Во время Мандалорской войны часто использовался для проведения рейдов по коммуникациям, поскольку дальнобойные турболазеры позволяли быстро нанести большой урон, не входя в зону поражения орудий противника.";
                    return;

                case Destroyer _:
                    InfoLabel.Text += "Небольшой корабль, разработанный для борьбы с истребителями и слабо защищенными кораблями." + "\n"
                        + "Имеет на борту множество слабых, но скорострельных и дальнобойных орудий, способных создать вокруг корабля плотную завесу огня.";
                    return;

                case Freighter _:
                    InfoLabel.Text += "Небольшой корабль, созданный на базе фрахтовика с учетом потребностей военных." + "\n"
                        + " Прост в эксплуатации, быстр, дешев, неприхотлив и довольно живуч. ";
                    return;

                case Transport _:
                    InfoLabel.Text += "Военно-транспортный корабль, предназначенный для перевозки десанта, бронетехники и атмосферной авиации. " + "\n";
                    return;
            }
        }
        public void SetMoneyLabelText()
        {
            if (Main.Money[Main.Player] == 0)
            {
                MoneyLabel.Text = "NO MONEY";
                MoneyLabel.ForeColor = Color.Red;
            }
            else
            {
                MoneyLabel.Text = "MONEY: " + Main.Money[Main.Player] + " CREDITS";
            }
            MoneyLabel.Refresh();
        }

        private void DrawShipIco(Ship ship, Graphics graphics, Point center)
        {
            var corners = Main.GetHexCorners(center);
            if ((SelectedShip != null) && (SelectedShip.GetType() == ship.GetType()))
            {
                graphics.FillPolygon(new SolidBrush(Color.FromArgb(255, 242, 176, 10)), corners);
            }
            else
            {
                graphics.FillPolygon(new SolidBrush(Color.White), corners);
            }
            Main.DrawShipIcon(ship, center, graphics);

            graphics.DrawPolygon(new Pen(Brushes.Black, 2), corners);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            DrawShipIco(new Battleship(0), graphics, ShipIcoCenters[0]);
            DrawShipIco(new Carrier(0), graphics, ShipIcoCenters[1]);
            DrawShipIco(new HeavyCruiser(0), graphics, ShipIcoCenters[2]);
            DrawShipIco(new Cruiser(0), graphics, ShipIcoCenters[3]);
            DrawShipIco(new Fregate(0), graphics, ShipIcoCenters[4]);
            DrawShipIco(new Gunship(0), graphics, ShipIcoCenters[5]);
            DrawShipIco(new Destroyer(0), graphics, ShipIcoCenters[6]);
            DrawShipIco(new Freighter(0), graphics, ShipIcoCenters[7]);
            DrawShipIco(new Transport(0), graphics, ShipIcoCenters[8]);
        }
    }
}
