using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using DCL.Configuration;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

public class IntegrationTestController : MonoBehaviour
{
    string entityId = "a5f571bd-bce1-4cf8-a158-b8f3e92e4fb0";

    void Awake()
    {
        var sceneController = Object.FindObjectOfType<SceneController>();

        var scenesToLoad = new
        {
            parcelsToLoad = new[]
            {
                new LoadParcelScenesMessage.UnityParcelScene()
                {
                    id = "the-loaded-scene",
                    basePosition = new Vector2Int(3, 3),
                    parcels = new []
                    {
                        new Vector2Int(3, 3),
                        new Vector2Int(3, 4)
                    },
                    baseUrl = "http://localhost:9991/local-ipfs/contents/"
                }
            }
        };

        Assert.IsTrue(sceneController != null, "Cannot find SceneController");

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(JsonConvert.SerializeObject(scenesToLoad));

        var scene = sceneController.loadedScenes["the-loaded-scene"];

        //NOTE(Brian): This is making my eyes bleed.
        sceneController.SendSceneMessage(
          TestHelpers.CreateSceneMessage(
                "the-loaded-scene",
                "CreateEntity",
                JsonConvert.SerializeObject(
                    new CreateEntityMessage
                    {
                        id = entityId
                    }))
                );

        //NOTE(Brian): This is making my eyes bleed.
        sceneController.SendSceneMessage(
          TestHelpers.CreateSceneMessage(
            "the-loaded-scene",
            "SetEntityParent",
            JsonConvert.SerializeObject(
                new
                {
                    entityId = entityId,
                    parentId = "0"
                })
             )
        );

        Assert.IsTrue(scene.entities[entityId].meshGameObject == null, "meshGameObject must be null");

        // 1st message
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");

        scene.EntityComponentCreate(JsonConvert.SerializeObject(new EntityComponentCreateMessage
        {
            entityId = entityId,
            name = "transform",
            classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
            json = "{\"tag\":\"transform\",\"position\":{\"x\":0,\"y\":0,\"z\":0},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1},\"scale\":{\"x\":1,\"y\":1,\"z\":1}}"
        }));

        // 2nd message
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");

        scene.EntityComponentCreate(JsonConvert.SerializeObject(new EntityComponentCreateMessage
        {
            entityId = entityId,
            name = "transform",
            classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
            json = "{\"tag\":\"transform\",\"position\":{\"x\":6,\"y\":0,\"z\":5},\"rotation\":{\"x\":0,\"y\":0.39134957508996265,\"z\":0,\"w\":0.9202420931897769},\"scale\":{\"x\":1,\"y\":1,\"z\":1}}"
        }));

        TestHelpers.InstantiateEntityWithTextShape(scene, new Vector3(10, 10, 10), new TextShape.Model() { value = "Hello World!!!" });
    }

    public void Verify()
    {
        var sceneController = FindObjectOfType<SceneController>();
        var scene = sceneController.loadedScenes["the-loaded-scene"];
        var cube = scene.entities[entityId];

        Assert.IsTrue(cube != null);
        Vector3 cubePosition = new Vector3(6, 0, 5);
        Assert.AreEqual(cube.gameObject.transform.localPosition, cubePosition);

        // because basePosition is at 3,3
        Assert.AreEqual(cube.gameObject.transform.position, new Vector3(3 * ParcelSettings.PARCEL_SIZE + cubePosition.x, cubePosition.y, 3 * ParcelSettings.PARCEL_SIZE + cubePosition.z));
        Assert.IsNotNull(cube.meshGameObject);
        Assert.IsNotNull(cube.meshGameObject.GetComponentInChildren<MeshFilter>());

        var mesh = cube.meshGameObject.GetComponentInChildren<MeshFilter>().mesh;

        Assert.AreEqual(mesh.name, "DCL Box Instance");

        {
            // 3nd message, the box should remain the same, including references
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");

            var newMesh = cube.meshGameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Box Instance");
            Assert.AreEqual(mesh.name, newMesh.name, "A new instance of the box was created");
        }

        {
            // 3nd message, the box should remain the same, including references
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");

            var newMesh = cube.meshGameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Box Instance");
            Assert.AreEqual(mesh.name, newMesh.name, "A new instance of the box was created");
        }

        {
            // 4nd message, the box should be disposed and the new mesh should be a sphere
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.SPHERE_SHAPE, "{\"withCollisions\":false,\"billboard\":0,\"visible\":true,\"tag\":\"sphere\"}");

            var newMesh = cube.meshGameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Sphere Instance");
            Assert.AreNotEqual(mesh.name, newMesh.name, "The mesh instance remains the same, a new instance should have been created.");
        }


        // TODO: test ComponentRemoved
    }
}
