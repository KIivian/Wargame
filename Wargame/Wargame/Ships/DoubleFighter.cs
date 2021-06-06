namespace Wargame
{
    public class DoubleFighter : Ship
    {
        public int FirstFighterMovement { get; set; }
        public int SecondFighterMovement { get; set; }
        public DoubleFighter(byte owner)
            : base(
                  owner,
                  4,
                  5,
                  new Cannon[] { new Cannon(6, 1), new Cannon(6, 1) },
                  0,
                  0)
        {
            FirstFighterMovement = Speed;
            SecondFighterMovement = Speed;
        }
    }
}

