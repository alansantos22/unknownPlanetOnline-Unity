using System.Collections.Generic;
using System.Linq;

public class CombatSystem
{
    private Player player;
    private List<Monster> monsters;

    public CombatSystem(Player player, List<Monster> monsters)
    {
        this.player = player;
        this.monsters = monsters;
    }

    public void StartCombat()
    {
        while (monsters.Count > 0)
        {
            TakeTurn();
        }
        RewardPlayer();
    }

    private void TakeTurn()
    {
        var turnOrder = DetermineTurnOrder();
        foreach (var character in turnOrder)
        {
            if (character is Player)
            {
                PlayerAction();
            }
            else
            {
                MonsterAction((Monster)character);
            }
        }
    }

    private List<ICharacter> DetermineTurnOrder()
    {
        return monsters.Cast<ICharacter>().Concat(new List<ICharacter> { player })
            .OrderByDescending(character => character.Agility).ToList();
    }

    private void PlayerAction()
    {
        // Logic for player action (attack, defend, flee)
    }

    private void MonsterAction(Monster monster)
    {
        // Logic for monster action (attack)
    }

    private void RewardPlayer()
    {
        // Logic for rewarding the player after combat
    }
}
