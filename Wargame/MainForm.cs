using System.Drawing;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Wargame
{
    public class MainForm : Form
    {
        public readonly Map Map;
        public PointF[,] MapPoints { get; private set; }
        public Point? MousePos { get; private set; }
        private Point? SelectedPoint;

        public float HexRadius { get; private set; }
        public float HalfHexWidth { get; private set; }

        private bool isPlacement;
        private bool isFiring;
        private bool isMoving;
        private bool isChecking;

        private bool firstFighterMoving;
        public int Player { get; private set; }
        public int[] Money { get; private set; }
        private Cannon selectedCannon;

        private Label TurnLabel;
        private Button TurnButton;
        private Panel ShipInfoLabel;
        private Button MovementButton;
        //private Button FighterMovementButton;
        private Button FighterDropButton;
        private Button FiringButton;
        private List<Button> ArmamentButtons;
        private Button PlacementButton;
        private PlacementForm PlaceForm;

        public MainForm()
        {
            DoubleBuffered = true;
            Text = "Wargame";
            BackgroundImage = Image.FromFile(Directory.GetCurrentDirectory() + "\\Images\\background.jpg");
            Icon = new Icon(Directory.GetCurrentDirectory() + "\\Images\\EbonHawk.ico");
            FormBorderStyle = FormBorderStyle.FixedSingle;

            HexRadius = 25;
            HalfHexWidth = (float)Math.Sqrt(0.75 * HexRadius * HexRadius);
            Map = new Map();
            MapPoints = GetMapPoints();
            CheckMouse();
            ClientSize = new Size((int)((Map.Width + 1) * HalfHexWidth) + 210, (int)((Map.Height * 3 + 0.5) * HexRadius) + 5);

            Player = 0;
            Money = new int[2];
            Money[0] = 100;
            Money[1] = 100;
            SelectedPoint = null;
            firstFighterMoving = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            DrawMap(graphics);
        }
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (PlaceForm != null)
            {
                PlaceForm.SetPosition();
            }
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            {
                    CheckMouse();
                    if ((MousePos.HasValue) && (SelectedPoint != MousePos))
                    {
                        CheckShipInfoLabel();
                    }
            }
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (MousePos.HasValue)
            {
                var mouse = MousePos.Value;
                var hex = Map.Formation[mouse.X, mouse.Y];

                if ((!(hex is Ship)) && isPlacement && (PlaceForm.SelectedShip != null) && (Money[Player] >= PlaceForm.SelectedShip.Price))
                {
                    Map.AddShip(mouse.X, mouse.Y, PlaceForm.SelectedShip, (byte)Player);
                    Money[Player] -= PlaceForm.SelectedShip.Price;
                    PlaceForm.SetMoneyLabelText();
                    Invalidate();
                }
                else if (!isPlacement)
                {
                    if (SelectedPoint.HasValue)
                    {
                        CheckAction(hex, mouse);
                        return;
                    }
                    if ((hex is Ship) && (!isFiring) && ((hex as Ship).Player == Player))
                    {
                        SetSelectedPoint(mouse);
                        return;
                    }
                }
            }
        }

        private void CheckMouse()
        {
            var mousePos = PointToClient(MousePosition);
            var result = new Point?();
            var minDistance = HexRadius + 1;

            for (var i = 0; i < Map.Width; i++)
            {
                for (var j = 0; j < Map.Height; j++)
                {
                    var distance = GetDistance(mousePos, MapPoints[i, j]);
                    if (distance < minDistance)
                    {
                        minDistance = (float)distance;
                        result = new Point(i, j);
                    }
                }
            }
            MousePos = (minDistance > HexRadius) ? null : result;
        }

        //инициализация кнопок и баннеров
        private void InitTurnLabel()
        {
            TurnLabel = new Label
            {
                Top = 20,
                Left = (int)((Map.Width + 1) * HalfHexWidth + 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(100, 40),
                Text = "Player " + (Player + 1),
                Font = new Font("Microsoft Sans Serif", 12),
                BorderStyle = BorderStyle.None,
                BackColor = Color.Red
            };
            Controls.Add(TurnLabel);
        }
        private void InitShipInfoLabel()
        {
            ShipInfoLabel = new Panel
            {
                AutoSize = true,
                //TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Visible = false
            };
            Controls.Add(ShipInfoLabel);
        }
        private void InitTurnButton()
        {
            TurnButton = new Button
            {
                Top = TurnLabel.Bottom + 5,
                Left = TurnLabel.Left,
                Size = new Size(100, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "End turn",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White,
            };
            TurnButton.Click += TurnButton_Click;
            Controls.Add(TurnButton);
        }
        private void InitPlacementButton()
        {
            PlacementButton = new Button
            {
                Top = TurnButton.Bottom + 5,
                Left = TurnButton.Left,
                Size = new Size(100, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Start Placement",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White
            };
            PlacementButton.Click += PlacementButton_Click;
            Controls.Add(PlacementButton);
        }
        private void InitFiringButton()
        {
            FiringButton = new Button
            {
                Size = new Size(60, (int)HexRadius),
                Text = "Fire",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 37, 145, 20),
                ForeColor = Color.White,
                Visible = false,
            };
            Controls.Add(FiringButton);
            FiringButton.Click += FiringButton_Click;
        }
        private void InitMovementButton()
        {
            MovementButton = new Button
            {
                Size = new Size(60, (int)HexRadius),
                Text = "Move",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 75, 219, 53),
                ForeColor = Color.White,
                Visible = false,
            };
            Controls.Add(MovementButton);
            MovementButton.Click += MovementButton_Click;
        }
        private void InitFighterDropButton()
        {
            FighterDropButton = new Button
            {
                Size = new Size(100, (int)HexRadius),
                Text = "Drop fighters",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 75, 219, 53),
                ForeColor = Color.White,
                Visible = false,
            };
            Controls.Add(FighterDropButton);
            FighterDropButton.Click += FighterDropButton_Click;
        }
        private void InitArmamentButtons()
        {
            ArmamentButtons = new List<Button>();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitTurnLabel();
            InitShipInfoLabel();
            InitTurnButton();
            InitFiringButton();
            InitMovementButton();
            InitPlacementButton();
            InitFighterDropButton();
            InitArmamentButtons();
        }

        //получение соседних клеток
        public List<Point> GetBordering(Point point)
        {
            var result = new List<Point>();
            var y = (point.X % 2 != 0) ? 1 : -1;

            for (var i = -2; i <= 2; i++)
            {
                if (i != 0)
                {
                    result.Add(new Point(point.X + i, point.Y));
                }
            }
            result.Add(new Point(point.X - 1, point.Y + y));
            result.Add(new Point(point.X + 1, point.Y + y));
            return result.Where(point => (point.X >= 0 && point.X < Map.Width) && (point.Y >= 0 && point.Y < Map.Height))
                         .ToList();
        }
        public List<Point> GetBorderingInRange(Point point, int range)
        {
            var result = new List<Point>();
            var currentRes = new List<Point>();

            result.AddRange(GetBordering(point));

            for (var i = 1; i < range; i++)
            {
                foreach (var hex in result)
                {
                    currentRes.AddRange(GetBordering(hex));
                }
                result = currentRes.Distinct()
                                    .Where(point => point.X >= 0 &&
                                           point.X < Map.Width &&
                                           point.Y >= 0 &&
                                           point.Y < Map.Height)
                                   .ToList();

            }

            return result;
        }

        //Методы, действующие на выбранный корабль
        private void SetSelectedPoint(Point mouse)
        {
            SelectedPoint = new Point(mouse.X, mouse.Y);
            SetUnitButtonsPos(mouse);
            ShowMainUnitButtons();

            ShipInfoLabel.Visible = false;
            isFiring = false;
            isMoving = false;
            Invalidate();
        }
        private void MoveShip(Point oldPos, Point newPos)
        {
            var ship = Map.Formation[oldPos.X, oldPos.Y] as Ship;
            Map.MoveShip(oldPos, newPos);
            ship.Movement -= 1;

            if (ship.Movement > 0)
            {
                SelectedPoint = newPos;
            }
            else
            {
                isMoving = false;
                SelectedPoint = null;
                MovementButton.BackColor = Color.FromArgb(220, 57, 82, 89);
            }
            Invalidate();
        }
        private void MoveDoubleFighters(Point oldPos, Point newPos, bool isFirstFighterMoving)
        {
            var ship = Map.Formation[oldPos.X, oldPos.Y] as Ship;
            Map.MoveDoubleFighter(oldPos, newPos, isFirstFighterMoving);

            if ((Map.Formation[newPos.X, newPos.Y] as Ship).Movement > 0)
            {
                SelectedPoint = newPos;
            }
            else
            {
                isMoving = false;
                SelectedPoint = null;
                MovementButton.BackColor = Color.FromArgb(220, 57, 82, 89);
            }
            Invalidate();
        }
        private void Fire(Ship enemy, Ship selectedShip, Point enemyPos)
        {
            if (selectedShip is Destroyer)
            {
                Map.Formation[enemyPos.X, enemyPos.Y] = enemy.TakeDamageWithoutEvasion(selectedCannon.Fire());
            }
            else
            {
                Map.Formation[enemyPos.X, enemyPos.Y] = enemy.TakeDamage(selectedCannon.Fire());
            }
            selectedCannon = null;
            isFiring = false;
            HideMainUnitButtons();
            ShipInfoLabel.Visible = false;
            Invalidate();
        }
        private void DropFighters()
        {
            var carrier = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Carrier;
            Map.DropFighters(GetBordering(SelectedPoint.Value), carrier);
        }

        private void CheckAction(Hexagon hex, Point mouse)
        {
            var selectedPoint = SelectedPoint.Value;
            var selectedShip = Map.Formation[selectedPoint.X, selectedPoint.Y] as Ship;

            if ((mouse == selectedPoint) || (!isFiring && !isMoving))
            {
                isFiring = false;
                isMoving = false;
                SelectedPoint = null;
                HideMainUnitButtons();
                Invalidate();
                return;
            }

            switch (hex)
            {
                case null when isMoving &&
                               (selectedShip.Movement > 0) &&
                               GetBordering(selectedPoint).Contains(mouse):
                    if (selectedShip is DoubleFighter)
                    {
                        MoveDoubleFighters(selectedPoint, mouse, firstFighterMoving);
                    }
                    else
                    {
                        MoveShip(selectedPoint, mouse);
                    }
                    break;

                case Fighter _ when isMoving &&
                               (selectedShip.Movement > 0) &&
                               GetBordering(selectedPoint).Contains(mouse):
                    if (selectedShip is DoubleFighter)
                    {
                        MoveDoubleFighters(selectedPoint, mouse, firstFighterMoving);
                    }
                    else if (selectedShip is Fighter)
                    {
                        MoveShip(selectedPoint, mouse);
                    }

                    break;

                case Ship _ when isFiring &&
                                 (selectedCannon != null) &&
                                 (!selectedCannon.IsFired) &&
                                 (hex as Ship).Player != selectedShip.Player &&
                                 GetBorderingInRange(selectedPoint, selectedCannon.Range).Contains(mouse):
                    Fire(hex as Ship, selectedShip, mouse);
                    break;
            }
        }

        //расположение кнопок управления кораблем
        private void SetMovementButtonPos(PointF position)
        {
            MovementButton.Top = (int)(position.Y - MovementButton.Height);

            MovementButton.Left = position.X > ClientSize.Width / 2
                ? (int)(position.X - HalfHexWidth - MovementButton.Width)
                : (int)(position.X + HalfHexWidth);
            var selectedShip = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Ship;

            MovementButton.BackColor = selectedShip.Movement > 0
                ? Color.FromArgb(220, 37, 145, 20)
                : Color.FromArgb(220, 57, 82, 89);
        }
        private void SetFiringButtonPos(PointF position)
        {
            FiringButton.Top = (int)(position.Y);

            FiringButton.Left = position.X > ClientSize.Width / 2
                ? (int)(position.X - HalfHexWidth - FiringButton.Width)
                : (int)(position.X + HalfHexWidth);
            var selectedShip = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Ship;
            var noFiring = true;
            foreach (var cannon in selectedShip.Armament)
            {
                if (cannon.IsFired)
                {
                    continue;
                }
                else
                {
                    noFiring = false;
                }

            }
            FiringButton.BackColor = (selectedShip.Armament.Length > 0) && (!noFiring)
                ? Color.FromArgb(220, 37, 145, 20)
                : Color.FromArgb(220, 57, 82, 89);
        }
        private void SetFighterDropButtonPos(PointF position)
        {
            FighterDropButton.Top = (int)(position.Y) + FiringButton.Height;

            FighterDropButton.Left = position.X > ClientSize.Width / 2
                ? (int)(position.X - HalfHexWidth - FighterDropButton.Width)
                : (int)(position.X + HalfHexWidth);
            var selectedCarrier = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Carrier;
            if ((selectedCarrier.FightersCount > 0) && (!selectedCarrier.DropedFighters))
            {
                FighterDropButton.BackColor = Color.FromArgb(220, 37, 145, 20);
            }
            else
            {
                FighterDropButton.BackColor = Color.FromArgb(220, 57, 82, 89);
            }
        }
        private void SetUnitButtonsPos(Point position)
        {
            var pos = MapPoints[position.X, position.Y];
            SetMovementButtonPos(pos);
            SetFiringButtonPos(pos);
            if (Map.Formation[position.X, position.Y] is Carrier)
            {
                SetFighterDropButtonPos(pos);
            }
        }
        private void SetCannonButtons(PointF position)
        {
            var ship = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Ship;

            for (var i = 0; i < ship.Armament.Length; i++)
            {
                var gunBut = new Button
                {
                    Size = new Size(60, (int)HexRadius),
                    Text = "d" + ship.Armament[i].Power + "; " + ship.Armament[i].Range,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = ship.Armament[i].IsFired
                    ? Color.FromArgb(220, 57, 82, 89)
                    : Color.FromArgb(220, 37, 145, 20)
                };

                gunBut.Left = position.X > ClientSize.Width / 2
                ? (int)(position.X - HalfHexWidth - gunBut.Width)
                : (int)(position.X + HalfHexWidth);

                gunBut.Top = (int)position.Y + gunBut.Height * (i - 1);
                var index = i;

                gunBut.Click += (object sender, EventArgs e) =>
                 {
                     if (!ship.Armament[index].IsFired)
                     {
                         selectedCannon = ship.Armament[index];
                         isFiring = true;
                     }
                     RemoveArmamentButtons();
                     Invalidate();
                 };
                ArmamentButtons.Add(gunBut);
            }
            Controls.AddRange(ArmamentButtons.ToArray());
        }

        //Методы, отвечающие за клик по кнопкам
        private void MovementButton_Click(object sender, EventArgs e)
        {
            if ((Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Ship).Movement > 0)
            {
                isMoving = true;
            }
            HideMainUnitButtons();
            Invalidate();
        }
        private void FiringButton_Click(object sender, EventArgs e)
        {
            var position = MapPoints[SelectedPoint.Value.X, SelectedPoint.Value.Y];
            HideMainUnitButtons();
            SetCannonButtons(position);
            Invalidate();
        }
        private void FighterDropButton_Click(object sender, EventArgs e)
        {
            var carrier = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Carrier;
            if ((!carrier.DropedFighters) && (carrier.FightersCount > 0))
            {
                DropFighters();
            }

            HideMainUnitButtons();
            SelectedPoint = null;
            Invalidate();
        }
        private void TurnButton_Click(object sender, EventArgs e)
        {
            Player = (Player + 1) % 2;
            if (PlaceForm != null)
            {
                PlaceForm.Hide();
            }
            isPlacement = false;
            PlacementButton.Text = "Start placement";

            foreach (var hex in Map.Formation)
            {
                if (hex is Ship)
                {
                    (hex as Ship).Refresh();
                }
            }
            SelectedPoint = null;
            selectedCannon = null;
            isFiring = false;
            isMoving = false;
            HideMainUnitButtons();
            RemoveArmamentButtons();

            TurnLabel.BackColor = Player == 1 ? Color.Blue : Color.Red;
            TurnLabel.Text = "Player " + (Player + 1);
            Invalidate();
        }
        private void PlacementButton_Click(object sender, EventArgs e)
        {
            if (!isPlacement)
            {
                isPlacement = true;
                SelectedPoint = null;
                selectedCannon = null;
                isFiring = false;
                isMoving = false;
                RemoveArmamentButtons();
                HideMainUnitButtons();
                PlaceForm = new PlacementForm(this);
                PlacementButton.Text = "Stop placement";
                PlaceForm.SetPosition();
                PlaceForm.Show();
                FiringButton.Visible = false;
                MovementButton.Visible = false;
                Invalidate();
            }
            else
            {
                isPlacement = false;
                PlacementButton.Text = "Start placement";
                PlaceForm.Hide();
            }
        }

        //Показ или скрытие кнопок корабля
        private void HideMainUnitButtons()
        {
            MovementButton.Visible = false;
            FiringButton.Visible = false;
            FighterDropButton.Visible = false;
        }
        private void ShowMainUnitButtons()
        {
            if (SelectedPoint.HasValue && (Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Ship).Armament.Length > 0)
                FiringButton.Visible = true;
            MovementButton.Visible = true;
            if (SelectedPoint.HasValue && (Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] is Carrier))
            {
                FighterDropButton.Visible = true;
            }
        }
        private void RemoveArmamentButtons()
        {
            for (var j = 0; j < ArmamentButtons.ToArray().Length; j++)
            {
                ArmamentButtons[j].Visible = false;
                ArmamentButtons[j].Update();
                Controls.Remove(ArmamentButtons[j]);
            }
        }

        //Расположение таблички информации и установка данных
        private void SetInfoLabelPos(PointF position)
        {
            ShipInfoLabel.Top = position.Y > ClientSize.Height / 2
                ? (int)(position.Y - ShipInfoLabel.Height - HexRadius / 2)
                : (int)(position.Y + HexRadius / 2);

            ShipInfoLabel.Left = position.X > ClientSize.Width / 2
                ? (int)(position.X - HalfHexWidth - ShipInfoLabel.Width)
                : (int)(position.X + HalfHexWidth);
        }
        private void SetInfoLabelText(Ship ship)
        {
            switch (ship)
            {
                case Battleship _:
                    ShipInfoLabel.Text = "Type: Battleship\n";
                    break;
                case Carrier _:
                    ShipInfoLabel.Text = "Type: Carrier\n";
                    break;
                case HeavyCruiser _:
                    ShipInfoLabel.Text = "Type: Heavy Cruiser\n";
                    break;
                case Cruiser _:
                    ShipInfoLabel.Text = "Type: Cruiser\n";
                    break;
                case Fregate _:
                    ShipInfoLabel.Text = "Type: Fregate\n";
                    break;
                case Gunship _:
                    ShipInfoLabel.Text = "Type: Gunship\n";
                    break;
                case Destroyer _:
                    ShipInfoLabel.Text = "Type: Destroyer\n";
                    break;
                case Freighter _:
                    ShipInfoLabel.Text = "Type: Freighter\n";
                    break;
                case Transport _:
                    ShipInfoLabel.Text = "Type: Transport\n";
                    break;
                case Fighter _:
                    ShipInfoLabel.Text = "Type: Fighter\n";
                    break;
                case DoubleFighter _:
                    ShipInfoLabel.Text = "Type: DoubleFighter\n";
                    break;

            }
            ShipInfoLabel.Text +=
               "Shields: " + ship.Shields + "\n" +
               "Evasion: " + ship.Evasion + "\n" +
               "Speed: " + ship.Speed + "\n" +
               "Fire range: " + ((ship.Armament.Length > 0) ? ship.Armament[0].Range.ToString() : "none") + "\n" +
               "Firepower: " + ((ship.Armament.Length > 0) ? ("d" + ship.Armament[0].Power.ToString()) : "none") + "\n";
        }
        private void CheckShipInfoLabel()
        {
            if (MousePos.HasValue)
            {
                var mouse = MousePos.Value;
                if (Map.Formation[mouse.X, mouse.Y] is Ship)
                {
                    SetInfoLabelText(Map.Formation[mouse.X, mouse.Y] as Ship);
                    SetInfoLabelPos(MapPoints[mouse.X, mouse.Y]);
                    ShipInfoLabel.Visible = true;
                }
                else
                {
                    ShipInfoLabel.Visible = false;
                }
                ShipInfoLabel.Invalidate();
            }
        }

        //Отрисовка
        private void DrawMap(Graphics graphics)
        {
            for (var i = 0; i < Map.Width; i++)
            {
                for (var j = 0; j < Map.Height; j++)
                {
                    var corners = GetHexCorners(MapPoints[i, j]);
                    if (Map.Formation[i, j] is Ship)
                    {
                        if ((Map.Formation[i, j] as Ship).Player == 1)
                        {
                            graphics.FillPolygon(Brushes.Blue, corners);
                        }
                        else
                        {
                            graphics.FillPolygon(Brushes.Red, corners);
                        }
                        DrawShipIcon((Map.Formation[i, j]) as Ship, MapPoints[i, j], graphics);
                    }

                    graphics.DrawPolygon(new Pen(Brushes.White, 2), corners);
                    ////показ координат гексов
                    //var format = new StringFormat
                    //{
                    //    LineAlignment = StringAlignment.Center,
                    //    Alignment = StringAlignment.Center
                    //};
                    //graphics.DrawString((i.ToString() + ';' + j.ToString()), new Font("Arial", 12), Brushes.White, MapPoints[i, j], format);
                }
            }
            if (SelectedPoint.HasValue)
            {
                if (isMoving)
                {
                    DrawMoving(graphics);
                }
                if (isFiring)
                {
                    DrawFiring(graphics, selectedCannon.Range);
                }
            }
        }
        private void DrawMoving(Graphics graphics)
        {
            DrawSelectedShip(graphics);

            var borders = GetBordering(SelectedPoint.Value);
            foreach (var border in borders)
            {
                if (!(Map.Formation[border.X, border.Y] is Ship))
                {
                    var borderCorners = GetHexCorners(MapPoints[border.X, border.Y]);
                    graphics.FillPolygon(new SolidBrush(Color.FromArgb(100, 189, 189, 189)), borderCorners);
                    graphics.DrawPolygon(new Pen(Brushes.White, 2), borderCorners);
                }
            }
        }
        private void DrawFiring(Graphics graphics, int range)
        {
            if (!selectedCannon.IsFired)
            {
                DrawSelectedShip(graphics);

                var borders = GetBorderingInRange(SelectedPoint.Value, range);
                foreach (var border in borders)
                {
                    if (!(Map.Formation[border.X, border.Y] is Ship))
                    {
                        var borderCorners = GetHexCorners(MapPoints[border.X, border.Y]);
                        graphics.FillPolygon(new SolidBrush(Color.FromArgb(100, 189, 189, 189)), borderCorners);
                        graphics.DrawPolygon(new Pen(Brushes.White, 2), borderCorners);
                    }
                }
            }
        }
        private void DrawSelectedShip(Graphics graphics)
        {
            var position = SelectedPoint.Value;
            var pointCorners = GetHexCorners(MapPoints[position.X, position.Y]);

            graphics.FillPolygon(new SolidBrush(Color.FromArgb(255, 242, 176, 10)), pointCorners);
            DrawShipIcon(Map.Formation[position.X, position.Y] as Ship, MapPoints[position.X, position.Y], graphics);
            graphics.DrawPolygon(new Pen(Brushes.White, 2), pointCorners);
        }
        public void DrawShipIcon(Ship ship, PointF center, Graphics graphics)
        {
            var circleSize = HexRadius / 3;
            switch (ship)
            {
                case Battleship _:
                    graphics.FillPolygon(Brushes.Black, GetRhombCorners(center));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case Carrier _:
                    graphics.FillPolygon(Brushes.Black, GetRhombCorners(center));
                    break;

                case HeavyCruiser _:
                    graphics.FillPolygon(Brushes.Black, GetTriangleCorners(center, false));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case Cruiser _:
                    graphics.FillPolygon(Brushes.Black, GetTriangleCorners(center, false));
                    break;

                case Fregate _:
                    graphics.FillPolygon(Brushes.Black, GetTriangleCorners(center, true));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case Gunship _:
                    graphics.FillPolygon(Brushes.Black, GetTriangleCorners(center, true));
                    break;

                case Destroyer _:
                    graphics.FillPolygon(Brushes.Black, GetSquareCorners(center));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case Freighter _:
                    graphics.FillPolygon(Brushes.Black, GetSquareCorners(center));
                    break;

                case Transport _:
                    graphics.FillEllipse(Brushes.White, center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                    graphics.DrawEllipse(new Pen(Brushes.Black, 2), center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                    break;

                case Fighter _:
                    graphics.FillEllipse(Brushes.White, center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                    graphics.DrawEllipse(new Pen(Brushes.Black, 2), center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                    graphics.FillRectangle(Brushes.Black, center.X - circleSize / 6, center.Y - circleSize / 2, circleSize / 3, circleSize);
                    break;

                case DoubleFighter _:
                    {
                        var bigRect = new RectangleF(center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                        var smallRect = new RectangleF(center.X - circleSize / 6, center.Y - circleSize / 2 - 1, circleSize / 3, circleSize + 2);

                        graphics.FillEllipse(Brushes.White, center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                        graphics.DrawEllipse(new Pen(Brushes.Black, 2), center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                        graphics.FillRectangle(Brushes.Black, bigRect);
                        graphics.FillRectangle(Brushes.White, smallRect);
                        break;
                    }
            }
        }

        //Получение точек и геометрические вычисления
        public PointF[] GetHexCorners(PointF center)
        {
            var corners = new PointF[6];
            for (var i = 0; i < 6; i++)
            {
                var angle_deg = 60 * i + 30;
                var angle_rad = Math.PI / 180 * angle_deg;
                corners[i].X = (float)(center.X + HexRadius * Math.Cos(angle_rad));
                corners[i].Y = (float)(center.Y + HexRadius * Math.Sin(angle_rad));
            }
            return corners;
        }
        public double GetDistance(PointF p1, PointF p2) => Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        private PointF[] GetSquareCorners(PointF center)
        {
            var size = (float)(1.0 / 2 * HalfHexWidth);
            var result = new PointF[4];
            result[0] = new PointF(center.X - size, center.Y - size);
            result[1] = new PointF(center.X - size, center.Y + size);
            result[2] = new PointF(center.X + size, center.Y + size);
            result[3] = new PointF(center.X + size, center.Y - size);
            return result;
        }
        private PointF[] GetRhombCorners(PointF center)
        {
            var size = (float)(2.0 / 3 * HalfHexWidth);
            var result = new PointF[4];
            result[0] = new PointF(center.X, center.Y - size);
            result[1] = new PointF(center.X - size, center.Y);
            result[2] = new PointF(center.X, center.Y + size);
            result[3] = new PointF(center.X + size, center.Y);
            return result;
        }
        private PointF[] GetTriangleCorners(PointF center, bool isUpper)
        {
            var result = new PointF[3];
            var size = (float)(1.3 / 2 * HalfHexWidth);
            if (isUpper)
            {
                result[0] = new PointF(center.X, center.Y - size);
                result[1] = new PointF((float)(center.X - Math.Cos(60) * size), (float)(center.Y + size / 2));
                result[2] = new PointF((float)(center.X + Math.Cos(60) * size), (float)(center.Y + size / 2));
            }
            else
            {
                result[0] = new PointF(center.X, center.Y + size);
                result[1] = new PointF((float)(center.X - Math.Cos(60) * size), (float)(center.Y - size / 2));
                result[2] = new PointF((float)(center.X + Math.Cos(60) * size), (float)(center.Y - size / 2));
            }
            return result;
        }
        private PointF[,] GetMapPoints()
        {
            var mapPoints = new PointF[Map.Width, Map.Height];

            for (var i = 0; i < Map.Width; i++)
            {
                for (var j = 0; j < Map.Height; j++)
                {
                    mapPoints[i, j].X = HexRadius + HalfHexWidth * i;
                    mapPoints[i, j].Y = HexRadius + HexRadius * 3 * j + 1.5f * HexRadius * (i % 2);
                }
            }
            return mapPoints;
        }
    }
}
