# Matrices & Vectors

## Matrix delimiter styles

| Environment | Delimiters |
|-------------|-----------|
| `pmatrix` | Parentheses `( )` |
| `bmatrix` | Square brackets `[ ]` |
| `vmatrix` | Single bars `| |` (determinant) |
| `Vmatrix` | Double bars `‖ ‖` |
| `matrix` | No delimiters |

Separate columns with `&` and rows with `\\`.

```
$$
\begin{pmatrix}
a & b \\
c & d
\end{pmatrix}
$$
```

$$
\begin{pmatrix}
a & b \\
c & d
\end{pmatrix}
$$

## Square-bracket matrix

```
$$
\begin{bmatrix}
1 & 2 & 3 \\
4 & 5 & 6 \\
7 & 8 & 9
\end{bmatrix}
$$
```

$$
\begin{bmatrix}
1 & 2 & 3 \\
4 & 5 & 6 \\
7 & 8 & 9
\end{bmatrix}
$$

## Determinant

```
$$
\begin{vmatrix}
a & b \\
c & d
\end{vmatrix} = ad - bc
$$
```

$$
\begin{vmatrix}
a & b \\
c & d
\end{vmatrix} = ad - bc
$$

## Vectors

| Syntax | Renders as |
|--------|------------|
| `$\vec{v}$` | $\vec{v}$ |
| `$\mathbf{v}$` (bold, common alternative) | $\mathbf{v}$ |
| `$\hat{n}$` (unit vector) | $\hat{n}$ |
| `$\vec{a} \cdot \vec{b}$` (dot product) | $\vec{a} \cdot \vec{b}$ |
| `$\vec{a} \times \vec{b}$` (cross product) | $\vec{a} \times \vec{b}$ |
