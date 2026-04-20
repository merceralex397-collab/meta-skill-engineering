---
name: table-extraction
description: "Extract tables from PDFs, HTML pages, and images — detect row/column boundaries, infer headers, handle merged cells, and output structured data as CSV, JSON, or DataFrames using camelot, tabula-py, or custom parsers. Use when pulling tabular data from documents, converting document tables to structured formats, or building table detection pipelines. Do not use for creating tables in documents (prefer document-writing) or spreadsheet generation (prefer xlsx-generation)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: table-extraction
  maturity: draft
  risk: low
  tags: [table-extraction, camelot, tabula, row-detection, header-inference]
---

# Purpose

Extract tables from PDFs, HTML pages, and images into structured data formats. Detect row/column boundaries, infer headers, handle merged cells and multi-line rows, and output clean CSV, JSON, or pandas DataFrames using camelot, tabula-py, pdfplumber, or custom parsers — building reliable table extraction pipelines for data processing workflows.

# When to use this skill

- Extracting tabular data from PDF reports, financial statements, or government filings.
- Pulling tables from HTML pages where the data is not available via API.
- Building a pipeline that converts document tables into database-ready structured formats.
- Processing scanned documents with OCR to extract table content from images.
- Handling complex tables with merged cells, multi-row headers, or nested structures.

# Do not use this skill

- For creating tables in new documents — prefer `document-writing` or `docx-generation`.
- For generating spreadsheets — prefer `xlsx-generation`.
- For extracting non-tabular text from PDFs — prefer `pdf-extraction`.
- For editing or annotating existing PDFs — prefer `pdf-editor`.

# Operating procedure

1. **Identify the source format.** Determine whether the input is a native PDF (text-based), a scanned PDF (image-based requiring OCR), an HTML page, or a standalone image. This determines the extraction library and approach.
2. **Select the extraction tool.** For text-based PDFs with ruled lines, use camelot (`lattice` mode). For text-based PDFs without visible borders, use camelot (`stream` mode) or pdfplumber. For scanned PDFs, apply OCR first (Tesseract via pytesseract) then extract. For HTML tables, use pandas `read_html()` or BeautifulSoup. For tabula-py, use it as a wrapper when Java is available.
3. **Configure extraction parameters.** For camelot lattice: set `line_scale`, `joint_tol`, and `edge_tol` based on the PDF's ruling line thickness. For camelot stream: set `row_tol` and `column_tol` to control cell boundary detection. For pdfplumber: define explicit bounding boxes with `page.crop()` when tables occupy only part of the page.
4. **Extract raw table data.** Run the extraction and capture the raw DataFrame output. For multi-page documents, iterate pages and extract tables from each: `camelot.read_pdf(path, pages='1-end', flavor='lattice')`.
5. **Detect and validate headers.** Check if the first row contains header values or data. Heuristics: headers are typically bold, contain no numeric-only values, and have unique values. If headers span multiple rows, merge them into a single header row with concatenated names.
6. **Handle merged cells.** Identify cells spanning multiple rows or columns. For vertically merged cells, forward-fill the value down. For horizontally merged cells, assign the value to the first column and mark others as derived. Log each merge operation for traceability.
7. **Clean extracted data.** Strip whitespace from all cells. Remove empty rows and columns. Normalize number formats (remove thousands separators, fix decimal points). Convert date strings to ISO 8601 format. Replace OCR artifacts (e.g., `l` misread as `1`) using domain-specific rules.
8. **Validate table structure.** Confirm consistent column counts across all rows. Check that numeric columns parse as numbers. Verify header names are unique. Flag rows where the column count mismatches the header count.
9. **Output in the target format.** Export as CSV (`df.to_csv()`), JSON (`df.to_json(orient='records')`), or return the DataFrame directly. Include metadata: source file, page number, table index on page, extraction confidence score, and row/column counts.
10. **Compare against ground truth.** If a reference dataset exists, compare extracted values cell-by-cell. Report accuracy metrics: exact match rate, numeric tolerance match rate, and rows with discrepancies.

# Decision rules

- Use camelot lattice mode as the default for PDFs with visible table borders — it is the most reliable.
- Switch to camelot stream mode only when lattice produces zero tables (no ruling lines detected).
- Use pdfplumber when tables have complex layouts that camelot mishandles (nested tables, tables with footnotes embedded in cells).
- Use pandas `read_html()` for HTML sources — it handles most standard HTML table markup correctly.
- Apply OCR (Tesseract) only for scanned/image-based documents — it is slow and error-prone on text-based PDFs.
- When extraction confidence is below 80%, flag the output for manual review rather than silently delivering bad data.

# Output requirements

1. Structured data in the requested format (CSV, JSON, or DataFrame) with consistent column types.
2. Extraction metadata: source file path, page number, table index, row count, column count, and confidence score.
3. A cleaning log listing transformations applied (merged cells filled, whitespace stripped, formats normalized).
4. Flagged rows where extraction confidence is low or column counts are inconsistent.

# References

- Camelot documentation: https://camelot-py.readthedocs.io/
- pdfplumber documentation: https://github.com/jsvine/pdfplumber
- tabula-py documentation: https://tabula-py.readthedocs.io/
- Tesseract OCR: https://github.com/tesseract-ocr/tesseract
- pandas read_html: https://pandas.pydata.org/docs/reference/api/pandas.read_html.html

# Related skills

- `pdf-extraction` — for extracting non-tabular content (text, images, metadata) from PDFs.
- `document-to-structured-data` — for broader document-to-data conversion beyond tables.
- `xlsx-generation` — for producing Excel files from extracted table data.

# Anti-patterns

- Running extraction without inspecting the PDF first — always check if it is text-based or scanned before choosing a tool.
- Using OCR on text-based PDFs — wastes time and introduces errors that do not exist in the source.
- Ignoring merged cells — produces misaligned data where values shift to wrong columns.
- Hardcoding column indices instead of matching by header name — breaks when the source table format changes.
- Treating all extracted numbers as strings — prevents downstream numeric operations and aggregations.

# Failure handling

- If camelot finds zero tables, retry with `flavor='stream'` and adjusted tolerances before reporting failure.
- If OCR quality is poor (many garbled characters), recommend higher-resolution scanning (300+ DPI) and reprocessing.
- If column counts are inconsistent across rows, output the raw extraction with a warning and the specific row numbers that mismatch.
- If a required library is not installed, output the install command (`pip install camelot-py[cv]`, `pip install pdfplumber`) and halt.
- If the PDF is password-protected, report the error and request the password or an unlocked version.
