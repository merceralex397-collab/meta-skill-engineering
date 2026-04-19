---
name: image-editor
description: "Manipulate images programmatically using Pillow, Sharp, or ImageMagick — resize, crop, rotate, convert formats, add watermarks, strip or preserve EXIF data, and optimize file size. Use when processing images in pipelines, converting between formats, generating thumbnails, or automating image transformations. Do not use for AI image generation, design mockups, or image analysis/classification tasks."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: image-editor
  maturity: draft
  risk: low
  tags: [image, pillow, sharp, imagemagick, resize, format-conversion]
---

# Purpose

Manipulate images programmatically using Pillow (Python), Sharp (Node.js), or ImageMagick (CLI) — resize, crop, rotate, convert between formats, add watermarks, manage EXIF metadata, optimize file size, and batch-process image collections. This skill covers the full lifecycle of image transformation in automated pipelines.

# When to use this skill

- Images need resizing, cropping, or rotation for web display, thumbnails, or print
- Format conversion is required (PNG→JPEG, WEBP→PNG, SVG rasterization, HEIC→JPEG)
- Watermarks, overlays, or text annotations must be added to images
- EXIF data needs stripping (privacy), preserving (archival), or reading (metadata extraction)
- Image file size optimization is needed for web delivery or storage constraints
- Batch processing of multiple images with consistent transformations
- Thumbnail generation for a gallery, catalog, or document preview system

# Do not use this skill when

- The task involves AI-powered image generation or editing (Stable Diffusion, DALL-E) — this is programmatic manipulation only
- The goal is design mockup creation (Figma, Sketch) — this skill handles pixel manipulation, not design
- The task is image classification, object detection, or computer vision analysis
- The images are embedded in PDFs that need OCR — prefer `image-heavy-pdfs`
- The output is a PDF containing images — prefer `pdf-generation`

# Operating procedure

1. **Identify the input** — determine the source image(s): file path, URL, or byte stream. Check the format (JPEG, PNG, GIF, WEBP, TIFF, BMP, SVG, HEIC) and note the current dimensions, color space, and file size.
2. **Select the processing library** — use Pillow for Python pipelines (`pip install Pillow`), Sharp for Node.js (`npm install sharp`), or ImageMagick for CLI/batch operations (`convert`, `mogrify`). Choose based on the runtime environment.
3. **Load the image** — open with error handling: `Image.open(path)` (Pillow), `sharp(path)` (Sharp), or `convert input.png` (ImageMagick). Verify the image loaded successfully by checking dimensions are non-zero.
4. **Apply resize operations** — calculate target dimensions maintaining aspect ratio unless explicit distortion is requested. Use `Image.LANCZOS` (Pillow) or `sharp.resize({fit: 'inside'})` for high-quality downscaling. For upscaling, warn that quality loss is expected.
5. **Apply crop operations** — define the crop box as `(left, top, right, bottom)` coordinates. Validate that the crop box is within image bounds. For center-crop, calculate offsets from the image center.
6. **Apply rotation** — rotate by exact degrees. Use `expand=True` (Pillow) to prevent clipping. For 90/180/270° rotations, use transpose operations for lossless rotation of JPEG.
7. **Convert format** — save to the target format with appropriate quality settings: JPEG quality 85 (default), PNG compression level 6, WEBP quality 80. Strip alpha channel when converting PNG→JPEG (composite onto white background).
8. **Add watermark** — overlay a watermark image or text at a specified position (bottom-right default). Set opacity to 30-50% for visibility without obstruction. Scale watermark proportionally to image size.
9. **Handle EXIF metadata** — to strip: use `image.info.pop('exif', None)` (Pillow) or `sharp.withMetadata(false)`. To preserve: pass `exif=original_exif` on save. To read: use `image._getexif()` or `exifread` library.
10. **Optimize file size** — for JPEG: adjust quality (60-85 range), enable progressive encoding. For PNG: use `optimize=True` and consider palette reduction for simple images. For WEBP: use lossy mode at quality 75-80 for photos.
11. **Batch processing** — for multiple images, iterate with consistent transformation parameters. Log each file processed with input/output sizes. Use parallel processing for large batches (multiprocessing or Sharp's pipeline mode).
12. **Save the output** — write to the target path with explicit format specification. Verify the output file exists and has non-zero size. Log the output dimensions and file size.
13. **Validate results** — re-open the saved image and verify dimensions match expectations. For format conversions, confirm the output format header is correct. Spot-check visual quality on at least one sample.

# Decision rules

- Default to JPEG for photographs and PNG for graphics with transparency or sharp edges.
- Use WEBP when targeting modern web browsers and file size is critical.
- When resizing, always maintain aspect ratio unless the user explicitly requests distortion.
- If the source image is smaller than the target dimensions, warn about upscaling quality loss and prefer serving the original.
- For batch operations processing >100 images, implement progress reporting every 10% of completion.
- Prefer lossless operations (rotation by 90° multiples, PNG→PNG resize) when quality preservation is paramount.

# Output requirements

1. **Processed image file(s)** — images in the correct format, dimensions, and quality level
2. **Processing log** — for each image: input path, output path, input dimensions, output dimensions, input file size, output file size, operations applied
3. **Error report** — list of images that failed processing with the specific error (corrupt file, unsupported format, missing input)
4. **Metadata summary** — EXIF status (stripped/preserved/modified) for each output image

# References

- Pillow (PIL Fork) documentation — https://pillow.readthedocs.io/en/stable/
- Sharp documentation — https://sharp.pixelplumbing.com/
- ImageMagick command-line usage — https://imagemagick.org/script/command-line-processing.php
- EXIF specification — https://www.exif.org/Exif2-2.PDF

# Related skills

- `image-heavy-pdfs` — for extracting text from images via OCR
- `pdf-generation` — when images need to be assembled into a PDF document
- `screenshot` — for capturing screenshots that may then need processing

# Anti-patterns

- **Resizing without maintaining aspect ratio** — produces distorted images. Always calculate proportional dimensions unless distortion is explicitly requested.
- **Converting PNG with transparency to JPEG without compositing** — produces black backgrounds where transparency was. Always composite onto a white (or specified) background first.
- **Loading all images into memory for batch processing** — causes OOM on large batches. Process one at a time or in bounded chunks.
- **Using quality=100 for JPEG** — produces files 5-10× larger than quality=85 with negligible visual improvement. Use 85 as default.
- **Stripping all EXIF on archival images** — destroys camera settings, GPS, and timestamps that may be needed. Only strip for privacy-sensitive contexts.

# Failure handling

- If the input image is corrupt or unreadable, log the specific error (truncated file, unsupported codec) and skip to the next image in batch mode.
- If the processing library is not installed, output the exact install command and halt with a clear dependency error.
- If an output path is not writable, attempt writing to a temporary directory and report the permission issue.
- If memory is insufficient for a large image (>100MP), use tiled processing or reduce the image in stages.
- If format conversion produces an unexpectedly large file, retry with lower quality settings and log the size comparison.
