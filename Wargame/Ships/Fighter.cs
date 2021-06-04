namespace Wargame
{
    public class Fighter : Ship
    {
        public Fighter(byte owner)
            : base(
                  owner,
                  4,
                  5,
                  new Cannon[] { new Cannon(6, 1) },
                  0,
                  0)
        {
        }
    }
}

