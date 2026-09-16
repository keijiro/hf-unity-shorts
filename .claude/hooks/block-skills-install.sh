#!/bin/bash
# Block automatic agent-skill installation in this project.
#
# The HyperFrames skills instruct the agent to run `npx hyperframes skills update
# <name>` (and the `npx skills add` fallback), which installs into ~/.claude/skills
# and ~/.agents/skills -- outside this repo, with --yes and no prompt.
#
# Denied:  `skills add ...`, `skills update ...`, bare `skills` (installs everything)
# Allowed: `skills check` (read-only freshness diagnosis)

cmd="$(jq -r '.tool_input.command // empty')"
[ -z "$cmd" ] && exit 0

# A: an explicit `skills add` / `skills update` subcommand, in any command
# B: a bare `skills` subcommand (installs the full set), only inside a hyperframes command
if printf '%s' "$cmd" | grep -Eq '(^|[[:space:]])skills[[:space:]]+(add|update)([[:space:]]|$)' ||
   { printf '%s' "$cmd" | grep -q 'hyperframes' &&
     printf '%s' "$cmd" | grep -Eq '(^|[[:space:]])skills[[:space:]]*($|[;&|])'; }; then
  cat <<'JSON'
{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"deny","permissionDecisionReason":"Agent-skill installation is blocked in this project (.claude/hooks/block-skills-install.sh). `skills add` / `skills update` writes to ~/.claude/skills and ~/.agents/skills, outside this repo. Continue with the skills already present; `skills check` is allowed if you need the freshness status."}}
JSON
  exit 0
fi
exit 0
