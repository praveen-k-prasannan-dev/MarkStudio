# Entity-Relationship Diagrams

## Basic syntax

```
ENTITY1 ||--o{ ENTITY2 : relationship-label
```

```mermaid
erDiagram
CUSTOMER ||--o{ ORDER : places
ORDER ||--|{ LINE-ITEM : contains
CUSTOMER }|..|{ DELIVERY-ADDRESS : uses
```

## Cardinality notation

| Left symbol | Meaning |
|-------------|---------|
| `|o` | Zero or one |
| `||` | Exactly one |
| `}o` | Zero or more |
| `}|` | One or more |

Read the pair from each side inward, e.g. `||--o{` = "exactly one" on the left, "zero or more" on the right.

## Attributes

```mermaid
erDiagram
CUSTOMER {
  string id PK
  string name
  string email
}
ORDER {
  string id PK
  string customerId FK
  date placedOn
}
CUSTOMER ||--o{ ORDER : places
```

`PK` (primary key) and `FK` (foreign key) are conventional annotations, not required syntax.
