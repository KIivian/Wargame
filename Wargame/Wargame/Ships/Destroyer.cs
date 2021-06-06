namespace Wargame
{
    public class Destroyer : Ship
    {
        public Destroyer(byte owner)
            : base(
                  owner,
                  3,
                  3,
                  new Cannon[] { new Cannon(4, 4), new Cannon(4, 4), new Cannon(4, 4) },
                  10,
                  6)
        {
        }
    }
}

