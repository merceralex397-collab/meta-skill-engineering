---
name: image-heavy-pdfs
description: "Extract text from image-heavy and scanned PDFs using OCR engines (Tesseract, PaddleOCR) with layout analysis, page segmentation, and confidence scoring. Use when processing scanned documents, invoices, or image-based PDFs that standard text extraction cannot parse. Do not use for text-native PDFs (prefer pdf-extraction) or image manipulation unrelated to document OCR."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: image-heavy-pdfs
  maturity: draft
  risk: low
  tags: [ocr, tesseract, paddleocr, scanned-pdf, layout-analysis]
---

# Purpose

Extract text from image-heavy and scanned PDFs using OCR engines (Tesseract, PaddleOCR) combined with layout analysis, page segmentation, and confidence scoring. This skill handles documents where standard text extraction returns empty or garbled results because the content is embedded in images — scanned documents, photographed pages, image-only PDFs, and mixed-content PDFs with diagram-heavy sections.

# When to use this skill

- A PDF yields empty or garbled text when opened with pdfplumber/PyMuPDF, indicating image-only content
- Processing scanned documents: invoices, receipts, contracts, government forms, historical records
- Extracting text from PDFs containing photographs of pages, whiteboard captures, or screenshots
- A document processing pipeline encounters mixed-content PDFs where some pages are image-only
- OCR quality needs tuning: language selection, DPI optimization, preprocessing, or confidence filtering
- Batch OCR processing of a folder of scanned PDFs

# Do not use this skill when

- The PDF has a native text layer that extracts cleanly — prefer `pdf-extraction`
- The goal is image manipulation (resize, crop, watermark) not text extraction — prefer `image-editor`
- The task is modifying the PDF structure (merge, split, rotate) — prefer `pdf-editor`
- The extracted text needs further structuring into JSON/CSV — use this skill first for OCR, then pipe output to `document-to-structured-data`

# Operating procedure

