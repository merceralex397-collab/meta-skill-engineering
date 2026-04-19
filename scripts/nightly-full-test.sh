#!/bin/bash
# Nightly full test suite for Meta-Skill-Engineering
# Run this daily or before major releases

set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

REPORT_DIR="$REPO_ROOT/tasks/reports"
mkdir -p "$REPORT_DIR"

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
REPORT_FILE="$REPORT_DIR/nightly-test-$TIMESTAMP.md"

echo "# Nightly Test Report" > "$REPORT_FILE"
echo "Date: $(date)" >> "$REPORT_FILE"
echo "" >> "$REPORT_FILE"

echo "=========================================="
echo "Nightly Full Test Suite"
echo "=========================================="
echo "Report: $REPORT_FILE"
echo ""

TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

run_test() {
    local name="$1"
    local cmd="$2"
    
    echo "Running: $name"
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    
    if eval "$cmd" > /tmp/test-output.txt 2>&1; then
        echo -e "  ✓ PASSED"
        PASSED_TESTS=$((PASSED_TESTS + 1))
        echo "- $name: PASSED" >> "$REPORT_FILE"
        return 0
    else
        echo -e "  ✗ FAILED"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        echo "- $name: FAILED" >> "$REPORT_FILE"
        echo '```' >> "$REPORT_FILE"
        cat /tmp/test-output.txt >> "$REPORT_FILE"
        echo '```' >> "$REPORT_FILE"
        return 1
    fi
}

# 1. Skill validation
echo "1. Validating all skill packages..."
run_test "Skill Validation" "./scripts/validate-skills.sh"
echo ""

# 2. Eval file syntax check
echo "2. Checking all eval file syntax..."
for jsonl in $(find . -name "*.jsonl" -path "*/evals/*"); do
    run_test "Eval: $(basename $jsonl)" "python3 -c 'import json; [json.loads(l) for l in open(\"$jsonl\")]'"
done
echo ""

# 3. Dry-run evals (fast check)
echo "3. Running eval dry-runs..."
if [ -f "./scripts/run-evals.sh" ]; then
    for skill in skill-creator skill-improver skill-testing-harness; do
        if [ -f "$skill/evals/trigger-positive.jsonl" ]; then
            run_test "Dry-run: $skill" "./scripts/run-evals.sh --dry-run $skill"
        fi
    done
fi
echo ""

# 4. WPF build test
echo "4. Testing WPF build..."
if [ -d "windows-wpf" ] && command -v dotnet &> /dev/null; then
    cd windows-wpf
    run_test "WPF Restore" "dotnet restore"
    run_test "WPF Build" "dotnet build --no-restore"
    run_test "WPF Publish" "dotnet publish MetaSkillStudio -c Release -r win-x64 --self-contained -p:PublishSingleFile=true --no-build"
    cd ..
else
    echo "  ⚠ Skipping WPF tests (dotnet not available)"
fi
echo ""

# 5. Python syntax check
echo "5. Checking Python script syntax..."
for py in $(find . -name "*.py" | grep -v __pycache__); do
    run_test "Python: $(basename $py)" "python3 -m py_compile '$py'"
done
echo ""

# 6. Check for stale TODOs
echo "6. Checking for stale TODO markers (>30 days old)..."
# This is a heuristic - would need git blame integration for accurate age
TODO_COUNT=$(grep -r "TODO\|FIXME" --include="*.py" --include="*.cs" --include="*.sh" . 2>/dev/null | wc -l)
echo "  Found $TODO_COUNT TODO/FIXME markers"
echo "- TODO/FIXME count: $TODO_COUNT" >> "$REPORT_FILE"
echo ""

# Summary
echo "=========================================="
echo "Test Summary"
echo "=========================================="
echo "Total: $TOTAL_TESTS"
echo "Passed: $PASSED_TESTS"
echo "Failed: $FAILED_TESTS"
echo ""

echo "## Summary" >> "$REPORT_FILE"
echo "- Total: $TOTAL_TESTS" >> "$REPORT_FILE"
echo "- Passed: $PASSED_TESTS" >> "$REPORT_FILE"
echo "- Failed: $FAILED_TESTS" >> "$REPORT_FILE"
echo "- Pass Rate: $(($PASSED_TESTS * 100 / $TOTAL_TESTS))%" >> "$REPORT_FILE"
echo "" >> "$REPORT_FILE"

if [ $FAILED_TESTS -eq 0 ]; then
    echo "✓ All tests passed!"
    exit 0
else
    echo "✗ $FAILED_TESTS test(s) failed"
    exit 1
fi
