using Assets.Code.Entities;
using ImageCampus.ToolBox.Services;
using UnityEngine;

namespace Assets.Code
{
    public class View : MonoBehaviour
    {
        private Multiplayer.Arch.MultiplayerArchitecture multiplayerArchitecture;
        private EntityFactoryView entityFactoryView;

        private EntityLogicView entityLogicView;

        [SerializeField] private GameObject usersPrefabs;

        private void Awake()
        {
            Application.runInBackground = true;

            Screen.SetResolution(800, 600, false);

            multiplayerArchitecture = new Multiplayer.Arch.MultiplayerArchitecture();

            ServiceProvider.Instance.AddService<EntityRegistryView>(new EntityRegistryView());
            entityFactoryView = new EntityFactoryView(usersPrefabs);
            entityLogicView = new EntityLogicView();

            multiplayerArchitecture.Init();
            entityFactoryView.Init();
            entityLogicView.Init();
        }

        private void Start()
        {
            multiplayerArchitecture.LateInit();
            entityFactoryView.LateInit();
            entityLogicView.LateInit();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            multiplayerArchitecture.Tick(deltaTime);
        }

        private void OnApplicationQuit()
        {
            entityLogicView.Dispose();
            entityFactoryView.Dispose();
            multiplayerArchitecture.Dispose();
            ServiceProvider.Instance.ClearAllServices();
        }
    }
}
