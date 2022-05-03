using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//Holds the data for an AI player in the game
public class AI
{
    private Endimon[] AITeam;           //Roster of 4 Endimon
    private Item[] AIItems;             //Selected Items (0-5 depending on difficulty)
    private Endimon ActiveAIEndimon1;   //First slotted Endimon in combat
    private Endimon ActiveAIEndimon2;   //Second slotted Endimon in combat
    private Endimon Target;             //The current Target of the AI in combat (Hard AI only)
    private string LastTarget = "";     //The name of the Endimon who was previously being targeted by the AI
    private CharacterSelectController.DifficultySelection AIDifficulty;     //AI's difficulty (Easy/Medium/Hard)

    //Constructs an AI based upon the selected difficulty
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

        //Sets up storage for Items and Endimon 
        AIItems = new Item[1];
        AIItems[0] = null;
        AITeam = new Endimon[4];
        int counter = 0;    //Total in party already
        bool canAdd = true; //Endimon is allowed to be added to team
        int rand = 0;
        
        //While the team is not full, go through and add Endimon, checking each time if it'll be a duplicate
        while(counter != 4)
        {
            rand = Random.Range(0, 10);
            if (rand > -1 && rand < 10)
            {
                //Check for duplicate (not allowed)
                for(int i = 0; i < counter; i++)
                {
                    if(AITeam[i].GetName() == GameProfile.Roster[rand].GetName())
                    {
                        canAdd = false;
                        break;
                    }
                }
                //If not a dupe Endimon, insert onto team
                if (canAdd)
                {
                    AITeam[counter] = GameProfile.CreateEndimonInstance(rand);
                    counter++;
                }
                canAdd = true;
            }
        }
    }

    //Medium AI will generate a team of unique Endimon of all different types, and take 3 positive effect items
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

            //If not a duplicate and is a unique element, add to the team
            if(canAdd)
            {
                AITeam[counter] = GameProfile.CreateEndimonInstance(rand);
                counter++;
            }
            canAdd = true;
        }

        //Reset and now search through items
        counter = 0;
        while(counter != 3)
        {
            rand = Random.Range(0, 8);
            //Only look for effects that can be used on their own team
            if (GameProfile.Items[rand].GetUsabilityTeam())
            {
                AIItems[counter] = GameProfile.CreateItemInstance(rand);
                counter++;
            }
        }
    }

    //Hard AI will generate a team of unique Endimon in terms of roles, they will select 1 each from Tank, Balanced, Attacker, and Utility
    //They will also get 5 items of their own choosing
    public void CreateHardAI()
    {
        AIItems = new Item[5];
        AITeam = new Endimon[4];
        int counter = 0;
        bool canAdd = true;
        string roleToLookFor = "Balanced";  //Specifies what Endimmon role that is accepted

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

            //If it met the requirements to be added, insert into the array
            if (canAdd)
            {
                AITeam[counter] = GameProfile.CreateEndimonInstance(rand);
                counter++;
            }
            canAdd = true;
        }

        //Reset and now pick 5 items
        counter = 0;
        while (counter != 5)
        {
            int rand = Random.Range(0, 8);
            AIItems[counter] = GameProfile.CreateItemInstance(rand);
            counter++;
        }
    }

    //AI will randomly use moves without care, 80% chance to attack, 20% to swap at random 0% chance to use item and special moves
    public IEnumerator DecidingActionEasy(BattleController bc, Endimon AIEndimon, Animator[] anims, Image[] GlobalStatuses)
    {
        int rand;
        Move moveToUse = null;
        Endimon endimonToTarget = null;

        //Will first decide whether or not to switch out an Endimon on the team or do damage
        rand = Random.Range(1, 10);

        //Determined we will swap some Endimon out
        if(rand < 3 && CanAISwap())
        {
            //Randomly select and Endimon to put in
            while (true)
            {
                rand = Random.Range(1, 4);
                //Search for an Endimon that is alive and is not on the field right now
                if (AITeam[rand].GetCurrentHP() > 0 && AITeam[rand].GetName() != ActiveAIEndimon1.GetName() && AITeam[rand].GetName() != ActiveAIEndimon2.GetName())
                {
                    endimonToTarget = AITeam[rand];
                    break;
                }
            }

            //Figure out which Endimon on the field to swap out
            rand = Random.Range(1, 11);

            //Swap out the first slotted Endimon on the field
            if(rand > 5)
            {
                //Setting up to swap
                CameraController.SetGameStatus("Attacking", GetActiveEndimon1());
                bc.BattleTextPanel.SetActive(true);
                bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon1, endimonToTarget);
                yield return new WaitForSeconds(1.5f);

                //Swapping
                SwapEndimonOnTurn(1, endimonToTarget);
                bc.SwitchEndimonUI(ActiveAIEndimon1, ActiveAIEndimon1.GetActiveNumber());

                //Waiting action to finish up then switching back
                yield return new WaitForSeconds(1f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
            }
            //Swap the second slotted Endimon instead
            else
            {
                //Setting up to swap
                CameraController.SetGameStatus("Attacking", GetActiveEndimon2());
                bc.BattleTextPanel.SetActive(true);
                bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon2, endimonToTarget);
                yield return new WaitForSeconds(1.5f);

                //Swapping
                SwapEndimonOnTurn(2, endimonToTarget);
                bc.SwitchEndimonUI(ActiveAIEndimon2, ActiveAIEndimon2.GetActiveNumber());

                //Waiting action to finish up then switching back
                yield return new WaitForSeconds(1f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
            }
        }

        //We will attack if we didn't swap
        else
        {
            //The AI only can choose between the 2 damage moves
            rand = Random.Range(1, 11);
            if(rand > 5)
            {
                //They will use move 1
                moveToUse = AIEndimon.GetEndimonMove1();

                //Preparing to attack
                CameraController.SetGameStatus("Attacking", AIEndimon);

                //Determine the target ahead of time for the textbox
                rand = Random.Range(1, 11);
                //Check for confusion, if so, then apply the damage to itself
                if (AIEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Confusion && Random.Range(1, 10) > 7)
                {
                    endimonToTarget = AIEndimon;
                }
                else if (rand > 5 || GameProfile.CurrentCharacter.GetActiveEndimon2().GetCurrentHP() <= 0)
                {
                    endimonToTarget = GameProfile.GetCurrentCharacter().GetActiveEndimon1();
                }
                else
                {
                    endimonToTarget = GameProfile.GetCurrentCharacter().GetActiveEndimon2();
                }

                //Insert text into textbox
                bc.BattleTextPanel.SetActive(true);
                bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, moveToUse, endimonToTarget);
                yield return new WaitForSeconds(1.5f);

                //Playing attack animation
                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(0));
                bc.CastElementalEffectParticles(moveToUse, AIEndimon);
            }
            //Using move 2 otherwise
            else
            {
                moveToUse = AIEndimon.GetEndimonMove2();

                //Preparing to attack
                CameraController.SetGameStatus("Attacking", AIEndimon);

                //Determine the target ahead of time for the textbox
                rand = Random.Range(1, 11);
                //Check for confusion, if so, then apply the damage to itself
                if (AIEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Confusion && Random.Range(1, 10) > 7)
                {
                    endimonToTarget = AIEndimon;
                }
                else if (rand > 5 || GameProfile.CurrentCharacter.GetActiveEndimon2().GetCurrentHP() <= 0)
                {
                    endimonToTarget = GameProfile.GetCurrentCharacter().GetActiveEndimon1();
                }
                else
                {
                    endimonToTarget = GameProfile.GetCurrentCharacter().GetActiveEndimon2();
                }

                bc.BattleTextPanel.SetActive(true);
                bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, moveToUse, endimonToTarget);
                yield return new WaitForSeconds(1.5f);

                //Playing attack animation
                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(1));
                bc.CastElementalEffectParticles(moveToUse, AIEndimon);
            }
            
            //AI has picked a target, depending on who, attack that specific Endimon
            //Attack first slotted Endimon in this case
            if(rand > 5 || GameProfile.CurrentCharacter.GetActiveEndimon2().GetCurrentHP() <= 0)
            {
                //Switching camera angle
                yield return new WaitForSeconds(.5f);
                CameraController.SetGameStatus("Defending", endimonToTarget);
                yield return new WaitForSeconds(.5f);

                //Do the damage, update health, spawn text bubble, fire off animations, particles and audio for defending Endimon
                int damage = AIEndimon.UseDamageMove(AIEndimon, moveToUse, endimonToTarget, GlobalStatuses, false);
                bc.BattleText.text = BattleTextController.DefendDamageText(AIEndimon, moveToUse, endimonToTarget, damage, bc.CheckShadowCastStatus());
                AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
                bc.SpawnText(moveToUse.GetMoveType().ToString(), damage, endimonToTarget);
                endimonToTarget.TakeDamage(damage);
                bc.UpdateHealthValues();
                bc.SetTempEndimon(endimonToTarget);
                if(endimonToTarget.GetActiveNumber() == 0 || endimonToTarget.GetActiveNumber() == 1)
                {
                    bc.PlayParticleAtLocation(endimonToTarget, true, 5, 7f, 5f, 0);
                }
                else
                {
                    bc.PlayParticleAtLocation(endimonToTarget, true, 5, 7f, -5f, 0);
                }
                anims[endimonToTarget.GetActiveNumber()].Play(endimonToTarget.GetAnimationName(3));

                //Switch camera back to free state
                yield return new WaitForSeconds(2f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
            }
            //We targeted the 2nd slotted Endimon instead
            else
            {
                //Switching camera angle
                yield return new WaitForSeconds(.5f);
                CameraController.SetGameStatus("Defending", endimonToTarget);
                yield return new WaitForSeconds(.5f);

                //Do the damage, update health, spawn text bubble, fire off animations, particles and audio for defending Endimon
                int damage = AIEndimon.UseDamageMove(AIEndimon, moveToUse, endimonToTarget, GlobalStatuses, false);
                bc.BattleText.text = BattleTextController.DefendDamageText(AIEndimon, moveToUse, endimonToTarget, damage, bc.CheckShadowCastStatus());
                AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
                bc.SpawnText(moveToUse.GetMoveType().ToString(), damage, endimonToTarget);
                endimonToTarget.TakeDamage(damage);
                bc.UpdateHealthValues();
                bc.SetTempEndimon(endimonToTarget);
                if (endimonToTarget.GetActiveNumber() == 0 || endimonToTarget.GetActiveNumber() == 1)
                {
                    bc.PlayParticleAtLocation(endimonToTarget, true, 5, 7f, 5f, 0);
                }
                else
                {
                    bc.PlayParticleAtLocation(endimonToTarget, true, 5, 7f, -5f, 0);
                }
                anims[endimonToTarget.GetActiveNumber()].Play(endimonToTarget.GetAnimationName(3));
                Debug.Log("We used a damage move on the second target");

                //Switch camera back to free state
                yield return new WaitForSeconds(2.5f);
                CameraController.SetGameStatus("PlayerAwaitTurn", null);
            }
        }
        //End the turn for the AI
        bc.BattleTextPanel.SetActive(false);
        bc.UpdateHealthValues();
        bc.EndAITurn();
    }

    //AI will take control of the Easy/Hard AI's script (50/50 chance)
    public IEnumerator DecidingActionMedium(BattleController bc, Endimon AIEndimon, Animator[] anims, Image[] GlobalStatuses)
    {
        //Empty Function (This is handled via the Update method in BattleController because of Coroutines)
        return null;
    }

    //AI will use some intelligence in selecting a move for that turn, its goal is to target the highest impact Endimon in the most effective way
    public IEnumerator DecidingActionHard(BattleController bc, Endimon AIEndimon, Animator[] anims, Image[] GlobalStatuses)
    {
        //AI first determines who is the threat on the field at the moment. This is based off of Attack Damage and Health Ratio
        //Obtain copy of all the Endimon necessary to make a decision
        Endimon P1E = GameProfile.GetCurrentCharacter().GetActiveEndimon1();
        Endimon P2E = GameProfile.GetCurrentCharacter().GetActiveEndimon2();
        Endimon AI1E = GameProfile.GetCurrentAI().GetEndimon(0);
        Endimon AI2E = GameProfile.GetCurrentAI().GetEndimon(1);
        Endimon AI3E = GameProfile.GetCurrentAI().GetEndimon(2);
        Endimon AI4E = GameProfile.GetCurrentAI().GetEndimon(3);


        //Figure out who to prioritize
        int priority1 = 10000;
        int priority2 = 10000;
        if (P1E.GetCurrentHP() > 0) {
            priority1 = P1E.GetCurrentHP() - P1E.GetAttack();
        }
        if (P2E.GetCurrentHP() > 0) {
            priority2 = P2E.GetCurrentHP() - P2E.GetAttack();
        }

        //Smallest value becomes the target
        if (priority1 < priority2)
        {
            Target = P1E;
        }
        else
        {
            Target = P2E;
        }

        //We will first precalculate the damage moves, if any of these kills the target this is what the Endimon will do this turn
        bc.BattleTextPanel.SetActive(true);
        Move KillingMove = CanKillTarget(AIEndimon, Target, GlobalStatuses);    //Returns if a move can kill
        if (KillingMove != null || AIEndimon.GetEndimonPostiveEffect() != Endimon.StatusEffects.Nothing)
        {
            //If a move could kill, figure out which one it is and then use it
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

            //Check for confusion status, Endimon should hit itself if it rolls the chance
            Endimon tempTarget = null;
            int rand = 0;
            if (AIEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Confusion)
            {
                //Roll a chance to see if Endimon will hit itself (30%)
                rand = Random.Range(1, 10);
                if (rand > 7)
                {
                    tempTarget = Target;    //Want to save the real target but for this turn it hit itself
                    Target = AIEndimon;
                }
            }

            //Display in textbox
            bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, KillingMove, Target);
            yield return new WaitForSeconds(1.5f);

            //Play the correct animation and particle effect
            if(KillingMove.GetMoveName() == AIEndimon.GetEndimonMove1().GetMoveName())
            {
                bc.CastElementalEffectParticles(AIEndimon.GetEndimonMove1(), AIEndimon);
                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(0));
            }
            else
            {
                bc.CastElementalEffectParticles(AIEndimon.GetEndimonMove2(), AIEndimon);
                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(1));
            }

            //Switching camera angle to defender
            yield return new WaitForSeconds(.5f);
            CameraController.SetGameStatus("Defending", Target);
            yield return new WaitForSeconds(.5f);

            //Applying the damage, spawn the text bubble, update health, play audio, particle and animations on defending Endimon 
            int dmg = AIEndimon.UseDamageMove(AIEndimon, KillingMove, Target, GlobalStatuses, false);
            bc.BattleText.text = BattleTextController.DefendDamageText(AIEndimon, KillingMove, Target, dmg, bc.CheckShadowCastStatus());
            bc.SpawnText(KillingMove.GetMoveType().ToString(), dmg, Target);
            Target.TakeDamage(dmg);
            bc.UpdateHealthValues();
            bc.SetTempEndimon(Target);
            AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
            bc.PlayParticleAtLocation(Target, true, 5, 7f, 5f, 0);
            anims[Target.GetActiveNumber()].Play(Target.GetAnimationName(3));

            
            yield return new WaitForSeconds(2f);

            //Reset the primary target if confusion was applied
            if (tempTarget != null)
            {
                Target = tempTarget;
            }

            //Setting camera back to free mode
            CameraController.SetGameStatus("PlayerAwaitTurn", null);
        }
        
        //We determined that there is no killing move, we will now analyze if we should switch Endimon
        //We only check this if the Target has recently changed to prevent wasting turns
        else { 
            //Should we swap? Check to see if we actually can, and if it would be wise based upon subbing out a weak Endimon or putting in a stronger one
            if ((CanAISwap() && Target.GetName() != LastTarget) && ((IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetEndimonWeakness() == Target.GetEndimonType())
                || (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonWeakness() == Target.GetEndimonType()) || FindStrongerEndimon(Target))) {

                //Figure out who to swap, first check if first slotted Endimon is weak to the target
                if (IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetEndimonWeakness() == Target.GetEndimonType())
                {
                    //Prep camera and text
                    CameraController.SetGameStatus("Attacking", ActiveAIEndimon1);
                    bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon1, GetNonWeakEndimon(ActiveAIEndimon1.GetEndimonWeakness()));
                    yield return new WaitForSeconds(1.5f);

                    //Do the swapping of models and update the UI
                    SwapEndimonOnTurn(1, GetNonWeakEndimon(ActiveAIEndimon1.GetEndimonWeakness()));
                    bc.SwitchEndimonUI(ActiveAIEndimon1, ActiveAIEndimon1.GetActiveNumber());

                    //Waiting action to finish up then switching back
                    yield return new WaitForSeconds(1f);
                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                }

                //The second slotted Endimon is weak to the target, swap it out
                else if (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonWeakness() == Target.GetEndimonType())
                {
                    //Prep camera and text
                    CameraController.SetGameStatus("Attacking", ActiveAIEndimon2);
                    bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon2, GetNonWeakEndimon(ActiveAIEndimon2.GetEndimonWeakness()));
                    yield return new WaitForSeconds(1.5f);

                    //Do the swapping of models and update the UI
                    SwapEndimonOnTurn(2, GetNonWeakEndimon(ActiveAIEndimon2.GetEndimonWeakness()));
                    bc.SwitchEndimonUI(ActiveAIEndimon2, ActiveAIEndimon2.GetActiveNumber());

                    //Waiting for action to finish up then switching camera back
                    yield return new WaitForSeconds(1f);
                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                }

                //Otherwise, check to see if there is a stronger Endimon in reserve
                else if (FindStrongerEndimon(Target))
                {
                    //Let's swap the opposite turn Endimon so they're turn comes up faster
                    if(IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1 == AIEndimon)
                    {
                        //Prep camera and text
                        CameraController.SetGameStatus("Attacking", ActiveAIEndimon2);
                        bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon2, GetStrongerEndimon(Target));
                        yield return new WaitForSeconds(1.5f);

                        //Make the swap of models and UI
                        SwapEndimonOnTurn(2, GetStrongerEndimon(Target));
                        bc.SwitchEndimonUI(ActiveAIEndimon2, ActiveAIEndimon2.GetActiveNumber());

                        //Waiting for action to finish up then switching camera back
                        yield return new WaitForSeconds(1f);
                        CameraController.SetGameStatus("PlayerAwaitTurn", null);
                    }   
                    //Otherwise let's swap the 2nd Endimon out
                    else
                    {
                        //Prep camera and text
                        CameraController.SetGameStatus("Attacking", ActiveAIEndimon1);
                        bc.BattleText.text = BattleTextController.SwappingText(ActiveAIEndimon1, GetStrongerEndimon(Target));
                        yield return new WaitForSeconds(1.5f);

                        //Make the swap of models and UI
                        SwapEndimonOnTurn(1, GetStrongerEndimon(Target));
                        bc.SwitchEndimonUI(ActiveAIEndimon1, ActiveAIEndimon1.GetActiveNumber());

                        //Waiting for action to finish up then switching camera back
                        yield return new WaitForSeconds(1f);
                        CameraController.SetGameStatus("PlayerAwaitTurn", null);
                    }
                }
            }

            //We figured out we don't want to swap, we will now choose b/w attacking, using a special ability, and using an item
            //Typically, we want more attacking and to use abilities/items sparingly when appropriate
            else
            {
                while (true)
                {
                    int rand = Random.Range(1, 6);

                    //We will attack 60% of the time
                    if (rand < 4 || HasEffectiveMove(AIEndimon, Target))
                    {
                        int move1Dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove1(), Target, GlobalStatuses, true);
                        int move2Dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove2(), Target, GlobalStatuses, true);
                        int dmg = 0;

                        //Prep cameera
                        CameraController.SetGameStatus("Attacking", AIEndimon);

                        //Check for confusion status, Endimon should hit itself if chance is rolled
                        Endimon tempTarget = null;
                        if(AIEndimon.GetEndimonNegativeEffect() == Endimon.StatusEffects.Confusion)
                        {
                            //Roll a chance to see if Endimon will hit itself (30%)
                            rand = 0;
                            rand = Random.Range(1, 10);
                            if(rand > 7)
                            {
                                tempTarget = Target;    //Want to save the real target but for this turn it hit itself
                                Target = AIEndimon;
                            }
                        }

                        Move UsedMove = null;
                        //Determine the better move, display the textbox, and apply the animations and particles for an attack
                        if (move1Dmg > move2Dmg)
                        {
                            dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove1(), Target, GlobalStatuses, false);
                            bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, AIEndimon.GetEndimonMove1(), Target);
                            yield return new WaitForSeconds(1.5f);
                            bc.CastElementalEffectParticles(AIEndimon.GetEndimonMove1(), AIEndimon);
                            anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(0));
                            UsedMove = AIEndimon.GetEndimonMove1();
                        }
                        else
                        {
                            dmg = AIEndimon.UseDamageMove(AIEndimon, AIEndimon.GetEndimonMove2(), Target, GlobalStatuses, false);
                            bc.BattleText.text = BattleTextController.AttackDamageText(AIEndimon, AIEndimon.GetEndimonMove2(), Target);
                            yield return new WaitForSeconds(1.5f);
                            bc.CastElementalEffectParticles(AIEndimon.GetEndimonMove2(), AIEndimon);
                            anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(1));
                            UsedMove = AIEndimon.GetEndimonMove2();
                        }

                        //Switching camera angle to the defender
                        yield return new WaitForSeconds(.5f);
                        CameraController.SetGameStatus("Defending", Target);
                        yield return new WaitForSeconds(.5f);

                        //Apply the damage, spawn the correct particles, and display the animations and audio accordingly
                        bc.SpawnText(UsedMove.GetMoveType().ToString(), dmg, Target);
                        bc.BattleText.text = BattleTextController.DefendDamageText(AIEndimon, AIEndimon.GetEndimonMove1(), Target, dmg, bc.CheckShadowCastStatus());
                        AudioSource.PlayClipAtPoint(Audio.BeenHit, GameObject.Find("MainCamera").transform.position);
                        bc.PlayParticleAtLocation(Target, true, 5, 7f, 5f, 0);
                        anims[Target.GetActiveNumber()].Play(Target.GetAnimationName(3));
                        Target.TakeDamage(dmg);
                        bc.UpdateHealthValues();
                        Debug.Log("Hard AI: Used a damaging move ");

                        //Reset the primary target because it was moved for confusion
                        if(tempTarget != null)
                        {
                            Target = tempTarget;
                        }
                        //Waiting action to finish up then switching back
                        yield return new WaitForSeconds(2f);
                        
                        CameraController.SetGameStatus("PlayerAwaitTurn", null);
                        break;
                    }

                    //Using an item (20% chance)
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

                        //If we chose an item then we will use it
                        if(temp < 10) { 
                            //If the positive effect can be used on either of the Endimon, then place it
                            if (AIItems[rand].GetUsabilityTeam() && ((IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing)
                                || (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing))) {

                                //Prep the camera
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

                                //Play particle/audio on Endimon using the item
                                yield return new WaitForSeconds(1.5f);
                                bc.PlayParticleAtLocation(AIEndimon, false, 0, 2f, -3f, 0);
                                AudioSource.PlayClipAtPoint(Audio.UseItem, GameObject.Find("MainCamera").transform.position);
                                yield return new WaitForSeconds(1f);

                                //Determine if the first Endimon should recieve the item (Based on health and if its alive)
                                if ((IsEndimonAlive(ActiveAIEndimon1) && AIItems[rand].GetHealing() && ActiveAIEndimon1.GetCurrentHP() != ActiveAIEndimon1.GetHealth()) 
                                    || (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing))
                                {
                                    //Switching camera angle to defender
                                    yield return new WaitForSeconds(.5f);
                                    CameraController.SetGameStatus("Defending", ActiveAIEndimon2);
                                    yield return new WaitForSeconds(.5f);
                               
                                    //Use the item on the Endimon, removing it from the list and applying the particle effects & status
                                    Item UsedItem = AddTurnToItem(ActiveAIEndimon2, AIItems[rand], AIEndimon);
                                    int particleIndex = bc.FindLocationForItemParticle(AIEndimon, ActiveAIEndimon2, UsedItem);
                                    UseItem(AIItems[rand], ActiveAIEndimon2, bc);
                                    RemoveItem(AIItems[rand]);
                                    bc.UpdateStatusEffectBoxes(ActiveAIEndimon2, particleIndex, false);

                                    //Waiting action to finish up then switching camera back
                                    yield return new WaitForSeconds(2.5f);
                                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                                }

                                //The second Endimon was picked to recieve the item
                                else
                                {
                                    //Switching camera angle to defender
                                    yield return new WaitForSeconds(.5f);
                                    CameraController.SetGameStatus("Defending", ActiveAIEndimon1);
                                    yield return new WaitForSeconds(.5f);

                                    //Use the item on the Endimon, removing it from the list and applying the particle effects & status
                                    Item UsedItem = AddTurnToItem(ActiveAIEndimon1, AIItems[rand], AIEndimon);
                                    int particleIndex = bc.FindLocationForItemParticle(AIEndimon, ActiveAIEndimon1, UsedItem);
                                    UseItem(AIItems[rand], ActiveAIEndimon1, bc);
                                    RemoveItem(AIItems[rand]);
                                    bc.UpdateStatusEffectBoxes(ActiveAIEndimon1, particleIndex, false);

                                    //Waiting action to finish up then switching back
                                    yield return new WaitForSeconds(2.5f);
                                    CameraController.SetGameStatus("PlayerAwaitTurn", null);
                                }
                            }

                            //If its a negative effect item and the targetted endimon has no debuff then place it
                            else if(!AIItems[rand].GetUsabilityTeam() && Target.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing)
                            {
                                //Prep camera and text
                                CameraController.SetGameStatus("Attacking", AIEndimon);
                                bc.BattleText.text = BattleTextController.ItemUsedText(AIEndimon, AIItems[rand], Target);
                                yield return new WaitForSeconds(1.5f);

                                //Play particles/audio on the Endimon using the item
                                bc.PlayParticleAtLocation(AIEndimon, false, 0, 2f, -3f, 0);
                                AudioSource.PlayClipAtPoint(Audio.UseItem, GameObject.Find("MainCamera").transform.position);
                                yield return new WaitForSeconds(1f);

                                //Switching camera angle to defender
                                yield return new WaitForSeconds(.5f);
                                CameraController.SetGameStatus("Defending", Target);
                                yield return new WaitForSeconds(.5f);

                                //Use the item on the Endimon, removing it from the list and applying the particle effects & status
                                Item UsedItem = AddTurnToItem(Target, AIItems[rand], AIEndimon);
                                UseItem(UsedItem, Target, bc);
                                RemoveItem(AIItems[rand]);
                                int particleIndex = bc.FindLocationForItemParticle(AIEndimon, Target, UsedItem);
                                bc.UpdateStatusEffectBoxes(Target, particleIndex, true);

                                //Waiting action to finish up then switching camera back
                                yield return new WaitForSeconds(2.5f);
                                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            }
                            break;
                        }
                    }

                    //Using the special ability (20% chance)
                    else if(rand == 5)
                    {
                        //Check to see if this is a global move, if so cast it
                        if(!AIEndimon.GetEndimonMove3().GetHarmful() && !AIEndimon.GetEndimonMove3().GetTargetable() && CheckGlobals(AIEndimon.GetEndimonMove3(), GlobalStatuses))
                        {
                            //Prep camera and text
                            CameraController.SetGameStatus("Attacking", AIEndimon);
                            bc.BattleText.text = BattleTextController.SpecialAbilityText(AIEndimon, AIEndimon.GetEndimonMove3(), null);
                            yield return new WaitForSeconds(1.5f);

                            //Play cast animation and then change angles
                            anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(2));
                            yield return new WaitForSeconds(.5f);
                            CameraController.SetGameStatus("Globals", null);
                            bc.BattleText.text = BattleTextController.GlobalText(AIEndimon.GetEndimonMove3().GetMoveName());

                            //Cast global effect particles
                            int particleIndex = AIEndimon.UseSpecialMove(AIEndimon, null, AIEndimon.GetEndimonMove3(), bc);
                            bc.AddGlobalEffect(particleIndex);

                            //Return camera to free mode
                            yield return new WaitForSeconds(3f);
                            CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            break;
                        }

                        //If this is a negative effect move, we will use it on the target
                        else if(AIEndimon.GetEndimonMove3().GetHarmful() && Target.GetEndimonNegativeEffect() == Endimon.StatusEffects.Nothing)
                        {

                            //Prep camera and text
                            CameraController.SetGameStatus("Attacking", AIEndimon);
                            bc.BattleText.text = BattleTextController.SpecialAbilityText(AIEndimon, AIEndimon.GetEndimonMove3(), Target);
                            yield return new WaitForSeconds(1.5f);

                            //Play the animation
                            anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(2));

                            //Switching camera angle to defender
                            yield return new WaitForSeconds(.5f);
                            CameraController.SetGameStatus("Defending", Target);
                            yield return new WaitForSeconds(.5f);

                            //Hit target with a harmful effect (if -1 don't do anything)
                            int particleIndex = AIEndimon.UseSpecialMove(AIEndimon, Target, AIEndimon.GetEndimonMove3(), bc);
                            if (particleIndex == -1)
                            {
                                bc.BattleText.text = "The attack failed";
                            }
                            bc.CastAbilityEffect(particleIndex, Target, AIEndimon);
                            bc.UpdateStatusEffectBoxes(Target, particleIndex, true);

                            //Waiting action to finish up then switching camera back
                            yield return new WaitForSeconds(1.5f);
                            CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            break;

                        }

                        //Otherwise, we know the move is going to be a positive effect
                        else if(!AIEndimon.GetEndimonMove3().GetHarmful() && ((IsEndimonAlive(ActiveAIEndimon1) && ActiveAIEndimon1.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing) || (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing))) {

                            //If we should use this specific positive effect on the first slotted Endimon
                            if (IsEndimonAlive(ActiveAIEndimon2) && ActiveAIEndimon2.GetEndimonPostiveEffect() == Endimon.StatusEffects.Nothing)
                            {
                                //Prep camera and text
                                CameraController.SetGameStatus("Attacking", AIEndimon);
                                bc.BattleText.text = BattleTextController.SpecialAbilityText(AIEndimon, AIEndimon.GetEndimonMove3(), ActiveAIEndimon2);
                                yield return new WaitForSeconds(1.5f);

                                //Play animation
                                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(2));

                                //Switching camera angle to defneder
                                yield return new WaitForSeconds(.5f);
                                CameraController.SetGameStatus("Defending", ActiveAIEndimon2);
                                yield return new WaitForSeconds(.5f);

                                //Place it on AI2 Endimon
                                int particleIndex = AIEndimon.UseSpecialMove(AIEndimon, ActiveAIEndimon2, AIEndimon.GetEndimonMove3(), bc);
                                bc.CastAbilityEffect(particleIndex, ActiveAIEndimon2, AIEndimon);
                                bc.UpdateStatusEffectBoxes(ActiveAIEndimon2, particleIndex, false);

                                //Waiting action to finish up then switching camera back
                                yield return new WaitForSeconds(1.5f);
                                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            }
                            else
                            {
                                //Prep camera and move
                                CameraController.SetGameStatus("Attacking", AIEndimon);
                                bc.BattleText.text = BattleTextController.SpecialAbilityText(AIEndimon, AIEndimon.GetEndimonMove3(), ActiveAIEndimon1);
                                yield return new WaitForSeconds(1.5f);

                                //Play animation
                                anims[AIEndimon.GetActiveNumber()].Play(AIEndimon.GetAnimationName(2));

                                //Switching camera angle to defender
                                yield return new WaitForSeconds(.5f);
                                CameraController.SetGameStatus("Defending", ActiveAIEndimon1);
                                yield return new WaitForSeconds(.5f);

                                //Place it on AI1 Endimon
                                int particleIndex = AIEndimon.UseSpecialMove(AIEndimon, ActiveAIEndimon1, AIEndimon.GetEndimonMove3(), bc);
                                bc.CastAbilityEffect(particleIndex, ActiveAIEndimon1, AIEndimon);
                                bc.UpdateStatusEffectBoxes(ActiveAIEndimon1, particleIndex, false);

                                //Waiting action to finish up then switching camera back
                                yield return new WaitForSeconds(1.5f);
                                CameraController.SetGameStatus("PlayerAwaitTurn", null);
                            }
                            break;
                        }
                    }
                }
            }
        }
        //Assign the target we just hit and end the turn
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
    //Return value will tell the program which active Endimon it'll need to update, 1 or 2, 0 for if there is none
    public int SwapEndimonOnDeath(Endimon e)
    {
        Endimon NewEndimon = null;
        //Look for a new Endimon
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
                NewEndimon.SetTurnStatus(true);
            }
            NewEndimon.SetActiveNumber(ActiveAIEndimon1.GetActiveNumber());
            ActiveAIEndimon1 = NewEndimon;
            return 1;
        }
        else
        {
            if (ActiveAIEndimon2.GetEndimonTurnTaken() == true)
            {
                NewEndimon.SetTurnStatus(true);
            }
            NewEndimon.SetActiveNumber(ActiveAIEndimon2.GetActiveNumber());
            ActiveAIEndimon2 = NewEndimon;
            return 2;
        }
    }

    //Function swaps an Endimon at the request of the player. This function should be called assuming three or more Endimon are alive 
    //Slot is necessary to indicate which component needs to change in the battle screen
    public void SwapEndimonOnTurn(int slot, Endimon e)
    {
        //First slotted Endimon is changing
        if (slot == 1)
        {
            //Verify if this Endimon has already gone in the round
            if (ActiveAIEndimon1.GetEndimonTurnTaken() == true)
            {
                e.SetTurnStatus(true);
            }
            else
            {
                e.SetTurnStatus(false);
            }
            //Transfer over the active number
            ActiveAIEndimon1.SetTurnStatus(false);
            ActiveAIEndimon1 = e;
            ActiveAIEndimon1.SetActiveNumber(2);
        }
        //Second slottd Endimon is changing
        else if (slot == 2)
        {
            //Verify if this Endimon has already gone in the round
            if (ActiveAIEndimon2.GetEndimonTurnTaken() == true)
            {
                e.SetTurnStatus(true);
            }
            else
            {
                e.SetTurnStatus(false);
            }
            //Transfer over the active number
            ActiveAIEndimon2.SetTurnStatus(false);
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

    //Returns an Endimon that is weak to the Endimon's type that is passed in
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
        for(int i = 0; i < AITeam.Length; i++)
        {
            if(IsEndimonAlive(AITeam[i]) && AITeam[i].GetName() != ActiveAIEndimon1.GetName() && AITeam[i].GetName() != ActiveAIEndimon2.GetName()) {
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

    //Function that is called to determine if the AI has any Endimon left alive in their party (game over?)
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
    public void UseItem(Item UsedItem, Endimon TargettedEndimon, BattleController bc)
    {
        //Non-instant effect item (Apply the particles, play audio, add status)
        if (UsedItem.GetItemDuration() > 0)
        {
            AudioClip itemClip = Item.DetermineAudioClip(UsedItem.GetItemName());
            AudioSource.PlayClipAtPoint(itemClip, GameObject.Find("MainCamera").transform.position);
            TargettedEndimon.AddStatusEffect(!UsedItem.GetUsabilityTeam(), UsedItem.GetEffect(), UsedItem.GetItemDuration());
        }
        //Otherwise this is an instant effect so we won't be placing permanent particles
        else
        {
            //Small heal, give back 25% of HP
            if (UsedItem.GetEffect() == Endimon.StatusEffects.HealthRestore)
            {
                //We will hurt the target with negative damage to heal
                int HealthToRestore = (int)(TargettedEndimon.GetHealth() * 0.25 * -1);
                if (TargettedEndimon.GetCurrentHP() + HealthToRestore == TargettedEndimon.GetHealth())
                {
                    HealthToRestore = (int)(TargettedEndimon.GetHealth() - TargettedEndimon.GetCurrentHP() * -1);

                }
                AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
                bc.SpawnText("Healing", HealthToRestore*-1, TargettedEndimon);
                TargettedEndimon.TakeDamage(HealthToRestore);
            }
            //Large heal, give back 50% of HP, remove 15 defense
            else if (UsedItem.GetEffect() == Endimon.StatusEffects.LargeHealthRestore)
            {
                //We will hurt the target with negative damage to heal
                int HealthToRestore = (int)(TargettedEndimon.GetHealth() * 0.5 * -1);
                if (TargettedEndimon.GetCurrentHP() + HealthToRestore == TargettedEndimon.GetHealth())
                {
                    HealthToRestore = (int)(TargettedEndimon.GetHealth() - TargettedEndimon.GetCurrentHP() * -1);
                }
                AudioSource.PlayClipAtPoint(Audio.Heal, GameObject.Find("MainCamera").transform.position);
                bc.SpawnText("Healing", HealthToRestore * -1, TargettedEndimon);
                TargettedEndimon.TakeDamage(HealthToRestore);
                TargettedEndimon.SetDefense(-15);   //Endimon loses part of its defense for getting healed this amount
            }
        }
    }

    //SETTERS
    public void SetAITeam(Endimon[] theTeam) { AITeam = theTeam; }
    public void SetAIItems(Item[] theItems) { AIItems = theItems; }
    public void SetAIDifficulty(CharacterSelectController.DifficultySelection difficulty) { AIDifficulty = difficulty; }
    public void SetActiveEndimon1(int index) { ActiveAIEndimon1 = AITeam[index]; }
    public void SetActiveEndimon2(int index) { ActiveAIEndimon2 = AITeam[index]; }
    public void SetActiveEndimon1(Endimon e) { ActiveAIEndimon1 = e; }
    public void SetActiveEndimon2(Endimon e) { ActiveAIEndimon2 = e; }

    //GETTERS
    public Endimon[] GetAITeam() { return AITeam; }
    public Item[] GetAIItems() { return AIItems; }
    public CharacterSelectController.DifficultySelection GetAIDifficulty() { return AIDifficulty; }
    public Endimon GetEndimon(int index) { return AITeam[index]; }
    public Endimon GetActiveEndimon1() { return ActiveAIEndimon1; }
    public Endimon GetActiveEndimon2() { return ActiveAIEndimon2; }
}
