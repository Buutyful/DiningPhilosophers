# Dining Philosophers Problem

## Description

This project implements a simulation of the classic **Dining Philosophers Problem**.

- The first project demonstrates how different threads are handled to manage shared resources.
- The second project uses proper coroutines, leveraging C# async/await and tasks.

### Problem Overview

The simulation involves **N philosophers** sitting around a circular table, each with:
- One fork on their **left**
- One fork on their **right**

### Philosopher Behavior Cycle

Each philosopher repeatedly performs the following actions:

1. **Take left fork**
2. **Take right fork**
3. **Eat** for `time_to_eat` milliseconds
4. **Release both forks**
5. **Sleep** for `time_to_sleep` milliseconds
6. **Think**

### Simulation Rules

#### Death Condition
A philosopher **dies** if they do not start eating within `time_to_die` milliseconds since:
- Their previous meal, OR
- The start of the simulation (for the first meal)

#### Termination Conditions
The simulation ends immediately when:
- **First philosopher dies**, OR
- **All philosophers** have eaten at least `target_meals` times (only applicable if `target_meals` parameter is provided)

### Requirements

- **All state changes must be logged** to track philosopher actions and simulation progress
- Proper synchronization mechanisms must be implemented to prevent:
  - Deadlocks (all philosophers waiting indefinitely)
  - Starvation (philosophers unable to acquire forks)
  - Race conditions (concurrent access to shared resources)

## Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `N` | Integer | Number of philosophers |
| `time_to_die` | Milliseconds | Maximum time a philosopher can survive without eating |
| `time_to_eat` | Milliseconds | Duration a philosopher spends eating |
| `time_to_sleep` | Milliseconds | Duration a philosopher spends sleeping |
| `target_meals` | Integer (Optional) | Number of meals each philosopher must complete |

## Logging

The simulation must log all state changes, including:
- Fork acquisition (left and right)
- Eating start/end
- Sleeping start/end
- Thinking
- Death events
- Simulation termination

## Challenge

The key challenge is to implement thread-safe synchronization that:
- Prevents philosophers from starving
- Avoids deadlock situations
- Ensures fair resource allocation
- Maintains accurate timing for death detection
