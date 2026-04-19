import { joinSession } from "@github/copilot-sdk/extension";
import { execFile } from "node:child_process";
import { dirname, resolve } from "node:path";

const repoRoot = resolve(process.cwd());

function run(cmd, args) {
    return new Promise((resolve, reject) => {
        execFile(cmd, args, { cwd: repoRoot, timeout: 30000 }, (err, stdout, stderr) => {
            if (err) {
                resolve(`Exit code: ${err.code}\n\n${stdout}\n${stderr}`.trim());
            } else {
                resolve(stdout.trim() || "(no output)");
            }
        });
    });
}

const session = await joinSession({
    tools: [
        {
            name: "mse_validate_skill",
            description:
                "Validate a single skill package for structural compliance using the 10-point check_skill_structure.py scorer. " +
                "Returns a score and list of findings. Use after editing a SKILL.md to confirm the skill still passes.",
            parameters: {
                type: "object",
                properties: {
                    skill_dir: {
                        type: "string",
                        description:
                            "Path to the skill directory (e.g. 'skill-creator' or 'skill-improver'). Relative to repo root.",
                    },
                },
                required: ["skill_dir"],
            },
            handler: async (args) => {
                const skillDir = resolve(repoRoot, args.skill_dir);
                const skillMd = resolve(skillDir, "SKILL.md");
                return await run("python3", [
                    resolve(repoRoot, "scripts/check_skill_structure.py"),
                    skillMd,
                    "--skill-dir",
                    skillDir,
                    "--pretty",
                ]);
            },
        },
        {
            name: "mse_validate_all",
            description:
                "Run validate-skills.sh to check all 12 active skill packages for structural compliance. " +
                "Returns a summary with error/warning counts. Use for repo-wide compliance checks.",
            parameters: {
                type: "object",
                properties: {},
            },
            handler: async () => {
                return await run("bash", [
                    resolve(repoRoot, "scripts/validate-skills.sh"),
                ]);
            },
        },
        {
            name: "mse_lint_skill",
            description:
                "Lint a SKILL.md file for format issues (missing sections, wrong headings, " +
                "frontmatter problems). Returns a list of issues found.",
            parameters: {
                type: "object",
                properties: {
                    skill_md_path: {
                        type: "string",
                        description:
                            "Path to the SKILL.md file (e.g. 'skill-creator/SKILL.md'). Relative to repo root.",
                    },
                },
                required: ["skill_md_path"],
            },
            handler: async (args) => {
                const filePath = resolve(repoRoot, args.skill_md_path);
                // skill_lint.py expects the skill directory, not the SKILL.md file
                const skillDir = dirname(filePath);
                return await run("python3", [
                    resolve(repoRoot, "scripts/skill_lint.py"),
                    skillDir,
                ]);
            },
        },
        {
            name: "mse_check_preservation",
            description:
                "Check content preservation between an original and modified skill file. " +
                "Uses Jaccard similarity to verify purpose, constraints, references, and boundaries " +
                "are preserved. Use when reviewing skill modifications to ensure nothing important was lost.",
            parameters: {
                type: "object",
                properties: {
                    original: {
                        type: "string",
                        description:
                            "Path to the original SKILL.md (before changes). Relative to repo root.",
                    },
                    modified: {
                        type: "string",
                        description:
                            "Path to the modified SKILL.md (after changes). Relative to repo root.",
                    },
                },
                required: ["original", "modified"],
            },
            handler: async (args) => {
                const origPath = resolve(repoRoot, args.original);
                const modPath = resolve(repoRoot, args.modified);
                return await run("python3", [
                    resolve(repoRoot, "scripts/check_preservation.py"),
                    origPath,
                    modPath,
                ]);
            },
        },
    ],

    hooks: {
        onPostToolUse: async (input) => {
            if (
                (input.toolName === "edit" || input.toolName === "create") &&
                typeof input.toolArgs?.path === "string" &&
                input.toolArgs.path.endsWith("SKILL.md")
            ) {
                const filePath = input.toolArgs.path;
                const skillDir = dirname(filePath);

                try {
                    const result = await run("python3", [
                        resolve(repoRoot, "scripts/check_skill_structure.py"),
                        filePath,
                        "--skill-dir",
                        skillDir,
                    ]);
                    return {
                        additionalContext: `[meta-skill-tools] Auto-validation of ${skillDir}:\n${result}`,
                    };
                } catch {
                    // Validation failed to run — don't block the agent
                }
            }
        },
    },
});

await session.log("meta-skill-tools extension loaded — 4 tools + auto-validation hook active");
