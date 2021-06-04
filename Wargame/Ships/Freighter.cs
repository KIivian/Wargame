namespace Wargame
{
    public class Freighter : Ship
    {
        public Freighter(byte owner)
            : base(
                  owner,
                  4,
                  4,
                  new Cannon[] { new Cannon(10, 2) },
                  15,
                  4)
        {
        }
    }
}

