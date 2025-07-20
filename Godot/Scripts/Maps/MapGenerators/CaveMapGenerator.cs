namespace DungeonCarver.Godot
{
    using System.Collections.Generic;
    using System.Linq;
    using global::Godot;
    using Godot;

    /// <summary>
    /// The CaveMapGenerator creates a Map of the specified type by using a complex cave system for generated map.
    /// </summary>
    /// <seealso href="https://www.evilscience.co.uk/a-c-algorithm-to-build-roguelike-cave-systems-part-1/">An implementation of cave system</seealso>
    /// <typeparam name="T">The type of IMap that will be created</typeparam>
    public class CaveMapGenerator<T> : IMapGenerator<T> where T : class, IMap, new()
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _neighbours;
        private readonly int _iterations;
        private readonly int _closeTileProb;
        private readonly int _lowerLimit;
        private readonly int _upperLimit;
        private readonly int _emptyNeighbours;
        private readonly int _emptyTileNeighbours;
        private readonly int _corridorSpace;
        private readonly int _corridor_MaxTurns;
        private readonly int _corridor_Min;
        private readonly int _corridor_Max;
        private readonly int _breakOut;
        private readonly System.Random _random;

        private List<List<Vector2I>> _caves;
        private List<Vector2I> _corridors;

        private T _map;
        
        public CaveMapGenerator(int width, int height, int neighbours, int iterations, int closeTileProb, int lowerLimit, int upperLimit, int emptyNeighbours,
                                       int emptyTileNeighbours, int corridorSpace, int corridorMaxTurns, int corridorMin, int corridorMax, int breakOut, System.Random random)
        {
            _width = width;
            _height = height;
            _neighbours = neighbours;
            _iterations = iterations;
            _closeTileProb = closeTileProb;
            _lowerLimit = lowerLimit;
            _upperLimit = upperLimit;
            _emptyNeighbours = emptyNeighbours;
            _emptyTileNeighbours = emptyTileNeighbours;
            _corridorSpace = corridorSpace;
            _corridor_MaxTurns = corridorMaxTurns;
            _corridor_Min = corridorMin;
            _corridor_Max = corridorMax;
            _breakOut = breakOut;
            _random = random;

            _map = new T();
        }

        public T CreateMap()
        {
            _map.Initialize(_width, _height);
            _map.Clear(new Tile(Tile.Type.Block));

            BuildCaves();
            GetCaves();
            ConnectCaves();

            return _map;
        }

        private void BuildCaves()
        {
            foreach ((Vector2I tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    continue;
                }

                if (_random.Next(0, 100) < _closeTileProb)
                {
                    _map.SetTile(tileData.tilePosition.X, tileData.tilePosition.Y, new Tile(Tile.Type.Empty));
                }
            }

            Vector2I tilePosition;

            //Pick cells at random
            for (int x = 0; x <= _iterations; x++)
            {
                tilePosition = new Vector2I(_random.Next(0, _width), _random.Next(0, _height));

                //if the randomly selected cell has more closed neighbours than the property Neighbours
                //set it closed, else open it
                if (NeighboursGetNineDirections(tilePosition).Where(n => !_map.GetTile(n.X, n.Y).type.Equals(Tile.Type.Block)).Count() > _neighbours)
                {
                    _map.SetTile(tilePosition.X, tilePosition.Y, new Tile(Tile.Type.Empty));
                }
                else
                {
                    _map.SetTile(tilePosition.X, tilePosition.Y, new Tile(Tile.Type.Block));
                }
            }

            //
            //  Smooth of the rough cave edges and any single blocks by making several 
            //  passes on the map and removing any cells with 3 or more empty neighbours
            //
            for (int ctr = 0; ctr < 5; ctr++)
            {
                //examine each cell individually
                foreach ((Vector2I tilePosition, Tile tile) tileData in _map.GetAllTiles())
                {
                    if (_map.IsBorderTile(tileData.tilePosition))
                    {
                        continue;
                    }

                    tilePosition = tileData.tilePosition;

                    if (!_map.GetTile(tilePosition.X, tilePosition.Y).type.Equals(Tile.Type.Block) && NeighboursGetFourDirections(tilePosition).Where(position => _map.GetTile(position.X, position.Y).type.Equals(Tile.Type.Block)).Count() >= _emptyNeighbours)
                    {
                        _map.SetTile(tilePosition.X, tilePosition.Y, new Tile(Tile.Type.Block));
                    }
                }
            }

            //
            //  fill in any empty cells that have 4 full neighbours
            //  to get rid of any holes in an cave
            //
            foreach ((Vector2I tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    continue;
                }

                tilePosition = tileData.tilePosition;

                if (_map.GetTile(tilePosition.X, tilePosition.Y).type.Equals(Tile.Type.Block) && NeighboursGetFourDirections(tilePosition).Where(postion => !_map.GetTile(postion.X, postion.Y).type.Equals(Tile.Type.Block)).Count() >= _emptyTileNeighbours)
                {
                    _map.SetTile(tilePosition.X, tilePosition.Y, new Tile(Tile.Type.Empty));
                }
            }
        }
        
        private void GetCaves()
        {
            _caves = new List<List<Vector2I>>();

            List<Vector2I> cave;
            Vector2I tilePosition;

            //examine each cell in the map...
            foreach ((Vector2I tilePosition, Tile tile) tileData in _map.GetAllTiles())
            {
                if (_map.IsBorderTile(tileData.tilePosition))
                {
                    continue;
                }

                tilePosition = tileData.tilePosition;

                //if the cell is closed, and that cell doesn't occur in the list of caves..
                if (!_map.GetTile(tilePosition.X, tilePosition.Y).type.Equals(Tile.Type.Block) && _caves.Count(s => s.Contains(tilePosition)) == 0)
                {
                    cave = new List<Vector2I>();

                    //launch the recursive
                    LocateCave(tilePosition, cave);

                    //check that cave falls with the specified property range size...
                    if (cave.Count() <= _lowerLimit | cave.Count() > _upperLimit)
                    {
                        //it does, so bin it
                        foreach (Vector2I p in cave)
                        {
                            _map.SetTile(p.X, p.Y, new Tile(Tile.Type.Block));
                        }
                    }
                    else
                    {
                        _caves.Add(cave);
                    }
                }
            }

        }

        private void LocateCave(Vector2I tilePosition, List<Vector2I> cave)
        {
            foreach (Vector2I p in NeighboursGetFourDirections(tilePosition).Where(n => !_map.GetTile(n.X, n.Y).type.Equals(Tile.Type.Block)))
            {
                if (!cave.Contains(p))
                {
                    cave.Add(p);
                    LocateCave(p, cave);
                }
            }
        }
        
        public bool ConnectCaves()
        {
            if (_caves.Count() == 0)
            {
                return false;
            }

            List<Vector2I> currentcave;
            List<List<Vector2I>> ConnectedCaves = new List<List<Vector2I>>();
            Vector2I cor_point = new Vector2I();
            Vector2I cor_direction = new Vector2I();
            List<Vector2I> potentialcorridor = new List<Vector2I>();
            int breakoutctr = 0;

            _corridors = new List<Vector2I>(); //corridors built stored here

            //get started by randomly selecting a cave..
            currentcave = _caves[_random.Next(0, _caves.Count())];
            ConnectedCaves.Add(currentcave);
            _caves.Remove(currentcave);

            //starting builder
            do
            {

                //no corridors are present, sp build off a cave
                if (_corridors.Count() == 0)
                {
                    currentcave = ConnectedCaves[_random.Next(0, ConnectedCaves.Count())];
                    CaveGetEdge(currentcave, ref cor_point, ref cor_direction);
                }
                else
                {
                    //corridors are presnt, so randomly chose whether a get a start
                    //point from a corridor or cave
                    if (_random.Next(0, 100) > 50)
                    {
                        currentcave = ConnectedCaves[_random.Next(0, ConnectedCaves.Count())];
                        CaveGetEdge(currentcave, ref cor_point, ref cor_direction);
                    }
                    else
                    {
                        currentcave = null;
                        CorridorGetEdge(ref cor_point, ref cor_direction);
                    }
                }

                //using the points we've determined above attempt to build a corridor off it
                potentialcorridor = CorridorAttempt(cor_point, cor_direction, true);

                //if not null, a solid object has been hit
                if (potentialcorridor != null)
                {
                    //examine all the caves
                    for (int ctr = 0; ctr < _caves.Count(); ctr++)
                    {
                        //check if the last point in the corridor list is in a cave
                        if (_caves[ctr].Contains(potentialcorridor.Last()))
                        {
                            //we've built of a corridor or built of a room 
                            if (currentcave == null | currentcave != _caves[ctr])
                            {
                                //the last corridor point intrudes on the room, so remove it
                                potentialcorridor.Remove(potentialcorridor.Last());
                                //add the corridor to the corridor collection
                                _corridors.AddRange(potentialcorridor);
                                //write it to the map
                                foreach (Vector2I p in potentialcorridor)
                                {
                                    _map.SetTile(p.X, p.Y, new Tile(Tile.Type.Empty));
                                }

                                //the room reached is added to the connected list...
                                ConnectedCaves.Add(_caves[ctr]);
                                //...and removed from the Caves list
                                _caves.RemoveAt(ctr);

                                break;
                            }
                        }
                    }
                }

                //breakout
                if (breakoutctr++ > _breakOut)
                {
                    return false;
                }

            } while (_caves.Count() > 0);

            _caves.AddRange(ConnectedCaves);
            ConnectedCaves.Clear();

            return true;
        }

        private void CaveGetEdge(List<Vector2I> pCave, ref Vector2I pCavePoint, ref Vector2I pDirection)
        {
            do
            {
                //random point in cave
                pCavePoint = pCave.ToList()[_random.Next(0, pCave.Count())];

                pDirection = FourDirectiosnGet(pDirection);

                do
                {
                    pCavePoint += (pDirection);

                    if (!TilePositionCheck(pCavePoint))
                    {
                        break;
                    }
                    else if (_map.GetTile(pCavePoint.X, pCavePoint.Y).type.Equals(Tile.Type.Block))
                    {
                        return;
                    }

                } while (true);
            } while (true);
        }
        
        private void CorridorGetEdge(ref Vector2I pLocation, ref Vector2I pDirection)
        {
            List<Vector2I> validdirections = new List<Vector2I>();

            do
            {
                //the modifiers below prevent the first of last point being chosen
                pLocation = _corridors[_random.Next(1, _corridors.Count)];

                //attempt to locate all the empy map points around the location
                //using the directions to offset the randomly chosen point
                foreach (Vector2I p in MapUtils.FourDirections)
                {
                    if (TilePositionCheck(new Vector2I(pLocation.X + p.X, pLocation.Y + p.Y)))
                    {
                        if (_map.GetTile(pLocation.X + p.X, pLocation.Y + p.Y).type.Equals(Tile.Type.Block))
                        {
                            validdirections.Add(p);
                        }
                    }
                }

            } while (validdirections.Count == 0);

            pDirection = validdirections[_random.Next(0, validdirections.Count)];
            pLocation += pDirection;
        }

        private List<Vector2I> CorridorAttempt(Vector2I pStart, Vector2I pDirection, bool pPreventBackTracking)
        {

            List<Vector2I> lPotentialCorridor = new List<Vector2I>();
            lPotentialCorridor.Add(pStart);

            int corridorlength;
            Vector2I startdirection = new Vector2I(pDirection.X, pDirection.Y);

            int pTurns = _corridor_MaxTurns;

            while (pTurns >= 0)
            {
                pTurns--;

                corridorlength = _random.Next(_corridor_Min, _corridor_Max);
                //build corridor
                while (corridorlength > 0)
                {
                    corridorlength--;

                    //make a point and offset it
                    pStart += pDirection;

                    if (TilePositionCheck(pStart) && !_map.GetTile(pStart.X, pStart.Y).type.Equals(Tile.Type.Block))
                    {
                        lPotentialCorridor.Add(pStart);
                        return lPotentialCorridor;
                    }

                    if (!TilePositionCheck(pStart))
                    {
                        return null;
                    }
                    else if (!CorridorPointTest(pStart, pDirection))
                    {
                        return null;
                    }

                    lPotentialCorridor.Add(pStart);

                }

                if (pTurns > 1)
                {
                    if (!pPreventBackTracking)
                    {
                        pDirection = FourDirectiosnGet(pDirection);
                    }
                    else
                    {
                        pDirection = FourDirectiosnGet(pDirection, startdirection);
                    }
                }
            }

            return null;
        }

        private bool CorridorPointTest(Vector2I pPoint, Vector2I pDirection)
        {
            //using the property corridor space, check that number of cells on
            //either side of the point are empty
            foreach (int r in Enumerable.Range(-_corridorSpace, 2 * _corridorSpace + 1).ToList())
            {
                if (pDirection.X == 0)//north or south
                {
                    if (TilePositionCheck(new Vector2I(pPoint.X + r, pPoint.Y)))
                    {
                        if (!_map.GetTile(pPoint.X + r, pPoint.Y).type.Equals(Tile.Type.Block))
                        {
                            return false;
                        }
                    }
                }
                else if (pDirection.Y == 0)//east west
                {
                    if (TilePositionCheck(new Vector2I(pPoint.X, pPoint.Y + r)))
                    {
                        if (!_map.GetTile(pPoint.X, pPoint.Y + r).type.Equals(Tile.Type.Block))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        
        private Vector2I FourDirectiosnGet(Vector2I p)
        {
            Vector2I newdir;
            do
            {

                newdir = MapUtils.FourDirections[_random.Next(0, MapUtils.FourDirections.Count())];

            } while (newdir.X != -p.X & newdir.Y != -p.Y);

            return newdir;
        }
        
        private Vector2I FourDirectiosnGet(Vector2I pDir, Vector2I pDirExclude)
        {
            Vector2I NewDir;
            do
            {

                NewDir = MapUtils.FourDirections[_random.Next(0, MapUtils.FourDirections.Count())];

            } while (DirectionReverse(NewDir) == pDir | DirectionReverse(NewDir) == pDirExclude);


            return NewDir;
        }
       
        private Vector2I DirectionReverse(Vector2I pDir)
        {
            return new Vector2I(-pDir.X, -pDir.Y);
        }
        
        private List<Vector2I> NeighboursGetFourDirections(Vector2I tilePosition)
        {
            return MapUtils.FourDirections.Select(direction => new Vector2I(tilePosition.X + direction.X, tilePosition.Y + direction.Y)).Where(direction => TilePositionCheck(direction)).ToList();
        }
        
        private List<Vector2I> NeighboursGetNineDirections(Vector2I tilePosition)
        {
            return MapUtils.NineDirections.Select(direction => new Vector2I(tilePosition.X + direction.X, tilePosition.Y + direction.Y)).Where(direction => TilePositionCheck(direction)).ToList();
        }
        
        private bool TilePositionCheck(Vector2I tilePosition)
        {
            return tilePosition.X >= 0 & tilePosition.X < _width & tilePosition.Y >= 0 & tilePosition.Y < _height;
        }
    }
}