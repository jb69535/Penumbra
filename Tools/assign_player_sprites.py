"""Generate Unity sprite meta files and update CinderWisp_Player prefab."""
from __future__ import annotations

import hashlib
import re
import struct
from pathlib import Path

from PIL import Image

PROJECT = Path(__file__).resolve().parents[1]
ART = PROJECT / "Assets" / "Penumbra" / "Art" / "Characters" / "Player"
PREFAB = PROJECT / "Assets" / "Penumbra" / "Prefabs" / "Player" / "CinderWisp_Player.prefab"
TARGET_HEIGHT = 1.8
COLS, ROWS = 3, 2

SHEET_GUIDS = {
    "player_run_sheet.png": "11437c6b714ab5f41a5f07a15f95cc81",
    "player_jump_sheet.png": "cc721dc601c32094bbfe03e9078c63b5",
}

IDLE_GUID = "6f6f8af46893ca94bb23d7b815d98f27"
IDLE_INTERNAL = -5115618752126089108
DASH_GUID = "72dcece972b6ee545bee73f60d9ab0ca"
DASH_INTERNAL = -1768592330400461901


def internal_id(sheet: str, index: int) -> int:
    digest = hashlib.md5(f"{sheet}:{index}".encode()).digest()
    value = struct.unpack(">q", digest[:8])[0]
    if value == 0:
        value = -1
    if value > 0:
        value = -value
    return value


def trim_in_cell(img: Image.Image, col: int, row: int) -> tuple[int, int, int, int]:
    width, height = img.size
    cell_w = width / COLS
    cell_h = height / ROWS
    sx = int(col * cell_w)
    sy = int(row * cell_h)
    ex = int((col + 1) * cell_w)
    ey = int((row + 1) * cell_h)

    min_x, min_y, max_x, max_y = ex, ey, sx - 1, sy - 1
    for y in range(sy, ey):
        for x in range(sx, ex):
            if img.getpixel((x, y))[3] > 8:
                min_x = min(min_x, x)
                min_y = min(min_y, y)
                max_x = max(max_x, x)
                max_y = max(max_y, y)

    if max_x < min_x:
        return sx, sy, ex - sx, ey - sy

    return min_x, min_y, max_x - min_x + 1, max_y - min_y + 1


def extract_frames(sheet_name: str) -> list[dict]:
    path = ART / sheet_name
    img = Image.open(path).convert("RGBA")
    width, height = img.size
    frames: list[dict] = []
    index = 0

    for visual_row in range(ROWS - 1, -1, -1):
        for col in range(COLS):
            px, py, pw, ph = trim_in_cell(img, col, visual_row)
            unity_y = height - py - ph
            frames.append(
                {
                    "name": f"{Path(sheet_name).stem}_{index}",
                    "x": px,
                    "y": unity_y,
                    "w": pw,
                    "h": ph,
                    "internalID": internal_id(sheet_name, index),
                    "pivot": (0.5, 0.0),
                }
            )
            index += 1

    return frames


def sprite_id_line(guid: str, internal_id: int) -> str:
    return f"{{fileID: {internal_id}, guid: {guid}, type: 3}}"


def write_meta(sheet_name: str, frames: list[dict]) -> float:
    guid = SHEET_GUIDS[sheet_name]
    width, height = Image.open(ART / sheet_name).size
    max_h = max(frame["h"] for frame in frames)
    ppu = max(max_h / TARGET_HEIGHT, 1.0)

    sprite_entries = []
    name_table = []
    for frame in frames:
        sprite_id = f"{frame['internalID']:x}"
        sprite_entries.append(
            f"""    - serializedVersion: 2
      name: {frame['name']}
      rect:
        serializedVersion: 2
        x: {frame['x']}
        y: {frame['y']}
        width: {frame['w']}
        height: {frame['h']}
      alignment: 9
      pivot: {{x: {frame['pivot'][0]}, y: {frame['pivot'][1]}}}
      border: {{x: 0, y: 0, z: 0, w: 0}}
      customData: 
      outline: []
      physicsShape: []
      tessellationDetail: -1
      bones: []
      spriteID: {sprite_id}
      internalID: {frame['internalID']}
      vertices: []
      indices: 
      edges: []
      weights: []"""
        )
        name_table.append(f"      {frame['name']}: {frame['internalID']}")

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
  spriteMode: 2
  spriteExtrude: 1
  spriteMeshType: 1
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
  - serializedVersion: 4
    buildTarget: Standalone
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
  - serializedVersion: 4
    buildTarget: WebGL
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
    sprites:
{chr(10).join(sprite_entries)}
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
    nameFileIdTable:
{chr(10).join(name_table)}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    (ART / f"{sheet_name}.meta").write_text(meta, encoding="utf-8")
    return ppu


def update_prefab(run_frames: list[dict], jump_frames: list[dict]) -> None:
    text = PREFAB.read_text(encoding="utf-8")

    run_lines = "\n".join(
        f"  - {sprite_id_line(SHEET_GUIDS['player_run_sheet.png'], frame['internalID'])}"
        for frame in run_frames
    )
    jump_lines = "\n".join(
        f"  - {sprite_id_line(SHEET_GUIDS['player_jump_sheet.png'], frame['internalID'])}"
        for frame in jump_frames
    )

    replacement = f"""  idleSprite: {sprite_id_line(IDLE_GUID, IDLE_INTERNAL)}
  runSprites:
{run_lines}
  jumpSprites:
{jump_lines}
  dashSprite: {sprite_id_line(DASH_GUID, DASH_INTERNAL)}
  runFramesPerSecond: 12
  jumpFramesPerSecond: 10
  sortingLayerName: Gameplay"""

    pattern = r"  idleSprite:.*?\n  sortingLayerName: Gameplay"
    if not re.search(pattern, text, flags=re.S):
        raise RuntimeError("Could not find PlayerController sprite block in prefab")

    text = re.sub(pattern, replacement, text, count=1, flags=re.S)
    PREFAB.write_text(text, encoding="utf-8")


def main() -> None:
    run_frames = extract_frames("player_run_sheet.png")
    jump_frames = extract_frames("player_jump_sheet.png")
    write_meta("player_run_sheet.png", run_frames)
    write_meta("player_jump_sheet.png", jump_frames)
    update_prefab(run_frames, jump_frames)
    print(f"Assigned {len(run_frames)} run + {len(jump_frames)} jump sprites to prefab.")


if __name__ == "__main__":
    main()
