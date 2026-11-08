using UnityEditor;
using UnityEngine;

/// <summary>
/// Gives icons rendered by Tools/PhosphorIcons/render.mjs the right import settings:
/// a UI sprite, Alpha Is Transparency, mip maps (clean when shown smaller than the source) and no compression.
/// Applies only on an icon's first import, so changes you make later in the Inspector stay.
/// </summary>
public class PhosphorIconImporter : AssetPostprocessor
{
    const string Folder = "Assets/Media/Sprites/Phosphor/";

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Folder) || !assetImporter.importSettingsMissing)
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
    }
}
