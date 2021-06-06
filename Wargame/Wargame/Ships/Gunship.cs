namespace Wargame
{
    public class Gunship : Ship
    {
        public bool IsDoubleFired { get; set; }
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
        public override void Refresh()
        {
            base.Refresh();
            IsDoubleFired = false;
        }
    }
}

