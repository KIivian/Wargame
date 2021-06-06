using System.Drawing;
using System.Collections.Generic;
namespace Wargame
{
    public class Map
    {
        public Hexagon[,] Formation { get; private set; }
        public readonly int Height;
        public readonly int Width;

        public Map() { Formation = new Hexagon[25, 8]; Width = 25; Height = 8; }
        public Map(int height, int width) { Formation = new Hexagon[width, height]; Width = width; Height = height; }

        public void AddShip(int x, int y, Ship ship, byte owner)
        {
            if (Formation[x, y] == null)
            {
                switch (ship)
                {
                    case Battleship _:
                        Formation[x, y] = new Battleship(owner);
                        return;
                    case Carrier _:
                        Formation[x, y] = new Carrier(owner);
                        return;
                    case HeavyCruiser _:
                        Formation[x, y] = new HeavyCruiser(owner);
                        return;
                    case Cruiser _:
                        Formation[x, y] = new Cruiser(owner);
                        return;
                    case Fregate _:
                        Formation[x, y] = new Fregate(owner);
                        return;
                    case Gunship _:
                        Formation[x, y] = new Gunship(owner);
                        return;
                    case Destroyer _:
                        Formation[x, y] = new Destroyer(owner);
                        return;
                    case Freighter _:
                        Formation[x, y] = new Freighter(owner);
                        return;
                    case Transport _:
                        Formation[x, y] = new Transport(owner);
                        return;
                    case Fighter _:
                        Formation[x, y] = new Fighter(owner);
                        return;
                    case DoubleFighter _:
                        Formation[x, y] = new DoubleFighter(owner);
                        return;

                }
            }
        }

        public void MoveShip(Point oldPos, Point newPos)
        {
            var oldPosition = Formation[oldPos.X, oldPos.Y];
            var newPosition = Formation[newPos.X, newPos.Y];
            if (oldPosition is Carrier)
            {
                (Formation[oldPos.X, oldPos.Y] as Carrier).DropedFighters = true;
            }
            
            if ((oldPosition is Fighter) && (newPosition is Fighter))
            {
                var secondFighterMovement = (newPosition as Fighter).Movement;
                var secondCannon = (newPosition as Fighter).Armament[0];

                newPosition = new DoubleFighter((oldPosition as Ship).Player);

                (newPosition as DoubleFighter).FirstFighterMovement = (oldPosition as Fighter).Movement - 1;
                (newPosition as DoubleFighter).SecondFighterMovement = secondFighterMovement;

                (newPosition as DoubleFighter).Armament[0] = (oldPosition as Ship).Armament[0];
                (newPosition as DoubleFighter).Armament[1] = secondCannon;
                Formation[newPos.X, newPos.Y] = newPosition;
            }
            else
            {
                Formation[newPos.X, newPos.Y] = oldPosition;
            }
            Formation[oldPos.X, oldPos.Y] = null;
        }

        public void MoveDoubleFighter(Point oldPos, Point newPos, bool firstFighterMoving)
        {

            var ship = Formation[oldPos.X, oldPos.Y] as DoubleFighter;
            var player = ship.Player;
            var firstFighterMovement = ship.FirstFighterMovement;
            var secondFighterMovement = ship.SecondFighterMovement;

            var firstCannon =ship.Armament[0];
            var secondCannon = ship.Armament[1];

            Formation[oldPos.X, oldPos.Y] = new Fighter(player);

            (Formation[oldPos.X, oldPos.Y] as Fighter).Movement =
                    firstFighterMoving
                    ? secondFighterMovement
                    : firstFighterMovement;
            (Formation[oldPos.X, oldPos.Y] as Fighter).Armament[0] =
               firstFighterMoving
               ? secondCannon
               : firstCannon;

            if (Formation[newPos.X, newPos.Y] is null)
            {
                Formation[newPos.X, newPos.Y] = new Fighter(player);

                (Formation[newPos.X, newPos.Y] as Fighter).Movement =
                    firstFighterMoving
                    ? (firstFighterMovement - 1)
                    : (secondFighterMovement - 1);
                (Formation[newPos.X, newPos.Y] as Fighter).Armament[0] =
                    firstFighterMoving
                    ? firstCannon
                    : secondCannon;
            }

            else if (Formation[newPos.X, newPos.Y] is Fighter)
            {
                var thirdFighterMovement = (Formation[newPos.X, newPos.Y] as Ship).Movement;
                var thirdCannon = (Formation[newPos.X, newPos.Y] as Ship).Armament[0];

                Formation[newPos.X, newPos.Y] = new DoubleFighter(player);

                (Formation[newPos.X, newPos.Y] as DoubleFighter).FirstFighterMovement =
                    firstFighterMoving
                    ? (firstFighterMovement - 1)
                    : (secondFighterMovement - 1);
                (Formation[newPos.X, newPos.Y] as DoubleFighter).SecondFighterMovement = thirdFighterMovement;
                
                (Formation[newPos.X, newPos.Y] as DoubleFighter).Armament[0] =
                   firstFighterMoving
                   ? firstCannon
                   : secondCannon;
                (Formation[newPos.X, newPos.Y] as DoubleFighter).Armament[1] = thirdCannon;
            }
        }

        public void DropFighters(List <Point> neighbors, Carrier carrier)
        {
            var player = carrier.Player;
            foreach (var hex in neighbors)
            {
                if (carrier.FightersCount > 0)
                {
                    if (Formation[hex.X, hex.Y] == null)
                    {
                        Formation[hex.X, hex.Y] = new Fighter(player);
                    }
                    else if ((Formation[hex.X, hex.Y] is Fighter) &&((Formation[hex.X, hex.Y] as Ship).Player == player))
                    {
                        var fighter = Formation[hex.X, hex.Y] as Fighter;
                        var fighterMovement = fighter.Movement;
                        var fighterCannon = new Cannon(fighter.Armament[0].Power,fighter.Armament[0].Range);

                        Formation[hex.X, hex.Y] = new DoubleFighter(carrier.Player);
                        (Formation[hex.X, hex.Y] as DoubleFighter).SecondFighterMovement = fighterMovement;
                        (Formation[hex.X, hex.Y] as Ship).Armament[1] = fighterCannon;
                    }
                }
                else 
                {
                    carrier.DropedFighters = true;
                    return; 
                }
                carrier.FightersCount -= 1;
            }
            carrier.DropedFighters = true;
            carrier.Movement = 0;

            foreach (var gun in carrier.Armament)
            {
                gun.Fire();
            }
        }
    }
}
