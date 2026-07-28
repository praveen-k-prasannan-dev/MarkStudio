# Other Diagram Types

These are supported by the same bundled Mermaid library but come up less often — included here for completeness.

## Quadrant chart

```mermaid
quadrantChart
title Effort vs Impact
x-axis Low Effort --> High Effort
y-axis Low Impact --> High Impact
quadrant-1 Quick Wins
quadrant-2 Major Projects
quadrant-3 Fill-ins
quadrant-4 Thankless Tasks
Task A: [0.3, 0.8]
Task B: [0.7, 0.7]
Task C: [0.2, 0.2]
```

## Requirement diagram

```mermaid
requirementDiagram
requirement req1 {
  id: 1
  text: "The system shall export to PDF"
  risk: low
  verifymethod: test
}
element pdfExport {
  type: "component"
}
pdfExport - satisfies -> req1
```

## Sankey diagram

```mermaid
sankey-beta
Writing,Editing,10
Editing,Preview,8
Editing,Export,2
```

## XY chart

```mermaid
xychart-beta
title "Words Written Per Day"
x-axis [Mon, Tue, Wed, Thu, Fri]
y-axis "Words" 0 --> 1000
bar [400, 600, 350, 800, 500]
```

## Block diagram

```mermaid
block-beta
columns 3
Editor Preview Export
```

Each of these has its own richer option set in the [official Mermaid documentation](https://mermaid.js.org/) if you need more than this quick reference.
