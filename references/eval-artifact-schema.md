# Evaluation Artifact Schema

This document defines the standardized JSONL formats for evaluation artifacts used across all skills in the Meta Skill Engineering repository.

## Overview

All eval artifacts follow JSONL format (one JSON object per line) for streaming compatibility and easy line-based processing.

---

## 1. trigger-positive.jsonl

Defines test cases that **should** trigger the skill (positive routing tests).

### Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["id", "trigger", "expected_output"],
  "properties": {
    "id": {
      "type": "string",
      "description": "Unique test case identifier (UUID or sequential)"
    },
    "trigger": {
      "type": "string",
      "description": "User input that should trigger this skill"
    },
    "context": {
      "type": "string",
      "description": "Optional conversation context or background"
    },
    "expected_output": {
      "type": "object",
      "required": ["contains"],
      "properties": {
        "contains": {
          "type": "array",
          "items": {"type": "string"},
          "description": "Strings that must appear in the output"
        },
        "excludes": {
          "type": "array",
          "items": {"type": "string"},
          "description": "Strings that must NOT appear in the output"
        },
        "min_length": {
          "type": "integer",
          "description": "Minimum expected output length in characters"
        },
        "max_length": {
          "type": "integer",
          "description": "Maximum expected output length in characters"
        }
      }
    },
    "metadata": {
      "type": "object",
      "properties": {
        "difficulty": {
          "type": "string",
          "enum": ["easy", "medium", "hard"],
          "description": "Test case difficulty level"
        },
        "category": {
          "type": "string",
          "description": "Test case category or tag"
        },
        "created_at": {
          "type": "string",
          "format": "date-time",
          "description": "ISO 8601 timestamp"
        }
      }
    }
  }
}
```

### Example

```jsonl
{"id": "tp-001", "trigger": "Create a skill for PDF processing", "context": "", "expected_output": {"contains": ["PDF", "skill"], "excludes": ["error"]}, "metadata": {"difficulty": "easy", "category": "creation", "created_at": "2026-04-14T10:00:00Z"}}
{"id": "tp-002", "trigger": "How do I extract text from a PDF?", "context": "Working on document automation", "expected_output": {"contains": ["procedure", "PDF"]}, "metadata": {"difficulty": "medium", "category": "extraction", "created_at": "2026-04-14T10:00:00Z"}}
```

---

## 2. output-tests.jsonl

Defines tests for validating skill output quality and format.

### Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["id", "input", "assertions"],
  "properties": {
    "id": {
      "type": "string",
      "description": "Unique test case identifier"
    },
    "input": {
      "type": "string",
      "description": "Input text to process"
    },
    "assertions": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["type"],
        "properties": {
          "type": {
            "type": "string",
            "enum": ["contains", "excludes", "matches_regex", "valid_json", "valid_yaml", "file_exists", "exit_code"],
            "description": "Type of assertion to perform"
          },
          "target": {
            "type": "string",
            "description": "Target string, regex pattern, or file path"
          },
          "value": {
            "description": "Expected value for comparison"
          },
          "message": {
            "type": "string",
            "description": "Custom error message on failure"
          }
        }
      }
    },
    "setup": {
      "type": "object",
      "description": "Pre-test setup commands or file creation",
      "properties": {
        "commands": {
          "type": "array",
          "items": {"type": "string"}
        },
        "files": {
          "type": "object",
          "additionalProperties": {"type": "string"}
        }
      }
    },
    "teardown": {
      "type": "object",
      "description": "Post-test cleanup commands",
      "properties": {
        "commands": {
          "type": "array",
          "items": {"type": "string"}
        }
      }
    }
  }
}
```

### Example

```jsonl
{"id": "ot-001", "input": "Generate a JSON object with name and age", "assertions": [{"type": "valid_json", "message": "Output must be valid JSON"}, {"type": "contains", "target": "name", "message": "Output must contain 'name' field"}], "setup": {"commands": [], "files": {}}, "teardown": {"commands": []}}
{"id": "ot-002", "input": "Create file test.txt with content 'hello'", "assertions": [{"type": "file_exists", "target": "test.txt", "message": "File must be created"}, {"type": "exit_code", "value": 0, "message": "Command must succeed"}]}
```

---

