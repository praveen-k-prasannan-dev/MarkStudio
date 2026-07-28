# Class Diagrams

## Basic class with members

```
class Animal {
  +String name
  -int age
  #String species
  +makeSound() void
}
```

```mermaid
classDiagram
class Animal {
  +String name
  -int age
  #String species
  +makeSound() void
}
```

Visibility markers: `+` public, `-` private, `#` protected, `~` package/internal.

## Relationships

| Syntax | Meaning |
|--------|---------|
| `A --|> B` | Inheritance (A extends B) |
| `A --* B` | Composition |
| `A --o B` | Aggregation |
| `A --> B` | Association |
| `A ..> B` | Dependency |
| `A ..|> B` | Realization/implements |
| `A -- B` | Solid link, no arrow |
| `A .. B` | Dashed link, no arrow |

```mermaid
classDiagram
Animal <|-- Dog
Animal <|-- Cat
Owner --> Animal : has
Animal *-- Heart : composed of
Animal o-- Collar : aggregates
```

## Multiplicity and labels

```
A "1" --> "*" B : contains
```

```mermaid
classDiagram
Owner "1" --> "*" Animal : owns
```

## Generics and notes

```mermaid
classDiagram
class Stack~T~ {
  +push(T item) void
  +pop() T
}
note for Stack "A simple generic stack"
```
