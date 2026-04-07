using Assets.Code.Architecture.Code.Math;
using ImageCampus.ToolBox.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Code.Architecture.Code.Entities.Events
{
    public struct SpawnRequestAcceptedEvent<EntityType> : IEvent
    {
        public uint networkID;

        public void Assign(params object[] parameters)
        {
            networkID = (uint)parameters[0];
        }

        public void Reset()
        {
            networkID = default(uint);
        }
    }

    public struct SpawnRequestAcceptedEvent : IEvent
    {
        public string blueprintToSpawn;
        public Coordinate coordinateToSpawn;
        public string blueprintTable;
        public string entityTypeName;

        public void Assign(params object[] parameters)
        {
            blueprintToSpawn = (string)parameters[0];
            coordinateToSpawn = (Coordinate)parameters[1];
            blueprintTable = (string)parameters[2];
            entityTypeName = (string)parameters[3];
        }

        public void Reset()
        {
            blueprintToSpawn = string.Empty;
            coordinateToSpawn = default(Coordinate);
            blueprintTable = default(string);
            entityTypeName = default(string);
        }
    }
}
