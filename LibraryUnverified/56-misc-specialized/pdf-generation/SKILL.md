---
name: pdf-generation
description: "Create PDF documents from data using ReportLab, WeasyPrint, wkhtmltopdf, or Puppeteer — design layouts, apply templates, insert tables and charts, handle pagination, and produce print-ready output. Use when generating invoices, reports, certificates, or any data-to-PDF pipeline. Do not use for editing existing PDFs (prefer pdf-editor) or extracting PDF content (prefer pdf-extraction)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: pdf-generation
  maturity: draft
  risk: low
  tags: [pdf-generation, reportlab, weasyprint, wkhtmltopdf, templates]
---

# Purpose

Create PDF documents from structured data using ReportLab, WeasyPrint, wkhtmltopdf, or Puppeteer. Build template-driven pipelines that produce invoices, reports, certificates, letters, and other print-ready output with consistent layout, pagination, and typography.

# When to use this skill

- The task requires generating a new PDF from data, templates, or HTML content.
- Building an automated pipeline that converts structured data (JSON, DB rows, API responses) into PDF output.
- The user needs invoices, reports, certificates, form letters, or any repeatable PDF artifact.
- A project needs programmatic control over page layout, headers/footers, page numbers, or table of contents.
- Converting HTML/CSS designs to PDF for print distribution.

# Do not use this skill

- To edit, annotate, or merge existing PDF files — prefer `pdf-editor`.
- To extract text, images, or metadata from existing PDFs — prefer `pdf-extraction`.
- To generate Word documents — prefer `docx-generation`.
- For simple text file output that does not require PDF formatting.

# Operating procedure

1. **Identify the PDF type and audience.** Determine whether the output is an invoice, report, certificate, letter, or custom layout. Confirm the target page size (A4, Letter, custom) and orientation.
2. **Select the generation engine.** Choose ReportLab for low-level layout control and complex graphics. Choose WeasyPrint for HTML/CSS-to-PDF conversion with web-standard styling. Choose wkhtmltopdf for quick HTML-to-PDF when CSS paged media is not critical. Choose Puppeteer/Playwright for JavaScript-rendered content.
3. **Design the template structure.** Define reusable templates with placeholder variables for dynamic data. Separate layout concerns (margins, fonts, colors) from data insertion points. Create a base template class or HTML skeleton.
4. **Build the data pipeline.** Write functions that accept structured input (dict, DataFrame, ORM model) and populate template variables. Validate all required fields before rendering — reject incomplete data with clear error messages.
5. **Implement page layout.** Set margins, headers, footers, and page numbers. For ReportLab, use `SimpleDocTemplate` or `BaseDocTemplate` with `PageTemplate` and `Frame` objects. For WeasyPrint, use CSS `@page` rules with `@top-center`, `@bottom-right` regions.
6. **Insert tables and charts.** Use ReportLab `Table` with `TableStyle` for row/column data. For charts, use ReportLab's `reportlab.graphics.charts` or embed matplotlib/plotly renders as images. Size all elements relative to the page frame.
7. **Handle pagination and page breaks.** Insert explicit page breaks before major sections. Use `KeepTogether` (ReportLab) or `page-break-inside: avoid` (CSS) to prevent orphaned rows. Test with realistic data volumes — edge cases appear at page boundaries.
8. **Add fonts and branding.** Register custom TTF/OTF fonts. Set a font fallback chain for Unicode support. Embed logos and watermarks at consistent positions using absolute or relative coordinates.
9. **Test the output.** Generate PDFs with minimum, typical, and maximum data volumes. Verify page breaks, table splits, header/footer consistency, and font rendering. Open in multiple viewers (browser, Adobe, Preview) to catch rendering differences.
10. **Optimize file size.** Compress images before embedding (target 150 DPI for print, 72 DPI for screen). Subset fonts to include only used glyphs. Remove unused metadata.

# Decision rules

- Default to WeasyPrint when the source content is HTML/CSS and layout requirements are standard.
- Default to ReportLab when pixel-perfect control, complex graphics, or programmatic drawing is required.
- Use wkhtmltopdf only for legacy projects already depending on it — prefer WeasyPrint for new work.
- Use Puppeteer when the page requires JavaScript execution to render charts or dynamic content.
- Always embed fonts rather than relying on system fonts for cross-platform consistency.
- Fail the generation pipeline loudly on missing data rather than producing PDFs with blank fields.

# Output requirements

1. A working PDF generation script or module with clearly separated template, data, and rendering layers.
2. Sample output PDF demonstrating the layout with representative test data.
3. Configuration for page size, margins, fonts, and branding as externalized parameters.
4. Error handling that rejects malformed input before rendering begins.

# References

- ReportLab User Guide: https://docs.reportlab.com/
- WeasyPrint documentation: https://doc.courtbouillon.org/weasyprint/
- CSS Paged Media spec (W3C): https://www.w3.org/TR/css-page-3/
- wkhtmltopdf usage: https://wkhtmltopdf.org/usage/wkhtmltopdf.txt

# Related skills

- `pdf-editor` — for modifying, annotating, or merging existing PDFs.
- `docx-generation` — for Word document output instead of PDF.
- `pptx-generation` — for slide deck output.

# Anti-patterns

- Generating HTML with inline styles and piping to wkhtmltopdf without testing pagination — produces broken page breaks and missing headers.
- Hardcoding absolute pixel positions for every element instead of using flowable layouts — breaks when data length varies.
- Embedding full-resolution images without compression — produces multi-megabyte PDFs for simple reports.
- Ignoring Unicode/multilingual text — causes missing characters when custom fonts are not registered.

# Failure handling

- If the selected engine is not installed, output the exact install command (`pip install reportlab`, `pip install weasyprint`) and halt.
- If required data fields are missing or null, raise a validation error listing each missing field — never render a partial PDF.
- If a font file is missing, fall back to Helvetica/DejaVu Sans and log a warning with the expected font path.
- If the output PDF exceeds a size threshold (e.g., 10 MB), log a warning suggesting image compression or font subsetting.
- If rendering produces zero pages, check for empty data input and report the root cause.
