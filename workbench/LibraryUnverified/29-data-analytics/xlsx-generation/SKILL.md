---
name: xlsx-generation
description: "Generate Excel (.xlsx) workbooks programmatically using openpyxl or xlsxwriter — create multi-sheet workbooks, apply cell formatting and conditional styles, insert formulas and charts, set print areas, and handle large datasets efficiently. Use when producing Excel reports, building data export pipelines, or creating templated spreadsheets. Do not use for CSV output (prefer csv-ready) or reading/parsing existing Excel files."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: xlsx-generation
  maturity: draft
  risk: low
  tags: [xlsx, openpyxl, xlsxwriter, excel, spreadsheets, formulas]
---

# Purpose

Generate Excel (.xlsx) workbooks programmatically using openpyxl or xlsxwriter. Create multi-sheet workbooks with cell formatting, conditional styles, formulas, charts, data validation, print areas, and named ranges — producing reports, data exports, and templated spreadsheets suitable for business users and automated pipelines.

# When to use this skill

- Producing Excel reports from structured data (database queries, API responses, DataFrames).
- Building automated data export pipelines that deliver .xlsx files to stakeholders.
- Creating templated spreadsheets with pre-built formulas, formatting, and data validation rules.
- Generating multi-sheet workbooks with summary dashboards, detail sheets, and chart sheets.
- Populating an existing Excel template with dynamic data (mail-merge style reporting).

# Do not use this skill

- For simple CSV output — prefer `csv-ready` (no formatting needed, smaller file size).
- For reading or parsing existing Excel files — that is an ingestion/extraction task.
- For generating PDF reports — prefer `pdf-generation`.
- For generating Word documents — prefer `docx-generation`.

# Operating procedure

1. **Define workbook structure.** Map out the sheets needed: summary/dashboard, detail data sheets, chart sheets, configuration/lookup sheets. Define the column layout for each sheet with column names, data types, and widths.
2. **Select the generation library.** Use openpyxl for workbooks that need template loading, cell-level formatting, and post-generation editing. Use xlsxwriter for write-only workbooks with better performance on large datasets (100K+ rows) and richer chart support. Do not mix libraries in the same workbook.
3. **Initialize the workbook.** For openpyxl: `wb = openpyxl.Workbook()` or `load_workbook('template.xlsx')`. For xlsxwriter: `wb = xlsxwriter.Workbook('output.xlsx')`. Set workbook-level properties: author, title, company.
4. **Create sheets and write headers.** Add sheets with descriptive names (`wb.create_sheet('Monthly Summary')`). Write header rows with bold formatting, frozen panes (`ws.freeze_panes = 'A2'`), and auto-filter (`ws.auto_filter.ref = 'A1:G1'`).
5. **Write data rows.** Iterate over the data source and write cell values with correct types: numbers as `int`/`float` (not strings), dates as `datetime` objects, currencies with number format `'#,##0.00'`. For xlsxwriter, use `write_number()`, `write_datetime()`, and `write_string()` explicitly.
6. **Apply cell formatting.** Define named styles or format objects for reuse: header style (bold, colored fill, border), currency style (number format, right-aligned), date style (ISO or locale format). Apply conditional formatting for highlighting: `ws.conditional_formatting.add('B2:B100', CellIsRule(operator='greaterThan', formula=['1000'], fill=greenFill))`.
7. **Insert formulas.** Add SUM, AVERAGE, VLOOKUP, or other formulas as string values: `ws['G2'] = '=SUM(D2:F2)'`. For xlsxwriter, use `write_formula()`. Test formulas by opening the output in Excel and verifying calculation results.
8. **Add charts.** For openpyxl: create chart objects (`BarChart()`, `LineChart()`, `PieChart()`), set data references with `Reference()`, configure titles, axis labels, and legend position, then add to the sheet. For xlsxwriter: use `wb.add_chart({'type': 'column'})` with `add_series()` for data ranges.
9. **Set print configuration.** Define print areas (`ws.print_area = 'A1:G50'`), page orientation (`ws.page_setup.orientation = 'landscape'`), fit-to-page settings, header/footer text, and repeat rows for headers across pages.
10. **Add data validation.** Apply dropdown lists (`DataValidation(type='list', formula1='"Yes,No,N/A"')`), numeric ranges, date constraints, and custom validation formulas to enforce data entry rules on editable cells.
11. **Optimize for large datasets.** For openpyxl, use `write_only=True` mode for workbooks over 50K rows. For xlsxwriter, enable `constant_memory` mode. Avoid storing the full dataset in memory — stream rows from the data source.
12. **Save and validate.** Save the workbook and verify: open in Excel to check formatting, formula calculations, chart rendering, and print preview. Test with minimum, typical, and maximum data volumes.

# Decision rules

- Default to openpyxl for workbooks under 50K rows or when template loading is needed.
- Default to xlsxwriter for write-only workbooks over 50K rows or when chart quality is critical.
- Always write numeric values as numbers, not strings — string numbers break formulas and sorting.
- Use named styles rather than inline formatting to maintain consistency and reduce code duplication.
- Set column widths based on the maximum expected data length plus padding — do not rely on auto-fit (it is not supported in openpyxl).
- Freeze the header row on every data sheet for usability.

# Output requirements

1. A working Python script or module that accepts structured data and produces a .xlsx file.
2. Separate concerns: data preparation, formatting definitions, and workbook assembly in distinct functions.
3. The output .xlsx must open without errors in Excel 2016+ and LibreOffice Calc.
4. All formula cells must calculate correctly when the file is opened (no circular references, no #REF errors).
5. Print configuration set so the workbook prints cleanly on A4/Letter without manual adjustment.

# References

- openpyxl documentation: https://openpyxl.readthedocs.io/
- xlsxwriter documentation: https://xlsxwriter.readthedocs.io/
- Excel number format codes: https://support.microsoft.com/en-us/office/number-format-codes-5026bbd6-04bc-48cd-bf33-80f18b4eae68
- OpenPyXL charts guide: https://openpyxl.readthedocs.io/en/stable/charts/introduction.html

# Related skills

- `csv-ready` — for simple flat-file output without formatting.
- `table-extraction` — for extracting data from documents to populate spreadsheets.
- `docx-generation` — for Word document output from similar data pipelines.

# Anti-patterns

- Writing all numbers as strings (`ws['A1'] = '42'` instead of `ws['A1'] = 42`) — breaks formulas, sorting, and charts.
- Creating a new Style object for every cell instead of defining reusable named styles — causes performance degradation and inconsistent formatting.
- Ignoring memory limits with openpyxl on large datasets — use `write_only` mode or switch to xlsxwriter.
- Hardcoding column letters (`ws['C2']`) instead of using row/column indices — breaks when columns are added or reordered.
- Embedding absolute file paths in formulas or hyperlinks — breaks when the file is moved or shared.

# Failure handling

- If the required library is not installed, output `pip install openpyxl` or `pip install xlsxwriter` and halt.
- If the template file is missing or corrupt, raise a clear error with the expected path and halt.
- If data types are mixed in a column (some strings, some numbers), coerce to the dominant type and log a warning with the affected row numbers.
- If the workbook exceeds 1 million rows (Excel limit), split into multiple sheets or files and log the split points.
- If a formula references a cell range that exceeds the data extent, adjust the range dynamically based on actual row count.
- If chart data is empty, skip chart creation and log a warning rather than producing a blank chart.
