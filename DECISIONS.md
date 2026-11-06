# Decisions

Keep this short and honest. Bullet points are fine. We read this before we read your code.

## Where the spec was unclear, contradictory, or silent

For each: what the question was, what you decided, what else you considered, and why you chose as you did. If you asked us instead of deciding, say so.

- **[To confirm] Rule 3.1: which loose duplicate survives?** The spec says duplicates are suppressed but not which copy is kept. Decided: the lowest `DocId` (ordinal), so the output does not depend on input order (6.1). Considered: first seen (order-dependent), earliest date (loose files have none), preferred custodian (no rule given). Needs confirmation from the spec owner.
- **[To confirm] Rule 3.1: hash comparison case.** The spec does not say whether `sha256` hex case matters. Decided: compare case-insensitively. Considered: exact string match, which would miss identical files reported with different hex case. Needs confirmation.
- **[To confirm] Rule 3.1: loose file vs email/attachment with the same hash.** 3.1 only mentions two loose files. Decided: they are never duplicates of each other; the rule dedups loose-to-loose only. Considered: dedup across types. Needs confirmation.
- **[To confirm] Rule 3.1 vs 3.4/section 4: cross-custodian duplicates.** The rule suppresses loose duplicates even when held by different custodians (3.4, batch-wide), which can leave a custodian's set incomplete (section 4). Implemented as written in 3.4; the conflict needs a spec decision.
- **[To confirm] Rule 3.2 vs 3.5: attachment dedup breaks families.** Removing an attachment by hash takes it out of its family, for example the same `contract.pdf` on two different emails, or two identical attachments inside one email. Decided: implement 3.2 literally (`AttachmentDeduplicationRule`) and enforce family integrity in a separate 3.5 rule that runs after it. Considered: family-level-only suppression (a no-op while 3.3 is out of scope) and same-parent-only suppression. Until the 3.5 rule exists, running 3.2 alone can break families. Needs a spec decision.
- **[To confirm] Rule 3.2: children of a suppressed attachment.** The rule leaves them in place, so they can be orphaned (their `ParentID` points at a removed document). Decided: the later 3.5 rule handles this. Considered: dropping the whole subtree inside this rule.
- **[To confirm] Rule 3.2: survivor and scope.** Same choices as 3.1: the lowest `DocId` survives, `sha256` is compared case-insensitively, and the rule only compares attachment to attachment (not to loose files or emails). The same-hash attachment on a different custodian is also suppressed (3.4 vs section 4).

## What I changed and why

- **New approach / Strategy Pattern** Worked on an extensible deduplication approach based on a common interface. So individual rules to be implemented, updated, or added independently without modifying the existing processing logic (I applied some SOLID principles)
- **Loose/Attachment Deduplication Rules:** Added two separate classes to isolate deduplication rules and improve maintainability. Each class handles its specific responsibilities and covers the expected business behavior.
- **Loose/Attachment Deduplication Tests:** Added unit tests for each new deduplication rule using AI assistance. Reviewed the tests to ensure basic scenarios and expected behaviors were covered.
- **No changes in Deduper**I kept the existing `Deduper.cs` implementation unchanged while completing the new rule-based deduplication approach. Once the new approach is ready, a collection of deduplication rules can be injected in the `Deduper` constructor.

## What I didn't get to

Known gaps, things you'd flag in a PR description, tests you know are missing.

- **Refactoring to review later (Big O):** `AttachmentDeduplicationRule` and `LooseFileDeduplicationRule` pick the survivor with a `Dictionary<hash, lowest DocId>`. That is O(n) time (one pass to build, one pass to filter) and O(k) extra space, where k is the number of distinct hashes. Kept the dictionary for now because batches reach about 2 million documents (6.2). Revisit after measuring both at that size, and together with extracting the "keep lowest DocId per hash for a given docType" logic that both rules duplicate.

- **Cross-mailbox email deduplication:** (SPEC.md/3.3) This feature is expected to be included in the current user story. The user story _"INGEST-4471 — Emails arriving in review without their attachments"_ addresses an existing issue, so fixing it should be the priority.
- 

## What I'd do next

- **Cross-mailbox email deduplication:** (SPEC.md/3.3) Include as new deputation rule "CrossMailboxDeduplicationRule"
- **Refactoring/Tech-debt**
  - **Deduper.cs** Remove multiple conditionals to simplify the processing logic, reduce complexity, and delegate deduplication decisions to the individual rule implementations.
  - **Model classes** `Batch`, `Document`, `Document`, `DedupResult` should have its own class files.
- **Edge Case Tests:** Reviewed the existing deduplication rules and added test scenarios to cover cross-rule deduplication cases. 
    
## If you reimplemented in another language

Why, and what you did to make sure the output contract is preserved.

-

## Working with the AI

What it got right, what you had to correct or reject, and anything you'd warn a teammate about.

- Please find the AI assistant conversations attached in docs `docs/2026-11-06-111204 KLD - Fixing the existing deduplication rules.txt` 

