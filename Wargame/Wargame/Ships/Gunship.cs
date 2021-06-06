namespace Wargame
{
    public class Gunship : Ship
    {
        public Gunship(byte owner)
            : base(
                  owner,
                  1,
                  1,
                  new Cannon[] { new Cannon(20, 10) },
                  10,
                  7)
        {
        }
    }
}

