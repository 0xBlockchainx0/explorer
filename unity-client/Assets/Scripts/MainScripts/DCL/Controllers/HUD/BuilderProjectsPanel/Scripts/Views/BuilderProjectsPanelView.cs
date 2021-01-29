﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

internal class BuilderProjectsPanelView : MonoBehaviour, IDeployedSceneListener, IProjectSceneListener
{
    [Header("References")]
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button createSceneButton;
    [SerializeField] internal Button importSceneButton;

    [SerializeField] internal Transform sectionsContainer;

    [SerializeField] internal LeftMenuButtonToggleView scenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView inWorldScenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView projectsToggle;
    [SerializeField] internal LeftMenuButtonToggleView landToggle;

    [Header("Prefabs")]
    [SerializeField] internal SceneCardView sceneCardViewPrefab;
    [SerializeField] internal SectionViewFactory sectionViewFactory;

    public event Action OnClosePressed;
    public event Action OnCreateScenePressed;
    public event Action OnImportScenePressed;
    public event Action<bool> OnScenesToggleChanged;
    public event Action<bool> OnInWorldScenesToggleChanged;
    public event Action<bool> OnProjectsToggleChanged;
    public event Action<bool> OnLandToggleChanged;

    private int deployedScenesCount = 0;
    private int projectScenesCount = 0;

    private void Awake()
    {
        MOCKUP();

        closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
        createSceneButton.onClick.AddListener(() => OnCreateScenePressed?.Invoke());
        importSceneButton.onClick.AddListener(() => OnImportScenePressed?.Invoke());

        scenesToggle.OnToggleValueChanged += OnScenesToggleChanged;
        inWorldScenesToggle.OnToggleValueChanged += OnInWorldScenesToggleChanged;
        projectsToggle.OnToggleValueChanged += OnProjectsToggleChanged;
        landToggle.OnToggleValueChanged += OnLandToggleChanged;
    }

    private void SubmenuScenesDirty()
    {
        inWorldScenesToggle.gameObject.SetActive(deployedScenesCount > 0);
        projectsToggle.gameObject.SetActive(projectScenesCount > 0);
    }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        deployedScenesCount = scenes.Count;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        projectScenesCount = scenes.Count;
        SubmenuScenesDirty();
    }

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        deployedScenesCount++;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        projectScenesCount++;
        SubmenuScenesDirty();
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        deployedScenesCount--;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        projectScenesCount--;
        SubmenuScenesDirty();
    }

    private void MOCKUP()
    {
        SectionsController sectionsController = new SectionsController(sectionViewFactory, sectionsContainer);
        ScenesViewController scenesViewController = new ScenesViewController(sceneCardViewPrefab);

        OnScenesToggleChanged += (isOn) =>
        {
            if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_MAIN);
        };
        OnInWorldScenesToggleChanged += (isOn) =>
        {
            if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_DEPLOYED);
        };
        OnProjectsToggleChanged += (isOn) =>
        {
            if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_PROJECT);
        };
        OnLandToggleChanged += (isOn) =>
        {
            if (isOn) sectionsController.OpenSection(SectionsController.SectionId.LAND);
        };

        sectionsController.OnSectionShow += sectionBase =>
        {
            if (sectionBase is IDeployedSceneListener deployedSceneListener)
            {
                scenesViewController.OnDeployedSceneAdded += deployedSceneListener.OnSceneAdded;
                scenesViewController.OnDeployedSceneRemoved += deployedSceneListener.OnSceneRemoved;
                scenesViewController.OnDeployedScenesSet += deployedSceneListener.OnSetScenes;
                scenesViewController.NotifySet(deployedSceneListener);
            }
            if (sectionBase is IProjectSceneListener projectSceneListener)
            {
                scenesViewController.OnProjectSceneAdded += projectSceneListener.OnSceneAdded;
                scenesViewController.OnProjectSceneRemoved += projectSceneListener.OnSceneRemoved;
                scenesViewController.OnProjectScenesSet += projectSceneListener.OnSetScenes;
                scenesViewController.NotifySet(projectSceneListener);
            }
        };

        sectionsController.OnSectionHide += sectionBase =>
        {
            if (sectionBase is IDeployedSceneListener deployedSceneListener)
            {
                scenesViewController.OnDeployedSceneAdded -= deployedSceneListener.OnSceneAdded;
                scenesViewController.OnDeployedSceneRemoved -= deployedSceneListener.OnSceneRemoved;
                scenesViewController.OnDeployedScenesSet -= deployedSceneListener.OnSetScenes;
            }
            if (sectionBase is IProjectSceneListener projectSceneListener)
            {
                scenesViewController.OnProjectSceneAdded -= projectSceneListener.OnSceneAdded;
                scenesViewController.OnProjectSceneRemoved -= projectSceneListener.OnSceneRemoved;
                scenesViewController.OnProjectScenesSet -= projectSceneListener.OnSetScenes;
            }
        };

        IDeployedSceneListener thisDeployedSceneListener = this;
        IProjectSceneListener thisProjectSceneListener = this;
        scenesViewController.OnDeployedSceneAdded += thisDeployedSceneListener.OnSceneAdded;
        scenesViewController.OnDeployedSceneRemoved += thisDeployedSceneListener.OnSceneRemoved;
        scenesViewController.OnDeployedScenesSet += thisDeployedSceneListener.OnSetScenes;
        scenesViewController.OnProjectSceneAdded += thisProjectSceneListener.OnSceneAdded;
        scenesViewController.OnProjectSceneRemoved += thisProjectSceneListener.OnSceneRemoved;
        scenesViewController.OnProjectScenesSet += thisProjectSceneListener.OnSetScenes;
        scenesViewController.NotifySet(thisDeployedSceneListener);
        scenesViewController.NotifySet(thisProjectSceneListener);

        StartCoroutine(MOCKUP_COROUNTINE(scenesViewController));
    }

    private IEnumerator MOCKUP_COROUNTINE(ScenesViewController scenesViewController)
    {
        const float TIME = 2;
        List<ISceneData> scenes = new List<ISceneData>();

        yield return new WaitForSeconds(TIME);
        Debug.Log("ADD PROJECT");

        scenes.Add(new SceneData()
        {
            id = "MyProject1",
            isDeployed = false,
            name = "MyProject1"
        });
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("ADD DEPLOY");

        scenes.Add(new SceneData()
        {
            id = "MyDeploy1",
            isDeployed = true,
            name = "MyDeploy1"
        });
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("ADD DEPLOY2");

        scenes.Add(new SceneData()
        {
            id = "MyDeploy2",
            isDeployed = true,
            name = "MyDeploy2"
        });
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("ADD DEPLOY3");

        scenes.Add(new SceneData()
        {
            id = "MyDeploy3",
            isDeployed = true,
            name = "MyDeploy3"
        });
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("ADD DEPLOY4");

        scenes.Add(new SceneData()
        {
            id = "MyDeploy4",
            isDeployed = true,
            name = "MyDeploy4"
        });
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("REMOVE DEPLOY3");

        scenes = scenes.FindAll((data) => data.id != "MyDeploy3");
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("REMOVE DEPLOY4");

        scenes = scenes.FindAll((data) => data.id != "MyDeploy4");
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("REMOVE DEPLOY2");

        scenes = scenes.FindAll((data) => data.id != "MyDeploy2");
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("REMOVE DEPLOY1");

        scenes = scenes.FindAll((data) => data.id != "MyDeploy1");
        scenesViewController.SetScenes(scenes);

        yield return new WaitForSeconds(TIME);
        Debug.Log("REMOVE ALL");
        scenesViewController.SetScenes(new List<ISceneData>());

    }
}
