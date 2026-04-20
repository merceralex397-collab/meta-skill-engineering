---
name: pdf-extraction
description: "Extract text, tables, and metadata from text-native PDFs using pdfplumber, PyMuPDF (fitz), tabula-py, or camelot with layout-aware parsing and coordinate-based region selection. Use when pulling structured content from PDFs, extracting specific tables, or reading PDF metadata. Do not use for scanned/image PDFs (prefer image-heavy-pdfs) or modifying PDF files (prefer pdf-editor)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: pdf-extraction
  maturity: draft
  risk: low
  tags: [pdf-extraction, pdfplumber, pymupdf, tabula, camelot]
---

# Purpose

Extract text, tables, and metadata from text-native PDFs using pdfplumber, PyMuPDF (fitz), tabula-py, or camelot with layout-aware parsing, coordinate-based region selection, and structural analysis. This skill handles PDFs that have embedded text layers — reports, papers, financial statements, technical documents — and produces clean text, structured table data, and document metadata.

# When to use this skill

- Extracting body text from a PDF for further processing, indexing, or analysis
- Pulling specific tables from financial reports, invoices, or data sheets
- Reading PDF metadata: title, author, creation date, page count, producer
- Extracting text from specific regions of a page (headers, footers, sidebars) using coordinate-based selection
- Building a PDF processing pipeline that feeds text into NLP, search, or structured data extraction
- Comparing text content across multiple PDF versions

# Do not use this skill when

- The PDF is scanned or image-based (text extraction returns empty/garbled) — prefer `image-heavy-pdfs` for OCR
- The task is modifying the PDF (merge, split, rotate, watermark) — prefer `pdf-editor`
- The task is creating a new PDF from data — prefer `pdf-generation`
- The extracted data needs entity extraction and schema mapping — use this skill first, then pipe to `document-to-structured-data`

# Operating procedure

1. **Install dependencies** — ensure the extraction library is available. For pdfplumber: `pip install pdfplumber`. For PyMuPDF: `pip install PyMuPDF`. For tabula-py: `pip install tabula-py` (requires Java runtime). For camelot: `pip install camelot-py[cv]`.
2. **Open the PDF** — load with `pdfplumber.open('file.pdf')` or `fitz.open('file.pdf')`. Verify the file opens without errors and is not encrypted. Check page count and log it.
3. **Detect content type** — check if pages contain extractable text by calling `page.extract_text()` on the first 3 pages. If text extraction returns fewer than 10 characters per page, the PDF is likely image-based — redirect to `image-heavy-pdfs`.
4. **Extract full text** — iterate over all pages and extract text. Use `page.extract_text()` (pdfplumber) or `page.get_text("text")` (PyMuPDF). Preserve page boundaries in the output with page number markers.
5. **Extract layout-aware text** — for documents with columns, headers, or complex layouts, use `page.get_text("blocks")` (PyMuPDF) to get text blocks with bounding box coordinates. Sort blocks by reading order: top-to-bottom, left-to-right. Detect multi-column layouts by identifying vertical gaps between text blocks.
6. **Extract tables** — use pdfplumber's `page.extract_tables()` for simple grid tables. For complex tables without visible borders, use camelot with `flavor='stream'`. For tables spanning multiple pages, extract from each page and concatenate matching column structures.
7. **Extract specific regions** — define a bounding box `(x0, y0, x1, y1)` for the target region. Use `page.within_bbox(bbox).extract_text()` (pdfplumber) or `page.get_text("text", clip=rect)` (PyMuPDF). Use this for extracting headers, footers, margin notes, or specific form fields.
8. **Extract metadata** — read the document info dictionary: title, author, subject, keywords, creator, producer, creation date, modification date. Use `pdf.metadata` (pdfplumber) or `pdf.metadata` (PyMuPDF). Also extract: page count, page dimensions, PDF version.
9. **Extract hyperlinks and annotations** — identify clickable links, bookmarks, and annotations on each page. Use `page.annots()` (PyMuPDF) to get annotation coordinates, types, and linked URIs.
10. **Clean extracted text** — remove soft hyphens at line breaks (re-join hyphenated words), normalize whitespace (collapse multiple spaces/newlines), fix common encoding artifacts (ligatures: fi, fl, ff), and strip headers/footers that repeat on every page.
11. **Structure the output** — organize extracted content by page. For each page, output: page number, full text, tables (as list of row dictionaries), and annotations. Produce the final output as JSON, Markdown, or plain text per the requester's needs.
12. **Validate extraction quality** — spot-check 3-5 pages by comparing extracted text against the visual PDF content. Verify table column counts match, reading order is correct, and no text blocks were missed.

# Decision rules

- Use pdfplumber as the default for general text and table extraction — it handles most documents well.
- Use PyMuPDF (fitz) when speed is critical (10× faster than pdfplumber for text-only extraction) or when coordinate-level precision is needed.
- Use tabula-py or camelot when pdfplumber fails on complex tables — camelot's stream mode handles borderless tables better.
- If text extraction returns garbled Unicode, try different extraction parameters or switch libraries before assuming the PDF is image-based.
- For multi-column academic papers, always use layout-aware extraction (block-based) rather than simple text extraction.
- If a table has merged cells, extract the raw grid and post-process to fill merged values downward/rightward.

# Output requirements

1. **Extracted text** — clean text organized by page number, with reading order preserved
2. **Table data** — each table as a list of row dictionaries with normalized column headers, exportable to CSV or JSON
3. **Document metadata** — title, author, creation date, page count, PDF version, producer
4. **Extraction report** — pages processed, tables found, extraction method used per page, any pages that returned empty text
5. **Quality flags** — pages with potential extraction issues (low text density, garbled characters, missing expected tables)

# References

- pdfplumber documentation — https://github.com/jsvine/pdfplumber
- PyMuPDF (fitz) documentation — https://pymupdf.readthedocs.io/en/latest/
- tabula-py documentation — https://tabula-py.readthedocs.io/en/latest/
- camelot documentation — https://camelot-py.readthedocs.io/en/master/

# Related skills

- `image-heavy-pdfs` — for scanned/image-based PDFs requiring OCR before text extraction
- `pdf-editor` — for modifying PDF structure (merge, split, rotate, watermark)
- `table-extraction` — for focused table extraction across multiple document formats
- `document-to-structured-data` — for mapping extracted text to target schemas

# Anti-patterns

- **Using simple text extraction on multi-column PDFs** — produces interleaved column text that reads as nonsense. Always use layout-aware extraction for complex layouts.
- **Assuming all PDFs have text layers** — failing to detect image-only PDFs wastes time on empty extraction. Always check text density first.
- **Extracting tables as plain text** — loses column structure entirely. Always use table-specific extraction methods.
- **Ignoring page headers and footers** — repeated headers/footers contaminate extracted text. Detect and strip them by identifying text that repeats at the same coordinates across pages.
- **Hardcoding bounding boxes** — coordinates differ between PDF versions and page sizes. Use content-anchored regions (find a label, then extract relative to it) when possible.

# Failure handling

- If the PDF is encrypted, attempt to open with an empty password (some PDFs have owner-only restrictions). If that fails, report the encryption type and halt.
- If a page returns zero text but is not blank (has visible content), flag it as a potential image page and suggest running `image-heavy-pdfs` on that page.
- If table extraction produces inconsistent column counts across rows, log the raw extraction and attempt re-extraction with a different library or parameters.
- If the required library is not installed, output the exact install command and halt.
- If the PDF is malformed (broken cross-references, truncated), attempt repair with pikepdf's `open(path, allow_overwriting_input=True)` before extraction. Log that repair was needed.
