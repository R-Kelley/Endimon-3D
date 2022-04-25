using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Endimon
{
    private string EndimonName;                     //Name of the Endimon
    private int EndimonModelNumber;                 //Number assigned to Endimon to indicate the model to place into battle
    private Color32[] EndimonTextColors;            //Colors of Type, Move 1, and Move 2
    private Sprite EndimonLargeImage;               //Endimon's full image to display in character selection & in battle
    public enum Endimontypes { Pyro, Frost, Electro, Earth, Shadow, Normal};    //Type of Endimon (their element)
    private Endimontypes EndimonType;               //The type of this Endimon
    private Endimontypes Weakness;                  //The type that this Endimon is weak against
    private string EndimonRole;                     //Declared role of Endimon (give user general purpose of how to use Endimon)
    private int EndimonAttack;                      //Attack stat, added onto move damage
    private int EndimonDefense;                     //Defense stat, subtracted from TOTAL damage
    private float EndimonHealth;                    //The max hp of the Endimon
    private int EndimonCurrentHealth;               //The current hp of the endimon (their max hp - damage taken)
    private Move EndimonMove1;                      //The first damage move the Endimon has
    private Move EndimonMove2;                      //The second damage move the Endimon has
    private SpecialMove EndimonMove3;               //Special move the Endimon has
    private bool EndimonTurnTaken;                  //Determine if Endimon has gone during a round of turns
    public enum StatusEffects {AttackUp, DefenseUp, HealthRestore, LargeHealthRestore, Paralyze, Poison, Sleep, Confusion, IcicleBaracade, Screech, Synthesis, HeatUp, Speedray, Nothing }//Status effect names that can be applied to a given Endimon through Items/Special Abilities
    private StatusEffects[] Statuses;               //Current status effects being suffered
    private int[] StatusTurns;                      //Remaining turns on both status effects
    private string[] EndimonAnimationNames;

    //Tracks which Endimon it is on the field for the UI Components & Animations
    //0 & 1 and Player's first and second on the field while 2 and 3 are the AI's
    private int ActiveNumber;

    //Constructor to create an Endimon
    public Endimon(string theName, int number, Endimontypes theType, string role, int att, int def, int hp, string move1Name, 
        string move2Name, string move3Name, Endimontypes move1Type, Endimontypes move2Type, int move1Dmg, int move2Dmg, string move3Details, bool move1Boost,
        bool move2Boost, bool move3Target, bool move3Harm)
    {
        //Basic stats of the Endimon are set
        EndimonName = theName;
        EndimonModelNumber = number;
        EndimonType = theType;
        EndimonRole = role;
        EndimonAttack = att;
        EndimonDefense = def;
        EndimonHealth = hp;
        EndimonMove1 = new Move(move1Name, move1Type, move1Dmg, true, move1Boost);
        EndimonMove2 = new Move(move2Name, move2Type, move2Dmg, true, move2Boost);
        EndimonMove3 = new SpecialMove(move3Name, move1Type, 0, false, false, move3Details, move3Target, move3Harm);

        //Determine the correct images to link to this Endimon
        DetermineImages();

        //Determine weakness/strength based upon type
        FindWeakness();

        //Fill in the strings for the animations (0-2 are the moves 1-3, 3 is taking damage and 4 is death)
        EndimonAnimationNames = new string[5];
        EndimonAnimationNames[0] = "Move1";
        EndimonAnimationNames[1] = "Move2";
        EndimonAnimationNames[2] = "Move3";
        EndimonAnimationNames[3] = "Take Damage";
        EndimonAnimationNames[4] = "Die";

        //Determine the colors for the text
        EndimonTextColors = new Color32[3];
        EndimonTextColors[0] = DetermineColor(EndimonType);
        EndimonTextColors[1] = DetermineColor(EndimonMove1.GetMoveType());
        EndimonTextColors[2] = DetermineColor(EndimonMove2.GetMoveType());

        //Stats for upcoming battle are set
        EndimonCurrentHealth = hp;
        EndimonTurnTaken = false;
        ActiveNumber = -1;
        Statuses = new StatusEffects[2];    //One harming effect, one helpful
        StatusTurns = new int[2];           //Turns for each effect
        Statuses[0] = StatusEffects.Nothing;
        Statuses[1] = StatusEffects.Nothing;
        StatusTurns[0] = -1;
        StatusTurns[1] = -1;
    }

    //With a given type, an Endimon recieves an accurate color
    public Color DetermineColor(Endimontypes EType)
    {
        if (EType == Endimontypes.Pyro)
        {
            return new Color32(174, 3, 17, 255);
        }
        else if (EType == Endimontypes.Frost)
        {
            return new Color32(11, 199, 195, 255);
        }
        else if (EType == Endimontypes.Electro)
        {
            return new Color32(154, 20, 202, 255);
        }
        else if (EType == Endimontypes.Shadow)
        {
            return new Color32(30, 30, 30, 255);
        }
        else if (EType == Endimontypes.Earth)
        {
            return new Color32(4, 127, 12, 255);
        }
        else if (EType == Endimontypes.Normal)
        {
            return new Color32(71, 73, 72, 255);
        }
        return Color.cyan;
    }

    //Function will look at the model number and determine which images should be assigned as sprites for the Endimon
    public void DetermineImages()
    {
        if(EndimonModelNumber == 1)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Bug-Background", typeof(Sprite)) as Sprite;
        }
        else if(EndimonModelNumber == 2)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Snake-Background", typeof(Sprite)) as Sprite;
        }
        else if (EndimonModelNumber == 3)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Flame-Background", typeof(Sprite)) as Sprite;
        }
        else if (EndimonModelNumber == 4)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Icicle-Background", typeof(Sprite)) as Sprite;
        }
        else if (EndimonModelNumber == 5)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Demon-Background", typeof(Sprite)) as Sprite;
        }
        else if (EndimonModelNumber == 6)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Bat-Background", typeof(Sprite)) as Sprite;
        }
        else if (EndimonModelNumber == 7)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Ghost-Background", typeof(Sprite)) as Sprite;
        }
        else if (EndimonModelNumber == 8)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Cactus-Background", typeof(Sprite)) as Sprite;
        }
        else if (EndimonModelNumber == 9)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Shadow-Background", typeof(Sprite)) as Sprite;
        }
        else if (EndimonModelNumber == 10)
        {
            EndimonLargeImage = Resources.Load("EndimonImages/Cat-Background", typeof(Sprite)) as Sprite;
        }

    }

    //Based on what type the Endimon is, assign a weakness to it
    public void FindWeakness()
    {
        if(EndimonType == Endimontypes.Pyro)
        {
            Weakness = Endimontypes.Frost;
        }
        else if(EndimonType == Endimontypes.Frost)
        {
            Weakness = Endimontypes.Electro;
        }
        else if(EndimonType == Endimontypes.Electro)
        {
            Weakness = Endimontypes.Earth;
        }
        else if(EndimonType == Endimontypes.Earth)
        {
            Weakness = Endimontypes.Pyro;
        }
        else if(EndimonType == Endimontypes.Shadow)
        {
            Weakness = EndimonType;
        }
    }

    //String of the exact type text is sent back to display on the UI
    public string GetEndimonTypeText(Endimontypes EType)
    {
        if (EType == Endimontypes.Pyro)
        {
            return "Pyro";
        }
        else if (EType == Endimontypes.Frost)
        {
            return "Frost";
        }
        else if (EType == Endimontypes.Electro)
        {
            return "Electro";
        }
        else if (EType == Endimontypes.Shadow)
        {
            return "Shadow";
        }
        else if (EType == Endimontypes.Earth)
        {
            return "Earth";
        }
        else if (EType == Endimontypes.Normal)
        {
            return "Normal";
        }
        return "Error";
    }

    //Function will calculate the damage of the move, then return the correct damage
    //This function does not handle the animations or timing sequence as of yet
    public int UseDamageMove(Endimon Attacker, Move DamageMove, Endimon Defender, Image[] GlobalStatuses, bool justCalculate)
    {
        float TotalDamage = 0f;

        //Confusion status will make Endimon hit themselves with their own attack 70% of the time
        if(Statuses[1] == StatusEffects.Confusion && !justCalculate)
        {
            int rand = Random.Range(1, 11);
            if(rand > 3)
            {
                Debug.Log("CONFUSED!");
                Defender = Attacker;
            }
        }

        //This move will do no damage as the Endimon is paralyzed and cannot attack using non-normal type moves
        if(Statuses[1] == StatusEffects.Paralyze && DamageMove.GetMoveType() != Endimontypes.Normal)
        {
            return 0;
        }

        //Checking for "ShadowCast" effect, if a non shadow type move is used then the endimon has a chance of missing the attack (0 damage)
        Sprite shadowGlobal = Resources.Load("StatusIcons/ShadowGlobal", typeof(Sprite)) as Sprite;
        if ((GlobalStatuses[8].sprite == shadowGlobal|| GlobalStatuses[9].sprite == shadowGlobal) && DamageMove.GetMoveType() != Endimontypes.Shadow && !justCalculate)
        {
            Debug.Log("Move could be 0 by shadow global");
            int rand = Random.Range(1, 11);
            if(rand < 3)
            {
                return 0;
            }
        }

        //Checks for "Speedray" effect, if so, this has a 33% chance of doing no damage/missing
        if(Defender.GetEndimonPostiveEffect() == StatusEffects.Speedray && !justCalculate)
        {
            int rand = Random.Range(1, 4);
            //Return 0 if attack missed
            if(rand == 1)
            {
                return 0;
            }
        }

        //If under the item influence of AttackUp, an extra 15 damage is pre-given
        if(Statuses[0] == StatusEffects.AttackUp)
        {
            TotalDamage += 20f;
        }

        //Calculate damage that the move should do based upon given stats
        TotalDamage += Attacker.GetAttack() + DamageMove.GetDamage();

        //Move does more damage baseed on the enemy being under a negative effect
        if(DamageMove.GetDoesBoost() && Defender.Statuses[1] != StatusEffects.Nothing) {
            TotalDamage += 30;
        }

        //Check to see if any type multipliers should apply in this case
        if(DamageMove.GetMoveType() == Defender.GetEndimonWeakness())
        {
            TotalDamage = TotalDamage * 1.5f;   //Boost for using move enemy is weak to
        }
        else if(DamageMove.GetMoveType() == Defender.GetEndimonType() && DamageMove.GetMoveType() != Endimontypes.Shadow)
        {
            TotalDamage = TotalDamage * 0.75f;  //Boost down for using same move type as the enemy's type
        }
        else if(DamageMove.GetMoveType() == Endimontypes.Normal && Defender.GetEndimonType() == Endimontypes.Shadow)
        {
            return 0;   //Normal move used against shadow, no damage
        }

        //Checking for "Defense Up" effect from item, if applied to defender it will reduce the damage by 20
        if (Defender.GetEndimonPostiveEffect() == StatusEffects.DefenseUp)
        {
            TotalDamage -= 15f;
        }

        //Checking for "Icicle Baracade" status, if applied to defender it will reduce the damage by 25
        if (Defender.GetEndimonPostiveEffect() == StatusEffects.IcicleBaracade)
        {
            TotalDamage -= 25f;
        }

        //Checking for "Screech" status, if applied to attacker it will boost the damage by 25
        if (Defender.GetEndimonPostiveEffect() == StatusEffects.Screech)
        {
            TotalDamage += 30f;
        }

        //Checking if the "Ring of Fire" global effect is in play, adding 20 damage to fire element moves
        Color32 red = new Color32(11, 199, 195, 255);
        if ((GlobalStatuses[8].color == red || GlobalStatuses[9].color == red) && DamageMove.GetMoveType() == Endimontypes.Pyro)
        {
            TotalDamage += 35f;
        }

        TotalDamage -= Defender.GetDefense();

        if(Attacker.GetEndimonPostiveEffect() == StatusEffects.HeatUp)
        {
            TotalDamage = TotalDamage * 1.5f;
        }

        if(TotalDamage < 0)
        {
            TotalDamage = 0;
        }

        return (int)TotalDamage;
    }

    //Special move used by an Endimon
    //Return value here relates to the index of the particle to apply
    public int UseSpecialMove(Endimon Attacker, Endimon Target, SpecialMove UsedMove, BattleController bc)
    {
        //Gives a bit of healing to the targeted Endimon (We will apply negative damage)
        if(UsedMove.GetMoveName() == "Rejuvenation")
        {
            int HealthToRestore = (int)(Target.GetHealth() * 0.125 * -1);
            if(HealthToRestore + Target.GetCurrentHP() > Target.GetHealth())
            {
                HealthToRestore = (int)(Target.GetHealth() - Target.GetCurrentHP() * -1);
            }
            AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
            bc.SpawnText("Healing", HealthToRestore*-1, Target);
            Target.TakeDamage(HealthToRestore);
            return 6;
        }
        //Applies sleep if the random chance occurs
        else if(UsedMove.GetMoveName() == "Noxious Fumes")
        {
            int rand = Random.Range(1, 101);
            if(rand < 50)
            {
                Debug.Log("Sleep attack missed");
                return -1;
            }
            else
            {
                Debug.Log("Sleep attack hit");
                AudioSource.PlayClipAtPoint(Audio.Sleep, GameObject.Find("MainCamera").transform.position);
                Target.AddStatusEffect(true, StatusEffects.Sleep, 1);
                return 1;
            }
        }
        //Boosts the attacks of fire, code will return to BattleController that this effect needs to be added
        else if(UsedMove.GetMoveName() == "Ring of Fire")
        {
            //Fire moves will do an extra 20 damage (added onto final calculation)
            AudioSource.PlayClipAtPoint(Audio.GlobalFlames, GameObject.Find("MainCamera").transform.position);
            return 9;
        }
        //Applies a defense boost similar to the item
        else if(UsedMove.GetMoveName() == "Icicle Baracade")
        {
            //An extra 10 defense is given
            AudioSource.PlayClipAtPoint(Audio.DefenseUp, GameObject.Find("MainCamera").transform.position);
            //Add an extra turn to the effect if casted on itself because this turn is now over
            if (Target.GetName() == Attacker.GetName())
            {
                Target.AddStatusEffect(false, StatusEffects.IcicleBaracade, 2);
            }
            else
            {
                Target.AddStatusEffect(false, StatusEffects.IcicleBaracade, 1);
            }
            return 8;
        }
        //Global Effect that makes all non dark moves have chance to miss
        else if(UsedMove.GetMoveName() == "Shadowcast")
        {
            //Each move has a 33% chance to miss
            AudioSource.PlayClipAtPoint(Audio.GlobalShadows, GameObject.Find("MainCamera").transform.position);
            return 11;
        }
        //Applies a attack boost similar to the item
        else if(UsedMove.GetMoveName() == "Screech")
        {
            //An extra 10 attack is given
            AudioSource.PlayClipAtPoint(Audio.AttackUp, GameObject.Find("MainCamera").transform.position);
            //Add an extra turn to the effect if casted on itself because this turn is now over
            if (Target.GetName() == Attacker.GetName())
            {
                Target.AddStatusEffect(false, StatusEffects.Screech, 2);
            }
            else
            {
                Target.AddStatusEffect(false, StatusEffects.Screech, 1);
            }
            return 7;
        }
        //Global Effect that does damage to the opposing team **This might have to be ammended somehow
        else if(UsedMove.GetMoveName() == "Blizzard")
        {
            //Causes 20 damage per turn (raw damage)
            AudioSource.PlayClipAtPoint(Audio.GlobalBlizzard, GameObject.Find("MainCamera").transform.position);
            return 10;
        }
        //Applies a healing overtime effect to the target
        else if(UsedMove.GetMoveName() == "Synthesis")
        {
            //15 Healing is given per turn
            AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
            Target.AddStatusEffect(false, StatusEffects.Synthesis, 4);
            return 5;
        }
        //Applies a damage buff that lasts a turn
        else if(UsedMove.GetMoveName() == "Heating Up")
        {
            //Doubles the damage of the attack (final calculation)
            AudioSource.PlayClipAtPoint(Audio.AttackUp, GameObject.Find("MainCamera").transform.position);
            //If the Endimon using this uses it on itself then it needs an extra turn otherwise it will fall off right after
            if (Target.GetName() == Attacker.GetName())
            {
                Target.AddStatusEffect(false, StatusEffects.HeatUp, 2);
            }
            else
            {
                Target.AddStatusEffect(false, StatusEffects.HeatUp, 1);
            }
            return 7;
        }
        //Applies buff that gives user a chance to dodge incoming attacks
        else if(UsedMove.GetMoveName() == "Speedray")
        {
            //33% chance to dodge the incoming attack
            AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
            if (Target.GetName() == Attacker.GetName())
            {
                Target.AddStatusEffect(false, StatusEffects.Speedray, 4);
            }
            else
            {
                Target.AddStatusEffect(false, StatusEffects.Speedray, 3);
            }
            return 8;
        }
        else
        {
            Debug.Log("Error, special attack doesn't exist");
            return -1;
        }
    }

    //Damage the Endimon with the given value, gives back if the Endimon died or not
    public bool TakeDamage(int dmg)
    {
        EndimonCurrentHealth -= dmg;
        if(EndimonCurrentHealth > EndimonHealth)
        {
            EndimonCurrentHealth = (int)EndimonHealth;
        }
        if(EndimonCurrentHealth <= 0)
        {
            return true;
        }
        return false;
    }

    //Set the status of the Endimon's participation in the current round of turns
    public void SetTurnStatus(bool t)
    {
        EndimonTurnTaken = t;
    }

    //Adds an effect into the array, the slot is dependant on whether its a harmful effect or positive
    public void AddStatusEffect(bool isHarmful, StatusEffects effectName, int duration)
    {
        if(isHarmful)
        {
            Statuses[1] = effectName;
            StatusTurns[1] = duration;
        }
        else
        {
            Statuses[0] = effectName;
            StatusTurns[0] = duration;
        }
    }

    //Gets rid of a status effect on the Endimon
    public void RemoveStatusEffect(bool isHarmful)
    {
        if(isHarmful)
        {
            Statuses[1] = StatusEffects.Nothing;
        }
        else
        {
            Statuses[0] = StatusEffects.Nothing;
        }
    }

    //Subtracts a turn from both the positive and negative effect applied (if there even is one) Removes the effect is duration is up
    public void DecreaseStatusEffectTurns()
    {
        StatusTurns[0] -= 1;
        StatusTurns[1] -= 1;
        if(StatusTurns[0] < 0)
        {
            StatusTurns[0] = 0;
        }
        if(StatusTurns[1] < 0)
        {
            StatusTurns[1] = 0;
        }

        if(StatusTurns[0] == 0)
        {
            RemoveStatusEffect(false);
        }
        if(StatusTurns[1] == 0)
        {
            RemoveStatusEffect(true);
        }
    }

    //GETTERS
    public string GetName() { return EndimonName; }
    public Endimontypes GetEndimonType() { return EndimonType; }
    public Endimontypes GetEndimonWeakness() { return Weakness; }
    public string GetRole() { return EndimonRole; }
    public int GetAttack() { return EndimonAttack; }
    public int GetDefense() { return EndimonDefense; }
    public float GetHealth() { return EndimonHealth; }
    public int GetCurrentHP() { return EndimonCurrentHealth; }
    public Move GetEndimonMove1() { return EndimonMove1; }
    public Move GetEndimonMove2() { return EndimonMove2; }
    public SpecialMove GetEndimonMove3() { return EndimonMove3; }
    public Color32[] GetEndimonTextColors() { return EndimonTextColors; }
    public Color32 GetPrimaryEndimonColor() { return EndimonTextColors[0]; }
    public bool GetEndimonTurnTaken() { return EndimonTurnTaken; }
    public int GetActiveNumber() { return ActiveNumber; }
    public int[] GetStatusTurns() { return StatusTurns; }
    public int GetModelNumber() { return EndimonModelNumber; }
    public string GetAnimationName(int i) { return EndimonAnimationNames[i]; }
    public Sprite GetEndimonLargeImage() { return EndimonLargeImage; }
    public StatusEffects GetEndimonPostiveEffect() { return Statuses[0]; }
    public StatusEffects GetEndimonNegativeEffect() { return Statuses[1]; }

    //SETTERS (Selected stats need to be modified mid battle)
    public void SetDefense(int num) { EndimonDefense += num; }
    public void SetActiveNumber(int i) { ActiveNumber = i; }

}
