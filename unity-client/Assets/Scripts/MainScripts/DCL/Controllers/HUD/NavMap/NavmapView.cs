using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;
using DCL.Helpers;
using TMPro;

namespace DCL
{
    public class NavmapView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] InputAction_Trigger toggleNavMapAction;
        [SerializeField] InputAction_Trigger selectParcelAction;
        [SerializeField] Button closeButton;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] Transform scrollRectContentTransform;
        [SerializeField] TextMeshProUGUI currentSceneNameText;
        [SerializeField] TextMeshProUGUI currentSceneCoordsText;
        [SerializeField] internal NavmapToastView toastView;

        InputAction_Trigger.Triggered toggleNavMapDelegate;
        InputAction_Trigger.Triggered selectParcelDelegate;
        RectTransform minimapViewport;
        Transform mapRendererMinimapParent;
        Vector3 atlasOriginalPosition;
        MinimapMetadata mapMetadata;
        bool cursorLockedBeforeOpening = true;
        Vector2Int lastParcelClicked = new Vector2Int();

        // TODO: remove this bool and its usage once the feature is ready to be shippped.
        bool enableInProduction = false;

        public static bool isOpen
        {
            private set;
            get;
        } = false;

        void Start()
        {
            mapMetadata = MinimapMetadata.GetMetadata();

            closeButton.onClick.AddListener(() => { ToggleNavMap(); });
            scrollRect.onValueChanged.AddListener((x) => { if (isOpen) MapRenderer.i.atlas.UpdateCulling(); });

            toggleNavMapDelegate = (x) => { if (!Input.GetKeyDown(KeyCode.Escape) || isOpen) ToggleNavMap(); };
            toggleNavMapAction.OnTriggered += toggleNavMapDelegate;
            toastView.OnGotoClicked += ToggleNavMap;

            MapRenderer.OnParcelClicked += (x, y) => OnParcelClicked(x, y);
            mapMetadata.OnSceneInfoUpdated += OnMapMetadataInfoUpdated;

            MinimapHUDView.OnUpdateData += UpdateCurrentSceneData;
            MinimapHUDView.OnOpenNavmapClicked += ToggleNavMap;

            toastView.gameObject.SetActive(false);
            scrollRect.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            toastView.OnGotoClicked -= ToggleNavMap;
            MinimapHUDView.OnUpdateData -= UpdateCurrentSceneData;
            mapMetadata.OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;
            MinimapHUDView.OnOpenNavmapClicked -= ToggleNavMap;
        }

        internal void ToggleNavMap()
        {
            if (MapRenderer.i == null) return;

#if !UNITY_EDITOR
            if (!enableInProduction) return;
#endif

            scrollRect.StopMovement();

            isOpen = !isOpen;
            scrollRect.gameObject.SetActive(isOpen);
            MapRenderer.i.parcelHighlightEnabled = isOpen;

            if (isOpen)
            {
                cursorLockedBeforeOpening = Utils.isCursorLocked;
                if (cursorLockedBeforeOpening)
                    Utils.UnlockCursor();

                minimapViewport = MapRenderer.i.atlas.viewport;
                mapRendererMinimapParent = MapRenderer.i.transform.parent;
                atlasOriginalPosition = MapRenderer.i.atlas.chunksParent.transform.localPosition;

                MapRenderer.i.atlas.viewport = scrollRect.viewport;
                MapRenderer.i.transform.SetParent(scrollRectContentTransform);
                MapRenderer.i.atlas.UpdateCulling();

                scrollRect.content = MapRenderer.i.atlas.chunksParent.transform as RectTransform;

                // Reparent the player icon parent to scroll everything together
                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(scrollRect.content);

                // Center map
                MapRenderer.i.atlas.CenterToTile(Utils.WorldToGridPositionUnclamped(CommonScriptableObjects.playerWorldPosition));
            }
            else
            {
                if (cursorLockedBeforeOpening)
                    Utils.LockCursor();

                toastView.OnCloseClick();

                MapRenderer.i.atlas.viewport = minimapViewport;
                MapRenderer.i.transform.SetParent(mapRendererMinimapParent);
                MapRenderer.i.atlas.chunksParent.transform.localPosition = atlasOriginalPosition;
                MapRenderer.i.atlas.UpdateCulling();

                // Restore the player icon to its original parent
                MapRenderer.i.atlas.overlayLayerGameobject.transform.SetParent(MapRenderer.i.atlas.chunksParent.transform.parent);
                (MapRenderer.i.atlas.overlayLayerGameobject.transform as RectTransform).anchoredPosition = Vector2.zero;

                MapRenderer.i.UpdateRendering(Utils.WorldToGridPositionUnclamped(CommonScriptableObjects.playerWorldPosition.Get()));
            }
        }

        void UpdateCurrentSceneData(MinimapHUDModel model)
        {
            currentSceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? "Unnamed" : model.sceneName;
            currentSceneCoordsText.text = model.playerPosition;
        }

        void OnParcelClicked(int mouseTileX, int mouseTileY, bool requestSceneInfoIfMissing = true)
        {
            var sceneInfo = mapMetadata.GetSceneInfo(mouseTileX, mouseTileY);
            if (requestSceneInfoIfMissing && sceneInfo == null)
                WebInterface.RequestScenesInfoAroundParcel(new Vector2(mouseTileX, mouseTileY), 1);

            toastView.Populate(new Vector2Int(mouseTileX, mouseTileY), sceneInfo);
            lastParcelClicked.Set(mouseTileX, mouseTileY);
        }

        void OnMapMetadataInfoUpdated(MinimapMetadata.MinimapSceneInfo sceneInfo)
        {
            if (toastView.isOpen) OnParcelClicked(lastParcelClicked.x, lastParcelClicked.y, false);
        }
    }
}