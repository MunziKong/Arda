using System;
using UnityEngine;

[Serializable]
public class SkillSfxEntry
{
    public AudioClip clip;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    [Range(0f, 1f)]
    public float volume = 1f;
}