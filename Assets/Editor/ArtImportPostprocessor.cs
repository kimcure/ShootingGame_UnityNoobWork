using UnityEditor;
using UnityEngine;

/// <summary>
/// Assets/Art 폴더에 PNG를 넣으면 자동으로 픽셀아트용 Sprite 설정을 적용한다.
///  - Texture Type = Sprite (2D and UI), Single
///  - Pixels Per Unit = 32  (32x32 픽셀 스프라이트 = 1 유닛)
///  - Filter Mode = Point (픽셀이 번지지 않게),  Compression = None
///
/// 아트 담당자가 새 그림을 넣어도 설정을 일일이 바꿀 필요가 없다. (아트 작업 연동)
/// 이 스크립트는 Editor 폴더에 있어야 하며, 빌드에는 포함되지 않는다.
/// </summary>
public class ArtImportPostprocessor : AssetPostprocessor
{
    private const string ArtFolder = "Assets/Art/";
    private const int PixelsPerUnit = 32;

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ArtFolder)) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
    }
}