## 3. baseline-cases.jsonl

Defines baseline test cases for regression testing and skill comparison.

### Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["id", "name", "inputs", "expected_behavior"],
  "properties": {
    "id": {
      "type": "string",
      "description": "Unique baseline case identifier"
    },
    "name": {
      "type": "string",
      "description": "Human-readable test name"
    },
    "description": {
      "type": "string",
      "description": "Detailed description of the test case"
    },
    "inputs": {
      "type": "array",
      "items": {"type": "string"},
      "description": "Multiple inputs to test consistency"
    },
    "expected_behavior": {
      "type": "object",
      "required": ["action"],
      "properties": {
        "action": {
          "type": "string",
          "enum": ["trigger", "pass_through", "error"],
          "description": "Expected skill action"
        },
        "min_quality_score": {
          "type": "integer",
          "minimum": 0,
          "maximum": 100,
          "description": "Minimum acceptable quality score"
        },
        "max_latency_ms": {
          "type": "integer",
          "description": "Maximum acceptable latency in milliseconds"
        }
      }
    },
    "tags": {
      "type": "array",
      "items": {"type": "string"},
      "description": "Tags for categorization"
    },
    "version": {
      "type": "string",
      "description": "Baseline version (semver)"
    }
  }
}
```

### Example

```jsonl
{"id": "bl-001", "name": "Basic PDF extraction", "description": "Tests basic PDF text extraction functionality", "inputs": ["Extract text from sample.pdf", "Get text content of PDF file"], "expected_behavior": {"action": "trigger", "min_quality_score": 70, "max_latency_ms": 5000}, "tags": ["pdf", "extraction", "basic"], "version": "1.0.0"}
{"id": "bl-002", "name": "Invalid file handling", "description": "Tests error handling for non-existent files", "inputs": ["Extract from nonexistent.pdf"], "expected_behavior": {"action": "trigger", "min_quality_score": 60}, "tags": ["pdf", "error_handling"], "version": "1.0.0"}
```

---

## 4. judge-evaluation.jsonl

Defines LLM judge evaluation criteria for output quality assessment.

### Schema

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["id", "criteria", "scoring"],
  "properties": {
    "id": {
      "type": "string",
      "description": "Evaluation criteria identifier"
    },
    "criteria": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["name", "description", "weight"],
        "properties": {
          "name": {
            "type": "string",
            "description": "Criterion name"
          },
          "description": {
            "type": "string",
            "description": "Detailed description for the judge"
          },
          "weight": {
            "type": "number",
            "minimum": 0,
            "maximum": 1,
            "description": "Weight in final score (0-1)"
          }
        }
      }
    },
    "scoring": {
      "type": "object",
      "required": ["min_score", "max_score"],
      "properties": {
        "min_score": {
          "type": "integer",
          "description": "Minimum possible score"
        },
        "max_score": {
          "type": "integer",
          "description": "Maximum possible score"
        },
        "passing_threshold": {
          "type": "integer",
          "description": "Score required to pass"
        }
      }
    },
    "judge_model": {
      "type": "string",
      "description": "Recommended judge model (e.g., 'gpt-4', 'claude-3')"
    }
  }
}
```

### Example

```jsonl
{"id": "judge-skill-quality", "criteria": [{"name": "accuracy", "description": "Output is factually correct and complete", "weight": 0.4}, {"name": "clarity", "description": "Output is clear and well-structured", "weight": 0.3}, {"name": "completeness", "description": "All requested information is provided", "weight": 0.3}], "scoring": {"min_score": 0, "max_score": 100, "passing_threshold": 70}, "judge_model": "gpt-4"}
```

---

## File Locations

Eval artifacts are stored within each skill package:

```
<skill-name>/
  └── evals/
      ├── trigger-positive.jsonl    # Positive routing tests
      ├── trigger-negative.jsonl    # Negative routing tests
      ├── output-tests.jsonl        # Output validation tests
      └── baseline-cases.jsonl      # Regression baseline
```

---

## Validation

Use the provided validation script to check eval artifact format:

```bash
python scripts/validate_evals.py <skill-name>
```

## Versioning

- Eval schemas are versioned independently of skills
- Breaking schema changes require migration of existing eval files
- Schema version is tracked in the eval file metadata