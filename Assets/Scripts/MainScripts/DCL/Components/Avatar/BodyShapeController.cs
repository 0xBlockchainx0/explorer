using DCL.Helpers;
using UnityEngine;

public class BodyShapeController : WearableController
{
    public string bodyShapeType => wearable.id;

    public BodyShapeController(WearableItem wearableItem) : base(wearableItem, wearableItem?.id) { }

    public Animation PrepareAnimation()
    {
        return assetContainer.GetOrCreateComponent<Animation>();
    }

    public SkinnedMeshRenderer GetSkinnedMeshRenderer()
    {
        return assetContainer.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    public void RemoveUnusedParts()
    {
        AvatarUtils.RemoveUnusedBodyParts_Hack(assetContainer.gameObject);
    }

    public void SetupEyes(Material material, Texture texture, Texture mask, Color color)
    {
        var eyesMaterial = new Material(material);

        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) =>
            {
                eyesMaterial.SetTexture(AvatarUtils._EyesTexture, texture);
                eyesMaterial.SetTexture(AvatarUtils._IrisMask, mask);
                eyesMaterial.SetColor(AvatarUtils._EyeTint, color);

                return eyesMaterial;
            },
            "eyes");
    }

    public void SetupEyebrows(Material material, Texture texture, Color color)
    {
        var eyebrowsMaterial = new Material(material);
        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) =>
            {
                eyebrowsMaterial.SetTexture(AvatarUtils._BaseMap, texture);

                //NOTE(Brian): This isn't an error, we must also apply hair color to this mat
                eyebrowsMaterial.SetColor(AvatarUtils._BaseColor, color);

                return eyebrowsMaterial;
            },
            "eyebrows");
    }

    public void SetupMouth(Material material, Texture texture, Color color)
    {
        var mouthMaterial = new Material(material);
        AvatarUtils.MapSharedMaterialsRecursively(assetContainer.transform,
            (mat) =>
            {
                mouthMaterial.SetTexture(AvatarUtils._BaseMap, texture);

                //NOTE(Brian): This isn't an error, we must also apply skin color to this mat
                mouthMaterial.SetColor(AvatarUtils._BaseColor, color);

                return mouthMaterial;
            },
            "mouth");
    }
}