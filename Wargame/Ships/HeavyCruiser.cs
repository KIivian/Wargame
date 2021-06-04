namespace Wargame
{
    public class HeavyCruiser : Ship
    {
        public HeavyCruiser(byte owner)
            : base(
                  owner,
                  2,
                  2,
                  new Cannon[] { new Cannon(10, 5),
                        new Cannon(10, 1) },
                  50,
                  8)
        {
        }
    }
}

