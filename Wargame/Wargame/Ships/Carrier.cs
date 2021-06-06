namespace Wargame
{
    public class Carrier : Ship
    {
        public int FightersCount { get; set; }
        public bool DropedFighters { get; set; }
        public Carrier(byte owner)
            : base(
                  owner,
                  1,
                  1,
                  new Cannon[] { new Cannon(6, 2) },
                  50,
                  10)
        {
            DropedFighters = false;
            FightersCount = 12;
        }
        public override void Refresh()
        {
            Movement = Speed;
            DropedFighters = false;
            foreach (var cannon in Armament)
            {
                cannon.Charge();
            }
        }
    }
}

