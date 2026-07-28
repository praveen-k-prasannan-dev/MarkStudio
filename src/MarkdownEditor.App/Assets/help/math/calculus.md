# Sums, Products, Integrals & Limits

## Sums and products

| Syntax | Renders as |
|--------|------------|
| `$\sum_{i=1}^{n} i$` | $\sum_{i=1}^{n} i$ |
| `$\prod_{i=1}^{n} i$` | $\prod_{i=1}^{n} i$ |

```
$$
\sum_{i=1}^{n} i = \frac{n(n+1)}{2}
$$
```

$$
\sum_{i=1}^{n} i = \frac{n(n+1)}{2}
$$

## Integrals

| Syntax | Renders as |
|--------|------------|
| `$\int f(x)\,dx$` | $\int f(x)\,dx$ |
| `$\int_a^b f(x)\,dx$` (definite) | $\int_a^b f(x)\,dx$ |
| `$\iint f(x,y)\,dA$` (double) | $\iint f(x,y)\,dA$ |
| `$\iiint f\,dV$` (triple) | $\iiint f\,dV$ |
| `$\oint f(x)\,dx$` (contour/closed) | $\oint f(x)\,dx$ |

```
$$
\int_0^\infty e^{-x} \, dx = 1
$$
```

$$
\int_0^\infty e^{-x} \, dx = 1
$$

## Limits

| Syntax | Renders as |
|--------|------------|
| `$\lim_{x \to 0} f(x)$` | $\lim_{x \to 0} f(x)$ |
| `$\lim_{x \to \infty} f(x)$` | $\lim_{x \to \infty} f(x)$ |
| `$\lim_{n \to \infty} \left(1+\frac{1}{n}\right)^n$` | $\lim_{n \to \infty} \left(1+\frac{1}{n}\right)^n$ |

## Derivatives

| Syntax | Renders as |
|--------|------------|
| `$\frac{d}{dx} f(x)$` | $\frac{d}{dx} f(x)$ |
| `$\frac{d^2y}{dx^2}$` | $\frac{d^2y}{dx^2}$ |
| `$\partial f / \partial x$` or `$\frac{\partial f}{\partial x}$` | $\frac{\partial f}{\partial x}$ |
| `$f'(x)$`, `$f''(x)$` | $f'(x)$, $f''(x)$ |

## Multi-line / aligned equations

```
$$
\begin{aligned}
(a+b)^2 &= a^2 + 2ab + b^2 \\
(a-b)^2 &= a^2 - 2ab + b^2
\end{aligned}
$$
```

$$
\begin{aligned}
(a+b)^2 &= a^2 + 2ab + b^2 \\
(a-b)^2 &= a^2 - 2ab + b^2
\end{aligned}
$$

`&` marks the alignment column, `\\` ends a line.
