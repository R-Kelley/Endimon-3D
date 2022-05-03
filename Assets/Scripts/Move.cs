
//Base class for a damaging move an Endimon can do
public class Move
{
    internal string MoveName;                   //Name of the move
    internal Endimon.Endimontypes MoveType;     //What type does the move take (Pyro, Frost, etc.)
    internal int Damage;                        //Amount of damage it does (0 for special effects)
    internal bool DoesDamage;                   //Does this move do any damage?
    internal bool DoesBoost;                    //Does this move recieve an increase if the target is under a status effect

    //Constructor
    public Move(string name, Endimon.Endimontypes type, int dmg, bool doesdmg, bool doesbst)
    {
        MoveName = name;
        MoveType = type;
        Damage = dmg;
        DoesDamage = doesdmg;
        DoesBoost = doesbst;
    }

    //GETTERS
    public string GetMoveName() { return MoveName; }
    public Endimon.Endimontypes GetMoveType() { return MoveType; }
    public int GetDamage() { return Damage; }
    public bool GetDoesDamage() { return DoesDamage; }
    public bool GetDoesBoost() { return DoesBoost; }

    //SETTERS
    public void SetMoveName(string s) { MoveName = s; }
    public void SetMoveType(Endimon.Endimontypes t) { MoveType = t; }
    public void SetDamage(int d) { Damage = d; }
    public void SetDoesDamage(bool b) { DoesDamage = b; }
    public void SetDoesBoost(bool b) { DoesBoost = b; }
}
