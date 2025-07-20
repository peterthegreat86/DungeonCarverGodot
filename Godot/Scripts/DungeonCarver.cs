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
            generator1 = value; NotifyPropertyListChanged();
        }
    }

    [Export]
    public int mapWidth = 25;
    [Export]
    public int mapHeight = 25;

    //BSP Tree Specific Var

    public int maxLeafSize = 24;

    public int minLeafSize = 10;

    public int roomMaxSize = 15;

    public int roomMinSize = 6;

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
        GD.Print("DungeonCarver run");
        System.Random random = new System.Random(DateTime.Now.Millisecond);
        IMapGenerator<Map> mapGenerator;

        switch (generator)
        {
            case Generators.BorderOnlyMapGenerator:
                {
                    mapGenerator = new BorderOnlyMapGenerator<Map>(mapWidth, mapHeight);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.BSPTreeMapGenerator:
                {
                    mapGenerator = new BSPTreeMapGenerator<Map>(mapWidth, mapHeight, maxLeafSize, minLeafSize, roomMaxSize, roomMinSize, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.CaveMapGenerator:
                {
                    mapGenerator = new CaveMapGenerator<Map>(mapWidth, mapHeight, neighbours, iterations, closeTileProb, lowerLimit, upperLimit, emptyNeighbours, emptyTileNeighbours, corridorSpace, corridorMaxTurns, corridorMin, corridorMax, breakOut, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.CellularAutomataMapGenerator:
                {
                    mapGenerator = new CellularAutomataMapGenerator<Map>(mapWidth, mapHeight, fillProbability, totalIterations, cutoffOfBigAreaFill, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.CityMapGenerator:
                {
                    mapGenerator = new CityMapGenerator<Map>(mapWidth, mapHeight, maxCityLeafSize, minCityLeafSize, roomMaxCitySize, roomMinCitySize, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.DFSMazeMapGenerator:
                {
                    mapGenerator = new DFSMazeMapGenerator<Map>(mapWidth, mapHeight, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.DrunkardsWalkMapGenerator:
                {
                    mapGenerator = new DrunkardsWalkMapGenerator<Map>(mapWidth, mapHeight, percentGoal, walkIterations, weightedTowardCenter, weightedTowardPreviousDirection, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.TunnelingMazeMapGenerator:
                {
                    mapGenerator = new TunnelingMazeMapGenerator<Map>(mapWidth, mapHeight, magicNumber, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
            case Generators.TunnelingWithRoomsMapGenerator:
                {
                    mapGenerator = new TunnelingWithRoomsMapGenerator<Map>(mapWidth, mapHeight, maxTunnelingRooms, roomMaxTunnelingSize, roomMinTunnelingSize, random);
                    _map = Map.Create(mapGenerator);
                    break;
                }
        }

        //Camera3D mainCamera = GetNode<Camera3D>("Path/To/Your/Camera3D"); // Replace "Path/To/Your/Camera3D" with the actual path

        //Camera.main.transform.localPosition = new Vector3(_map.Width / 2, _map.Height / 2, -10);

        if (mainCamera != null)
        {
            mainCamera.Position = new Vector3(_map.Width / 2.0f, _map.Height / 2.0f, 10.0f); // Note: Z-axis is typically positive for camera depth in Godot's 3D space relative to the scene.
        }
        else
        {
            GD.PrintErr("Camera3D not found! Please ensure the path is correct.");
        }

        RenderMap();
    }

    private void RenderMap()
    {
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                Node3D newTile = tilePrefab.Instantiate<Node3D>();
                newTile.Position = new Vector3(x, y, 0);
                AddChild(newTile);

                Sprite3D tileSprite = newTile.GetNodeOrNull<Sprite3D>("Sprite3D"); //This is null

                switch (_map.GetTile(x, y).type)
                {
                    case Tile.Type.Block:
                        {
                            if (wall != null)
                            {
                                tileSprite.Texture = wall;
                            }
                            break;
                        }
                    case Tile.Type.Empty:
                        {
                            tileSprite.Texture = empty;
                            break;
                        }
                }
            }
        }
    }

}