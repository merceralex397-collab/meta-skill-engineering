from __future__ import annotations

from dataclasses import asdict, dataclass
from typing import Dict, List, Mapping, Tuple


@dataclass(frozen=True)
class ActionSpec:
    name: str
    category: str
    summary: str
    required_args: Tuple[str, ...] = ()
    optional_args: Tuple[str, ...] = ()
    output_kind: str = "run-artifact"
    requires_role_config: bool = False
    aliases: Tuple[str, ...] = ()

    def as_dict(self) -> Dict[str, object]:
        payload = asdict(self)
        payload["required_args"] = list(self.required_args)
        payload["optional_args"] = list(self.optional_args)
        payload["aliases"] = list(self.aliases)
        return payload


ACTION_SPECS: Tuple[ActionSpec, ...] = (
    ActionSpec(
        name="create",
        category="authoring",
        summary="Create a new skill package in a target library tier.",
        required_args=("brief",),
        optional_args=("library",),
        requires_role_config=True,
    ),
    ActionSpec(
        name="improve",
        category="authoring",
        summary="Improve an existing skill package against a concrete goal.",
        required_args=("skill", "goal"),
        requires_role_config=True,
    ),
    ActionSpec(
        name="evaluate-skill",
        category="evaluation",
        summary="Run validation, evals, and a structured judge summary for one skill or all skills.",
        optional_args=("skill",),
        requires_role_config=True,
        aliases=("test",),
    ),
    ActionSpec(
        name="benchmark-skill",
        category="evaluation",
        summary="Generate benchmark fixtures for a skill.",
        required_args=("skill", "goal"),
        optional_args=("cases",),
        requires_role_config=True,
        aliases=("benchmarks",),
    ),
    ActionSpec(
        name="validate-skills",
        category="evaluation",
        summary="Run the structural validator across repo-owned root skill packages.",
    ),
    ActionSpec(
        name="run-evals",
        category="evaluation",
        summary="Invoke the JSONL eval runner for one skill or all skills.",
        optional_args=("skill",),
    ),
    ActionSpec(
        name="compare-runs",
        category="evaluation",
        summary="Compare two Studio run artifacts and report quality and execution deltas.",
        required_args=("before-run", "after-run"),
    ),
    ActionSpec(
        name="improvement-brief",
        category="evaluation",
        summary="Extract a prioritized improvement brief from a Studio run artifact.",
        required_args=("run-file",),
    ),
    ActionSpec(
        name="meta-manage",
        category="library",
        summary="Run a general meta-management workflow across the repository.",
        required_args=("goal",),
        requires_role_config=True,
    ),
    ActionSpec(
        name="catalog-audit",
        category="library",
        summary="Audit library/workbench organization while keeping the 17 root skills distinct.",
        optional_args=("goal",),
        requires_role_config=True,
    ),
    ActionSpec(
        name="find-skills",
        category="library",
        summary="Search for external skills relevant to a topic.",
        required_args=("goal",),
        requires_role_config=True,
    ),
    ActionSpec(
        name="import-skill",
        category="library",
        summary="Import a local folder or GitHub-hosted skill into a library tier/category.",
        required_args=("source",),
        optional_args=("library", "category"),
    ),
    ActionSpec(
        name="promote-skill",
        category="library",
        summary="Promote a skill upward between library tiers.",
        required_args=("skill", "category", "from-library"),
    ),
    ActionSpec(
        name="demote-skill",
        category="library",
        summary="Demote a skill downward between library tiers.",
        required_args=("skill", "category", "from-library"),
    ),
    ActionSpec(
        name="move-skill",
        category="library",
        summary="Move a skill to another category within the same library tier.",
        required_args=("skill", "category", "to-category", "library"),
    ),
    ActionSpec(
        name="safety-review",
        category="governance",
        summary="Run a skill-safety-review style audit for a target skill.",
        required_args=("skill",),
        optional_args=("goal",),
        requires_role_config=True,
    ),
    ActionSpec(
        name="provenance-review",
        category="governance",
        summary="Run provenance and trust review for a target skill.",
        required_args=("skill",),
        optional_args=("goal",),
        requires_role_config=True,
    ),
    ActionSpec(
        name="package-skill",
        category="distribution",
        summary="Prepare packaging work for a target skill.",
        required_args=("skill",),
        optional_args=("destination", "goal"),
        requires_role_config=True,
    ),
    ActionSpec(
        name="install-skill",
        category="distribution",
        summary="Prepare or execute installation work for a target skill.",
        required_args=("skill",),
        optional_args=("destination", "goal"),
        requires_role_config=True,
    ),
    ActionSpec(
        name="lifecycle-review",
        category="distribution",
        summary="Review lifecycle state and next-step governance for a target skill.",
        required_args=("skill",),
        optional_args=("goal",),
        requires_role_config=True,
    ),
    ActionSpec(
        name="run-pipeline",
        category="orchestration",
        summary="Create and execute a documented orchestrator pipeline.",
        required_args=("pipeline",),
        optional_args=("skill", "brief"),
        requires_role_config=True,
    ),
    ActionSpec(
        name="resume-pipeline",
        category="orchestration",
        summary="Resume an existing documented orchestrator pipeline by run id.",
        required_args=("run-id",),
        requires_role_config=True,
    ),
    ActionSpec(
        name="list-actions",
        category="introspection",
        summary="List the supported CLI actions and their contracts.",
        output_kind="json-query",
    ),
    ActionSpec(
        name="list-skills",
        category="introspection",
        summary="List repo-owned root skills or skills in a selected library tier.",
        optional_args=("library",),
        output_kind="json-query",
    ),
    ActionSpec(
        name="list-runs",
        category="introspection",
        summary="List recent Studio run artifacts.",
        output_kind="json-query",
    ),
    ActionSpec(
        name="show-run",
        category="introspection",
        summary="Print one Studio run artifact.",
        required_args=("run-file",),
        output_kind="json-query",
    ),
    ActionSpec(
        name="list-models",
        category="runtime",
        summary="List models visible through the configured OpenCode runtime.",
        output_kind="json-query",
    ),
    ActionSpec(
        name="list-providers",
        category="runtime",
        summary="List OpenCode auth providers and observed login state.",
        output_kind="json-query",
    ),
    ActionSpec(
        name="auth-provider",
        category="runtime",
        summary="Log in or out of a specific OpenCode provider.",
        required_args=("provider",),
        optional_args=("logout",),
        output_kind="json-query",
    ),
    ActionSpec(
        name="opencode-stats",
        category="runtime",
        summary="Return OpenCode runtime stats when available.",
        output_kind="json-query",
    ),
)

