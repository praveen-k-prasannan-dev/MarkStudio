# Gantt Charts

## Basic syntax

```
gantt
title Project Plan
dateFormat YYYY-MM-DD
section Design
Task A :a1, 2026-01-01, 5d
Task B :after a1, 3d
section Build
Task C :2026-01-10, 7d
```

```mermaid
gantt
title Project Plan
dateFormat YYYY-MM-DD
section Design
Task A :a1, 2026-01-01, 5d
Task B :after a1, 3d
section Build
Task C :2026-01-10, 7d
```

## Task status

| Suffix | Meaning |
|--------|---------|
| `:done, ...` | Rendered as completed |
| `:active, ...` | Rendered as in progress |
| `:crit, ...` | Highlighted as critical |
| `:milestone, ...` | Rendered as a single-point milestone |

```mermaid
gantt
title Status examples
dateFormat YYYY-MM-DD
section Example
Done task      :done, d1, 2026-01-01, 3d
Active task    :active, d2, after d1, 3d
Critical task  :crit, d3, after d2, 2d
Milestone      :milestone, m1, after d3, 0d
```

## Dependencies

Use `after <id>` (as above) instead of a literal date to chain a task after another one finishes.
