using Assets.Code;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using UnityEditor;
using UnityEngine;

public class MapView : ViewComponent, IService
{
    public Vector3[] spawnPoints;
    public Vector3[] raceMarkers;

    public float collisionSize = 2.0f;

    public bool IsPersistance => false;

    private void Awake()
    {
        Coordinate[] coordinates = new Coordinate[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; ++i)
        {
            Vector3 coordinate = spawnPoints[i];
            coordinates[i] = new Coordinate(coordinate.x, coordinate.y, coordinate.z);
        }

        ServiceProvider.Instance.AddService<Map>(new Map(coordinates));
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.yellow;
        Handles.color = Color.black;
        for (int i = 0; i < raceMarkers.Length; ++i)
        {
            Handles.Label(raceMarkers[i], "RaceMark: " + i + ".");
            Gizmos.DrawCube(raceMarkers[i], Vector3.one * collisionSize);
        }

        Gizmos.color = Color.violet;
        Gizmos.DrawLineStrip(raceMarkers, true);
# endif
    }
}
