---
name: pptx-generation
description: "Generate PowerPoint (.pptx) presentations programmatically using python-pptx — apply slide layouts, insert charts and tables, set speaker notes, manage slide masters, and produce presentation-ready decks from data. Use when creating automated slide decks, building presentation pipelines, or converting structured data into slide format. Do not use for manual presentation design advice or non-PowerPoint slide formats."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: pptx-generation
  maturity: draft
  risk: low
  tags: [pptx, python-pptx, powerpoint, slides, presentations]
---

# Purpose

Generate PowerPoint (.pptx) presentations programmatically using python-pptx. Build pipelines that convert structured data into slide decks with consistent layouts, charts, tables, images, and speaker notes — suitable for automated reporting, executive summaries, and data-driven presentations.

# When to use this skill

- The task requires generating .pptx files from structured data (JSON, CSV, database queries, API responses).
- Building an automated reporting pipeline that produces slide decks on a schedule.
- Converting analysis results, dashboards, or metrics into presentation format.
- The user needs a template-based slide generation system with branded layouts.
- Populating an existing .pptx template with dynamic content (mail-merge style).

# Do not use this skill

- For manual presentation design advice or visual design critiques — this skill is for programmatic generation.
- For generating Google Slides, Keynote, or reveal.js — this skill targets the .pptx format via python-pptx.
- For generating PDF slide handouts — prefer `pdf-generation` after creating the .pptx.
- For Word document generation — prefer `docx-generation`.

# Operating procedure

1. **Define the deck structure.** List the slides needed: title slide, agenda, content slides, chart slides, summary, appendix. Map each slide to a data source (dict key, DataFrame, image path).
2. **Select or create a slide master.** Open an existing branded .pptx template with `Presentation('template.pptx')` or create a blank presentation. Identify available slide layouts by iterating `prs.slide_layouts` and mapping layout names to indices.
3. **Initialize the presentation object.** Instantiate `Presentation()` or load the template. Set slide dimensions if non-standard: `prs.slide_width` and `prs.slide_height` using `Inches()` or `Emu()`.
4. **Add slides with appropriate layouts.** For each content section, select the matching layout: `slide_layout = prs.slide_layouts[index]` then `slide = prs.slides.add_slide(slide_layout)`. Access placeholders by index to populate title and body text.
5. **Insert tables.** Use `slide.shapes.add_table(rows, cols, left, top, width, height)` to create tables. Populate cells with `table.cell(row, col).text = value`. Apply formatting: font size, bold headers, cell margins, and alternating row colors via `cell.fill.solid()`.
6. **Insert charts.** Build chart data with `CategoryChartData()` or `XyChartData()`. Add charts via `slide.shapes.add_chart(chart_type, x, y, cx, cy, chart_data)`. Configure axes, legends, data labels, and colors through the `chart` object properties.
7. **Insert images and shapes.** Add images with `slide.shapes.add_picture(image_path, left, top, width, height)`. For generated charts (matplotlib, plotly), save to a BytesIO buffer first, then insert. Maintain aspect ratios.
8. **Set speaker notes.** Access `slide.notes_slide.notes_text_frame.text` to add presenter notes for each slide. Include data sources, talking points, and caveats.
9. **Apply consistent formatting.** Set font families, sizes, and colors programmatically across all text frames. Use `run.font.size = Pt(N)` and `run.font.color.rgb = RGBColor(r, g, b)`. Maintain brand consistency.
10. **Save and validate.** Save with `prs.save('output.pptx')`. Open the result in PowerPoint or LibreOffice to verify layouts, chart rendering, and text overflow. Test with varying data lengths.

# Decision rules

- Always start from a branded template .pptx when one exists — avoid recreating slide masters in code.
- Use placeholder-based population when the template has defined placeholders — it preserves the designer's layout.
- Fall back to absolute-positioned shapes only when placeholders are insufficient.
- Prefer `CategoryChartData` for bar/line/pie charts and `XyChartData` for scatter plots.
- Limit slides to 1 key message per slide — split dense data across multiple slides rather than cramming.
- Embed images at 150 DPI for presentation use — higher resolution inflates file size without visible benefit on projectors.

# Output requirements

1. A Python script or module that accepts structured data and produces a .pptx file.
2. Clear separation between data preparation, template selection, and slide population logic.
3. Speaker notes on every content slide with data source attribution.
4. The output .pptx must open without errors in PowerPoint 2016+ and LibreOffice Impress.

# References

- python-pptx documentation: https://python-pptx.readthedocs.io/
- python-pptx API reference for charts: https://python-pptx.readthedocs.io/en/latest/api/chart.html
- Office Open XML (OOXML) slide format: https://learn.microsoft.com/en-us/openspecs/office_standards/ms-pptx/

# Related skills

- `docx-generation` — for Word document output from similar data pipelines.
- `pdf-generation` — for PDF output or converting the generated .pptx to PDF.
- `document-writing` — for narrative-form documents rather than slide decks.

# Anti-patterns

- Hardcoding slide layout indices (e.g., `slide_layouts[1]`) without verifying the layout name — breaks when the template changes.
- Adding 50+ data rows to a single slide table instead of paginating across multiple slides.
- Ignoring text overflow in placeholders — long strings silently clip or shrink to unreadable sizes.
- Using `add_textbox` everywhere instead of leveraging template placeholders — produces inconsistent formatting.
- Saving matplotlib charts as low-resolution PNGs — use SVG or high-DPI PNG for crisp rendering.

# Failure handling

- If python-pptx is not installed, output `pip install python-pptx` and halt.
- If the template file is missing or corrupt, raise a clear error with the expected path.
- If a slide layout index is out of range, list available layouts with names and indices, then halt.
- If chart data is empty, skip the chart slide and log a warning rather than producing a blank chart.
- If an image file is missing, insert a placeholder shape with error text instead of crashing the pipeline.
- If the output file exceeds 50 MB, log a warning suggesting image compression.
