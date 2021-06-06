namespace Wargame
{
    public class Transport : Ship
    {
        public Transport(byte owner)
            : base(
                  owner,
                  5,
                  0,
                  new Cannon[0],
                  60,
                  8)
        {
        }
    }
}

