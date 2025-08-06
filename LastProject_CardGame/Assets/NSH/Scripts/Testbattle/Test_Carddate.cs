// CardData.cs
[System.Serializable]
public class Test_Carddate
{
    public string cardName;
    public int attack;
    public int health;

    public Test_Carddate(string name, int atk, int hp)
    {
        cardName = name;
        attack = atk;
        health = hp;
    }
}
