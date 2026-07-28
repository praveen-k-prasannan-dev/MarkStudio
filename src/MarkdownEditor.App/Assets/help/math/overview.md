# Math Formulas — Overview

MarkStudio Editor renders math using [MathJax](https://www.mathjax.org/), bundled locally (no network calls), loaded only when a document actually contains math.

## Inline math

Wrap a formula in single dollar signs, in the middle of a sentence:

```
The quadratic formula is $x = \frac{-b \pm \sqrt{b^2-4ac}}{2a}$.
```

The quadratic formula is $x = \frac{-b \pm \sqrt{b^2-4ac}}{2a}$.

## Block math

Put `$$` on its own line, then your formula, then `$$` on its own line:

```
$$
\sum_{i=1}^{n} i = \frac{n(n+1)}{2}
$$
```

$$
\sum_{i=1}^{n} i = \frac{n(n+1)}{2}
$$

> **Important:** block math needs the `$$` delimiters on their **own separate lines**. A single line like `$$x=y$$` is treated as inline math, not a centered block.

## Reference pages

- [Basics](basics.md) — arithmetic, fractions, exponents, subscripts, roots
- [Greek Letters & Functions](greek-functions.md) — α β γ …, trig, log, named functions
- [Sums, Products, Integrals & Limits](calculus.md)
- [Matrices & Vectors](matrices.md)
- [Sets, Logic & Relations](sets-logic.md)
- [Cheat Sheet](cheat-sheet.md) — every symbol on this page in one scannable table
