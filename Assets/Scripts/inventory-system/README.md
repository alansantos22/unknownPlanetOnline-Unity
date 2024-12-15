# Sistema de Inventário

Este projeto implementa um sistema de inventário para um jogo, permitindo que os jogadores gerenciem itens de forma eficiente.

## Funcionalidades

- **Gerenciamento de Itens**: Adiciona, remove e empilha itens no inventário.
- **Tipos de Itens**: Suporta vários tipos de itens incluindo Saque, Material, Moeda, Arma e Armadura.
- **Raridade de Itens**: Categoriza itens em diferentes níveis de raridade como Normal, Raro e Lendário.
- **Scriptable Objects**: Utiliza ScriptableObjects para fácil gerenciamento de dados de itens no Unity.
- **Interface do Usuário**: Fornece uma interface amigável para gerenciamento do inventário.
- **Sistema de Equipamento**: Permite que jogadores equipem e desequipem itens, afetando as estatísticas do jogador.
- **Sistema de Buff**: Aplica buffs e debuffs baseados nos efeitos dos itens.

## Componentes Principais

### 1. Sistema Base de Itens
- Classe `Item` define todas as propriedades dos itens (recuperação de HP/MP, aumento de status, etc)
- `ItemData` ScriptableObject permite criar itens no Inspector do Unity
- `ItemType` e `ItemRarity` são enums que definem tipos e raridades

### 2. Sistema de Inventário
- `Inventory` gerencia slots e itens
- Suporta empilhamento de itens (até 64 por slot) quando `IsPiled = true`
- Fornece métodos `AddItem()` e `RemoveItem()`
- `ItemDatabase` mantém registro de todos os itens do jogo

### 3. Sistema de Equipamentos
- `EquipmentSystem` gerencia itens equipados
- Controla equipamentos por tipo (armas, armaduras, etc)
- Gerencia itens na mão secundária
- Atualiza stats do jogador ao equipar/desequipar

### 4. Sistema de Buffs
- `BuffSystem` gerencia buffs temporários
- Aplica/remove efeitos de itens consumíveis
- Controla duração dos buffs
- `ItemEffectHandler` processa os efeitos dos itens

### 5. Interface do Usuário
- `InventoryUI` mostra os slots e itens visualmente
- Permite interação com os itens (usar, equipar, etc)
- Atualiza automaticamente quando o inventário muda

Todos os componentes se integram com `PlayerStats` para modificar os atributos do jogador conforme necessário.

## Estrutura de Arquivos

- **Scripts/Core**: Contém classes principais para gerenciamento de inventário.
- **Scripts/Enums**: Define enumerações para tipos e raridade de itens.
- **Scripts/ScriptableObjects**: Gerencia dados de itens como ScriptableObjects.
- **Scripts/UI**: Manipula a interface do usuário para inventário e slots de itens.
- **Scripts/Systems**: Gerencia equipamentos, buffs e efeitos de itens.
- **Resources/Items**: Contém arquivos de dados dos itens.

## Como Começar

1. Clone o repositório.
2. Abra o projeto no Unity.
3. Importe os assets necessários.
4. Execute o jogo para testar o sistema de inventário.

## Licença

Este projeto está licenciado sob a Licença MIT.