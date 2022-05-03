
//Class is a sub-class of move that has extra properties (and does no real damage)
public class SpecialMove : Move
{
    private string MoveDescription; //What does the special move do
    private bool Targetable;        //Is the special move usable on Endimon?
    private bool IsHarmful;         //Is the speicla move something that harms enemies or helps allies?

    //Constructor
    public SpecialMove(string name, Endimon.Endimontypes type, int dmg, bool doesdmg, bool doesbst, string desc, bool target, bool harm) : base(name, type, dmg, doesdmg, doesbst)
    {
        MoveName = name;
        MoveType = type;
        Damage = 0;
        DoesDamage = false;
        DoesBoost = false;
        MoveDescription = desc;
        Targetable = target;
        IsHarmful = harm;
    }

    //GETTERS
    public string GetMoveDescription() { return MoveDescription; }
    public bool GetTargetable() { return Targetable; }
    public bool GetHarmful() { return IsHarmful; }

    //SETTERS
    public void SetMoveDescription(string s) { MoveDescription = s; }
    public void SetTargetable(bool b) { Targetable = b; }
    public void SetHarmful(bool b) { IsHarmful = b; }
}
