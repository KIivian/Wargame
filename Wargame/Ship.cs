using System.Collections.Generic;
namespace Wargame
{
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

    public class Ship : Hexagon
    {
        public readonly byte player;
        public readonly int speed;
        public readonly int evasion;
        public Cannon[] Armament { get; private set; }
        public readonly int price;
        public ShipType Type { get; private set; }
        public bool IsDestroyed { get; private set; }
        public int Shields { get; private set; }
        public int Movement { get; private set; }

        public Ship(ShipType type, byte owner)
        {
            switch (type)
            {
                case ShipType.battleship:
                    speed = 2;
                    evasion = 1;
                    Armament = new Cannon[] {
                        new Cannon(20, 5),
                        new Cannon(10, 1)};
                    Shields = 70;
                    price = 10;
                    break;

                case ShipType.carrier:
                    speed = 1;
                    evasion = 1;
                    Armament = new Cannon[] { new Cannon(6, 2) };
                    Shields = 50;
                    price = 10;
                    break;

                case ShipType.heavyCruiser:
                    speed = 2;
                    evasion = 2;
                    Armament = new Cannon[] { new Cannon(10, 5),
                        new Cannon(10, 1) };
                    Shields = 50;
                    price = 8;
                    break;

                case ShipType.cruiser:
                    speed = 3;
                    evasion = 3;
                    Armament = new Cannon[] { new Cannon(12, 3), new Cannon(10, 1) };
                    Shields = 30;
                    price = 5;
                    break;

                case ShipType.fregate:
                    speed = 3;
                    evasion = 3;
                    Armament = new Cannon[] { new Cannon(10, 3), new Cannon(8, 1) };
                    Shields = 20;
                    price = 4;
                    break;

                case ShipType.gunship:
                    speed = 1;
                    evasion = 1;
                    Armament = new Cannon[] { new Cannon(20, 10) };
                    Shields = 10;
                    price = 7;
                    break;

                case ShipType.destroyer:
                    speed = 3;
                    evasion = 3;
                    Armament = new Cannon[] { new Cannon(4, 4), new Cannon(4, 4), new Cannon(4, 4) };
                    Shields = 10;
                    price = 6;
                    break;

                case ShipType.freighter:
                    speed = 4;
                    evasion = 4;
                    Armament = new Cannon[] { new Cannon(10, 2) };
                    Shields = 15;
                    price = 4;
                    break;

                case ShipType.transport:
                    speed = 5;
                    evasion = 1;
                    Armament = new Cannon[0];
                    Shields = 60;
                    price = 8;
                    break;

                case ShipType.fighter:
                    speed = 4;
                    evasion = 5;
                    Armament = new Cannon[] { new Cannon(6, 1) };
                    Shields = 0;
                    price = 0;
                    break;

                case ShipType.doubleFighter:
                    speed = 4;
                    evasion = 5;
                    Armament = new Cannon[] { new Cannon(6, 1), new Cannon(6, 1) };
                    Shields = 0;
                    price = 0;
                    break;
            }
            Movement = speed;
            player = owner;
            Type = type;
        }


        public void TakeDamage(int damage)
        {
            if (damage > evasion)
            {
                if (Shields == 0)
                {
                    if (Type == ShipType.doubleFighter)
                    {
                        Type = ShipType.fighter;
                        Armament = new Cannon[] { new Cannon(6, 1) };
                        return;
                    }
                    IsDestroyed = true;
                    return;
                }
                Shields -= damage;
                if (Shields < 0)
                {
                    Shields = 0;
                }
            }
        }

        public void TakeDamageWithoutEvasion(int damage)
        {
            if (Shields == 0)
            {
                IsDestroyed = true;
                return;
            }
            Shields -= damage;
            if (Shields < 0)
            {
                Shields = 0;
            }
        }
        public void Refresh()
        {
            Movement = speed;
            foreach (var cannon in Armament)
            {
                cannon.Charge();
            }
        }
    }
}
