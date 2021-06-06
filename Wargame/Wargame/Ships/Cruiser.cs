namespace Wargame
{
    public class Cruiser : Ship
    {
        public Cruiser(byte owner)
            : base(
                  owner,
                  3,
                  3,
                  new Cannon[] { new Cannon(12, 3), new Cannon(10, 1) },
                  30,
                  5)
        {
        }
    }
}

