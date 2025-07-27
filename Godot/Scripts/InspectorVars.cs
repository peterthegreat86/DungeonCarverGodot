using Godot;
using System;
using Godot.Collections; // Needed for Array and Dictionary

namespace DungeonCarver.Godot
{
    // Make sure this class inherits from a Godot.Object type, e.g., Node3D, Node, Resource.
    // For a map generator that might be attached to a scene, Node3D or Node is common.
    // If it's just data/logic, a Resource could be an option.
    [Tool]
    public partial class DungeonCarver : Node3D // Or Node, Resource, etc. based on its role
    {
       


        // --- Godot Editor Customization ---

        // This method tells the Godot editor what properties to display.
        // It's like Unity's OnInspectorGUI, but you're describing the properties
        // rather than drawing them directly.
        public override Array<Dictionary> _GetPropertyList()
        {
            //GD.Print("_GetPropertyList()");
            var properties = new Array<Dictionary>();

            // Always visible properties (already handled by [Export] but can be added here too)
            // It's cleaner to rely on [Export] for the static ones unless you need special hints.

            // Properties for the currently selected generator
            switch (generator)
            {
                case Generators.BSPTreeMapGenerator:
                    AddProperty(properties, nameof(maxLeafSize), Variant.Type.Int, "Max Leaf Size:", PropertyHint.None);
                    AddProperty(properties, nameof(minLeafSize), Variant.Type.Int, "Min Leaf Size:", PropertyHint.None);
                    AddProperty(properties, nameof(roomMaxSize), Variant.Type.Int, "Room Max Size:", PropertyHint.None);
                    AddProperty(properties, nameof(roomMinSize), Variant.Type.Int, "Room Min Size:", PropertyHint.None);
                    break;
                case Generators.CaveMapGenerator:
                    AddProperty(properties, nameof(neighbours), Variant.Type.Int, "Neighbours:", PropertyHint.None);
                    AddProperty(properties, nameof(iterations), Variant.Type.Int, "Iterations:", PropertyHint.None);
                    AddProperty(properties, nameof(closeTileProb), Variant.Type.Int, "CloseTileProb:", PropertyHint.None);
                    AddProperty(properties, nameof(lowerLimit), Variant.Type.Int, "Lower Limit:", PropertyHint.None);
                    AddProperty(properties, nameof(upperLimit), Variant.Type.Int, "Upper Limit:", PropertyHint.None);
                    AddProperty(properties, nameof(emptyNeighbours), Variant.Type.Int, "Empty Neighbours:", PropertyHint.None);
                    AddProperty(properties, nameof(emptyTileNeighbours), Variant.Type.Int, "Empty Tile Neighbours:", PropertyHint.None);
                    AddProperty(properties, nameof(corridorSpace), Variant.Type.Int, "Corridor Space:", PropertyHint.None);
                    AddProperty(properties, nameof(corridorMaxTurns), Variant.Type.Int, "Corridor Max Turns:", PropertyHint.None);
                    AddProperty(properties, nameof(corridorMin), Variant.Type.Int, "Corridor Min:", PropertyHint.None);
                    AddProperty(properties, nameof(corridorMax), Variant.Type.Int, "Corridor Max:", PropertyHint.None);
                    AddProperty(properties, nameof(breakOut), Variant.Type.Int, "BreakOut:", PropertyHint.None);
                    break;
                case Generators.CellularAutomataMapGenerator:
                    AddProperty(properties, nameof(fillProbability), Variant.Type.Int, "Fill Probability:", PropertyHint.None);
                    AddProperty(properties, nameof(totalIterations), Variant.Type.Int, "Total Iterations:", PropertyHint.None);
                    AddProperty(properties, nameof(cutoffOfBigAreaFill), Variant.Type.Int, "Cutoff Of Big Area Fill:", PropertyHint.None);
                    break;
                case Generators.CityMapGenerator:
                    AddProperty(properties, nameof(maxCityLeafSize), Variant.Type.Int, "Max Leaf Size:", PropertyHint.None);
                    AddProperty(properties, nameof(minCityLeafSize), Variant.Type.Int, "Min Leaf Size:", PropertyHint.None);
                    AddProperty(properties, nameof(roomMaxCitySize), Variant.Type.Int, "Room Max Size:", PropertyHint.None);
                    AddProperty(properties, nameof(roomMinCitySize), Variant.Type.Int, "Room Min Size:", PropertyHint.None);
                    break;
                case Generators.DrunkardsWalkMapGenerator:
                    AddProperty(properties, nameof(percentGoal), Variant.Type.Float, "Percent Goal:", PropertyHint.None);
                    AddProperty(properties, nameof(walkIterations), Variant.Type.Int, "Walk Iterations:", PropertyHint.None); // Changed label from "Room Max Size"
                    AddProperty(properties, nameof(weightedTowardCenter), Variant.Type.Float, "Weighted Toward Center:", PropertyHint.None);
                    AddProperty(properties, nameof(weightedTowardPreviousDirection), Variant.Type.Float, "Weighted Toward Previous Direction:", PropertyHint.None);
                    break;
                case Generators.TunnelingMazeMapGenerator:
                    AddProperty(properties, nameof(magicNumber), Variant.Type.Int, "Magic Number:", PropertyHint.None);
                    break;
                case Generators.TunnelingWithRoomsMapGenerator:
                    AddProperty(properties, nameof(maxTunnelingRooms), Variant.Type.Int, "Max Rooms:", PropertyHint.None);
                    AddProperty(properties, nameof(roomMaxTunnelingSize), Variant.Type.Int, "Room Max Size:", PropertyHint.None);
                    AddProperty(properties, nameof(roomMinTunnelingSize), Variant.Type.Int, "Room Min Size:", PropertyHint.None);
                    break;
            }

            return properties;
        }

        // Helper method to add a property dictionary
        private void AddProperty(Array<Dictionary> properties, string name, Variant.Type type, string hintString, PropertyHint hintType = PropertyHint.None)
        {
            var p = new Dictionary();
            p["name"] = name;
            p["type"] = (int)type;
            p["hint_string"] = hintString; // Display name in inspector
            p["hint"] = (int)hintType;
            // Always set these flags for editor-visible, script-exposed properties
            p["usage"] = (int)PropertyUsageFlags.Default;
            properties.Add(p);
        }
    }
}