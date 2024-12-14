# Unknown Planet Navigation System

A sophisticated 2D pathfinding and movement system for Unity games, featuring smooth character movement, intelligent obstacle avoidance, and dynamic path optimization.

## Features

- Dynamic grid-based pathfinding
- Automatic obstacle avoidance
- Smooth character movement
- Visual debugging tools
- Path optimization
- Adjustable safety margins
- Customizable movement parameters
- Runtime path visualization
- Automatic grid generation

## System Requirements

- Unity 2021.3 or newer
- 2D project setup
- Physics2D system enabled

## Installation

1. Copy the entire `Navigation` folder into your project's Assets folder.
2. Import the required namespaces in your scripts:

## Setup Instructions

1. Create the required hierarchy in your scene:
   - Add a GameObject with the `GridManager` component
   - Add an area GameObject with a Collider2D to serve as the walkable area
   - Add obstacle GameObjects with Collider2D components (optional)

2. Configure the GridManager:
   - Assign the walkable area GameObject to the `Walkable Area` field
   - Add any obstacles to the `Obstacles` array
   - Adjust the `Grid Spacing` (default: 0.25) and `Safety Margin` (default: 0.3)
   - Enable `Edit Mode` to generate the initial navigation grid

3. Add movement to your character:
   - Attach the `PlayerMovement` script to your player GameObject
   - Assign the `GridManager` reference
   - Configure movement settings (speed, arrival threshold, etc.)

## Usage

### Basic Movement

## Movement Blocking System

### Overview
O sistema de bloqueio de movimento permite que qualquer componente pause temporariamente o movimento do jogador. Isso é útil para:
- Menus e UIs
- Cutscenes
- Diálogos
- Eventos especiais
- Estados de jogo específicos (loading, pause, etc)

### Implementações Comuns

#### 1. UI Panels
```csharp
public class InventoryUI : MonoBehaviour
{
    private void OnEnable()
    {
        MovementBlocker.AddBlock(); // Bloqueia quando o inventário abre
    }

    private void OnDisable()
    {
        MovementBlocker.RemoveBlock(); // Desbloqueia quando fecha
    }
}
```

#### 2. Cutscenes
```csharp
public class CutsceneManager : MonoBehaviour
{
    public async void PlayCutscene()
    {
        MovementBlocker.AddBlock();
        await PlayCutsceneSequence();
        MovementBlocker.RemoveBlock();
    }
}
```

#### 3. Diálogos
```csharp
public class DialogueSystem : MonoBehaviour
{
    public void StartDialogue(DialogueData dialogue)
    {
        MovementBlocker.AddBlock();
        ShowDialogueUI();
    }

    public void EndDialogue()
    {
        HideDialogueUI();
        MovementBlocker.RemoveBlock();
    }
}
```

#### 4. Bloqueio Temporário
```csharp
public class TemporaryBlock : MonoBehaviour
{
    public async void BlockForSeconds(float duration)
    {
        MovementBlocker.AddBlock();
        await new WaitForSeconds(duration);
        MovementBlocker.RemoveBlock();
    }
}
```

#### 5. Estados de Jogo
```csharp
public class GameStateManager : MonoBehaviour
{
    public void PauseGame()
    {
        Time.timeScale = 0;
        MovementBlocker.AddBlock();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        MovementBlocker.RemoveBlock();
    }
}
```

### Verificando o Estado de Bloqueio

```csharp
// Em qualquer script
if (MovementBlocker.IsBlocked)
{
    // O movimento está bloqueado
    // Útil para UI feedback, animações, etc
}
```

### Melhores Práticas

1. **Gerenciamento de Estados**
```csharp
public class StateBasedUI : MonoBehaviour
{
    private bool isBlocking = false;

    public void ToggleUI()
    {
        if (!isBlocking)
        {
            MovementBlocker.AddBlock();
            isBlocking = true;
        }
        else
        {
            MovementBlocker.RemoveBlock();
            isBlocking = false;
        }
    }

    private void OnDisable()
    {
        if (isBlocking)
        {
            MovementBlocker.RemoveBlock();
            isBlocking = false;
        }
    }
}
```

2. **Bloqueio com Escopo**
```csharp
public class ScopedBlocker : IDisposable
{
    public ScopedBlocker()
    {
        MovementBlocker.AddBlock();
    }

    public void Dispose()
    {
        MovementBlocker.RemoveBlock();
    }
}

// Uso:
using (new ScopedBlocker())
{
    // Código que requer bloqueio de movimento
    // O bloqueio é automaticamente removido ao sair do escopo
}
```

### Dicas de Depuração

1. Monitore o número de bloqueios ativos
2. Use try-catch ao trabalhar com bloqueios
3. Sempre remova bloqueios em OnDisable/OnDestroy
4. Considere adicionar logging em desenvolvimento

### Integração com Sistemas Existentes

#### Sistema de Salvamento
```csharp
[System.Serializable]
public class GameState
{
    public bool wasMovementBlocked;

    public void RestoreState()
    {
        if (wasMovementBlocked)
            MovementBlocker.AddBlock();
    }
}
```

## Performance Optimizations

### Priority Queue Implementation
The system uses a custom PriorityQueue for efficient pathfinding:
