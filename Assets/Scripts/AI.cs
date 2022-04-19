using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Class is more of a data storage unit, however it will be casting effects and needs to be a monobehavior
public class AI
{
    private string AIName;              //Name of AI for use in dialog boxes
    private Endimon[] AITeam;           //Roster of 4 Endimon
    private Item[] AIItems;             //Selected Items (0-5 depending on difficulty)
    private CharacterSelectController.DifficultySelection AIDifficulty;     //AI's difficulty (Easy/Medium/Hard)
    private Endimon ActiveAIEndimon1;   //First slotted Endimon in the battle
    private Endimon ActiveAIEndimon2;   //Second slotted Endimon in the battle

    private Endimon Target;
    private string LastTarget = "";

    //Constructor: Build an AI depeneding on the difficulty. If this is a campaign battle, skip this as the game will load in preset values
    public AI (CharacterSelectController.DifficultySelection difficulty)
    {
        //Use the given difficulty to create an AI. Ignore when this has already been created for a campaign
        if (!GameProfile.PlayingACampaignBattle)
        {
            if (difficulty == CharacterSelectController.DifficultySelection.Easy)
            {
                CreateEasyAI();
            }
            else if (difficulty == CharacterSelectController.DifficultySelection.Medium)
            {
                CreateMediumAI();
            }
            else if (difficulty == CharacterSelectController.DifficultySelection.Hard)
            {
                CreateHardAI();
            }
        }
        AIDifficulty = difficulty;
    }

    //Generates the AI team. Will choose 4 random Endimon for their team and take no items with them
    public void CreateEasyAI()
    {
        int rand; 
        //*Do name generation here

        //Sets item value, will make a null value as easy difficulty won't use any items
        AIItems = new Item[1];
        AIItems[0] = null;

        //Set up creation for making a team
        AITeam = new Endimon[4];
        int counter = 0;
        bool canAdd = true;
        
        //While the team is not full, go through and add Endimon, checking each time if it'll be a duplicate
        while(counter != 4)
        {
            rand = Random.Range(0, 9);
            if (rand > -1 && rand < 10)
            {
                //Check for duplicate
                for(int i = 0; i < counter; i++)
                {
                    if(AITeam[i].GetName() == GameProfile.Roster[rand].GetName())
                    {
                        canAdd = false;
                        break;
                    }
                }
                //If not a dupe
                if (canAdd)
                {
                    AITeam[counter] = GameProfile.CreateEndimonInstance(rand);
                    Debug.Log("AI added to party: " + AITeam[counter].GetName());
                    counter++;
                }
                canAdd = true;
            }
        }
    }

    //Medium AI will generate a team of unique Endimon of all different types, making sure they have 1 each
    //They will take only positive effect items
    public void CreateMediumAI()
    {
        AIItems = new Item[3];
        AITeam = new Endimon[4];
        int counter = 0;
        bool canAdd = true;
        int rand;
        while(counter != 4)
        {
            rand = Random.Range(0, 10);
            for(int i = 0; i < counter; i++)
            {
                if(AITeam[i].GetName() == GameProfile.Roster[rand].GetName() || AITeam[i].GetEndimonType() == GameProfile.Roster[rand].GetEndimonType())
                {
                    canAdd = false;
                    break;
                }
            }
            if(canAdd)
            {
                AITeam[counter] = GameProfile.CreateEndimonInstance(rand);
                Debug.Log("AI added to party: " + AITeam[counter].GetName());
                counter++;
            }
            canAdd = true;
        }
        counter = 0;
        while(counter != 3)
        {
            rand = Random.Range(0, 8);
            if (GameProfile.Items[rand].GetUsabilityTeam())
            {
                AIItems[counter] = GameProfile.CreateItemInstance(rand);
                Debug.Log("Item added to AI inv: " + AIItems[counter].GetItemName());
                counter++;
            }
        }
    }

    //Hard AI will generate a team of unique Endimon in terms of roles, they will select 1 Tank, 1 Attacker, 1 Utility, and 1 Balanced
    public void CreateHardAI()
    {
        AIItems = new Item[5];
        AITeam = new Endimon[4];
        int counter = 0;
        bool canAdd = true;
        string roleToLookFor = "Balanced";
        while (counter != 4)
        {
            //Switch roles everytime we find an Endimon
            if(counter == 1)
            {
                roleToLookFor = "Tank";
            }
            else if(counter == 2)
            {
                roleToLookFor = "Utility";
            }
            else if(counter == 3)
            {
                roleToLookFor = "Attacker";
            }

            int rand = Random.Range(0, 10);
            //Determine if we can add this Endimon
            for (int i = 0; i < counter; i++)
            {
                if (AITeam[i].GetName() == GameProfile.Roster[rand].GetName() || roleToLookFor != GameProfile.Roster[rand].GetRole())
                {
                    canAdd = false;
                }
            }

            if (canAdd)
            {
                AITeam[counter] = GameProfile.CreateEndimonInstance(rand);
                Debug.Log("AI added to party: " + AITeam[counter].GetName());
                counter++;
            }
            canAdd = true;
        }

        counter = 0;
        Debug.Log("Finished filling in party, now to do items");
        while (counter != 5)
        {
            int rand = Random.Range(0, 8);
            Debug.Log("Rand: " + rand);
            AIItems[counter] = GameProfile.CreateItemInstance(rand);
            Debug.Log("Item added to AI inv: " + AIItems[counter].GetItemName());
            counter++;
        }
    }

