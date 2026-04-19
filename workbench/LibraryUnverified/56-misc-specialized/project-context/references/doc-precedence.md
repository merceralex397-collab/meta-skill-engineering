# Doc Precedence

Use this order unless the repo says otherwise.

1. explicit project instruction files such as `AGENTS.md`, `CLAUDE.md`, `START-HERE.md`
2. task-specific specs, ticket packs, decision docs, active implementation briefs
3. root `README.md` and subsystem `README.md`
4. config files and validation scripts that reveal actual toolchain expectations
5. archive docs, postmortems, brainstorm notes

When two sources disagree, prefer the source that is both closer to the code being changed and more clearly normative.