1. **Detect if OCR is needed** — open the PDF with PyMuPDF (`fitz.open`) and attempt text extraction on each page. If `page.get_text()` returns fewer than 10 characters per page, the PDF is image-based and needs OCR.
2. **Convert PDF pages to images** — render each page to a high-resolution image using PyMuPDF (`page.get_pixmap(dpi=300)`) or `pdf2image` (`convert_from_path(path, dpi=300)`). Use 300 DPI as the default; increase to 600 DPI for small-print documents.
3. **Preprocess images for OCR** — apply preprocessing to improve OCR accuracy: convert to grayscale, apply adaptive thresholding (Otsu's method), deskew rotated pages (detect skew angle and rotate), remove noise with median filtering, and increase contrast.
4. **Select the OCR engine** — use Tesseract (`pytesseract`) as the default for Latin-script languages. Use PaddleOCR for CJK languages, complex layouts, or when Tesseract accuracy is insufficient. Install with `pip install pytesseract` or `pip install paddleocr`.
5. **Configure OCR parameters** — set the page segmentation mode (PSM): use PSM 3 (fully automatic) for standard documents, PSM 6 (single block) for uniform text regions, PSM 4 (single column) for narrow-column layouts. Set the language: `eng` for English, `eng+fra` for multilingual.
6. **Run OCR** — execute OCR on each page image. For Tesseract: `pytesseract.image_to_data(image, lang='eng', config='--psm 3', output_type=Output.DICT)`. For PaddleOCR: `ocr.ocr(image_path, cls=True)`. Capture bounding boxes, text, and confidence scores.
7. **Perform layout analysis** — group OCR text blocks by spatial position to reconstruct reading order. Detect columns (multiple text blocks at similar Y coordinates with X gaps), headers (large font / top of page), and footers (bottom of page). Sort blocks into reading order: top-to-bottom, left-to-right within multi-column layouts.
8. **Reconstruct paragraphs** — merge adjacent text blocks that belong to the same paragraph based on spatial proximity and indentation patterns. Insert paragraph breaks where vertical spacing exceeds 1.5× the line height.
9. **Detect and extract tables** — identify tabular regions by detecting grid lines or aligned text columns. Extract table content into structured row/column format. Use `img2table` or custom grid detection for complex tables within scanned pages.
10. **Score confidence** — calculate per-page and per-block confidence scores from OCR output. Flag pages with average confidence below 70% for manual review. Report the overall document confidence as the weighted average across all pages.
11. **Post-process text** — apply spell-checking or dictionary-based correction for common OCR errors (e.g., `rn` → `m`, `0` → `O` in names, `l` → `1` in numbers). Remove OCR artifacts: stray single characters, repeated headers/footers from page stamps.
12. **Generate output** — produce clean text output organized by page. Include page numbers, reading order, table data (if any), and confidence metadata. Output as plain text, JSON with page structure, or Markdown.
13. **Validate extraction** — spot-check OCR output against the original image for 3-5 pages. Verify that reading order is correct, table structures are preserved, and no content blocks were missed.

# Decision rules

- If Tesseract confidence is below 60% on a page, retry with PaddleOCR before flagging for manual review.
- If the document is in a non-Latin script, use PaddleOCR as the primary engine (better CJK and Arabic support).
- If the scanned image has visible skew (>2°), always deskew before OCR — skew dramatically reduces accuracy.
- If the document has mixed pages (some text-native, some scanned), extract text directly from native pages and OCR only the image pages.
- For documents with >50 pages, process in parallel (multiprocessing) to reduce total processing time.
- If DPI is unknown, default to 300. Only increase to 600 for documents with small font sizes (<8pt).

# Output requirements

1. **Extracted text** — full text organized by page number in reading order
2. **Confidence report** — per-page confidence scores, overall document confidence, list of low-confidence pages
3. **Table data** — any detected tables extracted as CSV or JSON with row/column structure
4. **Page metadata** — page count, DPI used, OCR engine used, preprocessing steps applied, language detected
5. **Review flags** — list of pages requiring manual review with reason (low confidence, complex layout, mixed orientation)

# References

- Tesseract OCR documentation — https://tesseract-ocr.github.io/tessdoc/
- PaddleOCR documentation — https://github.com/PaddlePaddle/PaddleOCR
- PyMuPDF (fitz) rendering — https://pymupdf.readthedocs.io/en/latest/page.html#Page.get_pixmap
- pdf2image documentation — https://github.com/Belval/pdf2image

# Related skills

- `pdf-extraction` — for text-native PDFs that do not require OCR
- `document-to-structured-data` — for converting OCR output into structured records
- `table-extraction` — for focused table extraction from OCR results or native PDFs

# Anti-patterns

- **Running OCR on text-native PDFs** — wastes processing time and produces worse results than direct text extraction. Always check for text layers first.
- **Using default 72 DPI for page rendering** — produces tiny images that OCR cannot read. Always render at 300+ DPI.
- **Skipping preprocessing** — feeding noisy, skewed, or low-contrast images directly to OCR produces garbage output. Always preprocess.
- **Ignoring confidence scores** — accepting all OCR output without confidence filtering propagates errors into downstream systems.
- **Processing all pages serially for large documents** — a 200-page scanned PDF takes hours without parallel processing.

# Failure handling

- If Tesseract is not installed, output the system-level install command (`apt install tesseract-ocr` or `brew install tesseract`) and the Python binding (`pip install pytesseract`).
- If a page renders as a blank image, check if the page uses a non-standard color space or encryption. Log the page number and skip it.
- If OCR returns zero text on a non-blank page, retry with a different PSM mode (try PSM 6, then PSM 4) and increased DPI.
- If memory is exhausted on large documents, process pages in batches of 10 and write intermediate results to disk.
- If the document is password-protected, report the error and do not attempt to bypass encryption.
