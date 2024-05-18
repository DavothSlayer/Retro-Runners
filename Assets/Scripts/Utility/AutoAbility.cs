using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RetroCode
{
    public class AutoAbility : ScriptableObject
    {
        public string abilityName;
        public float duration;
        public float cooldownTime;

        public virtual void ActivateAbility(AutoMobile auto)
        {

        }
    }

    public enum AbilityState
    {
        Ready,
        Active,
        Cooldown,
    }
}