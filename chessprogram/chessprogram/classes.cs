using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace chessprogram
{
    class piece
    {
        private moveBehavior pieceBehavior;
    }

    interface moveBehavior
    {
        //public moveBehavior();
        public bool canMove(position from,position to);
        public List<position> possibleMoves(position cur);
    }
    public class blackPawnBehavior : moveBehavior
    {
        //public blackPawnBehavior();
        public bool canMove(position from, position to)
        {
            if (to.y - from.y > 2) //if moving more than 2 spots
            {
                return false;
            }
            else if (to.y - from.y == 2)
            {
                if (from.y > 1) //if moving two spots in the middle of the board
                    return false;
                else if (from.x - to.x != 0)  //if moving 2 spots and some x
                    return false;
                else //if moving 2 spots
                    return true;
            }
            else if (to.y - from.y <= 0)//if moving backwards
                return false;
            else if (to.y > 7)//if moving past end
                return false;
            else
            {
                if (from.x - to.x == 0) //if moving 1
                    return true;
                else if ((from.x - to.x > 1) || (from.x - to.x < -1))
                    return false;
                else if ((to.x < 0) || (to.x > 7)) //if killing out of board
                    return false;
                else //if killing
                    return true;
            }
                
        }

        public List<position> possibleMoves(position cur)
        {
            List<position> retList = new List<position>();
            position to = cur;

            to.y = to.y + 1;
            if (canMove(cur, to))
                retList.Add(to);
            to.x = to.x + 1;
            if (canMove(cur, to))
                retList.Add(to);
            to.x = to.x - 2;
            if (canMove(cur, to))
                retList.Add(to);
            to.x = to.x + 1;
            to.y = to.y + 1;
            if (canMove(cur, to))
                retList.Add(to);
            return retList;
        }
    }
    public class whitePawnBehavior : moveBehavior
    {
        //public whitePawnBehavior();
        public bool canMove(position from, position to)
        {
            if (to.y - from.y < -2) //if moving more than 2 spots
            {
                return false;
            }
            else if (to.y - from.y == -2)
            {
                if (from.y < 6) //if moving two spots in the middle of the board
                    return false;
                else if (from.x - to.x != 0)  //if moving 2 spots and some x
                    return false;
                else //if moving 2 spots
                    return true;
            }
            else if (to.y - from.y >= 0)//if moving backwards
                return false;
            else if (to.y < 0)//if moving past end
                return false;
            else
            {
                if (from.x - to.x == 0) //if moving 1
                    return true;
                else if ((from.x - to.x > 1) || (from.x - to.x < -1))
                    return false;
                else if ((to.x < 0) || (to.x > 7)) //if killing out of board
                    return false;
                else //if killing
                    return true;
            }

        }

        public List<position> possibleMoves(position cur)
        {
            List<position> retList = new List<position>();
            position to = cur;

            to.y = to.y - 1;
            if (canMove(cur, to))
                retList.Add(to);
            to.x = to.x + 1;
            if (canMove(cur, to))
                retList.Add(to);
            to.x = to.x - 2;
            if (canMove(cur, to))
                retList.Add(to);
            to.x = to.x + 1;
            to.y = to.y - 1;
            if (canMove(cur, to))
                retList.Add(to);
            return retList;
        }
    }
    public class knightBehavior : moveBehavior
    {
        public bool canMove(position from, position to)
        {
            return true;
        }
    }
    struct position
    {
       public int x;
        public int y;
    }
}
