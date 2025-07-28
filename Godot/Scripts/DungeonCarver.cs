using DungeonCarver;
using System;
using global::Godot;
namespace DungeonCarver.Godot;

public partial class DungeonCarver : Node3D
{
    //[Export]
    //public Node3D dungeonParent = null;
    [Export]
    public PackedScene tilePrefab = null;
    [Export]
    public Texture2D wall = null;
    [Export]
    public Texture2D empty = null;
    [Export]
    public Camera3D mainCamera = null;

    //Generic Vars
    private Generators generator1;
    [Export]
    public Generators generator
    {
        get => generator1;
        set
        {
            generator1 = value;
            NotifyPropertyListChanged();
        }
    }

    [ExportToolButton("Render")]
    public Callable Render => Callable.From(RenderMapButton);

    [ExportToolButton("Clear")]
    public Callable Clear => Callable.From(ClearButton);

    [Export]
    public int mapWidth = 25;
    [Export]
    public int mapHeight = 25;

    //BSP Tree Specific Var

    public int leafMaxSize { get; set; } = 24;

    public int leafMinSize { get; set; } = 10;

    public int roomMaxSize { get; set; } = 15;

    private int _roomMinSize = 6;
    public int roomMinSize
    {
        get
        {
            //GD.Print($"Get {_roomMinSize}");
            return _roomMinSize;
        }
        set { _roomMinSize = value; GD.Print($"Set to {value}"); }
    }

    //Cave System Specific Vars

    public int neighbours = 4;

    public int iterations = 50000;

    public int closeTileProb = 45;

    public int lowerLimit = 16;

    public int upperLimit = 500;

    public int emptyNeighbours = 3;

    public int emptyTileNeighbours = 4;

    public int corridorSpace = 2;

    public int corridorMaxTurns = 10;

    public int corridorMin = 2;

    public int corridorMax = 5;

    public int breakOut = 100000;

    //Cellullar Automata Vars

    public int fillProbability = 50;

    public int totalIterations = 3;

    public int cutoffOfBigAreaFill = 3;

    // City Vars

    public int maxCityLeafSize = 30;

    public int minCityLeafSize = 8;

    public int roomMaxCitySize = 16;

    public int roomMinCitySize = 8;

    //Drunkards Walk

    public float percentGoal = 0.3f;

    public int walkIterations = 50000;

    public float weightedTowardCenter = 0.15f;

    public float weightedTowardPreviousDirection = 0.7f;

    //Tunneling Maze With Rooms

    public int magicNumber = 666;

    //Tunneling With Rooms

    public int maxTunnelingRooms = 30;

    public int roomMaxTunnelingSize = 15;

    public int roomMinTunnelingSize = 6;

    private IMap _map;

    // Start is called before the first frame update
    public override void _Ready()
    {
        RenderMap();
    }

