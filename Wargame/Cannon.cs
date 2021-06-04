using System;
namespace Wargame
{
    public class Cannon
    {
        public int Power { get; private set; }
        public int Range { get; private set; }
        public bool IsFired { get; private set; }
        public Cannon(int power, int range)
        {
            Power = power;
            Range = range;
            IsFired = false;
        }

        public int Fire()
        {
            IsFired = true;
            var result = 0;
            var currentRes = 0;
            do
            {
                currentRes = new Random().Next() % (Power + 1);
                result += currentRes;
            }
            while (currentRes == Power);
            return result;
        }

        public void Charge()
        {
            IsFired = false;
        }
    }
}
