# Reference Extraction Guide

When a SKILL.md exceeds 500 lines or 10KB, or contains large code blocks/schemas not needed every invocation, extract reference material into a `references/` directory.

## When to extract

- SKILL.md exceeds 500 lines or 10 KB
- Contains code examples, schemas, or lookup tables over ~20 lines not needed every invocation
- Multiple skills could share the same reference material
- User says "this is too long", "extract references", "slim down"

## When NOT to extract

- Skill is already under 200 lines — use Mode 1 (Surgical edit) for general tightening
- Candidate material is core procedure, not reference
- Total extractable content is under 50 lines — indirection cost exceeds benefit
- Extraction would break the skill's procedural flow

## Classification rules

**Reference material → extract:**
- Lookup tables, enum listings, API schema dumps
- Example collections (>3 examples demonstrating the same pattern)
- Configuration templates and boilerplate
- Format specifications the agent consults only for specific sub-cases

**Core procedure → keep inline:**
- Decision logic and conditional branches
- Ordered steps the agent must follow every time
- Output format definitions (the contract, not examples of it)
- Failure handling tables

**Heuristic:** If the content is consulted on every invocation → keep inline. If consulted only for specific sub-cases → extract.

**Size rule:** Any individual reference block >20 lines should be extracted unless it IS the skill's primary procedure. A 40-line lookup table is an extraction candidate; a 40-line decision tree is not.

## Extraction procedure

1. **Identify candidates** — scan for blocks that are reference, not procedure:
   - Code examples > 20 lines
   - Schema definitions or lookup tables
   - API documentation excerpts
   - Configuration templates
   - Extended case studies
   - Example collections with >3 examples of the same pattern

2. **Create `references/` directory** with descriptive filenames:
   ```
   skill-name/
   ├── SKILL.md
   └── references/
       ├── README.md          ← index table
       ├── schema.json
       └── examples.md
   ```

3. **Extract** — move each block, preserve formatting, name files by content not sequence.

4. **Add inline pointers** in SKILL.md — replace each extracted block with a one-line summary and a path reference.

5. **Write `references/README.md`** — a table mapping each file to its contents and when an agent should read it.

6. **Verify**:
   - SKILL.md is understandable without reading any reference file
   - All procedure steps remain inline
   - Every reference file is signposted from SKILL.md
   - **Reference link quality check:** For every line that says "see references/X.md", verify it tells the agent (a) WHEN to consult it and (b) WHAT to look for. Bad: `Full schema: see references/schema.json`. Good: `When validating output format, consult the full JSON schema: references/schema.json (defines required fields, types, and nesting for the trigger-test JSONL format).`

## Extraction output template

```
## Reference Extraction: [skill-name]

### Extracted
| Block | Original location | Destination | Size |
|-------|-------------------|-------------|------|

### Reduction
- Before: [lines], [KB]
- After: [lines], [KB]
- Reduction: [%]

### Verification
- [ ] Procedure intact
- [ ] All references signposted
- [ ] references/README.md created
```

## Failure handling

- **Cannot decide whether material is procedure or reference** → keep inline; false-negative extraction is safer than breaking the skill
- **Extraction would fragment a coherent procedure section** → leave inline, note in summary
- **Circular cross-references between extracted files** → flatten into a single reference file
- **Two skills want the same reference** → create a skill-specific copy; shared references are fragile
