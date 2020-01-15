﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace DownloadableClient
{
    public class EntrytPoint : MonoBehaviour
    {
        [SerializeField] TMPro.TextMeshProUGUI logText;
        [SerializeField] Canvas canvas;
        [SerializeField] Camera sceneCamera;

        private Scene thisScene;

        private void Awake()
        {
            thisScene = SceneManager.GetActiveScene();
            Application.logMessageReceived += OnLogMessageReceived;
            logText.text = "";
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(LoadInitialScene());
        }

        private IEnumerator LoadInitialScene()
        {
            AsyncOperation sceneOperation = SceneManager.LoadSceneAsync("InitialScene", LoadSceneMode.Additive);
            yield return sceneOperation;

            RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;

            DCL.WSSController.i.openBrowserWhenStart = true;
            DCL.WSSController.i.baseUrlMode = DCL.WSSController.BaseUrl.LOCAL_HOST;
            DCL.WSSController.i.forceLocalComms = true;
            DCL.WSSController.i.environment = DCL.WSSController.Environment.ZONE;
            DCL.WSSController.i.debugPanelMode = DCL.WSSController.DebugPanel.Off;

            sceneCamera.GetComponent<AudioListener>().enabled = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != thisScene)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.SetActiveScene(scene);
            }
        }

        private void OnRenderingStateChanged(bool isRendering)
        {
            if (isRendering)
            {
                RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;
                Application.logMessageReceived -= OnLogMessageReceived;
                canvas.gameObject.SetActive(false);
                sceneCamera.gameObject.SetActive(false);
            }
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            logText.text += string.Format("\n{0}", condition);
        }
    }
}