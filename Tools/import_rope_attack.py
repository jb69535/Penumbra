"""Import rope textures and attack animation frames from Downloads/game."""
from __future__ import annotations

import re
import uuid
from collections import deque
from pathlib import Path

from PIL import Image

PROJECT = Path(__file__).resolve().parents[1]
SOURCE = Path(r"C:\Users\FLEXBOY\Downloads\game")
ROPE_ART = PROJECT / "Assets" / "Penumbra" / "Art" / "Combat" / "Rope"
ATTACK_FRAMES = PROJECT / "Assets" / "Penumbra" / "Art" / "Characters" / "Player" / "Frames" / "Attack"
TARGET_HEIGHT = 1.8
WHITE_BACKGROUND_MIN = 230
WHITE_BACKGROUND_NEUTRAL_RANGE = 6


def new_guid() -> str:
    return uuid.uuid4().hex


def read_existing_guid(asset_path: Path) -> str | None:
    meta_path = asset_path.with_suffix(asset_path.suffix + ".meta")
    if not meta_path.exists():
        return None
    match = re.search(r"^guid: ([0-9a-f]{32})$", meta_path.read_text(encoding="utf-8"), re.MULTILINE)
    return match.group(1) if match else None


def is_white_background_pixel(r: int, g: int, b: int, a: int) -> bool:
    if a < 12:
        return True
    if r <= WHITE_BACKGROUND_MIN or g <= WHITE_BACKGROUND_MIN or b <= WHITE_BACKGROUND_MIN:
        return False
    return max(r, g, b) - min(r, g, b) <= WHITE_BACKGROUND_NEUTRAL_RANGE


def flood_remove_background(image: Image.Image, pixel_predicate) -> Image.Image:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    visited = bytearray(width * height)
    queue: deque[tuple[int, int]] = deque()

    def enqueue(x: int, y: int) -> None:
        if 0 <= x < width and 0 <= y < height:
            queue.append((x, y))

    for x in range(width):
        enqueue(x, 0)
        enqueue(x, height - 1)
    for y in range(height):
        enqueue(0, y)
        enqueue(width - 1, y)

    while queue:
        x, y = queue.popleft()
        index = y * width + x
        if visited[index]:
            continue
        visited[index] = 1

        r, g, b, a = pixels[x, y]
        if not pixel_predicate(r, g, b, a):
            continue

        pixels[x, y] = (0, 0, 0, 0)
        enqueue(x + 1, y)
        enqueue(x - 1, y)
        enqueue(x, y + 1)
        enqueue(x, y - 1)

    return rgba


def crop_to_content(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    min_x = width
    min_y = height
    max_x = -1
    max_y = -1

    for y in range(height):
        for x in range(width):
            if pixels[x, y][3] > 20:
                min_x = min(min_x, x)
                min_y = min(min_y, y)
                max_x = max(max_x, x)
                max_y = max(max_y, y)

    if max_x < min_x or max_y < min_y:
        return rgba

    return rgba.crop((min_x, min_y, max_x + 1, max_y + 1))


def process_rope_body(source: Path) -> Image.Image:
    image = Image.open(source)
    image = flood_remove_background(image, is_white_background_pixel)
    return crop_to_content(image)


def process_rope_sprite(source: Path) -> Image.Image:
    image = Image.open(source).convert("RGBA")
    return crop_to_content(image)


def reference_ppu() -> float:
    idle_meta = PROJECT / "Assets" / "Penumbra" / "Art" / "Characters" / "Player" / "player_idle_0.png.meta"
    if idle_meta.exists():
        for line in idle_meta.read_text(encoding="utf-8").splitlines():
            if "spritePixelsToUnits:" in line:
                try:
                    return float(line.split(":", 1)[1].strip())
                except ValueError:
                    break

    idle_path = PROJECT / "Assets" / "Penumbra" / "Art" / "Characters" / "Player" / "player_idle_0.png"
    if idle_path.exists():
        with Image.open(idle_path) as image:
            if image.height > 0:
                return image.height / TARGET_HEIGHT

    return 100.0


def write_default_tile_meta(asset_path: Path, guid: str) -> None:
    meta = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 0
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 0
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    asset_path.with_suffix(asset_path.suffix + ".meta").write_text(meta, encoding="utf-8")


def write_sprite_meta(asset_path: Path, guid: str, ppu: float) -> None:
    meta = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    asset_path.with_suffix(asset_path.suffix + ".meta").write_text(meta, encoding="utf-8")


def import_rope_assets(ppu: float) -> None:
    ROPE_ART.mkdir(parents=True, exist_ok=True)
    rope_source = SOURCE / "rope"

    body_source = rope_source / "rope_body.png"
    if body_source.exists():
        body_dest = ROPE_ART / "rope_body_tile.png"
        processed = process_rope_body(body_source)
        processed.save(body_dest, "PNG")
        guid = read_existing_guid(body_dest) or new_guid()
        write_default_tile_meta(body_dest, guid)
        print(f"Rope body tile: {processed.size[0]}x{processed.size[1]} (background removed)")

    for name in ("rope_tip", "rope_handle"):
        source = rope_source / f"{name}.png"
        if not source.exists():
            continue
        dest = ROPE_ART / f"{name}.png"
        processed = process_rope_sprite(source)
        processed.save(dest, "PNG")
        guid = read_existing_guid(dest) or new_guid()
        write_sprite_meta(dest, guid, ppu)
        print(f"Rope sprite {name}: {processed.size[0]}x{processed.size[1]}")


def import_attack_frames(ppu: float) -> list[tuple[str, str]]:
    attack_source = SOURCE / "attack"
    if not attack_source.is_dir():
        print("Attack source folder missing.")
        return []

    import sys
    tools_dir = PROJECT / "Tools"
    if str(tools_dir) not in sys.path:
        sys.path.insert(0, str(tools_dir))

    import import_game_sprites as game_import

    idle_source = game_import.resolve_idle_source()
    reference_ppu, reference_canvas_height, reference_content_height = game_import.compute_reference_metrics(idle_source)

    entries = game_import.import_frame_folder(
        attack_source,
        ATTACK_FRAMES,
        "attack",
        reference_ppu,
        reference_canvas_height,
        reference_content_height,
        remove_animation_background=True,
        scale_content_to_reference=True,
    )
    for name, guid in entries:
        print(f"Attack frame: {name} guid={guid}")
    return entries


def main() -> None:
    if not SOURCE.is_dir():
        raise FileNotFoundError(f"Source folder not found: {SOURCE}")

    ppu = reference_ppu()
    print(f"Using PPU {ppu:.2f}")
    import_rope_assets(ppu)
    import_attack_frames(ppu)
    print("Rope and attack import complete.")


if __name__ == "__main__":
    main()
