using System;
using System.Drawing;

namespace Wargame
{
    public class Map
    {
        public Hexagon[,] Formation { get; private set; }

        public Map() { Formation = new Hexagon[9, 15]; }
        public Map(int height, int width) { Formation = new Hexagon[height, width]; }

        public void AddShip (int x, int y, ShipType type, byte owner)
        {
            if (Formation[x, y] == null)
            {
                Formation[x, y] = new Ship(type, owner);
            }
        }

        public void MoveShip(int hexX, int hexY, int newX, int newY)
        {
                Formation[newX, newX] = Formation[hexX, hexY];
                Formation[hexX, hexY] = new Hexagon();
        }
    }
    public enum ShipType
    {
        battleship,
        carrier,
        heavyCruiser,
        cruiser,
        fregate,
        gunship,
        destroyer,
        freighter,
        transport,
        fighter,
        doubleFighter
    }

    public class Hexagon
    {
        public double Rotation
        {
            get;
            private set;
        }

        public void SetRotation(double rotation)
        {
            Rotation = rotation;
        }
        public Hexagon() { Rotation = 0; }
        public Hexagon(double rotation) { Rotation = rotation; }
    }

    public class Ship : Hexagon
    {
        readonly byte player;
        readonly int fireRange;
        readonly int speed;
        readonly int evasion;
        readonly int[] armament;
        readonly int price;
        readonly ShipType shipType;
        public int Shields { get; private set; }

        public Ship(ShipType type, byte owner)
        {
            switch (type)
            {
                case ShipType.battleship:
                    player = owner;
                    fireRange = 5;
                    speed = 2;
                    evasion = 1;
                    armament = new int[] { 20, 20 };
                    Shields = 70;
                    price = 10;
                    break;

                case ShipType.carrier:
                    player = owner;
                    fireRange = 2;
                    speed = 1;
                    evasion = 1;
                    armament = new int[] { 6 };
                    Shields = 50;
                    price = 10;
                    break;

                case ShipType.heavyCruiser:
                    player = owner;
                    fireRange = 5;
                    speed = 2;
                    evasion = 2;
                    armament = new int[] { 10, 10 };
                    Shields = 50;
                    price = 8;
                    break;

                case ShipType.cruiser:
                    player = owner;
                    fireRange = 3;
                    speed = 3;
                    evasion = 3;
                    armament = new int[] { 12 };
                    Shields = 30;
                    price = 5;
                    break;

                case ShipType.fregate:
                    player = owner;
                    fireRange = 3;
                    speed = 3;
                    evasion = 3;
                    armament = new int[] { 10 };
                    Shields = 20;
                    price = 4;
                    break;

                case ShipType.gunship:
                    player = owner;
                    fireRange = 10;
                    speed = 1;
                    evasion = 1;
                    armament = new int[] { 20 };
                    Shields = 10;
                    price = 7;
                    break;

                case ShipType.destroyer:
                    player = owner;
                    fireRange = 3;
                    speed = 3;
                    evasion = 3;
                    armament = new int[] { 4, 4, 4 };
                    Shields = 10;
                    price = 6;
                    break;

                case ShipType.freighter:
                    player = owner;
                    fireRange = 2;
                    speed = 4;
                    evasion = 4;
                    armament = new int[] { 10 };
                    Shields = 15;
                    price = 4;
                    break;

                case ShipType.transport:
                    player = owner;
                    fireRange = 0;
                    speed = 5;
                    evasion = 1;
                    armament = new int[0];
                    Shields = 60;
                    price = 8;
                    break;

                case ShipType.fighter:
                    player = owner;
                    fireRange = 1;
                    speed = 4;
                    evasion = 5;
                    armament = new int[] { 6 };
                    Shields = 0;
                    price = 0;
                    break;

                case ShipType.doubleFighter:
                    player = owner;
                    fireRange = 1;
                    speed = 4;
                    evasion = 5;
                    armament = new int[] { 6, 6 };
                    Shields = 0;
                    price = 0;
                    break;
            }
        }


        public void TakeDamage(int damage)
        {
            if (damage > evasion)
            {
                Shields -= damage;
                if (Shields < 0)
                {
                    Shields = 0;
                }
            }
        }

        public void TakeDamageWithoutEvasion(int damage)
        {
            Shields -= damage;
            if (Shields < 0)
            {
                Shields = 0;
            }
        }

        public int Fire()
        {
            var result = 0;
            var rand = new Random();
            rand.Next();
            foreach (var cannon in armament)
            {
                result += rand.Next() % cannon;
            }

            return result;
        }
    }

    private PointF[] GetHexCorners(PointF center, double size)
    {
        var corners = new PointF[6];
        for (var i = 0; i < 6; i++)
        {
            var angle_deg = 60 * i + 30;
            var angle_rad = Math.PI / 180 * angle_deg;
            corners[i].X = (float)(center.X + size * Math.Cos(angle_rad));
            corners[i].Y = (float)(center.Y + size * Math.Sin(angle_rad));
        }
        return corners;
    }
}
