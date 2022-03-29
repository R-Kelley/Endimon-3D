using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Audio
{
    //*WE HAVE TO INCLUDE AUDIO DURING A TURN FOR STATUS EFFECTS AS WELL
    //WE SHOULD HAVE A INTRO CAMERA EFFECT TO START BY LISTING THE STATUS EFFECTS THEY ARE UNDER WITH NOISES
    //Menu Audio
    public static AudioClip ButtonHover = Resources.Load("Audio/ButtonHover", typeof(AudioClip)) as AudioClip;
    public static AudioClip ButtonClick = Resources.Load("Audio/ButtonClick", typeof(AudioClip)) as AudioClip;
    public static AudioClip ButtonCancel = Resources.Load("Audio/ButtonCancel", typeof(AudioClip)) as AudioClip;

    //Attacks
    public static AudioClip PyroAttack = Resources.Load("Audio/FireHit", typeof(AudioClip)) as AudioClip;
    public static AudioClip FrostAttack = Resources.Load("Audio/FrostHit", typeof(AudioClip)) as AudioClip;
    public static AudioClip ElectroAttack = Resources.Load("Audio/ElectroHit", typeof(AudioClip)) as AudioClip;
    public static AudioClip EarthAttack = Resources.Load("Audio/EarthHit", typeof(AudioClip)) as AudioClip;
    public static AudioClip ShadowAttack = Resources.Load("Audio/ShadowHit", typeof(AudioClip)) as AudioClip;
    public static AudioClip BeenHit = Resources.Load("Audio/Hit", typeof(AudioClip)) as AudioClip;

    //Special Effects
    public static AudioClip UseItem = Resources.Load("Audio/UseItem", typeof(AudioClip)) as AudioClip;
    public static AudioClip Heal = Resources.Load("Audio/Healing", typeof(AudioClip)) as AudioClip;
    public static AudioClip Confusion = Resources.Load("Audio/Confusion", typeof(AudioClip)) as AudioClip;
    public static AudioClip Sleep = Resources.Load("Audio/Sleep", typeof(AudioClip)) as AudioClip;
    public static AudioClip Paralyze = Resources.Load("Audio/Paralyze", typeof(AudioClip)) as AudioClip;
    public static AudioClip Poison = Resources.Load("Audio/Poison", typeof(AudioClip)) as AudioClip;
    public static AudioClip AttackUp = Resources.Load("Audio/AttackUp", typeof(AudioClip)) as AudioClip;
    public static AudioClip DefenseUp = Resources.Load("Audio/DefenseUp", typeof(AudioClip)) as AudioClip;

    //Switching
    public static AudioClip Death = Resources.Load("Audio/Death", typeof(AudioClip)) as AudioClip;
    public static AudioClip SwapIn = Resources.Load("Audio/Swap", typeof(AudioClip)) as AudioClip;

    //Global Effects
    public static AudioClip GlobalFlames = Resources.Load("Audio/GlobalFlame", typeof(AudioClip)) as AudioClip;
    public static AudioClip GlobalBlizzard = Resources.Load("Audio/Blizzard", typeof(AudioClip)) as AudioClip;
    public static AudioClip GlobalShadows = Resources.Load("Audio/GlobalShadow", typeof(AudioClip)) as AudioClip;

    //Game Status
    public static AudioClip WinningChime = Resources.Load("Audio/Winner", typeof(AudioClip)) as AudioClip;
    public static AudioClip LosingChime = Resources.Load("Audio/Loser", typeof(AudioClip)) as AudioClip;
}
