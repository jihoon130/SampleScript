using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSettings", menuName = "KINEMATION/FPS Animation Pack/FPS Player Settings")]
public class FPSPlayerSettings : ScriptableObject
{

    public IKMotion aimingMotion;
    public IKMotion fireModeMotion;

    public AudioClip[] MoveMents;
    public AudioClip[] Sprints;
}