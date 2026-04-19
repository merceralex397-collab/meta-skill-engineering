---
name: docx-generation
description: "Generate Word (.docx) documents programmatically using python-docx or similar libraries — apply templates, set styles and headings, insert tables and images, configure headers/footers, and produce print-ready output. Use when creating .docx files from data, building document generation pipelines, or automating report creation in Word format. Do not use for editing existing Word files (prefer document-writing) or PDF output (prefer pdf-generation)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: docx-generation
  maturity: draft
  risk: low
  tags: [docx, python-docx, word, document-generation, templates]
---

# Purpose

Generate Word (.docx) documents programmatically using python-docx or equivalent libraries. This skill covers template-based generation, style application, heading hierarchies, table insertion, image embedding, headers/footers, page numbering, and print-ready formatting to produce professional Word documents from structured data.

# When to use this skill

- A `.docx` file must be generated programmatically from data (reports, contracts, letters)
- A document template needs to be populated with dynamic content (mail merge, data-driven reports)
- The output requires Word-specific formatting: custom styles, headers/footers, page numbers, table of contents
- Building a document generation pipeline that produces Word format for downstream consumption
- The recipient requires `.docx` format specifically (not PDF, not Markdown)
- Batch generation of multiple documents from a data source (e.g., one letter per customer)

# Do not use this skill when

- The task is writing document content (prose, structure, tone) — prefer `document-writing` for content, then use this skill for formatting
- The output should be PDF — prefer `pdf-generation`
- The output needs PowerPoint slides — prefer `pptx-generation`
- The task is modifying an existing complex Word document with track changes — this skill focuses on generation, not collaborative editing
- Plain Markdown output is sufficient for the consumer

# Operating procedure

1. **Install dependencies** — ensure `python-docx` is available: `pip install python-docx`. For advanced features (charts, complex tables), verify the library version supports the required features.
2. **Load or create the document** — if a template exists, open it with `Document('template.docx')`. If generating from scratch, create a new `Document()` and define base styles.
3. **Configure page layout** — set page size (Letter or A4), margins (default: 1 inch all sides), and orientation. Use `document.sections[0].page_width` and related properties.
4. **Define styles** — create or modify named styles for headings (Heading 1–4), body text, captions, and code blocks. Set font family (default: Calibri), size, color, spacing, and paragraph alignment. Reuse built-in Word styles where possible.
5. **Build the heading hierarchy** — add headings using `document.add_heading(text, level=N)`. Ensure level 1 is used only for the document title, level 2 for major sections, level 3 for subsections.
6. **Insert body content** — add paragraphs with `document.add_paragraph()`. Apply styles explicitly. For emphasis, use `run.bold = True` or `run.italic = True` on specific text runs within paragraphs.
7. **Insert tables** — create tables with `document.add_table(rows, cols, style='Table Grid')`. Populate header row first, then data rows. Apply cell shading to headers. Set column widths proportionally using `Inches()` or `Cm()`.
8. **Insert images** — add images with `document.add_picture(path, width=Inches(N))`. Ensure images are in PNG or JPEG format. Set alt text on the inline shape for accessibility.
9. **Configure headers and footers** — access `section.header` and `section.footer`. Add page numbers using `WD_FIELD` or direct paragraph formatting. Include document title in the header and generation date in the footer.
10. **Add page breaks** — insert explicit page breaks between major sections using `document.add_page_break()`. Use section breaks for layout changes (landscape pages for wide tables).
11. **Generate table of contents** — insert a TOC field code at the document start. Note: python-docx cannot auto-update TOC; include instructions for the user to press F9 in Word to refresh, or use a post-processing step with LibreOffice.
12. **Save the document** — save with `document.save('output.docx')`. Verify the file size is reasonable (not bloated by uncompressed images).
13. **Validate the output** — open the generated `.docx` in LibreOffice or Word to verify formatting renders correctly. Check heading hierarchy, table alignment, image scaling, and page breaks.

# Decision rules

- Use template-based generation when the document layout is complex or must match a corporate standard. Use from-scratch generation for simple reports.
- Prefer built-in Word styles over custom styles — they render more reliably across Word versions.
- If a table exceeds 8 columns, switch to landscape orientation for that section or break into multiple tables.
- If images are larger than 2MB each, compress them before embedding to keep the `.docx` file size manageable.
- If the document requires live charts, consider generating the chart as an image and embedding it, since python-docx has limited chart support.
- For batch generation (>10 documents), implement a template-once-populate-many pattern to avoid redundant style configuration.

# Output requirements

1. **DOCX file** — valid `.docx` file that opens without errors in Microsoft Word and LibreOffice
2. **Consistent styles** — all headings, body text, and tables use named styles (no inline formatting overrides)
3. **Metadata** — document properties set: title, author, creation date, and subject
4. **Accessibility** — images have alt text, tables have header rows marked, reading order follows logical document flow
5. **File size** — output file under 10MB unless image-heavy content justifies larger size

# References

- python-docx documentation — https://python-docx.readthedocs.io/en/latest/
- python-docx API reference — https://python-docx.readthedocs.io/en/latest/api/document.html
- OOXML specification (ISO/IEC 29500) — https://www.ecma-international.org/publications-and-standards/standards/ecma-376/

# Related skills

- `pdf-generation` — when the output must be PDF rather than Word format
- `pptx-generation` — when the output is a PowerPoint presentation
- `document-writing` — for authoring the content and structure before formatting into DOCX

# Anti-patterns

- **Inline formatting everywhere** — setting font/size/color on individual runs instead of using named styles creates unmaintainable documents that break on template changes.
- **Ignoring template-based generation** — building complex layouts from scratch in code when a template would be faster and more reliable.
- **Embedding uncompressed images** — inserting raw BMP or uncompressed TIFF files bloats the document to hundreds of megabytes.
- **Hardcoding page dimensions in inches for international audiences** — always parameterize page size; A4 and Letter have different dimensions.
- **Skipping validation** — generated documents can have invisible corruption (broken styles, missing images) that only appears when opened in Word.

# Failure handling

- If python-docx is not installed, output the exact install command (`pip install python-docx`) and halt.
- If a template file is missing or corrupted, fall back to from-scratch generation and log a warning identifying the missing template.
- If an image path is invalid, insert a placeholder paragraph reading `[IMAGE NOT FOUND: filename]` and continue generating the rest of the document.
- If the output file exceeds 50MB, log a warning listing the largest embedded resources and suggest image compression.
- If a style name does not exist in the template, create the style dynamically with sensible defaults and log the fallback.
