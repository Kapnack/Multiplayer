using UnityEngine;

public class MultiplayerView : MonoBehaviour
{
    MultiplayerArchitecture architecture;

    private void Awake()
    {
        Application.runInBackground = true;

        Screen.fullScreen = false;
        Screen.SetResolution(800, 600, false);

        architecture = new MultiplayerArchitecture();

        architecture.Init();
    }

    private void Start()
    {
        architecture.LateInit();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        architecture.Tick(deltaTime);
    }

    private void OnApplicationQuit()
    {
        architecture.Dispose();
    }
}
