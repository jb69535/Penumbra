"""Remove backgrounds and export run/jump animation frames as individual PNGs."""
from __future__ import annotations

import sys
from collections import deque
from pathlib import Path

from PIL import Image

COLS = 3
ROWS = 2
TARGET_HEIGHT_PX = 172


def is_background_pixel(r: int, g: int, b: int, a: int) -> bool:
    if a < 12:
        return True
    if r > 232 and g > 232 and b > 232:
        return True
    if r < 48 and g < 48 and b < 48:
        return True

    channel_spread = max(r, g, b) - min(r, g, b)
    if channel_spread > 28:
        return False

    average = (r + g + b) / 3
    return 72 <= average <= 245


def flood_remove_background(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    visited = bytearray(width * height)
    queue: deque[tuple[int, int]] = deque()

    def enqueue(x: int, y: int) -> None:
        if x < 0 or x >= width or y < 0 or y >= height:
            return
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
        if not is_background_pixel(r, g, b, a):
            continue

        pixels[x, y] = (r, g, b, 0)
        enqueue(x + 1, y)
        enqueue(x - 1, y)
        enqueue(x, y + 1)
        enqueue(x, y - 1)

    return rgba


def make_transparent(image: Image.Image) -> Image.Image:
    return flood_remove_background(image)


def trim_image(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    bbox = rgba.getbbox()
    if bbox is None:
        return rgba
    return rgba.crop(bbox)


def slice_sheet(sheet_path: Path, output_dir: Path, prefix: str) -> list[Path]:
    output_dir.mkdir(parents=True, exist_ok=True)
    sheet = make_transparent(Image.open(sheet_path))
    width, height = sheet.size
    cell_w = width / COLS
    cell_h = height / ROWS
    outputs: list[Path] = []
    index = 0

    for visual_row in range(ROWS - 1, -1, -1):
        for col in range(COLS):
            left = int(col * cell_w)
            top = int(visual_row * cell_h)
            right = int((col + 1) * cell_w)
            bottom = int((visual_row + 1) * cell_h)
            frame = trim_image(sheet.crop((left, top, right, bottom)))
            out_path = output_dir / f"{prefix}_{index:02d}.png"
            frame.save(out_path, "PNG")
            outputs.append(out_path)
            print(f"Frame {prefix}_{index:02d}: {frame.size[0]}x{frame.size[1]}")
            index += 1

    return outputs


def process_pose(source: Path, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    result = trim_image(make_transparent(Image.open(source)))
    result.save(destination, "PNG")
    print(f"OK pose {source.name} -> {destination}")


def main() -> int:
    project_root = Path(__file__).resolve().parents[1]
    art_dir = project_root / "Assets" / "Penumbra" / "Art" / "Characters" / "Player"
    run_frames_dir = art_dir / "Frames" / "Run"
    jump_frames_dir = art_dir / "Frames" / "Jump"
    cursor_assets = Path(
        r"C:\Users\FLEXBOY\.cursor\projects\c-Users-FLEXBOY-OneDrive-Penumbra\assets"
    )

    sheet_sources = {
        "player_run_sheet.png": cursor_assets / (
            "c__Users_FLEXBOY_AppData_Roaming_Cursor_User_workspaceStorage_"
            "810478d7ebc3c55946403559d386d14b_images_runnig-aefa4cd1-3605-42ac-9cd5-1dd0c22f7f2e.png"
        ),
        "player_jump_sheet.png": cursor_assets / (
            "c__Users_FLEXBOY_AppData_Roaming_Cursor_User_workspaceStorage_"
            "810478d7ebc3c55946403559d386d14b_images_jumping-6d2a37b8-5ea8-4e85-abff-4011239686d0.png"
        ),
    }

    for dest_name, source in sheet_sources.items():
        if source.exists():
            process_pose(source, art_dir / dest_name)

    run_sheet = art_dir / "player_run_sheet.png"
    jump_sheet = art_dir / "player_jump_sheet.png"
    if run_sheet.exists():
        slice_sheet(run_sheet, run_frames_dir, "run")
    if jump_sheet.exists():
        slice_sheet(jump_sheet, jump_frames_dir, "jump")

    for name in ("player_idle_0.png", "player_dash_0.png"):
        path = art_dir / name
        if path.exists():
            process_pose(path, path)

    return 0


if __name__ == "__main__":
    sys.exit(main())
