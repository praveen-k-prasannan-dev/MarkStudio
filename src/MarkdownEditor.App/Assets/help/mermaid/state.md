# State Diagrams

## Basic states & transitions

```
[*] --> State1
State1 --> State2 : event
State2 --> [*]
```

`[*]` marks the start (as the source) or end (as the target) of the diagram.

```mermaid
stateDiagram-v2
[*] --> Idle
Idle --> Running : start
Running --> Paused : pause
Paused --> Running : resume
Running --> [*] : stop
```

## Composite (nested) states

```mermaid
stateDiagram-v2
[*] --> Active
state Active {
  [*] --> Idle
  Idle --> Processing : begin
  Processing --> Idle : done
}
Active --> [*] : shutdown
```

## Choice, fork, and join

```mermaid
stateDiagram-v2
state check <<choice>>
[*] --> check
check --> Approved : if valid
check --> Rejected : if invalid

state fork_state <<fork>>
[*] --> fork_state
fork_state --> TaskA
fork_state --> TaskB

state join_state <<join>>
TaskA --> join_state
TaskB --> join_state
join_state --> Done
```

## Notes

```mermaid
stateDiagram-v2
Idle --> Running
note right of Running : This is the busy state
```
