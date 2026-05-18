using Assets.Code.Scenes;
using Assets.MultiplayerArchitecture.Code.Entities.ItemBox;
using ImageCampus.ToolBox.Events;
using ImageCampus.ToolBox.Services;
using MultiplayerArchitecture;
using MultiplayerArchitecture.Code.Scenes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using MutliplayerView.Game.Mapping;

namespace Assets.Code
{
    public class View : MonoBehaviour
    {
        private Multiplayer.Arch.MultiplayerArchitecture multiplayerArchitecture;
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        private BaseSceneView CurrentScene => ServiceProvider.Instance.GetService<BaseSceneView>();
        private MapToLoad MapToLoad => ServiceProvider.Instance.GetService<MapToLoad>();

        [SerializeField] private GameObject usersPrefabs;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject bananaPrefab;
        [SerializeField] private GameObject oilPrefab;
        [SerializeField] private GameObject[] maps;
        [SerializeField] private Camera cam;
        [SerializeField] private TMP_Text pingText;

        [SerializeField] private InputField nameText;
        [SerializeField] private InputField ip;
        [SerializeField] private Button connect;
        [SerializeField] private Button connect2;

        private Dictionary<Maps, GameObject> mapsByEnum;

        public View()
        {
            mapsByEnum = new Dictionary<Maps, GameObject>();
        }

        private void Awake()
        {
            uint currentMapIterator = 0;

            ViewArchitectureMap.Init();
            ItemTypes.Init();

            foreach (object mapEnum in Enum.GetValues(typeof(Maps)))
                mapsByEnum.Add((Maps)mapEnum, currentMapIterator == maps.Length ? maps[currentMapIterator - 1] : maps[currentMapIterator++]);

            ServiceProvider.Instance.AddService<MapToLoad>(new MapToLoad());

            pingText.gameObject.SetActive(false);
            pingText.gameObject.SetActive(false);
            nameText.gameObject.SetActive(false);
            ip.gameObject.SetActive(false);
            connect.gameObject.SetActive(false);
            connect2.gameObject.SetActive(false);

            Application.runInBackground = true;
            Screen.SetResolution(800, 600, false);

            multiplayerArchitecture = new Multiplayer.Arch.MultiplayerArchitecture();

            multiplayerArchitecture.Init();

            EventBus.Subscribe<ChangeSceneEvent>(OnSceneChange);
        }

        private void Start()
        {
            multiplayerArchitecture.LateInit();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            multiplayerArchitecture.Tick(deltaTime);
            CurrentScene.Tick(deltaTime);
        }

        private void OnApplicationQuit()
        {
            EventBus.Unsubscribe<ChangeSceneEvent>(OnSceneChange);
            CurrentScene.Dispose();
            ServiceProvider.Instance.ClearAllServices();
        }

        private void OnSceneChange(in ChangeSceneEvent changeSceneEvent)
        {
            if (ServiceProvider.Instance.ContainsService<BaseSceneView>())
            {
                CurrentScene.Dispose();
                ServiceProvider.Instance.RemoveService<BaseSceneView>();
            }

            ServiceProvider.Instance.AddService<BaseSceneView>(changeSceneEvent.scene == Scene.MainMenu ?
                new MainMenuViewScene(nameText, ip, connect, connect2) :
                new GameplayViewScene(mapsByEnum[MapToLoad.mapToLoad], bulletPrefab, bananaPrefab, oilPrefab, pingText, usersPrefabs, cam));

            CurrentScene.Init();
            CurrentScene.LateInit();
        }
    }
}
