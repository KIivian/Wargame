using System.Drawing;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Drawing.Text;

namespace Wargame
{
    public class MainForm : Form
    {
        public readonly Map Map;
        public PointF[,] MapPoints { get; private set; }
        public Point? MousePos { get; private set; }
        public Timer Timer { get; private set; }
        private Point? SelectedPoint;
        private Point? EnemyPoint;
        private int distance;
        private int ticksCount;
        private bool isStarted;
        private int placementFieldWidth;

        public float HexRadius { get; private set; }
        public float HalfHexWidth { get; private set; }

        private bool isPlacement;
        private bool isFiring;
        private bool isMoving;
        private bool isVictory;
        private int damage;
        public int Player { get; private set; }
        private int winner;
        private bool firstTurn;

        private bool firstFighterMoving;
        public int[] FleetPoints { get; private set; }
        private List<Point>[] PlacementFields;
        private Cannon selectedCannon;

        private Button StartButton;
        private Button PlusPointsButton;
        private Button MinusPointsButton;
        private Label PointsLabel;

        private Label VictoryLabel;

        private Label TurnLabel;
        private Button TurnButton;
        private Button MovementButton;
        private Tuple<Button, Button> FighterMoveButtons;
        private Button FighterDropButton;
        private Button FiringButton;
        private List<Button> ArmamentButtons;
        private Button PlacementButton;
        private Graphics graphics;


        private PlacementForm PlaceForm;

        public MainForm()
        {
            DoubleBuffered = true;
            Text = "Wargame";
            BackgroundImage = Image.FromFile(Directory.GetCurrentDirectory() + "\\Resources\\main menu.jpg");
            Icon = new Icon(Directory.GetCurrentDirectory() + "\\Resources\\EbonHawk.ico");
            FormBorderStyle = FormBorderStyle.FixedSingle;

            HexRadius = 25;
            HalfHexWidth = (float)Math.Sqrt(0.75 * HexRadius * HexRadius);
            Map = new Map(31, 8);
            MapPoints = Geometry.GetMapPoints(HexRadius, Map);
            CheckMouse();
            MaximumSize = new Size((int)((Map.Width + 1) * HalfHexWidth) + 210, (int)((Map.Height * 3 + 2) * HexRadius) + 5);
            MinimumSize = new Size((int)((Map.Width + 1) * HalfHexWidth) + 210, (int)((Map.Height * 3 + 2) * HexRadius) + 5);
            ClientSize = new Size((int)((Map.Width + 1) * HalfHexWidth) + 210, (int)((Map.Height * 3 + 2) * HexRadius) + 5);

            Player = 0;
            PlacementFields = new List<Point>[2];
            placementFieldWidth = 5;
            PlacementFields[0] = Map.GetPlacementField(Map, 0, placementFieldWidth);
            PlacementFields[1] = Map.GetPlacementField(Map, Map.Width - placementFieldWidth, Map.Width);
            FleetPoints = new int[2];
            SelectedPoint = null;
            firstFighterMoving = true;
            firstTurn = true;
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (PlaceForm != null)
            {
                PlaceForm.SetPosition();
            }
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            CheckVictory();
            Parallel.Invoke(Invalidate);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            {
                CheckMouse();
            }
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (MousePos.HasValue && (!EnemyPoint.HasValue))
            {
                var mouse = MousePos.Value;
                var hex = Map.Formation[mouse.X, mouse.Y];

                if ((!(hex is Ship)) &&
                    isPlacement &&
                    (PlaceForm.SelectedShip != null) &&
                    (FleetPoints[Player] >= PlaceForm.SelectedShip.Price) &&
                    PlacementFields[Player].Contains(MousePos.Value))
                {
                    Map.AddShip(mouse.X, mouse.Y, PlaceForm.SelectedShip, (byte)Player);
                    FleetPoints[Player] -= PlaceForm.SelectedShip.Price;
                    PlaceForm.SetMoneyLabelText();
                    Parallel.Invoke(Invalidate);
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
                    var distance = Geometry.GetDistance(mousePos, MapPoints[i, j]);
                    if (distance < minDistance)
                    {
                        minDistance = (float)distance;
                        result = new Point(i, j);
                    }
                }
            }
            MousePos = (minDistance > HexRadius) ? null : result;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Exit();
        }

