using Assets.Code;
using ImageCampus.ToolBox.Services;
using UnityEditor;
using UnityEngine;

public class MapView : ViewComponent, IService
{
    public Vector3[] spawnPoints;
    public Vector3[] raceMarkers;

    public float collisionSize = 2.0f;

    public bool IsPersistance => false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Handles.color = Color.black;
        for (int i = 0; i < raceMarkers.Length; ++i)
        {
            Handles.Label(raceMarkers[i], "RaceMark: " + i + ".");
            Gizmos.DrawCube(raceMarkers[i], Vector3.one * collisionSize);
        }

        Gizmos.color = Color.violet;
        Gizmos.DrawLineStrip(raceMarkers, true);

    }
}
