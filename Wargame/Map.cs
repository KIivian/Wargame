using System.Drawing;
namespace Wargame
{
    public class Map
    {
        public Hexagon[,] Formation { get; private set; }
        public readonly int Height;
        public readonly int Width;

        public Map() { Formation = new Hexagon[25, 8]; Width = 25; Height = 8; }
        public Map(int height, int width) { Formation = new Hexagon[width, height]; Width = width; Height = height; }

        public void AddShip(int x, int y, ShipType type, byte owner)
        {
            if (Formation[x, y] == null)
            {
                Formation[x, y] = new Ship(type, owner);
            }
        }

        public void MoveShip(int hexX, int hexY, int newX, int newY)
        {
            Formation[newX, newY] = Formation[hexX, hexY];
            Formation[hexX, hexY] = null;
        }

        public void MoveShip(Point oldPos, Point newPos)
        {
            Formation[newPos.X, newPos.Y] = Formation[oldPos.X, oldPos.Y];
            Formation[oldPos.X, oldPos.Y] = null;
        }
        public void DestroyShip(Point position)
        {
            Formation[position.X, position.Y] = null;
        }
    }
}
