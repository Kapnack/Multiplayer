using Assets.Code.Scenes;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture.Code.Scenes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code
{
    public class View : MonoBehaviour
    {
        private Multiplayer.Arch.MultiplayerArchitecture multiplayerArchitecture;

        private BaseSceneView CurrentScene => ServiceProvider.Instance.GetService<BaseSceneView>();

        [SerializeField] private GameObject usersPrefabs;
        [SerializeField] private GameObject map1;
        [SerializeField] private GameObject map2;
        [SerializeField] private Camera cam;
        [SerializeField] private TMP_Text pingText;

        [SerializeField] private InputField nameText;
        [SerializeField] private InputField levelText;
        [SerializeField] private InputField ip;
        [SerializeField] private Button connect;

        private void Awake()
        {
            Application.runInBackground = true;
            Screen.SetResolution(800, 600, false);

            ServiceProvider.Instance.AddService<BaseScene>(new MainMenuViewScene());

            multiplayerArchitecture = new Multiplayer.Arch.MultiplayerArchitecture();

            multiplayerArchitecture.Init();
            CurrentScene.Init(nameText, levelText, ip, connect);
        }

        private void Start()
        {
            multiplayerArchitecture.LateInit();
            CurrentScene.LateInit();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            multiplayerArchitecture.Tick(deltaTime);
            CurrentScene.Tick(deltaTime);
        }

        private void OnApplicationQuit()
        {
            CurrentScene.Dispose();
        }
    }
}
