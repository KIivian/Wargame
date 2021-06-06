using System.Collections.Generic;
namespace Wargame
{
    public class Ship : Hexagon
    {
        public byte Player { get; private set; }
        public int Speed { get; private set; }
        public int Evasion { get; private set; }
        public Cannon[] Armament { get; private set; }
        public int Price { get; private set; }
        public int Shields { get; private set; }
        public int Movement { get; set; }

        public Ship(byte owner, int speed, int evasion, Cannon[] armament, int shields, int price)
        {
            Speed = speed;
            Evasion = evasion;
            Armament = armament;
            Shields = shields;
            Price = price;
            Movement = speed;
            Player = owner;
        }


        public Ship TakeDamage(int damage)
        {
            if (damage > Evasion)
            {
                if (Shields == 0)
                {
                    if (this is DoubleFighter)
                    {
                        return new Fighter(Player);
                    }
                    return null;
                }
                Shields -= damage;
                if (Shields < 0)
                {
                    Shields = 0;
                }
            }
            return this;
        }
        public Ship TakeDamageWithoutEvasion(int damage)
        {
            if (Shields == 0)
            {
                if (this is DoubleFighter)
                {
                    return new Fighter(Player);
                }
                return  null;
            }
            Shields -= damage;
            if (Shields < 0)
            {
                Shields = 0;
            }
            return this;
        }
        public virtual void Refresh()
        {
            Movement = Speed;
            foreach (var cannon in Armament)
            {
                cannon.Charge();
            }
        }
    }
}

