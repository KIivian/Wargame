namespace Wargame
{
    public class Hexagon
    {
        public int Rotation
        {
            get;
            private set;
        }
        public Hexagon() { Rotation = 0; }
        public Hexagon(int rotation) { Rotation = rotation; }
    }
}
