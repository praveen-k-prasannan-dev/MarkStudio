# Sequence Diagrams

## Participants & messages

| Syntax | Meaning |
|--------|---------|
| `participant A` | Declares a participant (box) |
| `actor A` | Declares a participant drawn as a stick figure |
| `A->>B: text` | Solid arrow, filled head (typical synchronous call) |
| `A-->>B: text` | Dashed arrow, filled head (typical response) |
| `A->B: text` | Solid arrow, open head |
| `A-->B: text` | Dashed arrow, open head |
| `A-xB: text` | Solid arrow with an X (failed/async message) |
| `A-)B: text` | Async arrow |

```mermaid
sequenceDiagram
participant Browser
participant Server
actor User
User->>Browser: Click "Save"
Browser->>Server: POST /save
Server-->>Browser: 200 OK
Browser-->>User: Show confirmation
```

## Activations

```
activate A
...
deactivate A
```
or shorthand `A->>+B: text` (activate) / `B-->>-A: text` (deactivate).

```mermaid
sequenceDiagram
A->>+B: request
B-->>-A: response
```

## Notes

```
Note left of A: text
Note right of A: text
Note over A,B: text
```

```mermaid
sequenceDiagram
A->>B: hello
Note over A,B: They shook hands
```

## Loops, alternatives, parallel, critical

```mermaid
sequenceDiagram
loop Every minute
  A->>B: heartbeat
end
alt Success
  B-->>A: 200 OK
else Failure
  B-->>A: 500 Error
end
opt Optional step
  A->>B: extra call
end
par Task 1
  A->>B: work
and Task 2
  A->>C: work
end
critical Must succeed
  A->>B: important call
option Network timeout
  A->>A: retry
end
```

## Autonumbering

```mermaid
sequenceDiagram
autonumber
A->>B: first
B->>A: second
```