    private void RenderMapButton()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        RenderMap();
    }
    private void ClearButton()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

    }

    private void RenderMap()
    {
        GD.Print("DungeonCarver run");
        System.Random random = new System.Random(DateTime.Now.Millisecond);
        IMapGenerator<Map> mapGenerator = null;

        switch (generator)
        {
            case Generators.BorderOnlyMapGenerator:
                {
                    if (mapWidth <= 0 || mapHeight <= 0)
                    {
                        GD.PrintErr("BorderOnlyMapGenerator: Map dimensions must be positive.");
                        return;
                    }
                    mapGenerator = new BorderOnlyMapGenerator<Map>(mapWidth, mapHeight);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.BSPTreeMapGenerator:
                {
                    if (leafMinSize <= 0 || leafMaxSize <= 0 || roomMinSize <= 0 || roomMaxSize <= 0)
                    {
                        GD.PrintErr("BSPTreeMapGenerator: All size parameters must be positive.");
                        return;
                    }
                    if (leafMinSize > leafMaxSize)
                    {
                        GD.PrintErr("BSPTreeMapGenerator: Leaf Minimum Size cannot be greater than Leaf Maximum Size.");
                        return;
                    }
                    if (roomMinSize > roomMaxSize)
                    {
                        GD.PrintErr("BSPTreeMapGenerator: Room Minimum Size cannot be greater than Room Maximum Size.");
                        return;
                    }
                    if (leafMinSize < roomMaxSize)
                    {
                        GD.PrintErr("BSPTreeMapGenerator: Leaf Minimum Size is less than Room Maximum Size. Rooms might not fit.");
                        return;
                    }
                    if (mapWidth < leafMaxSize || mapHeight < leafMaxSize)
                    {
                        GD.PrintErr("BSPTreeMapGenerator: Map size is smaller than Leaf Maximum Size.");
                        return;
                    }

                    mapGenerator = new BSPTreeMapGenerator<Map>(mapWidth, mapHeight, leafMaxSize, leafMinSize, roomMaxSize, roomMinSize, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.CaveMapGenerator:
                {
                    if (mapWidth <= 0 || mapHeight <= 0)
                    {
                        GD.PrintErr("CaveMapGenerator: Map dimensions must be positive.");
                        return;
                    }
                    if (neighbours <= 0)
                    {
                        GD.PrintErr("CaveMapGenerator: Neighbours count must be positive.");
                        return;
                    }
                    if (iterations <= 0)
                    {
                        GD.PrintErr("CaveMapGenerator: Iterations count must be positive.");
                        return;
                    }
                    if (closeTileProb < 0 || closeTileProb > 100)
                    {
                        GD.PrintErr("CaveMapGenerator: Close Tile Probability must be between 0 and 100.");
                        return;
                    }
                    if (lowerLimit <= 0 || upperLimit <= 0)
                    {
                        GD.PrintErr("CaveMapGenerator: Limits must be positive.");
                        return;
                    }
                    if (lowerLimit > upperLimit)
                    {
                        GD.PrintErr("CaveMapGenerator: Lower Limit cannot be greater than Upper Limit.");
                        return;
                    }
                    if (corridorSpace < 0 || corridorMaxTurns < 0 || corridorMin < 0 || corridorMax < 0)
                    {
                        GD.PrintErr("CaveMapGenerator: Corridor parameters cannot be negative.");
                        return;
                    }
                    if (corridorMin > corridorMax)
                    {
                        GD.PrintErr("CaveMapGenerator: Corridor Minimum length cannot be greater than Corridor Maximum length.");
                        return;
                    }

                    mapGenerator = new CaveMapGenerator<Map>(mapWidth, mapHeight, neighbours, iterations, closeTileProb, lowerLimit, upperLimit, emptyNeighbours, emptyTileNeighbours, corridorSpace, corridorMaxTurns, corridorMin, corridorMax, breakOut, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.CellularAutomataMapGenerator:
                {
                    if (mapWidth <= 0 || mapHeight <= 0)
                    {
                        GD.PrintErr("CellularAutomataMapGenerator: Map dimensions must be positive.");
                        return;
                    }
                    if (fillProbability < 0 || fillProbability > 100)
                    {
                        GD.PrintErr("CellularAutomataMapGenerator: Fill Probability must be between 0 and 100.");
                        return;
                    }
                    if (totalIterations <= 0)
                    {
                        GD.PrintErr("CellularAutomataMapGenerator: Iterations count must be positive.");
                        return;
                    }
                    if (cutoffOfBigAreaFill < 0)
                    {
                        GD.PrintErr("CellularAutomataMapGenerator: Cutoff cannot be negative.");
                        return;
                    }

                    mapGenerator = new CellularAutomataMapGenerator<Map>(mapWidth, mapHeight, fillProbability, totalIterations, cutoffOfBigAreaFill, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.CityMapGenerator:
                {
                    if (mapWidth <= 0 || mapHeight <= 0)
                    {
                        GD.PrintErr("CityMapGenerator: Map dimensions must be positive.");
                        return;
                    }
                    if (minCityLeafSize <= 0 || maxCityLeafSize <= 0 || roomMinCitySize <= 0 || roomMaxCitySize <= 0)
                    {
                        GD.PrintErr("CityMapGenerator: All size parameters must be positive.");
                        return;
                    }
                    if (minCityLeafSize > maxCityLeafSize)
                    {
                        GD.PrintErr("CityMapGenerator: Minimum Leaf Size cannot be greater than Maximum Leaf Size.");
                        return;
                    }
                    if (roomMinCitySize > roomMaxCitySize)
                    {
                        GD.PrintErr("CityMapGenerator: Room Minimum Size cannot be greater than Room Maximum Size.");
                        return;
                    }
                    if (minCityLeafSize < roomMaxCitySize)
                    {
                        GD.PrintErr("CityMapGenerator: Minimum Leaf Size is less than Room Maximum Size. Rooms might not fit.");
                        return;
                    }
                    mapGenerator = new CityMapGenerator<Map>(mapWidth, mapHeight, maxCityLeafSize, minCityLeafSize, roomMaxCitySize, roomMinCitySize, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.DFSMazeMapGenerator:
                {
                    if (mapWidth <= 0 || mapHeight <= 0)
                    {
                        GD.PrintErr("DFSMazeMapGenerator: Map dimensions must be positive.");
                        return;
                    }
                    if (mapWidth % 2 == 0 || mapHeight % 2 == 0)
                    {
                        GD.PrintErr("DFSMazeMapGenerator: Odd map dimensions are recommended for maze generation.");
                    }
                    mapGenerator = new DFSMazeMapGenerator<Map>(mapWidth, mapHeight, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.DrunkardsWalkMapGenerator:
                {
                    if (mapWidth <= 0 || mapHeight <= 0)
                    {
                        GD.PrintErr("DrunkardsWalkMapGenerator: Map dimensions must be positive.");
                        return;
                    }
                    if (percentGoal <= 0.0f || percentGoal > 1.0f)
                    {
                        GD.PrintErr("DrunkardsWalkMapGenerator: Percent Goal must be between 0 (exclusive) and 1 (inclusive).");
                        return;
                    }
                    if (walkIterations <= 0)
                    {
                        GD.PrintErr("DrunkardsWalkMapGenerator: Walk Iterations count must be positive.");
                        return;
                    }
                    if (weightedTowardCenter < 0.0f || weightedTowardCenter > 1.0f)
                    {
                        GD.PrintErr("DrunkardsWalkMapGenerator: Center Weight must be between 0 and 1.");
                        return;
                    }
                    if (weightedTowardPreviousDirection < 0.0f || weightedTowardPreviousDirection > 1.0f)
                    {
                        GD.PrintErr("DrunkardsWalkMapGenerator: Previous Direction Weight must be between 0 and 1.");
                        return;
                    }
                    if (weightedTowardCenter + weightedTowardPreviousDirection > 1.0f)
                    {
                        GD.PrintErr("DrunkardsWalkMapGenerator: The sum of weights cannot be greater than 1.0.");
                        return;
                    }
                    mapGenerator = new DrunkardsWalkMapGenerator<Map>(mapWidth, mapHeight, percentGoal, walkIterations, weightedTowardCenter, weightedTowardPreviousDirection, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.TunnelingMazeMapGenerator:
                {
                    if (mapWidth <= 0 || mapHeight <= 0)
                    {
                        GD.PrintErr("TunnelingMazeMapGenerator: Map dimensions must be positive.");
                        return;
                    }
                    if (magicNumber <= 0)
                    {
                        GD.PrintErr("TunnelingMazeMapGenerator: Magic Number must be positive.");
                        return;
                    }
                    mapGenerator = new TunnelingMazeMapGenerator<Map>(mapWidth, mapHeight, magicNumber, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.TunnelingWithRoomsMapGenerator:
                {
                    if (mapWidth <= 0 || mapHeight <= 0)
                    {
                        GD.PrintErr("TunnelingWithRoomsMapGenerator: Map dimensions must be positive.");
                        return;
                    }
                    if (maxTunnelingRooms <= 0)
                    {
                        GD.PrintErr("TunnelingWithRoomsMapGenerator: Maximum Rooms count must be positive.");
                        return;
                    }
                    if (roomMinTunnelingSize <= 0 || roomMaxTunnelingSize <= 0)
                    {
                        GD.PrintErr("TunnelingWithRoomsMapGenerator: Room sizes must be positive.");
                        return;
                    }
                    if (roomMinTunnelingSize > roomMaxTunnelingSize)
                    {
                        GD.PrintErr("TunnelingWithRoomsMapGenerator: Room Minimum Size cannot be greater than Room Maximum Size.");
                        return;
                    }
                    mapGenerator = new TunnelingWithRoomsMapGenerator<Map>(mapWidth, mapHeight, maxTunnelingRooms, roomMaxTunnelingSize, roomMinTunnelingSize, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            default:
                {
                    GD.PrintErr("Unknown map generator selected.");
                    return;
                }
        }

        //GD.Print(mapGenerator);

        if (mainCamera != null)
        {
            mainCamera.Position = new Vector3(_map.Width / 2.0f, _map.Height / 2.0f, 10.0f);
        }
        else
        {
            GD.PrintErr("Camera3D not assigned in the editor.");
            return;
        }

        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                Node3D newTile = tilePrefab.Instantiate<Node3D>();
                newTile.Position = new Vector3(x, y, 0);
                AddChild(newTile);

                Sprite3D tileSprite = newTile.GetNodeOrNull<Sprite3D>("Sprite3D");

                if (tileSprite == null)
                {
                    GD.PrintErr($"Tile prefab is missing a 'Sprite3D' node at position ({x},{y}).");
                    continue;
                }

                switch (_map.GetTile(x, y).type)
                {
                    case Tile.Type.Block:
                        {
                            if (wall != null)
                            {
                                tileSprite.Texture = wall;
                            }
                            else
                            {
                                GD.PrintErr("Wall texture is not assigned.");
                            }
                            break;
                        }
                    case Tile.Type.Empty:
                        {
                            if (empty != null)
                            {
                                tileSprite.Texture = empty;
                            }
                            else
                            {
                                //GD.PrintErr("Empty texture is not assigned.");
                            }
                            break;
                        }
                    default:
                        {
                            GD.PrintErr($"Unhandled tile type: {_map.GetTile(x, y).type} found at ({x},{y}).");
                            break;
                        }
                }
            }
        }
    }

}