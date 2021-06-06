namespace Wargame
{
    public class Battleship : Ship
    {
        public Battleship(byte owner)
            : base(
                  owner,
                  2,
                  1,
                  new Cannon[] {new Cannon(20, 5),
                        new Cannon(10, 1)},
                  70,
                  10)
        { }
    }
}

