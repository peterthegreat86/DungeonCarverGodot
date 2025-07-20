namespace DungeonCarver.Godot
{
    using System;
    using System.Collections.Generic;
    using global::Godot;
    using Godot;

    /// <summary>
    /// CityMapGenerator use the City Walls algorithm is very similar to the BSP Tree above. In fact their main difference is in how they generate rooms after the actual tree has been created. Instead of 
	/// starting with an array of solid walls and carving out rooms connected by tunnels, the City Walls generator starts with an array of floor tiles, then creates only the
	/// exterior of the rooms, then opens one wall for a door.
    /// </summary>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class CityMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private readonly int _mapWidth;
        private readonly int _mapHeight;
        private readonly int _maxLeafSize;
        private readonly int _minLeafSize;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;
        private readonly System.Random _random;

        private List<Leaf> _leafs = new List<Leaf>();
        private List<Rect2> _rooms = new List<Rect2>();
        private Rect2 _room;
        private T _map;

        /// <summary>
        /// Constructs a new BorderOnlyMapCreationStrategy with the specified parameters
        /// </summary>
        /// <param name="size">The size of the Map to be created</param>        
        public CityMapGenerator(int mapWidth, int mapHeight, int maxLeafSize, int minLeafSize, int roomMaxSize, int roomMinSize, System.Random random)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _maxLeafSize = maxLeafSize;
            _minLeafSize = minLeafSize;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
            _random = random;
        }

        public T CreateMap()
        {
            _map = new T();
            _map.Initialize(_mapWidth, _mapHeight);
            _map.Clear(new Tile(Tile.Type.Empty));

            _leafs = new List<Leaf>();

            Leaf rootLeaf = new Leaf(1, 1, _mapWidth - 1, _mapHeight - 1, _random);

            _leafs.Add(rootLeaf);

            bool splitSuccessfully = true;

            while (splitSuccessfully)
            {
                splitSuccessfully = false;

                for (int i = 0; i < _leafs.Count; i++)
                {
                    if ((_leafs[i].childLeafLeft == null) && (_leafs[i].childLeafRight == null))
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

            rootLeaf.CreateCityRooms<T>(this, _maxLeafSize, _roomMaxSize, _roomMinSize);
            CreateDoors();

            return _map;
        }

        public void createRoom(Rect2 room)
        {
            _rooms.Add(room);

            //Build Walls
            //set all tiles within a rectangle to 1
            for (int x = (int)room.Position.X; x <= room.End.X; x++)
            {
                for (int y = (int)room.Position.Y; y <= room.End.Y; y++)
                {
                    _map.SetTile(x, y, new Tile(Tile.Type.Block));
                }
            }

            for (int x = (int)room.Position.X + 1; x < room.End.X; x++)
            {
                for (int y = (int)room.Position.Y + 1; y < room.End.X; y++)
                {
                    _map.SetTile(x, y, new Tile(Tile.Type.Empty));
                }
            }
        }

        public void createHall(Rect2 room1, Rect2 room2)
        {
            if (_rooms.Find(item => item.Equals(room1)) == null)
            {
                _rooms.Add(room1);
            }

            if (_rooms.Find(item => item.Equals(room2)) == null)
            {
                _rooms.Add(room2);
            }
        }

        public void CreateDoors()
        {
            foreach (Rect2 room in _rooms)
            {
                Vector2I roomCenter = (Vector2I)room.GetCenter();
                Array values = Enum.GetValues(typeof(MapUtils.CardinalFourDirections));

                MapUtils.CardinalFourDirections randomDirection = (MapUtils.CardinalFourDirections)values.GetValue(_random.Next(0, values.Length));

                Vector2I doorPosition = Vector2I.Zero;
                switch (randomDirection)
                {
                    case MapUtils.CardinalFourDirections.NORTH:
                        {
                            doorPosition = new Vector2I(roomCenter.X, (int)room.End.Y);
                            break;
                        }
                    case MapUtils.CardinalFourDirections.SOUTH:
                        {
                            doorPosition = new Vector2I(roomCenter.X, (int)room.Position.Y);
                            break;
                        }
                    case MapUtils.CardinalFourDirections.EAST:
                        {
                            doorPosition = new Vector2I((int)room.Position.X, roomCenter.Y);
                            break;
                        }
                    case MapUtils.CardinalFourDirections.WEST:
                        {
                            doorPosition = new Vector2I((int)room.Position.X,  roomCenter.Y);
                            break;
                        }
                }

                _map.SetTile(doorPosition.X, doorPosition.Y, new Tile(Tile.Type.Empty));
            }
        }
    }
}