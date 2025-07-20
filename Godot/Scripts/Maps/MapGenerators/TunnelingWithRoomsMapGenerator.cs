namespace DungeonCarver.Godot
{
    using System;
    using System.Collections.Generic;
    using global::Godot;
    using Godot;

    /// <summary>
    /// TunnelingWithRoomsMapGenerator generate a map with room and this room are connect with tunnels.
    /// </summary>
    /// <seealso href="http://www.roguebasin.com/index.php?title=Complete_Roguelike_Tutorial,_using_python%2Blibtcod,_part_3#Dungeon_generator">Tunneling with rooms Generation RogueBasin</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class TunnelingWithRoomsMapGenerator<T> : IMapGenerator<T> where T : IMap, new()
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxRooms;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;
        private readonly System.Random _random;

        private T _map;
        
        public TunnelingWithRoomsMapGenerator(int width, int height, int maxRooms, int roomMaxSize, int roomMinSize, System.Random random)
        {
            _width = width;
            _height = height;
            _maxRooms = maxRooms;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
            _random = random;
        }
       
        public T CreateMap()
        {
            _map = new T();
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            var rooms = new List<Rect2>();

            for (int r = 0; r < _maxRooms; r++)
            {
                int roomWidth = _random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = _random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = _random.Next(0, _width - roomWidth);
                int roomYPosition = _random.Next(0, _height - roomHeight);

                Rect2 newRoom = new Rect2(roomXPosition, roomYPosition, roomWidth, roomHeight);
                bool newRoomIntersects = false;
                foreach (Rect2 room in rooms)
                {
                    if (newRoom.Intersects(room))
                    {
                        newRoomIntersects = true;
                        break;
                    }
                }

                if (!newRoomIntersects)
                {
                    rooms.Add(newRoom);
                }
            }

            foreach (Rect2 room in rooms)
            {
                MakeRoom(room);
            }

            for (int r = 0; r < rooms.Count; r++)
            {
                if (r == 0)
                {
                    continue;
                }

                int previousRoomCenterX = (int)rooms[r - 1].GetCenter().X;
                int previousRoomCenterY = (int)rooms[r - 1].GetCenter().Y;
                int currentRoomCenterX = (int)rooms[r].GetCenter().X;
                int currentRoomCenterY = (int)rooms[r].GetCenter().Y;

                if (_random.Next(0, 2) == 0)
                {
                    MakeHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                    MakeVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                }
                else
                {
                    MakeVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX);
                    MakeHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY);
                }
            }

            return _map;
        }

        private void MakeRoom(Rect2 room)
        {
            for (int x = (int)room.Position.X + 1; x < (int)room.End.X; x++)
            {
                for (int y = (int)room.Position.Y + 1; y < (int)room.End.Y; y++)
                {
                    _map.SetTile(x, y, new Tile(Tile.Type.Empty));
                }
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