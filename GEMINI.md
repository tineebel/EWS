# EWS Antigravity Instructions

Antigravity agents working in this repository must follow the shared project rules in:

- `AGENTS.md`
- `CLAUDE.md`
- `docs/EWS_UI_System_Instruction.md`

Use `AGENTS.md` as the main cross-tool instruction file. Use `CLAUDE.md` for core project architecture and safety rules. Use `docs/EWS_UI_System_Instruction.md` for all frontend/UI and design-system work.

For frontend work in `src/EWS.Web`, always use React 18, TypeScript, Ant Design v5, React Query, Axios, and token-driven styling. Do not hard-code colors, spacing, typography, border radius, or shadows.

For backend work, preserve the existing Clean Architecture layers:

```txt
src/EWS.Domain
src/EWS.Application
src/EWS.Infrastructure
src/EWS.API
```

Never revert unrelated user changes. Keep edits scoped, preserve auditability, and follow existing project patterns.

## Required Work Cycle

For non-trivial changes, follow this sequence before handing work back:

1. Ask when the request is ambiguous or risky.
2. Plan the smallest safe implementation path.
3. Implement scoped changes using existing project patterns.
4. Review Diff for unintended edits, secrets, formatting issues, and missing tests.
5. Run/Test the relevant build, tests, or smoke checks.
6. Commit only after review and verification, and only when the user has asked for or approved a commit.

Never skip Review Diff before Commit.
