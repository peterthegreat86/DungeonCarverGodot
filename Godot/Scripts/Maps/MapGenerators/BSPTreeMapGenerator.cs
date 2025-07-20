namespace DungeonCarver.Godot
{
    using System;
    using System.Collections.Generic;
    using global::Godot;

    /// <summary>
    /// The BSPTreeMapGenerator creates a Map of the specified type by making an empty map with only the outermost border being solid walls
    /// </summary>
    /// <seealso href="http://www.roguebasin.com/index.php?title=Basic_BSP_Dungeon_generation">BSP Dungeon Generation</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class BSPTreeMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxLeafSize;
        private readonly int _minLeafSize;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;
        private System.Random _random;

        private T _map;
        private List<Leaf> _leafs = new List<Leaf>();
        
        public BSPTreeMapGenerator(int width, int height, int maxLeafSize, int minLeafSize, int roomMaxSize, int roomMinSize, System.Random random)
        {
            _width = width;
            _height = height;
            _maxLeafSize = maxLeafSize;
            _minLeafSize = minLeafSize;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
            _random = random;

            _map = new T();
        }

        public T CreateMap()
        {
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            Leaf rootLeaf = new Leaf(0, 0, _map.Width, _map.Height, _random);
            _leafs.Add(rootLeaf);

            bool splitSuccessfully = true;

            //Loop through all leaves until they can no longer split successfully
            while (splitSuccessfully)
            {
                splitSuccessfully = false;

                for (int i = 0; i < _leafs.Count; i++)
                {
                    if (_leafs[i].childLeafLeft == null && _leafs[i].childLeafRight == null)
                    {
                        if ((_leafs[i].leafWidth > _maxLeafSize) || (_leafs[i].leafHeight > _maxLeafSize))
                        {
                            //Try to split the leaf
                            if (_leafs[i].SplitLeaf(_minLeafSize))
                            {
                                _leafs.Add(_leafs[i].childLeafLeft);
                                _leafs.Add(_leafs[i].childLeafRight);
                                splitSuccessfully = true;
                            }
                        }
                    }
                }
            }

            rootLeaf.CreateRooms<T>(this, _maxLeafSize, _roomMaxSize, _roomMinSize);

            return _map;
        }

        public void createRoom(Rect2I room)
        {
            for (int x = (int)room.Position.X + 1; x < room.End.X; x++)
            {
                for (int y = (int)room.Position.Y + 1; y < room.End.Y; y++)
                {
                    _map.SetTile(x, y, new Tile(Tile.Type.Empty));
                }
            }
        }

        public void createHall(Rect2 room1, Rect2 room2)
        {
            //# connect two rooms by hallways
            Vector2I room1Center = (Vector2I)room1.GetCenter().Ceil();
            Vector2I room2Center = (Vector2I)room2.GetCenter().Ceil();

            //# 50% chance that a tunnel will start horizontally
            bool chance = Convert.ToBoolean(_random.Next(0, 2));
            if (chance)
            {
                MakeHorizontalTunnel(room1Center.X, room2Center.X, room1Center.Y);
                MakeVerticalTunnel(room1Center.Y, room2Center.Y, room2Center.X);
            }
            else
            {
                MakeVerticalTunnel(room1Center.Y, room2Center.Y, room1Center.X);
                MakeHorizontalTunnel(room1Center.X, room2Center.X, room2Center.Y);
            }
        }

        private void MakeHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                _map.SetTile(x, yPosition, new Tile(Tile.Type.Empty));
            }
        }

        private void MakeVerticalTunnel(int yStart, int yEnd, int xPosition)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                _map.SetTile(xPosition, y, new Tile(Tile.Type.Empty));
            }
        }
    }
}
