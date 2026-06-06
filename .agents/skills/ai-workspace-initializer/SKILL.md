---
name: ai-workspace-initializer
description: Initialize a local-only .ai-workspace folder for AI-assisted software projects, including plans, task boards, scratchpads, logs, context, prompts, handoffs, reports, and coordination files. Use when the user wants to create an AI agent workspace, initialize .ai-workspace, set up local agent folders, prepare a project for AI-assisted development, create a project kickoff workspace, or organize AI task execution files without committing them to the main repository.
---

# AI Workspace Initializer

## Overview

Create a local-only `.ai-workspace/` operations folder for AI-assisted development. Treat this workspace as non-authoritative project support material: useful for agent coordination, plans, logs, prompts, and handoffs, but not a source of truth and not something to commit.

Never store secrets, credentials, `.env` values, tokens, production configs, or other sensitive material in `.ai-workspace/`.

## Quick Start

Run the bundled initializer from the project root:

```bash
python path/to/ai-workspace-initializer/scripts/init_ai_workspace.py .
```

The script:

- Checks whether `.ai-workspace/` already exists.
- Creates any missing directories and starter markdown files.
- Does not overwrite existing user files.
- Adds `.ai-workspace/` to `.gitignore` when it is not already ignored.
- Prints `Initialized `.ai-workspace/` as a local-only AI operations workspace.` on success.

## Required Layout

Create this structure when missing:

```text
.ai-workspace/
├── README.md
├── TASKS.md
├── AGENT_RULES.md
├── CONTEXT.md
├── decisions/
│   └── DECISIONS.md
├── executions/
│   └── EXECUTION_LOG.md
├── prompts/
│   └── PROMPT_HISTORY.md
├── scratchpads/
│   └── SCRATCHPAD.md
├── reports/
│   └── REPORTS.md
├── handoffs/
│   └── HANDOFF.md
├── worktrees/
│   └── README.md
└── temp/
    └── .gitkeep
```

## Manual Fallback

If the script cannot be run, create the layout manually with the same safety rules:

- Preserve existing `.ai-workspace/` files.
- Only add missing files or directories.
- Add `.ai-workspace/` to `.gitignore` exactly once.
- Keep content local-only, temporary, and non-authoritative.
- Avoid sensitive information in all generated files.

## Starter File Content

Use concise markdown instructions in the generated files:

- `README.md`: Explain that `.ai-workspace` is a local-only AI operations workspace and should not be committed to git.
- `TASKS.md`: Include sections for Backlog, In Progress, Blocked, and Done.
- `AGENT_RULES.md`: Include rules to avoid secrets, avoid overwriting without instruction, log major actions, keep summaries short, prefer small reversible changes, and ask only when blocked by missing critical information.
- `CONTEXT.md`: Include placeholders for project name, problem being solved, target users, MVP scope, tech stack, and current milestone.
- `decisions/DECISIONS.md`: Include decision log fields for date, decision, reason, impact, and status.
- `executions/EXECUTION_LOG.md`: Include log fields for date/time, agent, task, files changed, result, and next action.
- `prompts/PROMPT_HISTORY.md`: Store useful prompts and reusable instructions.
- `scratchpads/SCRATCHPAD.md`: Mark temporary notes as not a source of truth.
- `reports/REPORTS.md`: Reserve for agent-generated summaries, audits, and findings.
- `handoffs/HANDOFF.md`: Reserve for passing context between agents or sessions.
- `worktrees/README.md`: Explain that worktrees may be referenced here, but actual worktrees should be managed carefully.

## Resource

Use `scripts/init_ai_workspace.py` for the deterministic initializer.
