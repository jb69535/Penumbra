"""Import character sprites from Downloads/game into the Unity project."""
from __future__ import annotations

import re
import uuid
from collections import deque
from pathlib import Path

from PIL import Image

PROJECT = Path(__file__).resolve().parents[1]
ART = PROJECT / "Assets" / "Penumbra" / "Art" / "Characters" / "Player"
SCENE = PROJECT / "Assets" / "Penumbra" / "Scenes" / "Sandboxes" / "Sandbox_Movement2D.unity"
SOURCE = Path(r"C:\Users\FLEXBOY\Downloads\game")
TARGET_HEIGHT = 1.8
ALPHA_THRESHOLD = 40
CHARACTER_ALPHA_THRESHOLD = 32
SOLID_ALPHA = 255
MAX_SILHOUETTE_GAP = 3
PRESERVE_SOURCE_TRANSPARENCY = True

IDLE_GUID = "6f6f8af46893ca94bb23d7b815d98f27"
RIGHT_GUID = "72dcece972b6ee545bee73f60d9ab0ca"
LEFT_GUID = "8e4b2c19f7a64d0e9c3a1b5d6e7f8091"
SIT_GUID = "a3c7e2f14b9d4e6a8f1c0d2e4b6a8092"
POSE_SOURCES_USING_BLACK_BACKGROUND = {
    "player_idle.png",
    "player_sit.png",
    "player_right.png",
    "player_left.png",
}


BLACK_BACKGROUND_MAX_CHANNEL = 3
BLACK_BACKGROUND_MAX_SUM = 6
WHITE_BACKGROUND_MIN = 230
WHITE_BACKGROUND_NEUTRAL_RANGE = 6


def is_white_background_pixel(r: int, g: int, b: int, a: int) -> bool:
    if a < 12:
        return True
    if r <= WHITE_BACKGROUND_MIN or g <= WHITE_BACKGROUND_MIN or b <= WHITE_BACKGROUND_MIN:
        return False
    return max(r, g, b) - min(r, g, b) <= WHITE_BACKGROUND_NEUTRAL_RANGE


def is_black_background_pixel(r: int, g: int, b: int, a: int) -> bool:
    if a < 12:
        return True
    if max(r, g, b) > BLACK_BACKGROUND_MAX_CHANNEL:
        return False
    return r + g + b <= BLACK_BACKGROUND_MAX_SUM


def is_chroma_key_pixel(r: int, g: int, b: int, a: int) -> bool:
    if a < 12:
        return True
    if r > 1 or g > 1 or b > 1:
        return False
    return True


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


def flood_remove_white_background(image: Image.Image) -> Image.Image:
    return flood_remove_background(image, is_white_background_pixel)


def flood_remove_black_background(image: Image.Image) -> Image.Image:
    return flood_remove_background(image, is_black_background_pixel)


def flood_remove_chroma_background(image: Image.Image) -> Image.Image:
    return flood_remove_background(image, is_chroma_key_pixel)


def border_prefers_white_removal(image: Image.Image) -> bool:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    light = 0
    dark = 0

    def tally(x: int, y: int) -> None:
        nonlocal light, dark
        r, g, b, a = pixels[x, y]
        if a < 12:
            return
        if is_white_background_pixel(r, g, b, a):
            light += 1
        elif is_black_background_pixel(r, g, b, a):
            dark += 1

    for x in range(width):
        tally(x, 0)
        tally(x, height - 1)
    for y in range(height):
        tally(0, y)
        tally(width - 1, y)

    return light > dark


def strip_animation_background(image: Image.Image) -> Image.Image:
    """Remove only edge-connected backdrop pixels, preserving interior character shading."""
    if border_prefers_white_removal(image):
        return flood_remove_white_background(image)
    return flood_remove_black_background(image)


def strip_pose_background(image: Image.Image) -> Image.Image:
    return flood_remove_black_background(image)


