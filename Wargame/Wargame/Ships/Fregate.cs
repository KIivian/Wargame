namespace Wargame
{
    public class Fregate : Ship
    {
        public Fregate(byte owner)
            : base(
                  owner,
                  3,
                  3,
                  new Cannon[] { new Cannon(10, 3), new Cannon(8, 1) },
                  20,
                  4)
        {
        }
    }
}

