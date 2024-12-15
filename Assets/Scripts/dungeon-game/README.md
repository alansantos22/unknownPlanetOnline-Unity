# Jogo Dungeon

## Visão Geral
Um RPG dinâmico que mescla combate por turnos com elementos em tempo real. Os jogadores exploram masmorras geradas proceduralmente enquanto gerenciam estatísticas, inventário e enfrentam desafios únicos em diferentes modos de jogo.

## Características
- **Sistema de Combate**: 
  - Combate por turnos baseado em iniciativa
  - Sistema de pontos de ação (3 PA por turno)
  - Habilidades especiais e efeitos de status
  - Tipos de ataque:
    - Corpo a corpo: Alto dano, baixo alcance
    - À distância: Dano médio, alcance variável
    - Mágico: Dano variável, consome mana

- **Atributos do Personagem**:
  - Vida (HP): Determina resistência a danos
  - Mana (MP): Recurso para magias
  - Força: Afeta dano corpo a corpo
  - Destreza: Influencia precisão e esquiva
  - Inteligência: Aumenta poder mágico
  - Defesa: Redução de dano
  - Evasão: Chance de esquiva

- **Progressão**:
  - Sistema de níveis (1-50)
  - Árvore de habilidades
  - Equipamentos melhoráveis
  - Reputação com facções

- **Modos de Jogo**:
  - História: Campanha principal com 5 capítulos
  - Sobrevivência: Masmorras infinitas com dificuldade crescente
  - Arena: Combate PvP com rankings

- **Elementos das Masmorras**:
  - Layouts gerados proceduralmente
  - Inimigos variados (20+ tipos)
  - Tesouros e armadilhas
  - Encontros com chefes a cada 5 níveis
  - Eventos aleatórios

## Início Rápido

### Requisitos
- Unity 2021.3 ou superior
- .NET Framework 4.7.1
- 4GB RAM mínimo
- DirectX 11

### Instalação
1. Clone o repositório:
   ```
   git clone <url-do-repositório>
   ```
2. Abra o projeto no Unity
3. Carregue a cena principal em Assets/Scenes/MainMenu

### Configuração de Desenvolvimento
1. Configure as entradas no Unity
   - WASD: Movimento
   - Mouse: Mira/Seleção
   - Teclas 1-4: Habilidades
2. Configure as camadas:
   - Player
   - Enemy
   - Interactable
   - Environment
3. Importe os pacotes necessários do Asset Store

## Estrutura do Projeto
- **Scripts/**
  - **Combat/**: Sistema de combate e cálculos
  - **Character/**: Estatísticas e comportamentos
  - **UI/**: Interface do usuário
  - **Dungeons/**: Geração de masmorras
  - **Items/**: Sistema de inventário
  - **GameModes/**: Implementação dos modos
  - **AI/**: Inteligência artificial dos inimigos
  - **SaveSystem/**: Sistema de salvamento
  - **Network/**: Componentes multiplayer

## Debug e Testes
- Modo debug: Pressione F3 ingame
- Comandos de console disponíveis
- Ferramentas de análise de desempenho

## Contribuindo
1. Faça um fork do repositório
2. Crie uma branch para sua feature
3. Envie um pull request com suas alterações
4. Siga o guia de estilo de código

## Licença
Este projeto está licenciado sob a Licença MIT.