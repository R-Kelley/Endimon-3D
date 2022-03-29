using UnityEngine;

public class BattleTextController {

    //Used when looking at the attacker about to deal damage
    public static string AttackDamageText(Endimon Attacker, Move DamageMove, Endimon Defender)
    {
        return Attacker.GetName() + " used " + DamageMove.GetMoveName() + " on " + Defender.GetName() + ".";
    }

    //Used when looking at the defender of the damage
    public static string DefendDamageText(Endimon Attacker, Move DamageMove, Endimon Defender, int damage, bool isShadowcast)
    {
        var tempString = "";

        if (Attacker == Defender)
        {
            tempString = "Endimon hit itself in confusion for " + damage + " damage.";
        }
        else if (damage <= 0)
        {
            if(DamageMove.GetMoveType() == Endimon.Endimontypes.Normal && Defender.GetEndimonType() == Endimon.Endimontypes.Shadow)
            {
                return "This move had no effect on a shadow Endimon.";
            }
            else if(Attacker.GetEndimonNegativeEffect() == Endimon.StatusEffects.Paralyze)
            {
                return "Attack had no effect due to paralysis.";
            }
            else if(Defender.GetEndimonPostiveEffect() == Endimon.StatusEffects.Speedray)
            {
                return "Endimon was able to dodge the attack.";
            }
            else if(isShadowcast)
            {
                return "Shadowcast aura rendered this move ineffective.";
            }
            else
            {
                tempString = Defender.GetName() + " resisted the attack.";
            }
        }
        else
        {
            tempString = Defender.GetName() + " took " + damage + " damage.";
        }


        if (Defender.GetEndimonWeakness() == DamageMove.GetMoveType())
        {
            tempString += " This move was super effective!";
        }
        else if(Defender.GetEndimonType() == DamageMove.GetMoveType())
        {
            tempString += " This move was not very effective.";
        }

        if(DamageMove.GetDoesBoost() && Defender.GetEndimonNegativeEffect() != Endimon.StatusEffects.Nothing)
        {
            tempString += " Move's boost damage applied!";
        }
        return tempString;
    }

    //Used when an Endimonn casts any item
    public static string ItemUsedText(Endimon Attacker, Item UsedItem, Endimon Defender)
    {
        string tempString = "";
        tempString = Attacker.GetName() + " used a " + UsedItem.GetItemName() + " on " + Defender.GetName() + ".";

        if(UsedItem.GetEffect() == Endimon.StatusEffects.AttackUp)
        {
            tempString += " The attack stat of " + Defender.GetName() + " was raised slightly for three turns.";
        }
        else if(UsedItem.GetEffect() == Endimon.StatusEffects.DefenseUp)
        {
            tempString += " The defense stat of " + Defender.GetName() + " was raised slightly for three turns.";
        }
        else if(UsedItem.GetEffect() == Endimon.StatusEffects.Paralyze)
        {
            tempString += " " + Defender.GetName() + " has been paralyzed for two turns.";
        }
        else if(UsedItem.GetEffect() == Endimon.StatusEffects.Poison)
        {
            tempString += " " + Defender.GetName() + " has been poisoned for three turns.";
        }
        else if(UsedItem.GetEffect() == Endimon.StatusEffects.Sleep)
        {
            tempString += " " + Defender.GetName() + " is asleep for a turn.";
        }
        else if(UsedItem.GetEffect() == Endimon.StatusEffects.Confusion)
        {
            tempString += " " + Defender.GetName() + " is confused for two turns";
        }
        else if(UsedItem.GetEffect() == Endimon.StatusEffects.HealthRestore)
        {
            tempString += " " + Defender.GetName() + " recieved a small health boost.";
        }
        else if(UsedItem.GetEffect() == Endimon.StatusEffects.LargeHealthRestore)
        {
            tempString += " " + Defender.GetName() + " recieved a large health boost. Its defense has been lowered.";
        }
        return tempString;
    }

