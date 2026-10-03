#!/usr/bin/env python3

import argparse
from pathlib import Path

from PIL import Image
from transparent_background import Remover

SUPPORTED_EXTENSIONS = {
    ".jpg",
    ".jpeg",
    ".png",
    ".webp",
    ".bmp",
    ".tif",
    ".tiff",
}


def process_image(
    input_path: Path,
    output_path: Path,
    remover: Remover,
    threshold: float | None,
):
    print(f"Processing: {input_path}")

    original = Image.open(input_path).convert("RGB")
    original_width, original_height = original.size

    # Create a canvas twice the original size, filled white.
    expanded = Image.new(
        "RGB",
        (original_width * 2, original_height * 2),
        "white",
    )

    expanded.paste(
        original,
        (original_width // 2, original_height // 2),
    )

    # Remove the background.
    if threshold is None:
        result = remover.process(expanded)
    else:
        result = remover.process(expanded, threshold=threshold)

    result = result.convert("RGBA")

    # Crop back to the original image dimensions.
    left = original_width // 2
    top = original_height // 2

    result = result.crop(
        (
            left,
            top,
            left + original_width,
            top + original_height,
        )
    )

    # Limit the longest edge to 1000 pixels.
    width, height = result.size
    longest_edge = max(width, height)

    if longest_edge > 1000:
        scale = 1000 / longest_edge

        result = result.resize(
            (
                round(width * scale),
                round(height * scale),
            ),
            Image.Resampling.LANCZOS,
        )

    # Save as WebP.
    output_path.parent.mkdir(parents=True, exist_ok=True)

    result.save(
        output_path,
        "WEBP",
        quality=95,
        method=6,
    )

    print(f"  -> {output_path}")


def main():
    parser = argparse.ArgumentParser(
        description="Remove image backgrounds, resize images to a maximum of 1000 pixels, and save them as WebP."
    )

    parser.add_argument(
        "--source",
        "-s",
        required=True,
        type=Path,
        help="Path to the source. Single image, directory of images is supported.",
    )

    parser.add_argument(
        "--dest",
        "-d",
        type=Path,
        help="Path to destination. Results will be stored in current directory if not specified.",
    )

    parser.add_argument(
        "--threshold",
        "-th",
        type=float,
        help="Designate threshold. If specified, it will output hard prediction above threshold. If not specified, it will output soft prediction.",
    )

    parser.add_argument(
        "--mode",
        "-m",
        choices=("base", "fast", "base-nightly"),
        default="base",
        help="Choose between base and fast mode. Also, use base-nightly for nightly release checkpoint.",
    )

    args = parser.parse_args()

    source = args.source

    if not source.exists():
        parser.error(f"Source does not exist: {source}")

    remover = Remover(mode=args.mode)

    if source.is_file():
        destination = args.dest or source.with_suffix(".webp")

        process_image(
            source,
            destination,
            remover,
            args.threshold,
        )
        return

    if source.is_dir():
        destination = args.dest or source
        destination.mkdir(parents=True, exist_ok=True)

        for path in sorted(source.iterdir()):
            if not path.is_file():
                continue

            if path.suffix.lower() not in SUPPORTED_EXTENSIONS:
                continue

            output_path = destination / f"{path.stem}.webp"

            if path.resolve() == output_path.resolve():
                continue

            process_image(
                path,
                output_path,
                remover,
                args.threshold,
            )

        return

    parser.error(f"Source is neither a file nor a directory: {source}")


if __name__ == "__main__":
    main()