        //инициализация таймера, кнопок и баннеров
        private void InitTimer()
        {
            Timer = new Timer
            {
                Interval = 5,
                Enabled = false

            };
            Timer.Tick += OnTimerElapsed;
        }

        private void InitStartButton()
        {
            StartButton = new Button
            {
                Size = new Size(150, 60),
                Top = 100,
                Left = (int)((ClientSize.Width - 150) / 2),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "НАЧАТЬ ИГРУ",
                Font = new Font("Microsoft Sans Serif", 16),
                BackColor = Color.FromArgb(220, 57, 82, 89)
            };
            StartButton.Click += StartButton_Click;
            Controls.Add(StartButton);

        }
        private void InitPointsLabel()
        {
            PointsLabel = new Label
            {
                Size = new Size(100, 40),
                Top = StartButton.Bottom + 60,
                Left = StartButton.Left + StartButton.Width / 2 - 100 / 2,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Очки:\n0",
                Font = new Font("Microsoft Sans Serif", 12),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White
            };
            Controls.Add(PointsLabel);
        }
        private void InitPlusPointsButton()
        {
            PlusPointsButton = new Button
            {
                Top = PointsLabel.Top,
                Left = PointsLabel.Right + 10,
                Size = new Size(50, PointsLabel.Height),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft Sans Serif", 12),
                Text = "+10",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White
            };
            PlusPointsButton.Click += PlusPointsButton_Click;
            Controls.Add(PlusPointsButton);
        }
        private void InitMinusPointsButton()
        {
            MinusPointsButton = new Button
            {
                Top = PointsLabel.Top,
                Left = PointsLabel.Left - 10 - 50,
                Size = new Size(50, PointsLabel.Height),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft Sans Serif", 12),
                Text = "-10",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White
            };
            MinusPointsButton.Click += MinusPointsButton_Click;
            Controls.Add(MinusPointsButton);
        }

        private void InitVictoryLabel()
        {
            VictoryLabel = new Label
            {
                Size = new Size(200, 80),
                Top = (ClientSize.Height - 150) / 2,
                Left = (int)((ClientSize.Width - 150) / 2),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft Sans Serif", 20),
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White,
                Visible = false
            };
        }

