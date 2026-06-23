---
applyTo: "**"
description: "Use for all tasks to keep operations safe. Prevent destructive or irreversible commands unless the user explicitly requests them."
---

# Safe Command Rules

## Block By Default
Never run destructive commands unless the user explicitly and unambiguously asks for them.

Examples of blocked commands without explicit user approval:
- `git reset --hard`
- `git checkout -- <path>` or equivalent file-reverting commands
- recursive delete commands against project paths (for example `rm -rf`, `Remove-Item -Recurse -Force`)

## Preferred Alternatives
- Use non-destructive inspection commands first.
- Use targeted edits (`apply_patch`) instead of broad file replacements.
- Preserve unrelated local changes.

## Unexpected State Changes
- If unexpected modifications appear while working, stop and ask how to proceed.
- Do not revert user changes implicitly.
