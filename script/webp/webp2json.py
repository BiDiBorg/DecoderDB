#!/usr/bin/env python3

import argparse
import json
from pathlib import Path


def process_json(json_path: Path):
    print(f"Processing: {json_path}")

    image_dir = json_path.parent / "images"

    if not image_dir.is_dir():
        print(f"  No images directory: {image_dir}")
        return

    with json_path.open("r", encoding="utf-8") as file:
        data = json.load(file)

    images = data.get("decoder", {}).get("images")

    if not isinstance(images, list):
        print("  No decoder.images array, skipping.")
        return

    existing_names = {image.get("name") for image in images if isinstance(image, dict)}

    added = 0

    for image in list(images):
        if not isinstance(image, dict):
            continue

        name = image.get("name")

        if not isinstance(name, str):
            continue

        webp_name = f"{Path(name).stem}.webp"

        if webp_name in existing_names:
            continue

        webp_path = image_dir / webp_name

        if not webp_path.is_file():
            continue

        # Copy all original parameters
        webp_image = image.copy()

        # .. but remove source
        webp_image.pop("source", None)

        # The only remaining modification is the filename
        webp_image["name"] = webp_name

        images.append(webp_image)
        existing_names.add(webp_name)

        print(f"  Added: {webp_name}")
        added += 1

    if added == 0:
        print("  Nothing to add.")
        return

    with json_path.open("w", encoding="utf-8") as file:
        json.dump(data, file, indent=2, ensure_ascii=False)
        file.write("\n")

    print(f"  Added {added} image(s).")


def main():
    parser = argparse.ArgumentParser(
        description="Add WebP versions of decoder images to JSON files."
    )

    parser.add_argument(
        "--source",
        "-s",
        required=True,
        type=Path,
        help="Path to a JSON file or directory containing JSON files.",
    )

    args = parser.parse_args()

    if not args.source.exists():
        parser.error(f"Source does not exist: {args.source}")

    # Single JSON file
    if args.source.is_file():
        if args.source.suffix.lower() != ".json":
            parser.error(f"Not a JSON file: {args.source}")

        process_json(args.source)
        return

    # Directory of JSON files
    if args.source.is_dir():
        json_files = sorted(
            path
            for path in args.source.iterdir()
            if path.is_file() and path.suffix.lower() == ".json"
        )

        for json_path in json_files:
            process_json(json_path)

        return

    parser.error(f"Source is neither a file nor a directory: {args.source}")


if __name__ == "__main__":
    main()