        private void InitTurnLabel()
        {
            TurnLabel = new Label
            {
                Size = new Size(100, 40),
                Top = 20,
                Left = (int)((ClientSize.Width + (Map.Width + 1) * HalfHexWidth - 100) / 2),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "ИГРОК " + (Player + 1),
                Font = new Font("Microsoft Sans Serif", 12),
                BorderStyle = BorderStyle.None,
                BackColor = Color.Red,
                Visible = false
            };
            Controls.Add(TurnLabel);
        }
        private void InitTurnButton()
        {
            TurnButton = new Button
            {
                Top = TurnLabel.Bottom + 5,
                Left = TurnLabel.Left,
                Size = new Size(100, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft Sans Serif", 9),
                Text = "Закончить ход",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White,
                Visible = false
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
                Size = new Size(100, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft Sans Serif", 9),
                Text = "Начать расстановку",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 57, 82, 89),
                ForeColor = Color.White,
                Visible = false
            };
            PlacementButton.Click += PlacementButton_Click;
            Controls.Add(PlacementButton);
        }

        private void InitFiringButton()
        {
            FiringButton = new Button
            {
                Size = new Size(32, 32),
                Text = "",
                Font = new Font("Symbol", 14),
                TextAlign = ContentAlignment.MiddleCenter,
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
                Size = new Size(32, 32),
                Text = "",
                Font = new Font("Wingdings 3", 14),
                TextAlign = ContentAlignment.MiddleCenter,
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
                Size = new Size(32, 32),
                Text = "",
                Font = new Font("Wingdings 3", 14),
                TextAlign = ContentAlignment.MiddleCenter,
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
        private void InitFighterMovementButtons()
        {
            var button1 = new Button()
            {
                Size = new Size(80, 40),
                Font = new Font("Microsoft Sans Serif", 8),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 75, 219, 53),
                ForeColor = Color.White,
                Visible = false
            };
            Controls.Add(button1);
            button1.Click += FirstFighterMoveButton_Click;
            var button2 = new Button()
            {
                Size = new Size(80, 40),
                Font = new Font("Microsoft Sans Serif", 8),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 75, 219, 53),
                ForeColor = Color.White,
                Visible = false
            };
            Controls.Add(button2);
            button2.Click += SecondFighterMoveButton_Click;

            FighterMoveButtons = new Tuple<Button, Button>(button1, button2);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitTimer();

            InitStartButton();
            InitPointsLabel();
            InitPlusPointsButton();
            InitMinusPointsButton();

            InitVictoryLabel();

            InitTurnLabel();
            InitTurnButton();
            InitPlacementButton();

            InitFiringButton();
            InitMovementButton();
            InitFighterDropButton();
            InitArmamentButtons();
            InitFighterMovementButtons();

            DrawMainMenu();
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
            RemoveArmamentButtons();
            ShowMainUnitButtons();

            isFiring = false;
            isMoving = false;
            Parallel.Invoke(Invalidate);
        }
        private void MoveShip(Point oldPos, Point newPos)
        {
            var ship = Map.Formation[oldPos.X, oldPos.Y] as Ship;
            Map.MoveShip(oldPos, newPos);
            ship.Movement -= 1;

            if (ship.Movement > 0)
            {
                SelectedPoint = newPos;
                if ((Map.Formation[newPos.X, newPos.Y] is DoubleFighter) &&
                  (((Map.Formation[newPos.X, newPos.Y] as DoubleFighter).FirstFighterMovement > 0) ||
                   ((Map.Formation[newPos.X, newPos.Y] as DoubleFighter).SecondFighterMovement > 0)))
                {
                    SetFighterMoveButtonsPos(MapPoints[newPos.X, newPos.Y]);
                }
            }
            else
            {
                isMoving = false;
                SelectedPoint = null;
                MovementButton.BackColor = Color.FromArgb(220, 57, 82, 89);
            }
            Parallel.Invoke(Invalidate);
        }
        private void MoveDoubleFighters(Point oldPos, Point newPos, bool isFirstFighterMoving)
        {
            var ship = Map.Formation[oldPos.X, oldPos.Y] as Ship;
            Map.MoveDoubleFighter(oldPos, newPos, isFirstFighterMoving);

            if ((Map.Formation[newPos.X, newPos.Y] as Ship).Movement > 0)
            {
                SelectedPoint = newPos;
                if ((Map.Formation[newPos.X, newPos.Y] is DoubleFighter) &&
                  (((Map.Formation[newPos.X, newPos.Y] as DoubleFighter).FirstFighterMovement > 0) ||
                   ((Map.Formation[newPos.X, newPos.Y] as DoubleFighter).SecondFighterMovement > 0)))
                {
                    SetFighterMoveButtonsPos(MapPoints[newPos.X, newPos.Y]);
                }
            }
            else
            {
                isMoving = false;
                SelectedPoint = null;
                MovementButton.BackColor = Color.FromArgb(220, 57, 82, 89);
            }
            Parallel.Invoke(Invalidate);
        }
        private void Fire(Ship enemy, Ship selectedShip, Point enemyPos)
        {
            if ((selectedShip is Gunship) && ((selectedShip as Gunship).IsDoubleFired))
            {
                selectedShip.Movement = 0;
            }

            EnemyPoint = enemyPos;
            damage = selectedCannon.Fire();

            if (selectedShip is Carrier)
            {
                (selectedShip as Carrier).DropedFighters = true;
            }

            if (selectedShip is Destroyer)
            {
                Map.Formation[enemyPos.X, enemyPos.Y] = enemy.TakeDamageWithoutEvasion(damage);
            }
            else
            {
                Map.Formation[enemyPos.X, enemyPos.Y] = enemy.TakeDamage(damage);
            }

            if ((!(selectedShip is Destroyer)) && (enemy.Evasion >= damage))
            {
                damage = 0;
            }
            if ((selectedShip is Gunship) && (!(selectedShip as Gunship).IsDoubleFired) && (selectedShip.Movement == selectedShip.Speed))
            {
                selectedCannon.Charge();
                (selectedShip as Gunship).IsDoubleFired = true;
            }
            selectedCannon = null;
            isFiring = false;
            HideMainUnitButtons();
            Parallel.Invoke(Invalidate);
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
                RemoveArmamentButtons();
                Parallel.Invoke(Invalidate);
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

        //Проверка на выигрыш
        private void CheckVictory()
        {
            if (!firstTurn)
            {
                var owners = new List<int>();
                foreach (var hex in Map.Formation)
                {
                    if ((hex is Ship) && !owners.Contains((hex as Ship).Player))
                    {
                        owners.Add((hex as Ship).Player);
                    }
                }
                if (owners.Count() < 2)
                {
                    winner = Player;
                    if (!isVictory)
                    {
                        ticksCount = 0;
                    }
                    isVictory = true;
                }
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
            FighterDropButton.Top = (int)(position.Y + FiringButton.Height + FighterDropButton.Height <= ClientSize.Height
                                          ? position.Y + FiringButton.Height
                                          : position.Y - MovementButton.Height - FighterDropButton.Height);

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
                    Font = new Font("Microsoft Sans Serif", 9),
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
        private void SetFighterMoveButtonsPos(PointF position)
        {
            isMoving = false;
            var doubleFighter = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as DoubleFighter;
            var xPos1 = (int)(position.X + HalfHexWidth);
            var xPos2 = (int)(position.X - FighterMoveButtons.Item1.Width - HalfHexWidth);
            var yPos = (int)(position.Y + 2 * FighterMoveButtons.Item1.Height <= ClientSize.Height
                            ? position.Y
                            : (ClientSize.Height - 2 * FighterMoveButtons.Item1.Height));

            if (xPos1 <= ClientSize.Width / 2)
            {
                FighterMoveButtons.Item1.Location = new Point(xPos1, yPos);
                FighterMoveButtons.Item2.Location = new Point(xPos1, yPos + FighterMoveButtons.Item1.Height);
            }
            else
            {
                FighterMoveButtons.Item1.Location = new Point(xPos2, yPos);
                FighterMoveButtons.Item2.Location = new Point(xPos2, yPos + FighterMoveButtons.Item1.Height);
            }
            FighterMoveButtons.Item1.Text = "движение:" + doubleFighter.FirstFighterMovement + "\n" +
                                            (doubleFighter.Armament[0].IsFired ? "стрелял" : "не стрелял");
            FighterMoveButtons.Item2.Text = "движение:" + doubleFighter.SecondFighterMovement + "\n" +
                                            (doubleFighter.Armament[1].IsFired ? "стрелял" : "не стрелял");

            FighterMoveButtons.Item1.BackColor = (doubleFighter.FirstFighterMovement > 0)
                ? Color.FromArgb(220, 37, 145, 20)
                : Color.FromArgb(220, 57, 82, 89);
            FighterMoveButtons.Item2.BackColor = (doubleFighter.SecondFighterMovement > 0)
                ? Color.FromArgb(220, 37, 145, 20)
                : Color.FromArgb(220, 57, 82, 89);

            FighterMoveButtons.Item1.Visible = true;
            FighterMoveButtons.Item2.Visible = true;
        }

        //Методы, отвечающие за клик по кнопкам
        private void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Visible = false;
            PlusPointsButton.Visible = false;
            MinusPointsButton.Visible = false;
            PointsLabel.Visible = false;
            TurnLabel.Visible = true;
            TurnButton.Visible = true;
            PlacementButton.Visible = true;
            isStarted = true;
            Timer.Enabled = true;
        }
        private void PlusPointsButton_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < FleetPoints.Length; i++)
            {
                FleetPoints[i] += 10;
            }
            PointsLabel.Text = "Очки:\n" + FleetPoints[0];
            PointsLabel.Refresh();
        }
        private void MinusPointsButton_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < FleetPoints.Length; i++)
            {
                FleetPoints[i] = FleetPoints[i] - 10 > 0
                                 ? FleetPoints[i] - 10
                                 : 0;
            }
            PointsLabel.Text = "Очки:\n" + FleetPoints[0];
            PointsLabel.Refresh();
        }

