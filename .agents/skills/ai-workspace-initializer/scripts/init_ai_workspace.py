#!/usr/bin/env python3
"""Initialize a local-only .ai-workspace directory for AI-assisted projects."""

from __future__ import annotations

import argparse
from pathlib import Path


SUCCESS = "Initialized `.ai-workspace/` as a local-only AI operations workspace."


FILES = {
    "README.md": """# AI Workspace

`.ai-workspace/` is a local-only AI operations workspace for plans, task notes, execution logs, prompts, scratchpads, reports, handoffs, and coordination files.

Do not commit this directory to git. It is non-authoritative support material for AI-assisted development and should never contain secrets, credentials, `.env` values, tokens, or production configuration.
""",
    "TASKS.md": """# Tasks

## Backlog

- 

## In Progress

- 

## Blocked

- 

## Done

- 
""",
    "AGENT_RULES.md": """# Agent Rules

- Do not modify secrets, credentials, `.env` values, tokens, or production configs.
- Do not overwrite files without explicit instruction.
- Log major actions in `executions/EXECUTION_LOG.md`.
- Keep summaries short and actionable.
- Prefer small, reversible changes.
- Ask only when blocked by missing critical information.
""",
    "CONTEXT.md": """# Project Context

- Project name:
- Problem being solved:
- Target users:
- MVP scope:
- Tech stack:
- Current milestone:
""",
    "decisions/DECISIONS.md": """# Decisions

| Date | Decision | Reason | Impact | Status |
| --- | --- | --- | --- | --- |
|  |  |  |  |  |
""",
    "executions/EXECUTION_LOG.md": """# Execution Log

| Date/time | Agent | Task | Files changed | Result | Next action |
| --- | --- | --- | --- | --- | --- |
|  |  |  |  |  |  |
""",
    "prompts/PROMPT_HISTORY.md": """# Prompt History

Store useful prompts and reusable instructions here. Do not store secrets, tokens, credentials, `.env` values, or production configuration.
""",
    "scratchpads/SCRATCHPAD.md": """# Scratchpad

Temporary notes only. This file is not a source of truth.
""",
    "reports/REPORTS.md": """# Reports

Use this file for agent-generated summaries, audits, and findings.
""",
    "handoffs/HANDOFF.md": """# Handoff

Use this file to pass context between agents or sessions.
""",
    "worktrees/README.md": """# Worktrees

Reference worktrees here when helpful, but manage actual worktrees carefully. Keep paths, branch names, and ownership clear before making changes.
""",
    "temp/.gitkeep": "",
}


def ensure_gitignore(root: Path) -> None:
    gitignore = root / ".gitignore"
    entry = ".ai-workspace/"

    if gitignore.exists():
        text = gitignore.read_text(encoding="utf-8")
        lines = {line.strip() for line in text.splitlines()}
        if entry in lines or ".ai-workspace" in lines:
            return
        suffix = "" if text.endswith(("\n", "\r")) or not text else "\n"
        gitignore.write_text(f"{text}{suffix}{entry}\n", encoding="utf-8")
        return

    gitignore.write_text(f"{entry}\n", encoding="utf-8")


def init_workspace(root: Path) -> None:
    workspace = root / ".ai-workspace"
    workspace.mkdir(exist_ok=True)

    for relative_path, content in FILES.items():
        path = workspace / relative_path
        path.parent.mkdir(parents=True, exist_ok=True)
        if not path.exists():
            path.write_text(content, encoding="utf-8")

    ensure_gitignore(root)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Initialize .ai-workspace as a local-only AI operations workspace."
    )
    parser.add_argument(
        "project_root",
        nargs="?",
        default=".",
        help="Project root where .ai-workspace should be created.",
    )
    args = parser.parse_args()

    root = Path(args.project_root).expanduser().resolve()
    init_workspace(root)
    print(SUCCESS)


if __name__ == "__main__":
    main()