ACTION_SPEC_BY_NAME: Mapping[str, ActionSpec] = {spec.name: spec for spec in ACTION_SPECS}
ACTION_ALIASES: Mapping[str, str] = {
    alias: spec.name for spec in ACTION_SPECS for alias in spec.aliases
}
ALL_ACTION_NAMES: Tuple[str, ...] = tuple(
    [spec.name for spec in ACTION_SPECS] + list(ACTION_ALIASES.keys())
)

PIPELINE_CHOICES: Tuple[str, ...] = ("creation", "improvement", "library-management")

REQUIRED_PLATFORM_DOCS: Tuple[str, ...] = (
    "docs/cli/feature-inventory.md",
    "docs/cli/action-contract.md",
    "docs/evaluation/plugin-eval-disposition.md",
    "docs/architecture/surface-authority.md",
)


def resolve_action_name(action_name: str | None) -> str | None:
    if action_name is None:
        return None
    return ACTION_ALIASES.get(action_name, action_name)


def action_requires_role_config(action_name: str | None) -> bool:
    canonical = resolve_action_name(action_name)
    if canonical is None:
        return False
    spec = ACTION_SPEC_BY_NAME.get(canonical)
    return bool(spec and spec.requires_role_config)


def list_action_metadata() -> List[Dict[str, object]]:
    return [spec.as_dict() for spec in ACTION_SPECS]
