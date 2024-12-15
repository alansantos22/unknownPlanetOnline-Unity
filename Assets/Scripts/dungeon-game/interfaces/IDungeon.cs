public interface IDungeon
{
    void EnterDungeon();
    void ManageMonsters();
    int GetMonsterCount();
    void SpawnMonsters(int min, int max);
    void LootRewards();
}
