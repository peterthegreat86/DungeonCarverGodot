namespace DungeonCarver
{
    using global::Godot;
    using System.Collections.Generic;

    public class MapUtils
    {
        public enum CardinalFourDirections
        {
            NORTH = 0,
            EAST = 1,
            WEST = 2,
            SOUTH = 3
        }

        /// <summary>
        /// Generic list of points which contain 4 directions
        /// </summary>
        public static List<Vector2I> FourDirections = new List<Vector2I>()
        {
            new Vector2I (0,1)    //north
            , new Vector2I(0,-1)    //south
            , new Vector2I (1,0)   //east
            , new Vector2I (-1,0)  //west
        };

        /// <summary>
        /// Generic list of points which contain 9 directions
        /// </summary>
        public static List<Vector2I> NineDirections = new List<Vector2I>()
        {
            new Vector2I (0,-1)    //north
            , new Vector2I(0,1)    //south
            , new Vector2I (1,0)   //east
            , new Vector2I (-1,0)  //west
            , new Vector2I (1,-1)  //northeast
            , new Vector2I(-1,-1)  //northwest
            , new Vector2I (-1,1)  //southwest
            , new Vector2I (1,1)   //southeast
            , new Vector2I(0,0)    //centre
        };
    }
}
