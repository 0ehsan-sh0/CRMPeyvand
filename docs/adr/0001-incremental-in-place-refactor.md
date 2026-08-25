# Incremental in-place refactor over rewrite

We refactor the existing WPF/.NET Framework solution incrementally and behavior-preservingly, instead of rewriting it or upgrading the runtime first. With zero tests and no prior version control, small verifiable steps let us fix the two structural defects (invoice schema, permission model) while always keeping a runnable application. The runtime upgrade follows as a separate mechanical phase once tests exist.

## Considered Options

- Ground-up rewrite: rejected — highest risk of losing undocumented legacy behavior (Persian formatting, SMS flow, report templates).
- Strangler layer-rebuild alongside old layers: rejected — too much ceremony for an app of this size (~7,850 LOC).
- Upgrade-first: deferred — platform churn would mix with logic churn and mask regressions.

# Status: accepted