    //Used when an Endimon casts an ability
    public static string SpecialAbilityText(Endimon Attacker, SpecialMove SpecialMove, Endimon Defender)
    {
        string tempString = "";
        if(SpecialMove.GetMoveName() != "Blizzard" && SpecialMove.GetMoveName() != "Ring of Fire" && SpecialMove.GetMoveName() != "Shadowcast")
        tempString = Attacker.GetName() + " used " + SpecialMove.GetMoveName() + " on " + Defender.GetName() + ".";

        if(SpecialMove.GetMoveName() == "Rejuvenation")
        {
            tempString += " " + Defender.GetName() + " has been healed slightly.";
        }
        else if (SpecialMove.GetMoveName() == "Noxious Fumes")
        {
            tempString += " " + Defender.GetName() + " has been put to sleep for a turn.";
        }
        else if (SpecialMove.GetMoveName() == "Icicle Baracade")
        {
            tempString += " " + Defender.GetName() + " has had their defense raised for 2 turns.";
        }
        else if (SpecialMove.GetMoveName() == "Screech")
        {
            tempString += " " + Defender.GetName() + " has had their attack raised for 2 turns.";
        }
        else if (SpecialMove.GetMoveName() == "Synthesis")
        {
            tempString += " A healing aura has been placed on " + Defender.GetName() + " for 4 turns.";
        }
        else if (SpecialMove.GetMoveName() == "Heating Up")
        {
            tempString += " " + Defender.GetName() + "'s next move will do double the damage.";
        }
        else if (SpecialMove.GetMoveName() == "Speedray")
        {
            tempString += " " + Defender.GetName() + " has a chance to dodge attacks for the next three turns.";
        }
        else if (SpecialMove.GetMoveName() == "Blizzard")
        {
            tempString = Attacker.GetName() + " has casted a blizzard onto the battlefield";
        }
        else if (SpecialMove.GetMoveName() == "Ring of Fire")
        {
            tempString = Attacker.GetName() + " has casted a fire storm onto the battlefield";
        }
        else if (SpecialMove.GetMoveName() == "Shadowcast")
        {
            tempString = Attacker.GetName() + " has casted a dark aura onto the battlefield";
        }
        return tempString;
    }

    //Used when their is a duration effect on an Endimon
    public static string OvertimeEffectText(Endimon CurrentEndimon, Endimon.StatusEffects Status, string GlobalStatus)
    {
        if(Status == Endimon.StatusEffects.Synthesis)
        {
            return CurrentEndimon.GetName() + " has been healed 15 health.";
        }
        else if(Status == Endimon.StatusEffects.Poison)
        {
            return CurrentEndimon.GetName() + " is poisoned, its health and defense were lowered";
        }
        else if(Status == Endimon.StatusEffects.Sleep)
        {
            return CurrentEndimon.GetName() + "is asleep, its turn has been skipped.";
        }
        else if(GlobalStatus == "Blizzard")
        {
            return CurrentEndimon.GetName() + " is freezing, its health has been drained.";
        }
        else
        {
            return "Error in overtime effect text";
        }
    }

    //Used when looking at a global ability
    public static string GlobalText(string GlobalAbilityName)
    {
        if (GlobalAbilityName == "Ring of Fire")
        {
            return "Fire attack damage has been raised across the battlefield.";
        }
        else if(GlobalAbilityName == "Blizzard")
        {
            return "All non ice typed Endimon will be damaged.";
        }
        else if(GlobalAbilityName == "Shadowcast")
        {
            return "All non shadow typed moves have lower accuracy";
        }
        else
        {
            return "Error in global text";
        }
    }

    //Used when an Endimon has swapped out (death/by choice)
    public static string SwappingText(Endimon OriginalEndimon, Endimon ReplacementEndimon)
    {
        return OriginalEndimon.GetName() + " is switching out for " + ReplacementEndimon.GetName() + ".";
    }

    //Used when an Endimon has died
    public static string DeathText(Endimon DeadEndimon)
    {
        return DeadEndimon.GetName() + " has been defeated.";
    }

    //Used when a player has won/lost the game
    public static string GameOverText(bool PlayerWon)
    {
        if(PlayerWon)
        {
            return "You have defeated the opponent's whole team! You've claimed victory!";
        }
        else
        {
            return "Your team has been wiped out, better luck next time.";
        }
    }
}
