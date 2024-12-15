using System.Collections.Generic;
using System.Linq;

public class TurnSystem
{
    private List<ICharacter> turnOrder;

    public TurnSystem(List<Player> players, List<Monster> monsters)
    {
        turnOrder = new List<ICharacter>();
        InitializeTurnOrder(players, monsters);
    }

    private void InitializeTurnOrder(List<Player> players, List<Monster> monsters)
    {
        turnOrder = players.Cast<ICharacter>().Concat(monsters.Cast<ICharacter>())
            .OrderByDescending(character => character.Agility).ToList();
    }

    public void ExecuteTurn()
    {
        foreach (var character in turnOrder)
        {
            if (character is Player)
            {
                // Handle player actions (attack, defend, flee)
            }
            else if (character is Monster)
            {
                // Handle monster actions (attack)
            }
        }
        ResetTurnOrder();
    }

    private void ResetTurnOrder()
    {
        turnOrder.Clear();
        // Reinitialize turn order with the same players and monsters
    }
}
