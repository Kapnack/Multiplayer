using Assets.Code;
using ImageCampus.ToolBox.Services;
using UnityEngine;

public class MultiplayerView : MonoBehaviour
{
    MultiplayerArchitecture architecture;

    InputManager inputManager;

    EntityFactoryView entityFactoryView;


    [SerializeField] private GameObject usersPrefabs;

    private void Awake()
    {
        Application.runInBackground = true;

        Screen.fullScreen = false;
        Screen.SetResolution(800, 600, false);

        architecture = new MultiplayerArchitecture();
        architecture.Init();

        inputManager = new InputManager();

        ServiceProvider.Instance.AddService<EntityRegistryView>(new EntityRegistryView());

        entityFactoryView = new EntityFactoryView(usersPrefabs);

    }

    private void Start()
    {
        architecture.LateInit();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        architecture.Tick(deltaTime);
        inputManager.Tick(deltaTime);
    }

    private void OnApplicationQuit()
    {
        architecture.Dispose();
        entityFactoryView.Dispose();
    }
}
