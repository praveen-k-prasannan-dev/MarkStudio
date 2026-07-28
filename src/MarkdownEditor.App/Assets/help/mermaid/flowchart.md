# Flowcharts

## Direction

`graph` (or `flowchart`) followed by a direction: `TD`/`TB` (top-down), `BT` (bottom-up), `LR` (left-right), `RL` (right-left).

```mermaid
graph LR
A --> B --> C
```

## Node shapes

| Syntax | Shape |
|--------|-------|
| `A[Rectangle]` | Rectangle |
| `A(Rounded)` | Rounded rectangle |
| `A([Stadium])` | Stadium (pill) |
| `A[[Subroutine]]` | Subroutine |
| `A[(Cylinder)]` | Cylinder (database) |
| `A((Circle))` | Circle |
| `A{Rhombus}` | Rhombus (decision) |
| `A{{Hexagon}}` | Hexagon |
| `A[/Parallelogram/]` | Parallelogram |
| `A[\Parallelogram alt\]` | Parallelogram (reversed) |
| `A[/Trapezoid\]` | Trapezoid |
| `A[\Trapezoid alt/]` | Trapezoid (reversed) |

```mermaid
graph LR
A[Rectangle] --> B(Rounded) --> C([Stadium]) --> D[[Subroutine]]
E[(Cylinder)] --> F((Circle)) --> G{Rhombus} --> H{{Hexagon}}
I[/Parallelogram/] --> J[\Reversed\] --> K[/Trapezoid\]
```

## Arrows / edges

| Syntax | Meaning |
|--------|---------|
| `A --> B` | Solid arrow |
| `A --- B` | Solid line, no arrowhead |
| `A -.-> B` | Dotted arrow |
| `A -.- B` | Dotted line, no arrowhead |
| `A ==> B` | Thick arrow |
| `A === B` | Thick line, no arrowhead |
| `A -->|label| B` or `A -- label --> B` | Labeled arrow |
| `A ~~~ B` | Invisible link (layout only) |

```mermaid
graph LR
A -->|labeled| B -.-> C ==> D --- E
```

## Subgraphs

```mermaid
graph TD
subgraph One
  a1 --> a2
end
subgraph Two
  b1 --> b2
end
a2 --> b1
```

## Styling

```
style A fill:#f9f,stroke:#333,stroke-width:2px
classDef important fill:#f96
class A,B important
```

```mermaid
graph LR
A[Normal] --> B[Styled]
style B fill:#f96,stroke:#333,stroke-width:2px
```