    //Trainer will randomly use moves without care, 90% chance to attack, 10% to swap at random 0% chance to use item and special moves
    public IEnumerator DecidingActionEasy(BattleController bc, Endimon AIEndimon, Animator[] anims, Image[] GlobalStatuses)
    {
        int rand;
        Move moveToUse = null;
        Endimon endimonToTarget = null;

        //Will first decide whether or not to switch out an Endimon on the team or do damage
        rand = Random.Range(1, 10);
        if(rand == 1 && CanAISwap())    //Swap out Endimon
        {
            //Randomly select and Endimon to put in
            while (true)
            {
                rand = Random.Range(1, 4);
                if (AITeam[rand].GetCurrentHP() > 0 && AITeam[rand].GetName() != ActiveAIEndimon1.GetName() && AITeam[rand].GetName() != ActiveAIEndimon2.GetName())
                {
                    endimonToTarget = AITeam[rand];
                    Debug.Log("Swapping in for AI: " + endimonToTarget.GetName());
                    break;
                }
            }

            //Figure out which Endimon on the field to swap out
            rand = Random.Range(1, 11);
            if(rand > 5)
            {
                //Setting up to swap
                CameraController.SetGameStatus("Attacking", GetActiveEndimon1());
                bc.BattleTextPanel.SetActive(true);
                bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon1, endimonToTarget);
                yield return new WaitForSeconds(1.5f);

                //Swapping
                SwapEndimonOnTurn(1, endimonToTarget);
                Debug.Log("Before changing UI, active endimon is: " + ActiveAIEndimon1.GetName() + " it's box number is: " + ActiveAIEndimon1.GetActiveNumber());
                bc.SwitchEndimonUI(ActiveAIEndimon1, ActiveAIEndimon1.GetActiveNumber());

                //Waiting action to finish up then switching back
                yield return new WaitForSeconds(1f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
            }
            else
            {
                //Setting up to swap
                CameraController.SetGameStatus("Attacking", GetActiveEndimon2());
                bc.BattleTextPanel.SetActive(true);
                bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon2, endimonToTarget);
                yield return new WaitForSeconds(1.5f);

                //Swapping
                SwapEndimonOnTurn(2, endimonToTarget);
                Debug.Log("Before changing UI, active endimon is: " + ActiveAIEndimon2.GetName() + " it's box number is: " + ActiveAIEndimon2.GetActiveNumber());
                bc.SwitchEndimonUI(ActiveAIEndimon2, ActiveAIEndimon2.GetActiveNumber());

                //Waiting action to finish up then switching back
                yield return new WaitForSeconds(1f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
            }
        }

        //We will attack otherwise
        else
        {
            //The AI only can choose between the 2 damage moves
            rand = Random.Range(1, 11);
            if(rand > 5)
            {
                //They will use move 1
                Debug.Log("Used move one: " + AIEndimon.GetEndimonMove1().GetMoveName());
                moveToUse = AIEndimon.GetEndimonMove1();

                //Preparing to attack
                CameraController.SetGameStatus("Attacking", AIEndimon);

                //Determine the target ahead of time for the textbox
                rand = Random.Range(1, 11);
                if (rand > 5 || GameProfile.CurrentCharacter.GetActiveEndimon2().GetCurrentHP() <= 0) {
                    bc.BattleTextPanel.SetActive(true);
                    bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, moveToUse, GameProfile.CurrentCharacter.GetActiveEndimon1());
                }
                else
                {
                    bc.BattleTextPanel.SetActive(true);
                    bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, moveToUse, GameProfile.CurrentCharacter.GetActiveEndimon2());
                }
                yield return new WaitForSeconds(1.5f);

                //Playing attack animation
                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(0));
                bc.CastElementalEffect(moveToUse, AIEndimon);
            }
            else
            {
                //They will use move 2
                Debug.Log("Used move two: " + AIEndimon.GetEndimonMove2().GetMoveName());
                moveToUse = AIEndimon.GetEndimonMove2();

                //Preparing to attack
                CameraController.SetGameStatus("Attacking", AIEndimon);

                //Determine the target ahead of time for the textbox
                rand = Random.Range(1, 11);
                if (rand > 5 || GameProfile.CurrentCharacter.GetActiveEndimon2().GetCurrentHP() <= 0)
                {
                    bc.BattleTextPanel.SetActive(true);
                    bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, moveToUse, GameProfile.CurrentCharacter.GetActiveEndimon1());
                }
                else
                {
                    bc.BattleTextPanel.SetActive(true);
                    bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, moveToUse, GameProfile.CurrentCharacter.GetActiveEndimon2());
                }
                yield return new WaitForSeconds(1.5f);

                //Playing attack animation
                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(1));
                bc.CastElementalEffect(moveToUse, AIEndimon);
            }
            
            //AI chooses a target (Checks to see if there is one Endimon remaining just in case while deciding a target) 
            if(rand > 5 || GameProfile.CurrentCharacter.GetActiveEndimon2().GetCurrentHP() <= 0)
            {
                //They will use the selected move on the opponent's first Endimon  
                endimonToTarget = GameProfile.CurrentCharacter.GetActiveEndimon1();

                //Switching camera angle
                yield return new WaitForSeconds(.5f);
                CameraController.SetGameStatus("Defending", endimonToTarget);
                yield return new WaitForSeconds(.5f);

                //Do the damage action
                int damage = AIEndimon.UseDamageMove(AIEndimon, moveToUse, endimonToTarget, GlobalStatuses, false);
                bc.BattleText.text = BattleTextController.DefendDamageText(AIEndimon, moveToUse, endimonToTarget, damage, bc.CheckShadowCastStatus());
                AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
                endimonToTarget.TakeDamage(damage);
                bc.UpdateHealthValues();
                bc.SetTempEndimon(endimonToTarget);
                if(endimonToTarget.GetActiveNumber() == 0 || endimonToTarget.GetActiveNumber() == 1)
                {
                    bc.PlayParticleAtLocation(endimonToTarget, true, 5, 7f, 5f);
                }
                else
                {
                    bc.PlayParticleAtLocation(endimonToTarget, true, 5, 7f, -5f);
                }
                anims[endimonToTarget.GetActiveNumber()].Play(endimonToTarget.GetAnimationName(3));
                Debug.Log("We used a damage move on the first target");

                //Setting things back to normal
                yield return new WaitForSeconds(2f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
            }
            else
            {
                //They will use the selected move on the opponent's second Endimon
                endimonToTarget = GameProfile.CurrentCharacter.GetActiveEndimon2();

                //Switching camera angle
                yield return new WaitForSeconds(.5f);
                CameraController.SetGameStatus("Defending", endimonToTarget);
                yield return new WaitForSeconds(.5f);

                //Do the damage action
                int damage = AIEndimon.UseDamageMove(AIEndimon, moveToUse, endimonToTarget, GlobalStatuses, false);
                bc.BattleText.text = BattleTextController.DefendDamageText(AIEndimon, moveToUse, endimonToTarget, damage, bc.CheckShadowCastStatus());
                AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
                endimonToTarget.TakeDamage(damage);
                bc.UpdateHealthValues();
                bc.SetTempEndimon(endimonToTarget);
                if (endimonToTarget.GetActiveNumber() == 0 || endimonToTarget.GetActiveNumber() == 1)
                {
                    bc.PlayParticleAtLocation(endimonToTarget, true, 5, 7f, 5f);
                }
                else
                {
                    bc.PlayParticleAtLocation(endimonToTarget, true, 5, 7f, -5f);
                }
                anims[endimonToTarget.GetActiveNumber()].Play(endimonToTarget.GetAnimationName(3));
                Debug.Log("We used a damage move on the second target");

                //Setting things back to normal
                yield return new WaitForSeconds(2.5f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
            }
        }
        bc.BattleTextPanel.SetActive(false);
        bc.UpdateHealthValues();
        bc.EndAITurn();
    }

    //Trainer will take the persona of the Easy/Hard AI
    public IEnumerator DecidingActionMedium(BattleController bc, Endimon AIEndimon, Animator[] anims, Image[] GlobalStatuses)
    {
        //Empty Function (This is handled via the Update method in BattleController)
        return null;
    }

    public IEnumerator DecidingActionHard(BattleController bc, Endimon AIEndimon, Animator[] anims, Image[] GlobalStatuses)
    {
        //AI first determines who is the threat on the field at the moment. It's goal is to take down the Endimon with the lowest ratio of HP/Attack
        //The AI will solely target that Endimon first, this will be recalculated each turn in case of swap outs
        Endimon P1E = GameProfile.GetCurrentCharacter().GetActiveEndimon1();
        Endimon P2E = GameProfile.GetCurrentCharacter().GetActiveEndimon2();
        Endimon AI1E = GameProfile.GetCurrentAI().GetEndimon(0);
        Endimon AI2E = GameProfile.GetCurrentAI().GetEndimon(1);
        Endimon AI3E = GameProfile.GetCurrentAI().GetEndimon(2);
        Endimon AI4E = GameProfile.GetCurrentAI().GetEndimon(3);


        //The AI will typically hone in on the target it finds the most dangerous, and attack it until its dead rather than do damage to both
        //It must find the most dangerous opponent on the field and it will do this by measuring the lower HP vs the attack they do
        //The AI mostly cares about getting rid of Endimon as fast as possible, and take into account the damage they can do
        int priority1 = 10000;
        int priority2 = 10000;
        if (P1E.GetCurrentHP() > 0) {
            priority1 = P1E.GetCurrentHP() - P1E.GetAttack();
        }
        if (P2E.GetCurrentHP() > 0) {
            priority2 = P2E.GetCurrentHP() - P2E.GetAttack();
        }

        //Smallest value gets target
        if (priority1 < priority2)
        {
            Target = P1E;
            Debug.Log("Target: " + P1E.GetName() + " Previous: " + LastTarget);
        }
        else
        {
            Target = P2E;
            Debug.Log("Target: " + P2E.GetName() + " Previous: " + LastTarget);
        }

        //We will first precalculate the damage move, if this kills this is the move we take
        bc.BattleTextPanel.SetActive(true);
        Move KillingMove = CanKillTarget(AIEndimon, Target, GlobalStatuses);
        if (KillingMove != null || AIEndimon.GetEndimonPostiveEffect() != Endimon.StatusEffects.Nothing)
        {
            //If we have a positive effect on, we still need to choose a move
            if(KillingMove == null)
            {
                int move1Dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove1(), Target, GlobalStatuses, true);
                int move2Dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove2(), Target, GlobalStatuses, true);
                if(move1Dmg > move2Dmg)
                {
                    KillingMove = AIEndimon.GetEndimonMove1();
                }
                else
                {
                    KillingMove = AIEndimon.GetEndimonMove2();
                }
            }
            //Preparing move
            CameraController.SetGameStatus("Attacking", AIEndimon);
            bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, KillingMove, Target);
            yield return new WaitForSeconds(1.5f);

            //Doing the actions to show a cast
            if(KillingMove.GetMoveName() == AIEndimon.GetEndimonMove1().GetMoveName())
            {
                bc.CastElementalEffect(AIEndimon.GetEndimonMove1(), AIEndimon);
                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(0));
            }
            else
            {
                bc.CastElementalEffect(AIEndimon.GetEndimonMove2(), AIEndimon);
                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(1));
            }

            //Switching camera angle
            yield return new WaitForSeconds(.5f);
            CameraController.SetGameStatus("Defending", Target);
            yield return new WaitForSeconds(.5f);

            //Applying the damage 
            int dmg = AIEndimon.UseDamageMove(AIEndimon, KillingMove, Target, GlobalStatuses, false);
            bc.BattleText.text = BattleTextController.DefendDamageText(AIEndimon, KillingMove, Target, dmg, bc.CheckShadowCastStatus());
            Target.TakeDamage(dmg);
            bc.UpdateHealthValues();
            bc.SetTempEndimon(Target);
            AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
            bc.PlayParticleAtLocation(Target, true, 5, 7f, 5f);
            anims[Target.GetActiveNumber()].Play(Target.GetAnimationName(3));

            //Setting things back to normal
            yield return new WaitForSeconds(2f);
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
        }
        
        else { 
            //As long as we can swap, once we switch to a new target we should consider switching in something to counter or protect a weakness
            if ((CanAISwap() && Target.GetName() != LastTarget) && ((IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetEndimonWeakness() == Target.GetEndimonType())
                || (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonWeakness() == Target.GetEndimonType()) || FindStrongerEndimon(Target))) {

                //Figure out who to swap, first check if first slotted Endimon is weak to Target
                if (IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetEndimonWeakness() == Target.GetEndimonType())
                {
                    //Preparing swap
                    CameraController.SetGameStatus("Attacking", ActiveAIEndimon1);
                    bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon1, GetNonWeakEndimon(ActiveAIEndimon1.GetEndimonWeakness()));
                    yield return new WaitForSeconds(1.5f);

                    SwapEndimonOnTurn(1, GetNonWeakEndimon(ActiveAIEndimon1.GetEndimonWeakness()));
                    bc.SwitchEndimonUI(ActiveAIEndimon1, ActiveAIEndimon1.GetActiveNumber());

                    //Waiting action to finish up then switching back
                    yield return new WaitForSeconds(1f);
                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                }

                //The second slotted Endimon is weak to the target, we will swap with a different Endimon
                else if (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonWeakness() == Target.GetEndimonType())
                {
                    //Preparing swap
                    CameraController.SetGameStatus("Attacking", ActiveAIEndimon2);
                    bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon2, GetNonWeakEndimon(ActiveAIEndimon2.GetEndimonWeakness()));
                    yield return new WaitForSeconds(1.5f);

                    SwapEndimonOnTurn(2, GetNonWeakEndimon(ActiveAIEndimon2.GetEndimonWeakness()));
                    bc.SwitchEndimonUI(ActiveAIEndimon2, ActiveAIEndimon2.GetActiveNumber());

                    //Waiting action to finish up then switching back
                    yield return new WaitForSeconds(1f);
                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                }
                //We've determined an Endimon in reserve would be better on the field (Swap it in for one that isn't the same type)
                else if (FindStrongerEndimon(Target))
                {
                    //Look to swap the opposite Endimon's turn
                    if(IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1 == AIEndimon)
                    {
                        //Preparing swap
                        CameraController.SetGameStatus("Attacking", ActiveAIEndimon2);
                        bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon2, GetStrongerEndimon(Target));
                        yield return new WaitForSeconds(1.5f);

                        SwapEndimonOnTurn(2, GetStrongerEndimon(Target));
                        bc.SwitchEndimonUI(ActiveAIEndimon2, ActiveAIEndimon2.GetActiveNumber());

                        //Waiting action to finish up then switching back
                        yield return new WaitForSeconds(1f);
                        CameraController.SetGameStatus("PlayerAwaitTurn", null);
                    }   
                    else
                    {
                        //Preparing swap
                        CameraController.SetGameStatus("Attacking", ActiveAIEndimon1);
                        bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon1, GetStrongerEndimon(Target));
                        yield return new WaitForSeconds(1.5f);

                        SwapEndimonOnTurn(1, GetStrongerEndimon(Target));
                        bc.SwitchEndimonUI(ActiveAIEndimon1, ActiveAIEndimon1.GetActiveNumber());

                        //Waiting action to finish up then switching back
                        yield return new WaitForSeconds(1f);
                        CameraController.SetGameStatus("PlayerAwaitTurn", null);
                    }
                }
            }
            else
            {
                //If we should not swap, let's decide between an item/ability/attack
                //Higher chance to attack, utilize other abilities/items sparingly
                while (true)
                {
                    int rand = Random.Range(1, 6);
                    //We will attack
                    if (rand < 4 || HasEffectiveMove(AIEndimon, Target))
                    {
                        int move1Dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove1(), Target, GlobalStatuses, true);
                        int move2Dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove2(), Target, GlobalStatuses, true);
                        int dmg = 0;

                        //Preparing move
                        CameraController.SetGameStatus("Attacking", AIEndimon);

                        if (move1Dmg > move2Dmg)
                        {
                            dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove1(), Target, GlobalStatuses, false);
                            bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, AIEndimon.GetEndimonMove1(), Target);
                            yield return new WaitForSeconds(1.5f);
                            bc.CastElementalEffect(AIEndimon.GetEndimonMove1(), AIEndimon);
                            anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(0));
                        }
                        else
                        {
                            dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove2(), Target, GlobalStatuses, true);
                            bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, AIEndimon.GetEndimonMove2(), Target);
                            yield return new WaitForSeconds(1.5f);
                            bc.CastElementalEffect(AIEndimon.GetEndimonMove2(), AIEndimon);
                            anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(1));
                        }

                        //Switching camera angle
                        yield return new WaitForSeconds(.5f);
                        CameraController.SetGameStatus("Defending", Target);
                        yield return new WaitForSeconds(.5f);

                        Target.TakeDamage(dmg);
                        bc.UpdateHealthValues();
                        bc.BattleText.text = BattleTextController.DefendDamageText(AIEndimon, AIEndimon.GetEndimonMove1(), Target, dmg, bc.CheckShadowCastStatus());
                        AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
                        bc.PlayParticleAtLocation(Target, true, 5, 7f, 5f);
                        anims[Target.GetActiveNumber()].Play(Target.GetAnimationName(3));
                        Debug.Log("Hard AI: Used a damaging move ");

                        //Waiting action to finish up then switching back
                        yield return new WaitForSeconds(2f);
                        CameraController.SetGameStatus("PlayerAwaitTurn", null);
                        break;
                    }
                    else if (rand == 4 && HaveAnyItems())
                    {
                        rand = Random.Range(0, AIItems.Length);

                        int temp = 0;   //Looks to see if we couldn't find a usable item
                        //Take any item unless it is healing and both Endimon are full health
                        while(AIItems[rand] == null || NeedsHealing(AIItems[rand]))
                        {
                            rand = Random.Range(0, AIItems.Length);
                            temp++;
                            if(temp > 10)
                            {
                                break;
                            }
                        }
                        if(temp < 10) { 
                            //If the positive effect can be used on either of the Endimon, then place it
                            if (AIItems[rand].GetUsabilityTeam() && ((IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing)
                                || (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing))) {

                                //Preparing move
                                CameraController.SetGameStatus("Attacking", AIEndimon);
                                //Determine who the presumed target will be for the textbox
                                if(IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing)
                                {
                                    bc.BattleText.text = BattleTextController.ItemUsedText(AIEndimon, AIItems[rand], ActiveAIEndimon2);
                                }
                                else
                                {
                                    bc.BattleText.text = BattleTextController.ItemUsedText(AIEndimon, AIItems[rand], ActiveAIEndimon1);
                                }
                                yield return new WaitForSeconds(1.5f);

                                bc.PlayParticleAtLocation(AIEndimon, false, 0, 1f, -2.5f);
                                AudioSource.PlayClipAtPoint(Audio.UseItem, GameObject.Find("MainCamera").transform.position);
                                yield return new WaitForSeconds(1f);

                                if ((IsEndimonAlive(ActiveAIEndimon1) && AIItems[rand].GetHealing() && ActiveAIEndimon1.GetCurrentHP() != ActiveAIEndimon1.GetHealth()) 
                                    || (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing))
                                {
                                    //Switching camera angle
                                    yield return new WaitForSeconds(.5f);
                                    CameraController.SetGameStatus("Defending", ActiveAIEndimon2);
                                    yield return new WaitForSeconds(.5f);
                               
                                    Item UsedItem = AddTurnToItem(ActiveAIEndimon2, AIItems[rand], AIEndimon);
                                    int particleIndex = bc.FindLocationForItemParticle(AIEndimon, ActiveAIEndimon2, UsedItem);
                                    UseItem(AIItems[rand], ActiveAIEndimon2);
                                    RemoveItem(AIItems[rand]);
                                    bc.UpdateStatusEffectBoxes(ActiveAIEndimon2, particleIndex);

                                    //Waiting action to finish up then switching back
                                    yield return new WaitForSeconds(2.5f);
                                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                                }
                                else
                                {
                                    //Switching camera angle
                                    yield return new WaitForSeconds(.5f);
                                    CameraController.SetGameStatus("Defending", ActiveAIEndimon1);
                                    yield return new WaitForSeconds(.5f);

                                    Item UsedItem = AddTurnToItem(ActiveAIEndimon1, AIItems[rand], AIEndimon);
                                    int particleIndex = bc.FindLocationForItemParticle(AIEndimon, ActiveAIEndimon1, UsedItem);
                                    UseItem(AIItems[rand], ActiveAIEndimon1);
                                    RemoveItem(AIItems[rand]);
                                    bc.UpdateStatusEffectBoxes(ActiveAIEndimon1, particleIndex);

                                    //Waiting action to finish up then switching back
                                    yield return new WaitForSeconds(2.5f);
                                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                                }
                            }
                            //If its a negative effect item and the targetted endimon has no debuff then place it
                            else if(!AIItems[rand].GetUsabilityTeam() && Target.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing)
                            {
                                //Preparing move
                                CameraController.SetGameStatus("Attacking", AIEndimon);
                                bc.BattleText.text = BattleTextController.ItemUsedText(AIEndimon, AIItems[rand], Target);
                                yield return new WaitForSeconds(1.5f);

                                bc.PlayParticleAtLocation(AIEndimon, false, 0, 1f, -2.5f);
                                AudioSource.PlayClipAtPoint(Audio.UseItem, GameObject.Find("MainCamera").transform.position);
                                yield return new WaitForSeconds(1f);

                                //Switching camera angle
                                yield return new WaitForSeconds(.5f);
                                CameraController.SetGameStatus("Defending", Target);
                                yield return new WaitForSeconds(.5f);

                                Item UsedItem = AddTurnToItem(Target, AIItems[rand], AIEndimon);

                                UseItem(UsedItem, Target);
                                RemoveItem(AIItems[rand]);
                                int particleIndex = bc.FindLocationForItemParticle(AIEndimon, Target, UsedItem);
                                bc.UpdateStatusEffectBoxes(Target, particleIndex);
                                //Waiting action to finish up then switching back
                                yield return new WaitForSeconds(2.5f);
                                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            }
                            break;
                        }
                    }
                    else if(rand == 5)
                    {
                        if(!AIEndimon.GetEndimonMove3().GetHarmful() && !AIEndimon.GetEndimonMove3().GetTargetable() && CheckGlobals(AIEndimon.GetEndimonMove3(), GlobalStatuses))
                        {
                            //Preparing for move
                            CameraController.SetGameStatus("Attacking", AIEndimon);
                            bc.BattleText.text = BattleTextController.SpecialAbilityText(AIEndimon, AIEndimon.GetEndimonMove3(), null);
                            yield return new WaitForSeconds(1.5f);

                            //Play cast animation and then change angles
                            anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(2));
                            yield return new WaitForSeconds(.5f);
                            CameraController.SetGameStatus("Globals", null);
                            bc.BattleText.text = BattleTextController.GlobalText(AIEndimon.GetEndimonMove3().GetMoveName());

                            //Global effect can be used here
                            int particleIndex = AIEndimon.UseSpecialMove(AIEndimon, null, AIEndimon.GetEndimonMove3());
                            bc.AddGlobalEffect(particleIndex);
                            bc.UpdateStatusEffectBoxes(AIEndimon, particleIndex);

                            yield return new WaitForSeconds(3f);
                            CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            break;
                        }
                        else if(AIEndimon.GetEndimonMove3().GetHarmful() && Target.GetEndimonNegativeEffect() == Endimon.StatusEffects.Nothing)
                        {
                            //Preparing move
                            CameraController.SetGameStatus("Attacking", AIEndimon);
                            bc.BattleText.text = BattleTextController.SpecialAbilityText(AIEndimon, AIEndimon.GetEndimonMove3(), Target);
                            yield return new WaitForSeconds(1.5f);

                            anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(2));

                            //Switching camera angle
                            yield return new WaitForSeconds(.5f);
                            CameraController.SetGameStatus("Defending", Target);
                            yield return new WaitForSeconds(.5f);

                            //Use this effect as it won't override anything
                            int particleIndex = AIEndimon.UseSpecialMove(AIEndimon, Target, AIEndimon.GetEndimonMove3());
                            if (particleIndex == -1)
                            {
                                bc.BattleText.text = "The attack failed";
                            }
                            bc.CastAbilityEffect(particleIndex, Target);
                            bc.UpdateStatusEffectBoxes(Target, particleIndex);

                            //Waiting action to finish up then switching back
                            yield return new WaitForSeconds(1.5f);
                            CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            break;

                        }
                        else if(!AIEndimon.GetEndimonMove3().GetHarmful() && ((IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing) || (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing))) {

                            if (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing)
                            {
                                //Preparing move
                                CameraController.SetGameStatus("Attacking", AIEndimon);
                                bc.BattleText.text = BattleTextController.SpecialAbilityText(AIEndimon, AIEndimon.GetEndimonMove3(), ActiveAIEndimon2);
                                yield return new WaitForSeconds(1.5f);

                                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(2));

                                //Switching camera angle
                                yield return new WaitForSeconds(.5f);
                                CameraController.SetGameStatus("Defending", ActiveAIEndimon2);
                                yield return new WaitForSeconds(.5f);

                                //Place it on AI2
                                int particleIndex = AIEndimon.UseSpecialMove(AIEndimon, ActiveAIEndimon2, AIEndimon.GetEndimonMove3());
                                bc.CastAbilityEffect(particleIndex, ActiveAIEndimon2);
                                bc.UpdateStatusEffectBoxes(ActiveAIEndimon2, particleIndex);

                                //Waiting action to finish up then switching back
                                yield return new WaitForSeconds(1.5f);
                                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            }
                            else
                            {
                                //Preparing move
                                CameraController.SetGameStatus("Attacking", AIEndimon);
                                bc.BattleText.text = BattleTextController.SpecialAbilityText(AIEndimon, AIEndimon.GetEndimonMove3(), ActiveAIEndimon1);
                                yield return new WaitForSeconds(1.5f);

                                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(2));

                                //Switching camera angle
                                yield return new WaitForSeconds(.5f);
                                CameraController.SetGameStatus("Defending", ActiveAIEndimon1);
                                yield return new WaitForSeconds(.5f);

                                //Place it on itself
                                int particleIndex = AIEndimon.UseSpecialMove(AIEndimon, AIEndimon, AIEndimon.GetEndimonMove3());
                                bc.CastAbilityEffect(particleIndex, ActiveAIEndimon1);
                                bc.UpdateStatusEffectBoxes(AIEndimon, particleIndex);

                                //Waiting action to finish up then switching back
                                yield return new WaitForSeconds(1.5f);
                                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            }
                            break;
                        }
                    }
                }
            }
        }
        LastTarget = Target.GetName();
        bc.BattleTextPanel.SetActive(false);
        bc.UpdateHealthValues();
        bc.EndAITurn();
    }

    //Checks count of alive Endimon, a value over three allows the AI to swap
    public bool CanAISwap()
    {
        int alive = 0;
        for(int i = 0; i < 4; i++)
        {
            if(IsEndimonAlive(AITeam[i]))
            {
                alive++;
            }
        }

        if(alive >= 3)
        {
            return true;
        }
        return false;
    }

    //Will change an active endimon to one that is alive (function called when a death occurs)
    //Return value will tell the program which active Endimon it'll need to update, 0 for if there is none
    public int SwapEndimonOnDeath(Endimon e)
    {
        Endimon NewEndimon = null;
        for (int i = 0; i < 4; i++)
        {
            if (AITeam[i] != ActiveAIEndimon1 && AITeam[i] != ActiveAIEndimon2 && IsEndimonAlive(AITeam[i]))
            {
                NewEndimon = AITeam[i];
                break;
            }
        }

        //All the team's Endimon must be dead so let the program know by giving back 0
        if (NewEndimon == null)
        {
            return 0;
        }

        //Find the active endimon to replace
        //Will transfer over its box position as well as turn status in the rotation of turns
        if (ActiveAIEndimon1.GetName() == e.GetName())
        {
            if(ActiveAIEndimon1.GetEndimonTurnTaken() == true)
            {
                Debug.Log("We determined that this Endimon did already go that we replacing");
                NewEndimon.SetTurnStatus(true);
            }
            NewEndimon.SetActiveNumber(ActiveAIEndimon1.GetActiveNumber());
            Debug.Log("The Endimon being placed in is in box: " + NewEndimon.GetActiveNumber());
            ActiveAIEndimon1 = NewEndimon;
            return 1;
        }
        else
        {
            if (ActiveAIEndimon2.GetEndimonTurnTaken() == true)
            {
                Debug.Log("We determined that this Endimon did already go that we replacing");
                NewEndimon.SetTurnStatus(true);
            }
            NewEndimon.SetActiveNumber(ActiveAIEndimon2.GetActiveNumber());
            Debug.Log("The Endimon being placed in is in box: " + NewEndimon.GetActiveNumber());
            ActiveAIEndimon2 = NewEndimon;
            return 2;
        }
    }

    //Function swaps an Endimon at the request of the player. This function should be called assuming three or more Endimon are alive 
    //Slot is necessary to indicate which component needs to change in the battle screen
    public void SwapEndimonOnTurn(int slot, Endimon e)
    {
        if (slot == 1)
        {
            if (ActiveAIEndimon1.GetEndimonTurnTaken() == true)
            {
                e.SetTurnStatus(true);
            }
            else
            {
                e.SetTurnStatus(false);
            }
            ActiveAIEndimon1 = e;
            ActiveAIEndimon1.SetActiveNumber(2);
        }
        else if (slot == 2)
        {
            if (ActiveAIEndimon2.GetEndimonTurnTaken() == true)
            {
                e.SetTurnStatus(true);
            }
            else
            {
                e.SetTurnStatus(false);
            }
            ActiveAIEndimon2 = e;
            ActiveAIEndimon2.SetActiveNumber(3);
        }
    }

    //Determines if there is an Endimon in reserve that counters one of the human player's Endimon
    public bool FindStrongerEndimon(Endimon e)
    {
        for(int i = 0; i < AITeam.Length; i++)
        {
            if(IsEndimonAlive(AITeam[i]) && AITeam[i].GetName() != ActiveAIEndimon1.GetName() && AITeam[i].GetName() != ActiveAIEndimon2.GetName() && 
                AITeam[i].GetEndimonType() == e.GetEndimonWeakness())
            {
                return true;
            }
        }
        return false;
    }

    //Returns an Endimon of this type (Used to find a stronger Endimon)
    public Endimon GetStrongerEndimon(Endimon e)
    {
        for(int i = 0; i < AITeam.Length; i++)
        {
            if(IsEndimonAlive(AITeam[i]) && AITeam[i].GetName() != ActiveAIEndimon1.GetName() && AITeam[i].GetName() != ActiveAIEndimon2.GetName() &&
                AITeam[i].GetEndimonType() == e.GetEndimonWeakness()) {
                return AITeam[i];
            }
        }
        return null;
    }

    //Returns an Endimon that is not of the type you pass in (typically a weakness)
    public Endimon GetNonWeakEndimon(Endimon.Endimontypes weakness)
    {
        for(int i = 0; i < AITeam.Length; i++)
        {
            if (IsEndimonAlive(AITeam[i]) && AITeam[i].GetName() != ActiveAIEndimon1.GetName() && AITeam[i].GetName() != ActiveAIEndimon2.GetName() &&
                AITeam[i].GetEndimonType() != weakness)
            {
                return AITeam[i];
            }
        }
        return null;
    }

    //Function pre-calculates the damage each move will do and see if it kills the target, returning the one that did if it can
    public Move CanKillTarget(Endimon Attacker, Endimon Defender, Image[] Statuses)
    {
        int hp = Defender.GetCurrentHP();
        int dmg1 = Attacker.UseDamageMove(Attacker, Attacker.GetEndimonMove1(), Defender, Statuses, true);
        int dmg2 = Attacker.UseDamageMove(Attacker, Attacker.GetEndimonMove2(), Defender, Statuses, true);
        if (hp - dmg1 < 1)
        {
            return Attacker.GetEndimonMove1();
        }
        else if(hp - dmg2 < 1)
        {
            return Attacker.GetEndimonMove2();
        }
        else
        {
            return null;
        }
    }

    //Function gives back T/F based upon if the endimon attacking has a type effective move against the target
    public bool HasEffectiveMove(Endimon Attacker, Endimon Defender)
    {
        if(Attacker.GetEndimonMove1().GetMoveType() == Defender.GetEndimonWeakness() || Attacker.GetEndimonMove2().GetMoveType() == Defender.GetEndimonWeakness())
        {
            return true;
        }
        return false;
    }

    //Function determines if either of the 2 Endimon have any HP missing (assuming they are still alive)
    public bool NeedsHealing(Item item)
    {
        if(item.GetHealing() && (IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetCurrentHP() != ActiveAIEndimon1.GetHealth() ||
            (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetCurrentHP() != ActiveAIEndimon2.GetHealth())))
        {
            return true;
        }
        return false;
    }

    //Function that is called to determine if the AI has any Endimon left alive in their party
    public bool IsTeamAlive()
    {
        for (int i = 0; i < 4; i++)
        {
            if (IsEndimonAlive(AITeam[i]))
            {
                return true;
            }
        }
        return false;
    }

    //Helper function to check if an Endimon is alive
    public bool IsEndimonAlive(Endimon e)
    {
        if(e.GetCurrentHP() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Function determines if the AI has used up all of its items
    public bool HaveAnyItems()
    {
        for(int i = 0; i < AIItems.Length; i++)
        {
            if(AIItems[i] != null)
            {
                return true;
            }
        }
        return false;
    }

    //Functions removes an item basd upon the position in the array
    public void RemoveItem(int pos)
    {
        AIItems[pos] = null;
    }

    //Function removes an item based upon the item name
    public void RemoveItem(Item item)
    {
        for (int i = 0; i < 3; i++)
        {
            if (AIItems[i] != null && item.GetItemName() == AIItems[i].GetItemName())
            {
                AIItems[i] = null;
                break;  //Do not erase all of the same named items, just one
            }
        }
    }

    //Determine if the global effect being used has an open space to be used and is not already on the field
    public bool CheckGlobals(Move specialMove, Image[] globalsStatuses)
    {
        //First determine color
        Color32 theColor;
        if(specialMove.GetMoveName() == "Blizzard")
        {
            theColor = new Color32(11, 199, 195, 255);
        }
        else if(specialMove.GetMoveName() == "Ring of Fire")
        {
            theColor = new Color32(174, 3, 17, 255);
        }
        else
        {
            theColor = new Color32(30, 30, 30, 255);
        }

        if((globalsStatuses[8].color == Color.white || globalsStatuses[9].color == Color.white) && (globalsStatuses[8].color != theColor || globalsStatuses[9].color != theColor))
        {
            return true;
        }
        return false;
    }

    //Function adds on an extra turn to an item duration if its using the effect on itself so it does not waste a turn
    public Item AddTurnToItem(Endimon EndimonTakingItem, Item UsedItem, Endimon CurrentEndimon)
    {
        if (EndimonTakingItem == CurrentEndimon && UsedItem.GetUsabilityTeam() && UsedItem.GetItemDuration() > 0)
        {
            UsedItem.SetItemDuration(UsedItem.GetItemDuration() + 1);
        }
        return UsedItem;
    }

    //Function casts the effect of an item
    public void UseItem(Item UsedItem, Endimon TargettedEndimon)
    {
        //Non-instant effect item
        if (UsedItem.GetItemDuration() > 0)
        {
            AudioClip itemClip = Item.DetermineAudioClip(UsedItem.GetItemName());
            AudioSource.PlayClipAtPoint(itemClip, GameObject.Find("MainCamera").transform.position);
            Debug.Log("Endimon: " + TargettedEndimon.GetName() + " hit by " + UsedItem.GetItemName() + " for " + +UsedItem.GetItemDuration() + " turns");
            TargettedEndimon.AddStatusEffect(!UsedItem.GetUsabilityTeam(), UsedItem.GetEffect(), UsedItem.GetItemDuration());
        }
        //Otherwise this is an instant effect so put those here
        else
        {
            if (UsedItem.GetEffect() == Endimon.StatusEffects.HealthRestore)
            {
                //We will hurt the target with negative damage to heal
                int HealthToRestore = (int)(TargettedEndimon.GetHealth() * 0.25 * -1);
                if (TargettedEndimon.GetCurrentHP() + HealthToRestore == TargettedEndimon.GetHealth())
                {
                    HealthToRestore = (int)(TargettedEndimon.GetHealth() - TargettedEndimon.GetCurrentHP() * -1);

                }
                AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
                TargettedEndimon.TakeDamage(HealthToRestore);
            }
            else if (UsedItem.GetEffect() == Endimon.StatusEffects.LargeHealthRestore)
            {
                //We will hurt the target with negative damage to heal
                int HealthToRestore = (int)(TargettedEndimon.GetHealth() * 0.5 * -1);
                if (TargettedEndimon.GetCurrentHP() + HealthToRestore == TargettedEndimon.GetHealth())
                {
                    HealthToRestore = (int)(TargettedEndimon.GetHealth() - TargettedEndimon.GetCurrentHP() * -1);
                }
                AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
                TargettedEndimon.TakeDamage(HealthToRestore);
                TargettedEndimon.SetDefense(-15);   //Endimon loses part of its defense for getting healed this amount
            }
        }
    }

    //SETTERS
    public void SetAITeam(Endimon[] theTeam) { AITeam = theTeam; }
    public void SetAIItems(Item[] theItems) { AIItems = theItems; }
    public void SetAIName(string theName) { AIName = theName; }
    public void SetAIDifficulty(CharacterSelectController.DifficultySelection difficulty) { AIDifficulty = difficulty; }
    public void SetActiveEndimon1(int index) { ActiveAIEndimon1 = AITeam[index]; }
    public void SetActiveEndimon2(int index) { ActiveAIEndimon2 = AITeam[index]; }
    public void SetActiveEndimon1(Endimon e) { ActiveAIEndimon1 = e; }
    public void SetActiveEndimon2(Endimon e) { ActiveAIEndimon2 = e; }

    //GETTERS
    public Endimon[] GetAITeam() { return AITeam; }
    public Item[] GetAIItems() { return AIItems; }
    public string GetAIName() { return AIName; }
    public CharacterSelectController.DifficultySelection GetAIDifficulty() { return AIDifficulty; }
    public Endimon GetEndimon(int index) { return AITeam[index]; }
    public Endimon GetActiveEndimon1() { return ActiveAIEndimon1; }
    public Endimon GetActiveEndimon2() { return ActiveAIEndimon2; }
}
