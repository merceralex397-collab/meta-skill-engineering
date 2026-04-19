---
name: document-to-structured-data
description: "Extract structured data from unstructured documents (PDF, DOCX, HTML) — detect tables, extract named entities, map content to target schemas, and validate extraction quality. Use when converting documents to JSON/CSV, building document processing pipelines, or extracting specific fields from semi-structured files. Do not use for generating documents (prefer document-writing) or plain text summarization."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: document-to-structured-data
  maturity: draft
  risk: low
  tags: [document-parsing, extraction, entity-extraction, schema-mapping]
---

# Purpose

Extract structured, machine-readable data from unstructured or semi-structured documents including PDFs, DOCX files, HTML pages, and plain text. This skill covers table detection, named entity recognition, field extraction, schema mapping, and extraction quality validation to produce clean JSON, CSV, or database-ready records.

# When to use this skill

- A PDF, DOCX, or HTML document needs to be converted into JSON or CSV records
- Specific fields (dates, names, amounts, addresses) must be pulled from narrative text
- Tables embedded in documents need detection and extraction into structured formats
- A document processing pipeline requires schema mapping from raw content to a target data model
- Semi-structured files (invoices, resumes, contracts) need field-level extraction
- Extraction quality must be measured with confidence scores or validation rules

# Do not use this skill when

- The task is generating or writing documents from structured data — prefer `document-writing`
- The goal is plain text summarization without structured field extraction
- The input is a scanned/image-only PDF requiring OCR — prefer `image-heavy-pdfs` first, then pipe OCR output here
- The task only involves extracting tables from a known table-heavy source — prefer `table-extraction` for simpler cases
- The output format must be Excel with formatting — prefer `xlsx-generation` after extraction

# Operating procedure

1. **Classify the input document** — determine the file format (PDF, DOCX, HTML, TXT), identify whether it contains text layers, embedded tables, images, or mixed content. Check encoding and language.
2. **Define the target schema** — specify the exact fields, types, and structure expected in the output. Create a JSON Schema or a typed dictionary defining required fields, optional fields, and their data types.
3. **Select the extraction toolchain** — for PDFs use pdfplumber or PyMuPDF; for DOCX use python-docx; for HTML use BeautifulSoup or lxml. For entity extraction, use spaCy, regex patterns, or LLM-based extraction depending on complexity.
4. **Extract raw text regions** — parse the document into logical sections (headings, paragraphs, tables, lists). Preserve reading order and spatial relationships. Tag each region with its type.
5. **Detect and extract tables** — identify tabular structures using layout analysis (column alignment, grid lines, repeating patterns). Extract each table into a list of row dictionaries with column headers normalized to snake_case.
6. **Extract named entities** — run entity extraction on narrative sections to identify dates, monetary amounts, person names, organization names, addresses, and custom domain entities. Assign confidence scores to each extraction.
7. **Map extracted fields to target schema** — match raw extracted fields to target schema fields using exact name matching first, then fuzzy matching, then positional heuristics. Log every mapping decision.
8. **Apply validation rules** — check that required fields are present, date formats are parseable, numeric fields contain valid numbers, and enum fields match allowed values. Flag violations as extraction errors.
9. **Handle multi-page and multi-section documents** — maintain context across pages. Merge split tables that span page breaks. Link headers to their body content.
10. **Generate output** — produce the structured output in the requested format (JSON, CSV, or database INSERT statements). Include a metadata section with source filename, extraction timestamp, page count, and per-field confidence scores.
11. **Produce an extraction quality report** — calculate completeness (% of required fields populated), confidence distribution, and flag any fields that fell below the confidence threshold.

# Decision rules

- If the document contains both text and images, extract text layers first and only invoke OCR (via `image-heavy-pdfs`) for image-only regions.
- If a field cannot be extracted with >70% confidence, include it in the output with a `low_confidence` flag rather than omitting it silently.
- If the target schema has required fields that are completely missing from the source document, report them as `missing` in the quality report rather than fabricating values.
- Prefer deterministic regex/pattern extraction for well-formatted fields (dates, emails, phone numbers) over LLM-based extraction.
- If a table spans multiple pages, merge it into a single table before extracting rows.
- When multiple candidate values exist for a single field, include all candidates ranked by confidence.

# Output requirements

1. **Structured data file** — JSON or CSV matching the target schema with one record per logical entity
2. **Extraction metadata** — source filename, page count, extraction timestamp, toolchain used
3. **Field-level confidence scores** — numeric confidence (0.0–1.0) for each extracted field
4. **Quality report** — completeness percentage, count of low-confidence fields, list of missing required fields
5. **Error log** — rows or fields that failed validation, with source location (page number, line number)

# References

- pdfplumber documentation — https://github.com/jsvine/pdfplumber
- python-docx documentation — https://python-docx.readthedocs.io/
- spaCy NER pipeline — https://spacy.io/usage/linguistic-features#named-entities
- JSON Schema specification — https://json-schema.org/

# Related skills

- `pdf-extraction` — for PDF-specific text and table extraction with layout awareness
- `table-extraction` — for focused table detection and extraction from any document type
- `csv-ready` — for formatting extracted data as well-formed CSV output
- `image-heavy-pdfs` — for OCR preprocessing of scanned documents before structured extraction

# Anti-patterns

- **Extracting without a target schema** — produces unstructured key-value dumps that are unusable downstream. Always define the schema first.
- **Treating all PDFs as text-native** — scanned PDFs return empty text; detect this and redirect to OCR.
- **Ignoring table detection** — extracting table content as plain text loses column relationships.
- **Hardcoding page positions** — field positions shift between document versions; use content-based anchors (nearby labels) not coordinates.
- **Silently dropping low-confidence extractions** — downstream consumers cannot distinguish "not found" from "not attempted."

# Failure handling

- If the document format is unrecognized, attempt plain-text extraction as a fallback and log the format detection failure.
- If table detection produces zero tables in a document known to contain them, fall back to coordinate-based grid detection and retry with stricter parameters.
- If entity extraction confidence is below 50% for a majority of fields, flag the document for manual review and output partial results with explicit `needs_review` markers.
- If the document is password-protected, report the error immediately and do not attempt brute-force decryption.
- If extraction toolchain dependencies are missing, list the required packages with install commands and halt.
