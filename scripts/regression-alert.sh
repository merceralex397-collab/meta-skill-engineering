#!/bin/bash
# Regression alert - compare current state to baseline and alert on degradations
# Run this after test suites to detect quality regressions

set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

BASELINE_DIR="$REPO_ROOT/tasks/baselines"
REPORT_DIR="$REPO_ROOT/tasks/reports"
mkdir -p "$BASELINE_DIR" "$REPORT_DIR"

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
ALERT_FILE="$REPORT_DIR/regression-alert-$TIMESTAMP.md"

echo "# Regression Alert Report" > "$ALERT_FILE"
echo "Date: $(date)" >> "$ALERT_FILE"
echo "" >> "$ALERT_FILE"

REGRESSIONS=0
WARNINGS=0

# Function to check if value is below baseline
check_regression() {
    local metric="$1"
    local current="$2"
    local baseline="$3"
    local threshold="${4:-0}"  # Allowable difference
    
    if (( $(echo "$current < $baseline - $threshold" | bc -l) )); then
        echo "- 🔴 REGRESSION: $metric degraded from $baseline to $current" >> "$ALERT_FILE"
        REGRESSIONS=$((REGRESSIONS + 1))
        return 1
    elif (( $(echo "$current < $baseline" | bc -l) )); then
        echo "- 🟡 WARNING: $metric slightly degraded from $baseline to $current" >> "$ALERT_FILE"
        WARNINGS=$((WARNINGS + 1))
        return 0
    fi
    return 0
}

echo "=========================================="
echo "Regression Alert Check"
echo "=========================================="
echo ""

# Check if baseline exists
if [ ! -f "$BASELINE_DIR/eval-baseline.json" ]; then
    echo "⚠ No baseline found. Creating baseline from current state..."
    echo "Run with --create-baseline to establish initial baseline."
    exit 0
fi

# Load baselines
BASELINE=$(cat "$BASELINE_DIR/eval-baseline.json" 2>/dev/null || echo "{}")

# Current metrics
echo "Collecting current metrics..."

# 1. Skill validation pass rate
echo "1. Checking skill validation..."
if ./scripts/validate-skills.sh > /tmp/validate.txt 2>&1; then
    CURRENT_VALIDATION=100
else
    CURRENT_VALIDATION=0
fi
BASELINE_VALIDATION=$(echo "$BASELINE" | python3 -c "import sys,json; print(json.load(sys.stdin).get('validation_pass', 100))" 2>/dev/null || echo "100")
check_regression "Skill Validation" "$CURRENT_VALIDATION" "$BASELINE_VALIDATION"
echo ""

# 2. Eval file syntax check
echo "2. Checking eval syntax health..."
EVAL_ERRORS=0
TOTAL_EVALS=0
for jsonl in $(find . -name "*.jsonl" -path "*/evals/*" 2>/dev/null | head -50); do
    TOTAL_EVALS=$((TOTAL_EVALS + 1))
    if ! python3 -c "import json; [json.loads(l) for l in open('$jsonl')]" 2>/dev/null; then
        EVAL_ERRORS=$((EVAL_ERRORS + 1))
    fi
done

if [ $TOTAL_EVALS -gt 0 ]; then
    CURRENT_EVAL_HEALTH=$((100 - (EVAL_ERRORS * 100 / TOTAL_EVALS)))
    BASELINE_EVAL=$(echo "$BASELINE" | python3 -c "import sys,json; print(json.load(sys.stdin).get('eval_health', 100))" 2>/dev/null || echo "100")
    check_regression "Eval File Health" "$CURRENT_EVAL_HEALTH" "$BASELINE_EVAL" 5
fi
echo ""

# 3. Check for new skills without evals
echo "3. Checking for skills missing evals..."
SKILLS_WITHOUT_EVALS=0
for skill_dir in $(ls -d skill-* 2>/dev/null); do
    if [ ! -d "$skill_dir/evals" ]; then
        SKILLS_WITHOUT_EVALS=$((SKILLS_WITHOUT_EVALS + 1))
    fi
done

if [ $SKILLS_WITHOUT_EVALS -gt 0 ]; then
    echo "- 🟡 WARNING: $SKILLS_WITHOUT_EVALS skill(s) without eval directories" >> "$ALERT_FILE"
    WARNINGS=$((WARNINGS + 1))
fi
echo ""

# 4. Check manifest completeness
echo "4. Checking manifest coverage..."
SKILLS_WITHOUT_MANIFEST=0
for skill_dir in $(ls -d skill-* 2>/dev/null); do
    if [ ! -f "$skill_dir/manifest.yaml" ]; then
        SKILLS_WITHOUT_MANIFEST=$((SKILLS_WITHOUT_MANIFEST + 1))
    fi
done

if [ $SKILLS_WITHOUT_MANIFEST -gt 0 ]; then
    echo "- 🟡 WARNING: $SKILLS_WITHOUT_MANIFEST skill(s) without manifest.yaml" >> "$ALERT_FILE"
    WARNINGS=$((WARNINGS + 1))
fi
echo ""

# 5. Run count trend (if runs directory exists)
if [ -d "$HOME/.meta-skill-studio/runs" ]; then
    echo "5. Analyzing run activity..."
    RECENT_RUNS=$(find "$HOME/.meta-skill-studio/runs" -name "*.json" -mtime -7 2>/dev/null | wc -l)
    echo "   Recent runs (last 7 days): $RECENT_RUNS"
    echo "- Recent activity: $RECENT_RUNS runs in last 7 days" >> "$ALERT_FILE"
    if [ $RECENT_RUNS -eq 0 ]; then
        echo "- 🟡 WARNING: No runs in last 7 days" >> "$ALERT_FILE"
        WARNINGS=$((WARNINGS + 1))
    fi
    echo ""
fi

# Summary
echo "## Summary" >> "$ALERT_FILE"
echo "- Regressions: $REGRESSIONS" >> "$ALERT_FILE"
echo "- Warnings: $WARNINGS" >> "$ALERT_FILE"
echo "" >> "$ALERT_FILE"

echo "=========================================="
echo "Regression Check Complete"
echo "=========================================="
echo "Regressions: $REGRESSIONS"
echo "Warnings: $WARNINGS"
echo "Report: $ALERT_FILE"
echo ""

if [ $REGRESSIONS -gt 0 ]; then
    echo "🔴 Regressions detected! Review required."
    exit 1
elif [ $WARNINGS -gt 0 ]; then
    echo "🟡 Warnings present. Review recommended."
    exit 0
else
    echo "✓ No regressions detected."
    exit 0
fi
