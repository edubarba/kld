# Decisions

Keep this short and honest. Bullet points are fine. We read this before we read your code.

## Where the spec was unclear, contradictory, or silent

For each: what the question was, what you decided, what else you considered, and why you chose as you did. If you asked us instead of deciding, say so.

- **[To confirm] Rule 3.1: which loose duplicate survives?** The spec says duplicates are suppressed but not which copy is kept. Decided: the lowest `DocId` (ordinal), so the output does not depend on input order (6.1). Considered: first seen (order-dependent), earliest date (loose files have none), preferred custodian (no rule given). Needs confirmation from the spec owner.
- **[To confirm] Rule 3.1: hash comparison case.** The spec does not say whether `sha256` hex case matters. Decided: compare case-insensitively. Considered: exact string match, which would miss identical files reported with different hex case. Needs confirmation.
- **[To confirm] Rule 3.1: loose file vs email/attachment with the same hash.** 3.1 only mentions two loose files. Decided: they are never duplicates of each other; the rule dedups loose-to-loose only. Considered: dedup across types. Needs confirmation.
- **[To confirm] Rule 3.1 vs 3.4/section 4: cross-custodian duplicates.** The rule suppresses loose duplicates even when held by different custodians (3.4, batch-wide), which can leave a custodian's set incomplete (section 4). Implemented as written in 3.4; the conflict needs a spec decision.

## What I changed and why

-

## What I didn't get to

Known gaps, things you'd flag in a PR description, tests you know are missing.

-

## What I'd do next

-

## If you reimplemented in another language

Why, and what you did to make sure the output contract is preserved.

-

## Working with the AI

What it got right, what you had to correct or reject, and anything you'd warn a teammate about.

-
