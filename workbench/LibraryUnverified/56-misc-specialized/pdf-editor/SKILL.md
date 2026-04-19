---
name: pdf-editor
description: "Modify existing PDFs — merge multiple files, split by page ranges, rotate pages, add watermarks, fill form fields, edit metadata, and redact content using PyPDF2, pikepdf, or pdftk. Use when manipulating existing PDF files rather than creating new ones or extracting content. Do not use for PDF creation from scratch (prefer pdf-generation) or text extraction (prefer pdf-extraction)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: pdf-editor
  maturity: draft
  risk: low
  tags: [pdf, merge, split, watermark, form-fill, pikepdf]
---

# Purpose

Modify existing PDF files — merge multiple PDFs into one, split by page ranges, rotate pages, add watermarks and overlays, fill form fields, edit document metadata, redact sensitive content, and manipulate page order using PyPDF2, pikepdf, or pdftk. This skill covers structural PDF manipulation rather than content creation or text extraction.

# When to use this skill

- Multiple PDF files need to be merged into a single document
- A PDF needs to be split into separate files by page ranges or chapters
- Pages need rotation (scanned pages in wrong orientation)
- A watermark, stamp, or overlay must be added to existing PDF pages
- PDF form fields need to be filled programmatically (applications, contracts, government forms)
- Document metadata (title, author, keywords, creation date) needs editing
- Sensitive content must be redacted (blackout text, remove pages)
- Page order needs rearranging, or specific pages need extraction

# Do not use this skill when

- Creating a new PDF from scratch (HTML, Markdown, or data) — prefer `pdf-generation`
- Extracting text, tables, or metadata from a PDF for analysis — prefer `pdf-extraction`
- The PDF is image-based and needs OCR text extraction — prefer `image-heavy-pdfs`
- The task involves Word document manipulation — prefer `docx-generation`

# Operating procedure

1. **Install dependencies** — ensure the PDF library is available. For pikepdf: `pip install pikepdf`. For PyPDF2: `pip install PyPDF2`. For pdftk: install via system package manager (`apt install pdftk` or `brew install pdftk-java`). Prefer pikepdf for reliability and standards compliance.
2. **Open the source PDF** — load with `pikepdf.open('input.pdf')` or `PdfReader('input.pdf')`. Verify the file opens without errors. Check if the PDF is encrypted and handle password-protected files.
3. **Merge PDFs** — to combine multiple files, open each source PDF and append pages sequentially to a new writer object. Preserve bookmarks from each source if they exist. Set the output page order explicitly.
4. **Split PDF by page range** — extract specific pages using zero-based page indexing. Create a new PDF writer for each output segment. Validate that requested page numbers are within the document's page count. Name output files descriptively (e.g., `report_pages_1-5.pdf`).
5. **Rotate pages** — rotate individual pages by 90, 180, or 270 degrees using `page.rotate(degrees)`. Apply rotation to specific pages (by index) or all pages. Verify rotation direction matches the intended orientation.
6. **Add watermark** — create a watermark PDF (text or image on a transparent background) and overlay it on each page using `page.merge_page(watermark_page)` (PyPDF2) or stamp operations (pikepdf). Position the watermark centrally and set opacity for readability.
7. **Fill form fields** — identify fillable fields using `reader.get_fields()` or `pdf.pages[0]['/Annots']`. Map field names to values from the data source. Write values to each field and flatten the form if the output should be non-editable. Verify all required fields are populated.
8. **Edit metadata** — update the document info dictionary: title, author, subject, keywords, creator, and creation/modification dates. Use `writer.add_metadata({'/Title': 'New Title'})` or pikepdf's `pdf.docinfo` interface.
9. **Redact content** — for text redaction, use a library that supports content stream editing (pikepdf) or overlay opaque rectangles over sensitive regions. For page removal, simply exclude the page when writing output. Verify redacted content is not recoverable (not just hidden behind an overlay).
10. **Rearrange pages** — reorder pages by building a new PDF with pages inserted in the desired sequence. Remove duplicate pages if consolidating from multiple sources.
11. **Save the output** — write the modified PDF to a new file (never overwrite the original). Use `writer.write('output.pdf')` or `pdf.save('output.pdf')`. Verify the output file size is reasonable.
12. **Validate the result** — open the output PDF and verify: correct page count, rotation is applied, watermarks are visible, form fields contain expected values, metadata is updated, and no content corruption occurred. Compare file sizes between input and output.

# Decision rules

- Use pikepdf over PyPDF2 for encrypted PDFs, broken PDFs, or when strict PDF/A compliance is needed — pikepdf handles edge cases better.
- Use pdftk for simple merge/split/rotate operations in shell scripts where Python is not available.
- Never overwrite the source file — always write to a new output path to preserve the original.
- For form filling, flatten the form after filling if the recipient should not edit the values.
- If a PDF has both text and image layers, watermarks should be placed on the top layer to be visible over both.
- For redaction, verify the redacted data is removed from the content stream, not just visually obscured — overlays can be removed.

# Output requirements

1. **Modified PDF file** — valid PDF that opens without errors in Adobe Reader, Preview, and browser PDF viewers
2. **Operation log** — list of operations performed: pages merged/split/rotated, fields filled, metadata changed
3. **Page count verification** — expected vs actual page count in the output
4. **File size comparison** — input and output file sizes to detect bloat or corruption
5. **Validation confirmation** — statement that the output was opened and visually verified

# References

- pikepdf documentation — https://pikepdf.readthedocs.io/en/latest/
- PyPDF2 documentation — https://pypdf2.readthedocs.io/en/latest/
- pdftk manual — https://www.pdflabs.com/docs/pdftk-man-page/
- PDF specification (ISO 32000) — https://www.adobe.com/devnet-docs/acroforms/FormsAPIReference.pdf

# Related skills

- `pdf-extraction` — for extracting text and tables from PDFs
- `pdf-generation` — for creating new PDFs from scratch
- `image-heavy-pdfs` — for OCR processing of scanned PDFs before or after editing

# Anti-patterns

- **Overwriting the source PDF** — if the write fails mid-operation, the original is corrupted with no recovery. Always write to a new file.
- **Merging without checking page sizes** — combining Letter and A4 PDFs produces inconsistent page sizes. Normalize or warn.
- **Visual-only redaction** — placing a black rectangle over text without removing the underlying content stream leaves sensitive data extractable. Use proper content-stream redaction.
- **Ignoring encryption** — attempting to modify an encrypted PDF without the owner password silently produces corrupted output.
- **Flattening forms before verifying field values** — once flattened, form fields cannot be corrected. Always verify before flattening.

# Failure handling

- If the PDF is password-protected and no password is provided, report the encryption type (user password vs owner password) and halt.
- If a requested page index is out of range, report the valid page range and skip the invalid page rather than crashing.
- If pikepdf/PyPDF2 is not installed, output the exact install command and halt with a dependency error.
- If the output PDF is corrupted (zero bytes, fails to open), retry the operation with a different library (e.g., switch from PyPDF2 to pikepdf).
- If form field names do not match the provided data keys, list the available field names and report unmapped data keys for the user to reconcile.