        private void MovementButton_Click(object sender, EventArgs e)
        {
            var ship = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Ship;
            if (ship.Movement > 0)
            {
                HideMainUnitButtons();

                if (ship is DoubleFighter)
                {
                    SetFighterMoveButtonsPos(MapPoints[SelectedPoint.Value.X, SelectedPoint.Value.Y]);
                }
                else
                {
                    isMoving = true;
                }
            }
            Parallel.Invoke(Invalidate);
        }
        private void FiringButton_Click(object sender, EventArgs e)
        {
            var position = MapPoints[SelectedPoint.Value.X, SelectedPoint.Value.Y];
            HideMainUnitButtons();
            var noFiring = true;
            foreach (var cannon in (Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Ship).Armament)
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
            if (!noFiring)
            {
                SetCannonButtons(position);
            }
            Parallel.Invoke(Invalidate);
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
            Parallel.Invoke(Invalidate);
        }
        private void TurnButton_Click(object sender, EventArgs e)
        {
            if ((firstTurn) && (Player == 1))
            {
                firstTurn = false;
                PlacementButton.Visible = false;
            }

            Player = (Player + 1) % 2;
            if (PlaceForm != null)
            {
                PlaceForm.Hide();
            }
            isPlacement = false;
            PlacementButton.Text = "Начать расстановку";

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
            TurnLabel.Text = "ИГРОК " + (Player + 1);
            Parallel.Invoke(Invalidate);
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
                PlacementButton.Text = "Завершить расстановку";
                PlaceForm.SetPosition();
                PlaceForm.Show();
                FiringButton.Visible = false;
                MovementButton.Visible = false;
                Parallel.Invoke(Invalidate);
            }
            else
            {
                isPlacement = false;
                PlacementButton.Text = "Начать расстановку";
                PlaceForm.Hide();
            }
        }
        private void FirstFighterMoveButton_Click(object sender, EventArgs e)
        {
            firstFighterMoving = true;
            FighterMoveButtons.Item1.Visible = false;
            FighterMoveButtons.Item2.Visible = false;

            if ((Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as DoubleFighter).FirstFighterMovement > 0)
            {
                isMoving = true;
            }
        }
        private void SecondFighterMoveButton_Click(object sender, EventArgs e)
        {
            firstFighterMoving = false;
            FighterMoveButtons.Item1.Visible = false;
            FighterMoveButtons.Item2.Visible = false;
            if ((Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as DoubleFighter).SecondFighterMovement > 0)
            {
                isMoving = true;
            }
        }

        //Показ или скрытие кнопок корабля
        private void HideMainUnitButtons()
        {
            MovementButton.Visible = false;
            FiringButton.Visible = false;
            FighterDropButton.Visible = false;

            FighterMoveButtons.Item1.Visible = false;
            FighterMoveButtons.Item2.Visible = false;
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
        private Point SetInfoLabelPos(PointF position, Size rectSize)
        {
            var result = new Point();
            result.Y = position.Y + rectSize.Height + HexRadius / 2 > ClientSize.Height
                ? (int)(position.Y - rectSize.Height - HexRadius / 2 - 2)
                : (int)(position.Y + HexRadius / 2 + 2);

            result.X = position.X + rectSize.Width > Map.Width * HalfHexWidth + 1
                ? (int)(position.X - HalfHexWidth - rectSize.Width - 2)
                : (int)(position.X + HalfHexWidth + 2);
            return (result);
        }
        private string SetInfoLabelText(Ship ship)
        {
            var result = "";
            switch (ship)
            {
                case Battleship _:
                    result = "Линкор\n";
                    break;
                case Carrier _:
                    result = "Авианосец\n";
                    break;
                case HeavyCruiser _:
                    result = "Тяжелый крейсер\n";
                    break;
                case Cruiser _:
                    result = "Крейсер\n";
                    break;
                case Fregate _:
                    result = "Фрегат\n";
                    break;
                case Gunship _:
                    result = "Канонерка\n";
                    break;
                case Destroyer _:
                    result = "Эсминец\n";
                    break;
                case Freighter _:
                    result = "Фрейтер\n";
                    break;
                case Transport _:
                    result = "Транспорт\n";
                    break;
                case Fighter _:
                    result = "Истребитель\n";
                    break;
                case DoubleFighter _:
                    result = "Два истребителя\n";
                    break;

            }
            result +=
              "Щиты: " + ship.Shields + "\n" +
              "Уклонение: " + ship.Evasion + "\n" +
              "Скорость: " + ship.Speed + "\n" +
              "Орудия: ";

            if (ship.Armament.Length > 0)
            {
                foreach (var gun in ship.Armament)
                {
                    result += "d" + gun.Power + " r" + gun.Range + "; ";
                }
            }
            else
            {
                result += "Нет";
            }
            result += "\n";
            return result;
        }

        //Отрисовка
        protected override void OnPaint(PaintEventArgs e)
        {
            graphics = e.Graphics;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            if (isStarted)
            {
                Parallel.Invoke(DrawMap);
                isStarted = false;
            }
            if (isVictory)
            {
                graphics.DrawImage(Image.FromFile(Directory.GetCurrentDirectory() + "\\Resources\\background.jpg"), new Point(0, 0));
                Parallel.Invoke(DrawVictory);
            }
            else
            {
                Parallel.Invoke(DrawPlacement);
                Parallel.Invoke(DrawShot);
                Parallel.Invoke(DrawShips);
            }
        }

        private void DrawMap()
        {
            BackgroundImage = Image.FromFile(Directory.GetCurrentDirectory() + "\\Resources\\background.jpg");
            var graphics = Graphics.FromImage(BackgroundImage);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            for (var i = 0; i < Map.Width; i++)
            {
                for (var j = 0; j < Map.Height; j++)
                {
                    var corners = Geometry.GetHexCorners(MapPoints[i, j], HexRadius);
                    graphics.DrawPolygon(new Pen(Brushes.White, 2), corners);
                }
            }
            graphics.Dispose();
        }
        private void DrawShips()
        {
            for (var i = 0; i < Map.Width; i++)
            {
                for (var j = 0; j < Map.Height; j++)
                {
                    var corners = Geometry.GetHexCorners(MapPoints[i, j], HexRadius);
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
                        graphics.DrawPolygon(new Pen(Brushes.White, 2), corners);
                    }
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
                if (ticksCount > 0)
                {
                    DrawDamage(graphics);
                }
            }
            DrawShipInfo(graphics);
        }
        private void DrawMoving(Graphics graphics)
        {
            DrawSelectedShip(graphics);

            var borders = GetBordering(SelectedPoint.Value);
            foreach (var border in borders)
            {
                if (!(Map.Formation[border.X, border.Y] is Ship))
                {
                    var borderCorners = Geometry.GetHexCorners(MapPoints[border.X, border.Y], HexRadius);
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
                        var borderCorners = Geometry.GetHexCorners(MapPoints[border.X, border.Y], HexRadius);
                        graphics.FillPolygon(new SolidBrush(Color.FromArgb(100, 189, 189, 189)), borderCorners);
                        graphics.DrawPolygon(new Pen(Brushes.White, 2), borderCorners);
                    }
                }
            }
        }
        private void DrawSelectedShip(Graphics graphics)
        {
            var position = SelectedPoint.Value;
            var pointCorners = Geometry.GetHexCorners(MapPoints[position.X, position.Y], HexRadius);

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
                    graphics.FillPolygon(Brushes.Black, Geometry.GetRhombCorners(center, HexRadius));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case Carrier _:
                    graphics.FillPolygon(Brushes.Black, Geometry.GetRhombCorners(center, HexRadius));
                    break;

                case HeavyCruiser _:
                    graphics.FillPolygon(Brushes.Black, Geometry.GetTriangleCorners(center, false, HexRadius));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case Cruiser _:
                    graphics.FillPolygon(Brushes.Black, Geometry.GetTriangleCorners(center, false, HexRadius));
                    break;

                case Fregate _:
                    graphics.FillPolygon(Brushes.Black, Geometry.GetTriangleCorners(center, true, HexRadius));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case Gunship _:
                    graphics.FillPolygon(Brushes.Black, Geometry.GetTriangleCorners(center, true, HexRadius));
                    break;

                case Destroyer _:
                    graphics.FillPolygon(Brushes.Black, Geometry.GetSquareCorners(center, HexRadius));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case Freighter _:
                    graphics.FillPolygon(Brushes.Black, Geometry.GetSquareCorners(center, HexRadius));
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
        private void DrawShipInfo(Graphics graphics)
        {
            if ((MousePos.HasValue) && (MousePos != SelectedPoint))
            {
                var mouse = MousePos.Value;
                if (Map.Formation[mouse.X, mouse.Y] is Ship)
                {
                    var cegl = 9;
                    var ship = Map.Formation[mouse.X, mouse.Y] as Ship;
                    var text = SetInfoLabelText(ship);
                    var lineCount = text.Count(f => f == '\n');

                    var xSize = (ship.Armament.Length > 0)
                        ? 50
                        : 90;

                    foreach (var cannon in ship.Armament)
                    {
                        xSize += (cegl) * (2 + cannon.Power.ToString().Length + cannon.Range.ToString().Length);
                    }

                    var size = new Size(xSize, (int)((lineCount + 1) * cegl * 1.328));

                    var position = SetInfoLabelPos(MapPoints[mouse.X, mouse.Y], size);

                    var rect = new Rectangle(position, size);
                    var format = new StringFormat
                    {
                        LineAlignment = StringAlignment.Near,
                        Alignment = StringAlignment.Near
                    };
                    graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 57, 82, 89)), rect);
                    graphics.DrawString(text, new Font("Arial", cegl), Brushes.White, position, format);
                }
            }
        }
        private void DrawShot()
        {
            if (EnemyPoint.HasValue && SelectedPoint.HasValue)
            {
                var enemy = MapPoints[EnemyPoint.Value.X, EnemyPoint.Value.Y];
                var selectedPoint = MapPoints[SelectedPoint.Value.X, SelectedPoint.Value.Y];

                var circleRad = 10;
                var shotSpeed = 20;
                if (ticksCount == 0)
                {
                    distance += shotSpeed;
                }
                var Rab = Math.Sqrt((enemy.X - selectedPoint.X) * (enemy.X - selectedPoint.X)
                                    + (enemy.Y - selectedPoint.Y) * (enemy.Y - selectedPoint.Y));
                var koef = distance / Rab;
                var position = new Point
                {
                    X = (int)(selectedPoint.X + (enemy.X - selectedPoint.X) * koef),
                    Y = (int)(selectedPoint.Y + (enemy.Y - selectedPoint.Y) * koef)
                };

                if (distance >= Rab)
                {
                    distance = 0;
                }
                if (distance > 0)
                {
                    graphics.FillEllipse(Brushes.Yellow, position.X - circleRad, position.Y - circleRad, circleRad * 2, circleRad * 2);
                    return;
                }

                if (ticksCount < 40)
                {
                    ticksCount += 1;
                    DrawDamage(graphics);
                    return;
                }

                ticksCount = 0;
                EnemyPoint = null;
            }
        }
        private void DrawDamage(Graphics graphics)
        {
            var cegl = 14;
            var text = (damage > 0) ? "-" + damage.ToString() : "промах";
            var textFlySpeed = ticksCount / 3;
            var position = MapPoints[EnemyPoint.Value.X, EnemyPoint.Value.Y];
            position.Y = (position.Y - cegl * 1.328 - HexRadius < 0)
                 ? (int)(position.Y + cegl * 1.328 / 2 + textFlySpeed + HexRadius)
                 : (int)(position.Y - cegl * 1.328 / 2 - textFlySpeed - HexRadius);

            position.X += textFlySpeed;

            var rectSize = new Size(text.ToString().Length * cegl, (int)(cegl * 1.328));
            var rect = new Rectangle((int)position.X - rectSize.Width / 2,
                                     (int)(position.Y - rectSize.Height / 2),
                                     rectSize.Width,
                                     rectSize.Height);

            var format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };
            graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 57, 82, 89)), rect);
            graphics.DrawString(text, new Font("Microsoft Sans Serif", cegl), Brushes.Yellow, position, format);
        }
        private void DrawPlacement()
        {
            if (isPlacement)
            {
                foreach (var point in PlacementFields[Player])
                {
                    var corners = Geometry.GetHexCorners(MapPoints[point.X, point.Y], HexRadius);
                    graphics.FillPolygon(new SolidBrush(Color.FromArgb(100, 189, 189, 189)), corners);
                }
            }
        }
        private void DrawVictory()
        {
            Controls.Clear();
            Controls.Add(VictoryLabel);
            VictoryLabel.Text = "ВЫИГРАЛ ИГРОК " + (winner + 1);
            VictoryLabel.Visible = true;
        }

        private void DrawMainMenu()
        {
            var graphics = Graphics.FromImage(BackgroundImage);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            var text = "WARGAME";
            var cegl = 25;

            var fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(Directory.GetCurrentDirectory() + "\\Resources\\StarJedi-DGRW.ttf");
            var font = new Font(fontCollection.Families[0], cegl);

            var rectSize = new Size(cegl * text.Length + 40, (int)(cegl * 1.328) + 30);
            var position = new Point(ClientSize.Width / 2, 50);
            var rect = new Rectangle(position.X - rectSize.Width / 2, position.Y - rectSize.Height / 2, rectSize.Width, rectSize.Height);

            var format = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 57, 82, 89)), rect);
            graphics.DrawRectangle(new Pen(Brushes.White, 2), rect);
            graphics.DrawString(text, font, Brushes.Red, position, format);


        }
    }
}
