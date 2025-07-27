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

        //public override bool _Set(StringName property, Variant value)
        //{

        //    // First, check if the property being set is one of our dynamic ones
        //    switch (property.ToString())
        //    {
        //        case nameof(maxLeafSize):
        //            maxLeafSize = (int)value; return true;
        //        case nameof(minLeafSize):
        //            minLeafSize = (int)value; return true;
        //        case nameof(roomMaxSize):
        //            roomMaxSize = (int)value; return true;
        //        case nameof(roomMinSize):
        //            roomMinSize = (int)value; return true;

        //        case nameof(neighbours):
        //            neighbours = (int)value; return true;
        //        case nameof(iterations):
        //            iterations = (int)value; return true;
        //        case nameof(closeTileProb):
        //            closeTileProb = (int)value; return true;
        //        case nameof(lowerLimit):
        //            lowerLimit = (int)value; return true;
        //        case nameof(upperLimit):
        //            upperLimit = (int)value; return true;
        //        case nameof(emptyNeighbours):
        //            emptyNeighbours = (int)value; return true;
        //        case nameof(emptyTileNeighbours):
        //            emptyTileNeighbours = (int)value; return true;
        //        case nameof(corridorSpace):
        //            corridorSpace = (int)value; return true;
        //        case nameof(corridorMaxTurns):
        //            corridorMaxTurns = (int)value; return true;
        //        case nameof(corridorMin):
        //            corridorMin = (int)value; return true;
        //        case nameof(corridorMax):
        //            corridorMax = (int)value; return true;
        //        case nameof(breakOut):
        //            breakOut = (int)value; return true;

        //        case nameof(fillProbability):
        //            fillProbability = (int)value; return true;
        //        case nameof(totalIterations):
        //            totalIterations = (int)value; return true;
        //        case nameof(cutoffOfBigAreaFill):
        //            cutoffOfBigAreaFill = (int)value; return true;

        //        case nameof(maxCityLeafSize):
        //            maxCityLeafSize = (int)value; return true;
        //        case nameof(minCityLeafSize):
        //            minCityLeafSize = (int)value; return true;
        //        case nameof(roomMaxCitySize):
        //            roomMaxCitySize = (int)value; return true;
        //        case nameof(roomMinCitySize):
        //            roomMinCitySize = (int)value; return true;

        //        case nameof(percentGoal):
        //            percentGoal = (float)value; return true;
        //        case nameof(walkIterations):
        //            walkIterations = (int)value; return true;
        //        case nameof(weightedTowardCenter):
        //            weightedTowardCenter = (float)value; return true;
        //        case nameof(weightedTowardPreviousDirection):
        //            weightedTowardPreviousDirection = (float)value; return true;

        //        case nameof(magicNumber):
        //            magicNumber = (int)value; return true;

        //        case nameof(maxTunnelingRooms):
        //            maxTunnelingRooms = (int)value; return true;
        //        case nameof(roomMaxTunnelingSize):
        //            roomMaxTunnelingSize = (int)value; return true;
        //        case nameof(roomMinTunnelingSize):
        //            roomMinTunnelingSize = (int)value; return true;
        //    }

        //    // Let the base class handle all other properties (especially the [Export]ed ones)
        //    return base._Set(property, value);
        //}

        //// This method is called by the editor to get the current value of a property for display.
        //// Similar to _Set, for auto-properties, Godot's default _Get handles them.
        //// You would only need to override this if you had custom logic for retrieving values (e.g., calculated properties).
        //public override Variant _Get(StringName property)
        //{
        //    // If you had private backing fields, you would explicitly return them here.
        //    // Example:
        //    // if (property == nameof(maxLeafSize)) return maxLeafSize;
        //    // For auto-properties, base._Get often suffices.
        //    return base._Get(property);
        //}


        // --- Optional: Add a button to generate the map ---
        // This won't show up with _GetPropertyList() automatically,
        // but you can add a method that generates and call it from the editor
        // or add a custom editor plugin for a button. For now, let's assume
        // you'd trigger generation via another mechanism or a simple [Export] button.
        //[Export]
        //public bool GenerateMap { get => false; set { if (value) GD.Print("Generating Map..."); /* Call your generation logic here */ } }
    }
}