# EWS Agent Instructions

This repository is EWS (Enterprise Workflow System), an enterprise workflow and approval-routing platform.

All AI coding agents working in this repository must follow the project instructions below before making changes.

## Required Instruction Files

Read and follow these files as the source of truth:

- `CLAUDE.md` - Core project architecture, backend rules, workflow logic, safety rules, and command guidance.
- `docs/EWS_UI_System_Instruction.md` - Master UI system instruction for all frontend and design-system work.
- `docs/EWS_Frontend_Patterns.md` - Frontend API workflow patterns and page behavior.
- `docs/EWS_API_Playbook.md` - API behavior, endpoints, and usage details.

If instructions conflict, use this priority order:

1. User's latest explicit request.
2. `AGENTS.md` and tool-specific root instruction files.
3. `CLAUDE.md`.
4. `docs/EWS_UI_System_Instruction.md` for frontend/UI work.
5. Other docs in `docs/`.

## Project Stack

- Backend: ASP.NET Core 8 Web API
- Architecture: Domain / Application / Infrastructure / API
- Frontend: React 18, Vite, TypeScript
- UI: Ant Design v5, `@ant-design/icons`
- Data fetching: TanStack React Query
- HTTP client: Axios
- Database: SQL Server via Entity Framework Core

## Backend Rules

For backend work, follow the Clean Architecture structure already used in the repository:

```txt
src/
  EWS.Domain/
  EWS.Application/
  EWS.Infrastructure/
  EWS.API/
```

Rules:

- Keep domain entities and enums in `EWS.Domain`.
- Keep use cases, commands, queries, validators, and interfaces in `EWS.Application`.
- Keep EF Core, persistence, migrations, and infrastructure services in `EWS.Infrastructure`.
- Keep HTTP controllers, middleware, Swagger/API setup, and API entrypoint code in `EWS.API`.
- Use async/await for I/O.
- Preserve JSend response conventions.
- Preserve pagination conventions: `page`, `pageSize`, `totalRows`, `totalPage`.
- Never hard-delete employee, position, workflow, or audit data unless explicitly requested.
- Preserve auditability. Workflow history and audit logs must remain insert-only.

## Frontend UI Rules

For all frontend work in `src/EWS.Web`, follow:

- `docs/EWS_UI_System_Instruction.md`
- `docs/EWS_Frontend_Patterns.md`

Rules:

- Use React 18, TypeScript, Ant Design v5, React Query, and Axios.
- Use Ant Design components before creating custom UI.
- Use `@ant-design/icons` for icons.
- Use Ant Design theme tokens or project utility classes mapped to theme config.
- Do not hard-code colors, spacing, typography, border radius, or shadows in new UI code.
- Avoid arbitrary utility values such as `p-[13px]` or `text-[#ff4d4f]`.
- Tables must have `rowKey`, loading state, pagination when needed, and horizontal scroll for wide data.
- Forms must validate before submit and prevent duplicate submission.
- UI must cover loading, empty, error, disabled, hover, active, and responsive states.
- Enterprise UI should be dense, readable, quiet, and workflow-focused.

## Safety & Collaboration

- Do not revert unrelated user changes.
- Do not run destructive git or filesystem commands unless explicitly requested.
- Keep edits scoped to the user's request.
- Prefer existing project patterns over new abstractions.
- Add comments only for important business logic or non-obvious behavior.
- Before finalizing code changes, run the relevant build or tests when feasible.

## Required Work Cycle

For non-trivial code or documentation changes, every AI agent must follow this workflow:

1. Ask: clarify the goal only when the request is ambiguous or risky.
2. Plan: identify the smallest safe implementation path and affected files.
3. Implement: make scoped changes that follow the project architecture and patterns.
4. Review Diff: inspect the diff for correctness, unintended edits, secrets, formatting issues, and missing tests.
5. Run/Test: run the relevant build, tests, smoke checks, or explain why they could not be run.
6. Commit: commit only after review and verification, and only when the user has asked for or approved a commit.

Do not skip Review Diff before Commit. If a test or build fails, fix the cause or clearly document the remaining blocker before committing.
