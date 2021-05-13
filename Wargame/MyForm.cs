using System.Drawing;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Wargame
{
    partial class MyForm : Form
    {
        private Map Map;
        private float HexRadius;
        private float HalfHexWidth;
        private PointF[,] MapPoints;
        private Point MousePos;
        private Point? SelectedPoint;
        private Label MouseControlLabel;
        private Label ShipShieldsLabel;
        private Button ShipMovementButton;
        private Button ShipFiringButton;
        private bool isFiring;
        private bool isMoving;

        public MyForm()
        {
            DoubleBuffered = true;
            Text = "Wargame";
            BackgroundImage = Image.FromFile(Directory.GetCurrentDirectory() + "\\Images\\background.jpg");

            HexRadius = 25;
            HalfHexWidth = (float)Math.Sqrt(0.75 * HexRadius * HexRadius);
            Map = new Map();
            MapPoints = GetMapPoints();
            MousePos = CheckMouse();
            ClientSize = new Size((int)((Map.Width + 1) * HalfHexWidth) + 210, (int)((Map.Height * 3 + 0.5) * HexRadius));
            SelectedPoint = null;


            //добавление кораблей для тестов. В итоговом коде будет заменено
            Map.AddShip(0, 0, ShipType.freighter, 1);
            Map.AddShip(2, 1, ShipType.destroyer, 1);
            Map.AddShip(3, 2, ShipType.fighter, 1);
            Map.AddShip(3, 3, ShipType.doubleFighter, 1);
            Map.AddShip(5, 3, ShipType.cruiser, 2);
            Map.AddShip(10, 3, ShipType.battleship, 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            DrawMap(graphics);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MousePos = CheckMouse();
            MouseControlLabel.Text = MousePos.X.ToString() + ';' + MousePos.Y.ToString();
            MouseControlLabel.Update();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var hex = Map.Formation[MousePos.X, MousePos.Y];
            if ((hex is Ship) && (!isFiring))
            {
                SelectedPoint = new Point(MousePos.X, MousePos.Y);
                ShipFiringButton.Visible = true;
                ShipMovementButton.Visible = true;
                ShipShieldsLabel.Visible = true;
                ShipShieldsLabel.Text = "Shields: " + (hex as Ship).Shields.ToString();
                isFiring = false;
                isMoving = false;
                ShipFiringButton.BackColor = Color.White;
                ShipMovementButton.BackColor = Color.White;
            }

            if (SelectedPoint.HasValue)
            {
                var selectedPoint = SelectedPoint.Value;
                var selectedShip = Map.Formation[selectedPoint.X, selectedPoint.Y] as Ship;
                if ((hex == null) && isMoving && GetBordering(selectedPoint).Contains(MousePos))
                {
                    Map.MoveShip(selectedPoint, MousePos);
                    isMoving = false;
                    SelectedPoint = null;

                    ShipMovementButton.BackColor = Color.White;
                    ShipFiringButton.Visible = false;
                    ShipMovementButton.Visible = false;
                    ShipShieldsLabel.Visible = false;
                }
                if ((hex is Ship) &&
                    (selectedPoint != MousePos) &&
                    (selectedShip.Armament.Length > 0) &&
                    isFiring &&
                    ((hex as Ship).player != selectedShip.player) &&
                    GetBorderingInRange(selectedPoint, selectedShip.Armament[0].Range).Contains(MousePos))
                {
                    var enemy = hex as Ship;
                    if (selectedShip.Type == ShipType.destroyer)
                    {
                        enemy.TakeDamageWithoutEvasion(selectedShip.Armament[0].Fire());
                    }
                    else
                    {
                        enemy.TakeDamage(selectedShip.Armament[0].Fire());
                    }

                    if (enemy.IsDestroyed)
                    {
                        Map.DestroyShip(MousePos);
                    }

                    isFiring = false;
                    ShipFiringButton.BackColor = Color.White;
                    ShipFiringButton.Visible = false;
                    ShipMovementButton.Visible = false;
                    ShipShieldsLabel.Visible = false;
                }
            }
            Invalidate();
        }

        private Point CheckMouse()
        {
            MousePos = PointToClient(MousePosition);
            var result = new Point();
            var minDistance = HexRadius + 1;

            for (var i = 0; i < Map.Width; i++)
            {
                for (var j = 0; j < Map.Height; j++)
                {
                    var distance = GetDistance(MousePos, MapPoints[i, j]);
                    if (distance < minDistance)
                    {
                        minDistance = (float)distance;
                        result = new Point(i, j);
                    }
                }
            }
            return result;
        }

        private PointF[] GetHexCorners(PointF center)
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

        private double GetDistance(PointF p1, PointF p2) => Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MouseControlLabel = new Label
            {
                Top = 20,
                Left = (int)((Map.Width + 1) * HalfHexWidth + 10),
                Text = "not on the map",
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(70, 30)
            };
            Controls.Add(MouseControlLabel);

            ShipShieldsLabel = new Label
            {
                Top = 60,
                Left = (int)((Map.Width + 1) * HalfHexWidth + 10),
                Size = new Size(70, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            Controls.Add(ShipShieldsLabel);

            ShipFiringButton = new Button
            {
                Top = 100,
                Left = (int)((Map.Width + 1) * HalfHexWidth + 10),
                Size = new Size(60, 40),
                Text = "Fire",
                Visible = false
            };
            Controls.Add(ShipFiringButton);
            ShipFiringButton.Click += ShipFiringButton_Click;

            ShipMovementButton = new Button
            {
                Top = 100,
                Left = (int)((Map.Width + 1) * HalfHexWidth + 70),
                Size = new Size(60, 40),
                Text = "Move",
                Visible = false
            };
            Controls.Add(ShipMovementButton);
            ShipMovementButton.Click += ShipMovementButton_Click;
        }

        private List<Point> GetBordering(Point point)
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

        private List<Point> GetBorderingInRange(Point point, int range)
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

        private void ShipMovementButton_Click(object sender, EventArgs e)
        {
            if (!isMoving)
            {
                isMoving = true;
                isFiring = false;
                ShipMovementButton.BackColor = Color.Red;
                ShipFiringButton.BackColor = Color.White;
            }
            else
            {
                isMoving = false;
                ShipMovementButton.BackColor = Color.White;
            }
            ShipMovementButton.Update();
            ShipFiringButton.Update();
            Invalidate();
        }

        private void ShipFiringButton_Click(object sender, EventArgs e)
        {
            if (!isFiring)
            {
                isFiring = true;
                isMoving = false;
                ShipFiringButton.BackColor = Color.Red;
                ShipMovementButton.BackColor = Color.White;
            }
            else
            {
                isFiring = false;
                ShipFiringButton.BackColor = Color.White;
            }
            ShipMovementButton.Update();
            ShipFiringButton.Update();
            Invalidate();
        }

        private void DrawMap(Graphics graphics)
        {
            for (var i = 0; i < Map.Width; i++)
            {
                for (var j = 0; j < Map.Height; j++)
                {
                    var corners = GetHexCorners(MapPoints[i, j]);
                    if (Map.Formation[i, j] is Ship)
                    {
                        if ((Map.Formation[i, j] as Ship).player == 1)
                        {
                            graphics.FillPolygon(Brushes.Blue, corners);
                        }
                        else
                        {
                            graphics.FillPolygon(Brushes.Red, corners);
                        }
                        DrawShipIcon((Map.Formation[i, j] as Ship).Type, MapPoints[i, j], graphics);
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
                    var selectedShip = Map.Formation[SelectedPoint.Value.X, SelectedPoint.Value.Y] as Ship;
                    DrawFiring(graphics, selectedShip.Armament[0].Range);
                }
            }
        }

        private void DrawMoving(Graphics graphics)
        {
            var point = SelectedPoint.Value;
            var pointCorners = GetHexCorners(MapPoints[point.X, point.Y]);

            graphics.FillPolygon(new SolidBrush(Color.FromArgb(255, 242, 176, 10)), pointCorners);
            DrawShipIcon((Map.Formation[point.X, point.Y] as Ship).Type, MapPoints[point.X, point.Y], graphics);
            graphics.DrawPolygon(new Pen(Brushes.White, 2), pointCorners);

            var borders = GetBordering(point);
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
            var point = SelectedPoint.Value;
            var pointCorners = GetHexCorners(MapPoints[point.X, point.Y]);

            graphics.FillPolygon(new SolidBrush(Color.FromArgb(255, 242, 176, 10)), pointCorners);
            DrawShipIcon((Map.Formation[point.X, point.Y] as Ship).Type, MapPoints[point.X, point.Y], graphics);
            graphics.DrawPolygon(new Pen(Brushes.White, 2), pointCorners);

            var borders = GetBorderingInRange(point, range);
            foreach (var border in borders)
            {
                if (!(Map.Formation[border.X, border.Y] is Ship))
                {
                    var borderCorners = GetHexCorners(MapPoints[border.X, border.Y]);
                    graphics.FillPolygon(new SolidBrush(Color.FromArgb(100, 189, 189, 189)), borderCorners);
                    graphics.DrawPolygon(new Pen(Brushes.White, 2), borderCorners);
                }
                else
                {
                    var centerX = MapPoints[border.X, border.Y].X;
                    var centerY = MapPoints[border.X, border.Y].Y;
                }
            }
        }

        private void DrawShipIcon(ShipType type, PointF center, Graphics graphics)
        {
            var circleSize = HexRadius / 3;
            switch (type)
            {
                case ShipType.battleship:
                    graphics.FillPolygon(Brushes.Black, GetRhombCorners(center));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case ShipType.carrier:
                    graphics.FillPolygon(Brushes.Black, GetRhombCorners(center));
                    break;

                case ShipType.heavyCruiser:
                    graphics.FillPolygon(Brushes.Black, GetTriangleCorners(center, false));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case ShipType.cruiser:
                    graphics.FillPolygon(Brushes.Black, GetTriangleCorners(center, false));
                    break;

                case ShipType.fregate:
                    graphics.FillPolygon(Brushes.Black, GetTriangleCorners(center, true));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case ShipType.gunship:
                    graphics.FillPolygon(Brushes.Black, GetTriangleCorners(center, true));
                    break;

                case ShipType.destroyer:
                    graphics.FillPolygon(Brushes.Black, GetSquareCorners(center));
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;


                case ShipType.freighter:
                    graphics.FillPolygon(Brushes.Black, GetSquareCorners(center));
                    break;

                case ShipType.transport:
                    graphics.FillEllipse(Brushes.White, center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                    graphics.FillEllipse(Brushes.White, center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    break;

                case ShipType.fighter:
                    graphics.FillEllipse(Brushes.White, center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                    graphics.FillRectangle(Brushes.Black, center.X - circleSize / 6, center.Y - circleSize / 2, circleSize / 3, circleSize);
                    break;

                case ShipType.doubleFighter:
                    var bigRect = new RectangleF(center.X - circleSize / 2, center.Y - circleSize / 2, circleSize, circleSize);
                    var smallRect = new RectangleF(center.X - circleSize / 6, center.Y - circleSize / 2 - 1, circleSize / 3, circleSize + 2);

                    graphics.FillEllipse(Brushes.White, center.X - circleSize, center.Y - circleSize, circleSize * 2, circleSize * 2);
                    graphics.FillRectangle(Brushes.Black, bigRect);
                    graphics.FillRectangle(Brushes.White, smallRect);
                    break;

            }
        }
    }
}
