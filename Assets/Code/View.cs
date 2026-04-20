using Assets.Code.Entities;
using Assets.Code.Scenes;
using ImageCampus.ToolBox.Services;
using Multiplayer.Arch;
using MultiplayerArchitecture.Code.Scenes;
using UnityEngine;

namespace Assets.Code
{
    public class View : MonoBehaviour
    {
        private Multiplayer.Arch.MultiplayerArchitecture multiplayerArchitecture;
        private BaseScene currentScene;

        [SerializeField] private GameObject usersPrefabs;
        [SerializeField] private Camera cam;

        private void Awake()
        {
            Application.runInBackground = true;
            Screen.SetResolution(800, 600, false);

            multiplayerArchitecture = new Multiplayer.Arch.MultiplayerArchitecture();
            currentScene = new GameplayViewScene();

            multiplayerArchitecture.Init();
            currentScene.Init(usersPrefabs, cam);
        }

        private void Start()
        {
            multiplayerArchitecture.LateInit();
            currentScene.LateInit();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            multiplayerArchitecture.Tick(deltaTime);
            currentScene.Tick(deltaTime);
        }

        private void OnApplicationQuit()
        {
            currentScene.Dispose();
        }
    }
}