def defringe_near_white_halos(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    pixels = rgba.load()
    width, height = rgba.size
    to_clear: list[tuple[int, int]] = []

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if a < CHARACTER_ALPHA_THRESHOLD or a >= 250:
                continue
            if r < 220 or g < 220 or b < 220:
                continue

            touches_transparent = False
            for nx, ny in ((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)):
                if 0 <= nx < width and 0 <= ny < height and pixels[nx, ny][3] < CHARACTER_ALPHA_THRESHOLD:
                    touches_transparent = True
                    break

            if touches_transparent:
                to_clear.append((x, y))

    for x, y in to_clear:
        pixels[x, y] = (0, 0, 0, 0)

    return rgba


def solidify_character_alpha(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    pixels = rgba.load()
    width, height = rgba.size

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            pixels[x, y] = (r, g, b, SOLID_ALPHA if a >= CHARACTER_ALPHA_THRESHOLD else 0)

    return rgba


def fill_character_silhouette_gaps(image: Image.Image, max_gap: int = MAX_SILHOUETTE_GAP) -> Image.Image:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    box = measure_content_box(rgba)
    if box is None:
        return rgba

    x0, y0, x1, y1 = box

    for x in range(x0, x1 + 1):
        for row in range(y0, y1 + 1):
            if pixels[x, row][3] > CHARACTER_ALPHA_THRESHOLD:
                continue

            above = next(
                (scan for scan in range(row - 1, y0 - 1, -1) if pixels[x, scan][3] > CHARACTER_ALPHA_THRESHOLD),
                None,
            )
            below = next(
                (scan for scan in range(row + 1, y1 + 1) if pixels[x, scan][3] > CHARACTER_ALPHA_THRESHOLD),
                None,
            )
            if above is None or below is None or below - above > max_gap:
                continue

            above_color = pixels[x, above][0:3]
            below_color = pixels[x, below][0:3]
            color = tuple((above_color[index] + below_color[index]) // 2 for index in range(3))
            pixels[x, row] = (*color, SOLID_ALPHA)

    return rgba


def prepare_character_pixels(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    if PRESERVE_SOURCE_TRANSPARENCY:
        return rgba
    return finalize_character_pixels(rgba)


def trim_image(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    box = measure_content_box(rgba)
    if box is None:
        return rgba
    x0, y0, x1, y1 = box
    return rgba.crop((x0, y0, x1 + 1, y1 + 1))


def row_opaque_counts(image: Image.Image) -> list[int]:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    return [sum(1 for x in range(width) if pixels[x, y][3] > ALPHA_THRESHOLD) for y in range(height)]


def remove_bottom_sprite_bleed(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    if height <= 0:
        return rgba

    counts = row_opaque_counts(rgba)
    best_gap = (0, -1, -1)
    row = 0

    while row < height:
        if counts[row] != 0:
            row += 1
            continue

        gap_start = row
        while row < height and counts[row] == 0:
            row += 1
        gap_end = row
        gap_length = gap_end - gap_start
        gap_middle = (gap_start + gap_end) // 2
        if gap_middle <= height * 0.35 or gap_length <= best_gap[0]:
            continue

        lower_rows = [index for index in range(gap_end, height) if counts[index] > 0]
        upper_rows = [index for index in range(0, gap_start) if counts[index] > 0]
        if not lower_rows or not upper_rows:
            continue

        lower_height = lower_rows[-1] - lower_rows[0] + 1
        upper_height = upper_rows[-1] - upper_rows[0] + 1
        if lower_height >= upper_height * 0.35:
            continue

        best_gap = (gap_length, gap_start, gap_end)

    if best_gap[1] <= 0:
        return rgba

    return rgba.crop((0, 0, width, best_gap[1]))


def normalize_frame_height(image: Image.Image, target_height: int) -> Image.Image:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    if height <= 0 or height == target_height:
        return rgba

    scale = target_height / height
    scaled_width = max(1, int(round(width * scale)))
    return rgba.resize((scaled_width, target_height), Image.Resampling.LANCZOS)


def measure_content_box(image: Image.Image) -> tuple[int, int, int, int] | None:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    min_x, min_y, max_x, max_y = width, height, -1, -1
    found = False

    for y in range(height):
        for x in range(width):
            if pixels[x, y][3] > ALPHA_THRESHOLD:
                found = True
                min_x = min(min_x, x)
                max_x = max(max_x, x)
                min_y = min(min_y, y)
                max_y = max(max_y, y)

    if not found:
        return None

    return min_x, min_y, max_x, max_y


def measure_content_height(image: Image.Image) -> int:
    box = measure_content_box(image)
    if box is None:
        return max(image.size[1], 1)
    return box[3] - box[1] + 1


def fit_content_to_reference_canvas(
    image: Image.Image,
    reference_content_height: int,
    reference_canvas_height: int,
) -> Image.Image:
    rgba = image.convert("RGBA")
    box = measure_content_box(rgba)
    if box is None:
        return normalize_frame_height(rgba, reference_canvas_height)

    x0, y0, x1, y1 = box
    cropped = rgba.crop((x0, y0, x1 + 1, y1 + 1))
    content_width, content_height = cropped.size
    if content_height <= 0:
        return normalize_frame_height(rgba, reference_canvas_height)

    scale = reference_content_height / content_height
    scaled_width = max(1, int(round(content_width * scale)))
    scaled_height = max(1, int(round(content_height * scale)))
    scaled = cropped.resize((scaled_width, scaled_height), Image.Resampling.LANCZOS)

    canvas_width = max(scaled_width, 1)
    canvas = Image.new("RGBA", (canvas_width, reference_canvas_height), (0, 0, 0, 0))
    paste_x = (canvas_width - scaled_width) // 2
    paste_y = reference_canvas_height - scaled_height
    canvas.paste(scaled, (paste_x, paste_y), scaled)
    return prepare_character_pixels(canvas)


def bottom_align_on_canvas(image: Image.Image, reference_canvas_height: int) -> Image.Image:
    rgba = trim_image(image)
    width, height = rgba.size
    if height <= 0:
        return rgba
    if height > reference_canvas_height:
        return normalize_frame_height(rgba, reference_canvas_height)
    if height == reference_canvas_height:
        return rgba

    canvas_width = max(width, 1)
    canvas = Image.new("RGBA", (canvas_width, reference_canvas_height), (0, 0, 0, 0))
    paste_x = (canvas_width - width) // 2
    paste_y = reference_canvas_height - height
    canvas.paste(rgba, (paste_x, paste_y), rgba)
    return prepare_character_pixels(canvas)


def finalize_character_pixels(image: Image.Image) -> Image.Image:
    rgba = solidify_character_alpha(image)
    rgba = fill_character_silhouette_gaps(rgba, max_gap=MAX_SILHOUETTE_GAP)
    return solidify_character_alpha(rgba)


def process_frame(
    image: Image.Image,
    remove_white_background: bool,
    reference_canvas_height: int | None = None,
    remove_black_background: bool = False,
    reference_content_height: int | None = None,
    scale_content_to_reference: bool = False,
    remove_sprite_bleed: bool = False,
    remove_animation_background: bool = False,
) -> Image.Image:
    rgba = image.convert("RGBA")
    if not PRESERVE_SOURCE_TRANSPARENCY:
        if remove_animation_background:
            rgba = strip_animation_background(rgba)
        elif remove_black_background:
            rgba = strip_pose_background(rgba)
        elif remove_white_background:
            rgba = flood_remove_white_background(rgba)
        if remove_sprite_bleed:
            rgba = remove_bottom_sprite_bleed(rgba)
    rgba = prepare_character_pixels(rgba)
    rgba = trim_image(rgba)
    rgba = prepare_character_pixels(rgba)

    if reference_canvas_height is None:
        return rgba

    if scale_content_to_reference and reference_content_height is not None:
        return fit_content_to_reference_canvas(rgba, reference_content_height, reference_canvas_height)

    return prepare_character_pixels(bottom_align_on_canvas(rgba, reference_canvas_height))


def uses_chroma_key_removal(source: Path) -> bool:
    return source.name in POSE_SOURCES_USING_BLACK_BACKGROUND


def uses_black_background_removal(source: Path) -> bool:
    return uses_chroma_key_removal(source)


def resolve_idle_source() -> Path:
    single_pose = SOURCE / "player_idle.png"
    if single_pose.exists():
        return single_pose

    folder_frame = SOURCE / "player_idle" / "frame_01.png"
    if folder_frame.exists():
        return folder_frame

    raise FileNotFoundError(
        "Idle source not found. Expected Downloads/game/player_idle.png or Downloads/game/player_idle/frame_01.png"
    )


def resolve_dash_source() -> Path | None:
    dash_folder = SOURCE / "dash"
    if dash_folder.is_dir() and list(dash_folder.glob("frame_*.png")):
        return dash_folder

    for candidate in (SOURCE / "player_dash.png", SOURCE / "player_right.png", SOURCE / "player_run.png"):
        if candidate.exists():
            return candidate

    return None


def resolve_slide_source_dir() -> Path | None:
    slide_folder = SOURCE / "sliding"
    if slide_folder.is_dir() and list(slide_folder.glob("frame_*.png")):
        return slide_folder

    return None


def resolve_side_right_source() -> Path | None:
    candidate = SOURCE / "player_right.png"
    if candidate.exists():
        return candidate
    return None


def resolve_sit_source_dir() -> Path:
    for candidate in (SOURCE / "sit_moving", SOURCE / "sit"):
        if candidate.is_dir() and list(candidate.glob("frame_*.png")):
            return candidate

    raise FileNotFoundError(
        "Sit source not found. Expected Downloads/game/sit_moving/frame_*.png or Downloads/game/sit/frame_*.png"
    )


def compute_reference_metrics(idle_source: Path) -> tuple[float, int, int]:
    processed = process_frame(
        Image.open(idle_source),
        remove_white_background=False,
        remove_black_background=uses_chroma_key_removal(idle_source),
        scale_content_to_reference=False,
    )
    reference_canvas_height = processed.size[1]
    reference_content_height = measure_content_height(processed)
    reference_ppu = max(reference_canvas_height, 1) / TARGET_HEIGHT
    return reference_ppu, reference_canvas_height, reference_content_height


def new_guid() -> str:
    return uuid.uuid4().hex


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
    filterMode: 0
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
  spritePivot: {{x: 0.5, y: 0}}
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


def read_existing_guid(asset_path: Path) -> str | None:
    meta_path = asset_path.with_suffix(asset_path.suffix + ".meta")
    if not meta_path.exists():
        return None
    match = re.search(r"^guid: ([0-9a-f]{32})$", meta_path.read_text(encoding="utf-8"), re.MULTILINE)
    return match.group(1) if match else None


def import_frame_folder(
    source_dir: Path,
    dest_dir: Path,
    prefix: str,
    reference_ppu: float,
    reference_canvas_height: int,
    reference_content_height: int,
    remove_animation_background: bool = True,
    max_frames: int | None = None,
    scale_content_to_reference: bool = False,
    remove_sprite_bleed: bool = False,
) -> list[tuple[str, str]]:
    dest_dir.mkdir(parents=True, exist_ok=True)

    existing_guids: dict[str, str] = {}
    for old_png in dest_dir.glob("*.png"):
        guid = read_existing_guid(old_png)
        if guid is not None:
            existing_guids[old_png.name] = guid
        old_png.unlink()
        meta = old_png.with_suffix(old_png.suffix + ".meta")
        if meta.exists():
            meta.unlink()

    sources = sorted(source_dir.glob("frame_*.png"))
    if not sources:
        raise FileNotFoundError(f"No frame_*.png files in {source_dir}")
    if max_frames is not None:
        sources = sources[:max_frames]

    entries: list[tuple[str, str]] = []
    for index, source in enumerate(sources):
        processed = process_frame(
            Image.open(source),
            remove_white_background=False,
            reference_canvas_height=reference_canvas_height,
            remove_black_background=False,
            reference_content_height=reference_content_height,
            scale_content_to_reference=scale_content_to_reference,
            remove_sprite_bleed=remove_sprite_bleed,
            remove_animation_background=remove_animation_background,
        )
        dest_name = f"{prefix}_{index:02d}.png"
        dest_path = dest_dir / dest_name
        processed.save(dest_path, "PNG")
        guid = existing_guids.get(dest_name) or new_guid()
        write_sprite_meta(dest_path, guid, reference_ppu)
        entries.append((dest_name, guid))
        print(f"  {dest_name}: {processed.size[0]}x{processed.size[1]} ppu={reference_ppu:.2f}")

    return entries


def import_idle_frames(
    idle_source: Path,
    dest_dir: Path,
    reference_ppu: float,
    reference_canvas_height: int,
    reference_content_height: int,
) -> list[tuple[str, str]]:
    idle_folder = SOURCE / "player_idle"
    if idle_folder.is_dir() and list(idle_folder.glob("frame_*.png")):
        return import_frame_folder(
            idle_folder,
            dest_dir,
            "idle",
            reference_ppu,
            reference_canvas_height,
            reference_content_height,
            remove_animation_background=False,
            scale_content_to_reference=True,
        )

    dest_dir.mkdir(parents=True, exist_ok=True)
    for old_png in dest_dir.glob("*.png"):
        old_png.unlink()
        meta = old_png.with_suffix(old_png.suffix + ".meta")
        if meta.exists():
            meta.unlink()

    processed = process_frame(
        Image.open(idle_source),
        remove_white_background=False,
        reference_canvas_height=reference_canvas_height,
        remove_black_background=uses_black_background_removal(idle_source),
        reference_content_height=reference_content_height,
        scale_content_to_reference=True,
    )
    dest_name = "idle_00.png"
    dest_path = dest_dir / dest_name
    processed.save(dest_path, "PNG")
    guid = new_guid()
    write_sprite_meta(dest_path, guid, reference_ppu)
    print(f"  {dest_name}: {processed.size[0]}x{processed.size[1]} ppu={reference_ppu:.2f}")
    return [(dest_name, guid)]


def import_single_pose(
    source: Path,
    dest: Path,
    guid: str,
    reference_ppu: float,
    reference_canvas_height: int,
    reference_content_height: int,
    remove_white_background: bool,
    scale_content_to_reference: bool = False,
) -> None:
    processed = process_frame(
        Image.open(source),
        remove_white_background,
        reference_canvas_height,
        uses_black_background_removal(source),
        reference_content_height,
        scale_content_to_reference,
    )
    dest.parent.mkdir(parents=True, exist_ok=True)
    processed.save(dest, "PNG")
    write_sprite_meta(dest, guid, reference_ppu)
    print(f"  {dest.name}: {processed.size[0]}x{processed.size[1]} ppu={reference_ppu:.2f}")


def sprite_ref(guid: str) -> str:
    return f"{{fileID: 21300000, guid: {guid}, type: 3}}"


def update_scene_wanderer(
    idle_guid: str,
    idle_guids: list[str],
    run_guids: list[str],
    jump_guids: list[str],
    sit_guids: list[str],
    dash_guids: list[str],
    slide_guids: list[str],
    left_guid: str | None = None,
    right_guid: str | None = None,
    sit_idle_guid: str | None = None,
) -> None:
    if not SCENE.exists():
        print(f"Scene not found, skipping scene update: {SCENE}")
        return

    text = SCENE.read_text(encoding="utf-8")
    idle_lines = "\n".join(f"  - {sprite_ref(guid)}" for guid in idle_guids)
    run_lines = "\n".join(f"  - {sprite_ref(guid)}" for guid in run_guids)
    jump_lines = "\n".join(f"  - {sprite_ref(guid)}" for guid in jump_guids)
    sit_lines = "\n".join(f"  - {sprite_ref(guid)}" for guid in sit_guids)
    dash_lines = "\n".join(f"  - {sprite_ref(guid)}" for guid in dash_guids)
    slide_lines = "\n".join(f"  - {sprite_ref(guid)}" for guid in slide_guids)
    left_ref = sprite_ref(left_guid) if left_guid else "{fileID: 0}"
    right_ref = sprite_ref(right_guid) if right_guid else "{fileID: 0}"
    sit_idle_ref = sprite_ref(sit_idle_guid) if sit_idle_guid else "{fileID: 0}"

    replacement = f"""  bodySprite: {sprite_ref(idle_guid)}
  useGeneratedWandererAnimation: 0
  useConceptSpriteAnimation: 0
  animationMovementThreshold: 0.08
  idleFrameRate: 5
  walkFrameRate: 8
  runFrameRate: 14
  attackFrameRate: 18
  sortingLayerName: Gameplay
  idleColor: {{r: 1, g: 1, b: 1, a: 1}}
  dashColor: {{r: 0.74, g: 1, b: 0.9, a: 1}}
  attackColor: {{r: 1, g: 1, b: 1, a: 1}}
  hitColor: {{r: 1, g: 0.33, b: 0.28, a: 1}}
  animator: {{fileID: 0}}
  useCinderWispSpriteAnimation: 1
  cinderIdleSprites:
{idle_lines}
  cinderRunSprites:
{run_lines}
  cinderJumpSprites:
{jump_lines}
  cinderSitSprites:
{sit_lines}
  cinderSitIdleSprite: {sit_idle_ref}
  cinderDashSprites:
{dash_lines}
  cinderSlideSprites:
{slide_lines}
  cinderFrontIdleSprite: {sprite_ref(idle_guid)}
  cinderSideLeftSprite: {left_ref}
  cinderSideRightSprite: {right_ref}
  cinderIdleFrameRate: 6
  cinderRunFrameRate: 12
  cinderDashFrameRate: 14
  cinderSlideFrameRate: 12
  cinderSitFrameRate: 8
  cinderSitMoveSpeed: 3.5
  cinderSlideSpeed: 10
  cinderSlideDuration: 0.55
  cinderSlideCooldown: 0.3
  cinderSitCapsuleHeight: 1.05
  cinderSitCapsuleWidth: 0.68"""

    pattern = r"  bodySprite:.*?  cinderSitCapsuleHeight: [0-9.]+(?:\n  cinderSitCapsuleWidth: [0-9.]+)?"
    if not re.search(pattern, text, flags=re.S):
        raise RuntimeError("Could not find Wanderer Cinder sprite block in sandbox scene")

    text = re.sub(pattern, replacement, text, count=1, flags=re.S)
    SCENE.write_text(text, encoding="utf-8")


def ensure_folder_meta(folder: Path, guid: str | None = None) -> str:
    folder.mkdir(parents=True, exist_ok=True)
    meta_path = folder.with_suffix(folder.suffix + ".meta")
    if meta_path.exists():
        match = re.search(r"^guid: ([0-9a-f]+)$", meta_path.read_text(encoding="utf-8"), re.M)
        if match:
            return match.group(1)

    folder_guid = guid or new_guid()
    meta_path.write_text(
        f"""fileFormatVersion: 2
guid: {folder_guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
""",
        encoding="utf-8",
    )
    return folder_guid


def main() -> int:
    if not SOURCE.exists():
        print(f"Source folder not found: {SOURCE}")
        return 1

    print("Importing player sprites from Downloads/game ...")
    if PRESERVE_SOURCE_TRANSPARENCY:
        print("Using source PNG alpha as-is (no background removal or pixel cleanup).")

    idle_source = resolve_idle_source()
    sit_source_dir = resolve_sit_source_dir()
    dash_source = resolve_dash_source()
    slide_source_dir = resolve_slide_source_dir()
    side_right_source = resolve_side_right_source()
    reference_ppu, reference_canvas_height, reference_content_height = compute_reference_metrics(idle_source)
    print(f"Idle source: {idle_source.name}")
    print(f"Sit source: {sit_source_dir.name}")
    if dash_source is not None:
        print(f"Dash source: {dash_source.name}")
    if slide_source_dir is not None:
        print(f"Slide source: {slide_source_dir.name}")
    if side_right_source is not None:
        print(f"Side right source: {side_right_source.name}")
    print(
        f"Reference canvas: {reference_canvas_height}px, "
        f"content: {reference_content_height}px, PPU: {reference_ppu:.2f}"
    )

    ensure_folder_meta(ART / "Frames")
    ensure_folder_meta(ART / "Frames" / "Idle")
    ensure_folder_meta(ART / "Frames" / "Run")
    ensure_folder_meta(ART / "Frames" / "Jump")
    ensure_folder_meta(ART / "Frames" / "Sit")
    ensure_folder_meta(ART / "Frames" / "Dash")
    ensure_folder_meta(ART / "Frames" / "Slide")

    print("Idle frames:")
    idle_entries = import_idle_frames(
        idle_source,
        ART / "Frames" / "Idle",
        reference_ppu,
        reference_canvas_height,
        reference_content_height,
    )

    print("Run frames:")
    run_entries = import_frame_folder(
        SOURCE / "run",
        ART / "Frames" / "Run",
        "run",
        reference_ppu,
        reference_canvas_height,
        reference_content_height,
        scale_content_to_reference=True,
    )

    print("Jump frames:")
    jump_entries = import_frame_folder(
        SOURCE / "jump",
        ART / "Frames" / "Jump",
        "jump",
        reference_ppu,
        reference_canvas_height,
        reference_content_height,
        scale_content_to_reference=True,
        remove_sprite_bleed=True,
    )

    print("Sit frames:")
    sit_entries = import_frame_folder(
        sit_source_dir,
        ART / "Frames" / "Sit",
        "sit",
        reference_ppu,
        reference_canvas_height,
        reference_content_height,
        scale_content_to_reference=True,
    )

    dash_entries: list[tuple[str, str]] = []
    if dash_source is not None and dash_source.is_dir():
        print("Dash frames:")
        dash_entries = import_frame_folder(
            dash_source,
            ART / "Frames" / "Dash",
            "dash",
            reference_ppu,
            reference_canvas_height,
            reference_content_height,
            scale_content_to_reference=True,
        )

    slide_entries: list[tuple[str, str]] = []
    if slide_source_dir is not None:
        print("Slide frames:")
        slide_entries = import_frame_folder(
            slide_source_dir,
            ART / "Frames" / "Slide",
            "slide",
            reference_ppu,
            reference_canvas_height,
            reference_content_height,
            scale_content_to_reference=True,
            remove_animation_background=False,
        )

    print("Primary pose:")
    import_single_pose(
        idle_source,
        ART / "player_idle_0.png",
        IDLE_GUID,
        reference_ppu,
        reference_canvas_height,
        reference_content_height,
        remove_white_background=False,
        scale_content_to_reference=True,
    )

    dash_guid: str | None = None
    left_guid: str | None = None
    right_guid: str | None = None

    if side_right_source is not None:
        print("Side right pose:")
        import_single_pose(
            side_right_source,
            ART / "player_right_0.png",
            RIGHT_GUID,
            reference_ppu,
            reference_canvas_height,
            reference_content_height,
            remove_white_background=False,
            scale_content_to_reference=True,
        )
        right_guid = RIGHT_GUID

    left_source = SOURCE / "player_left.png"
    if left_source.exists():
        print("Side left pose:")
        import_single_pose(
            left_source,
            ART / "player_left_0.png",
            LEFT_GUID,
            reference_ppu,
            reference_canvas_height,
            reference_content_height,
            remove_white_background=False,
            scale_content_to_reference=True,
        )
        left_guid = LEFT_GUID

    sit_idle_guid: str | None = None
    sit_source = SOURCE / "player_sit.png"
    if sit_source.exists():
        print("Sit idle pose:")
        import_single_pose(
            sit_source,
            ART / "player_sit_0.png",
            SIT_GUID,
            reference_ppu,
            reference_canvas_height,
            reference_content_height,
            remove_white_background=False,
            scale_content_to_reference=True,
        )
        sit_idle_guid = SIT_GUID

    idle_guids = [guid for _, guid in idle_entries]
    run_guids = [guid for _, guid in run_entries]
    jump_guids = [guid for _, guid in jump_entries]
    sit_guids = [guid for _, guid in sit_entries]
    dash_guids = [guid for _, guid in dash_entries]
    slide_guids = [guid for _, guid in slide_entries]

    update_scene_wanderer(
        IDLE_GUID,
        idle_guids,
        run_guids,
        jump_guids,
        sit_guids,
        dash_guids,
        slide_guids,
        left_guid,
        right_guid,
        sit_idle_guid,
    )

    print(
        f"Done. Imported {len(idle_entries)} idle, {len(run_entries)} run, "
        f"{len(jump_entries)} jump, {len(sit_entries)} sit, "
        f"{len(dash_entries)} dash, {len(slide_entries)} slide frames."
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
