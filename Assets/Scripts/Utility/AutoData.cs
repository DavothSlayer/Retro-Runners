using System;
using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Car Data", menuName = "New Car Data", order = 1)]
    public class AutoData : ScriptableObject
    {
        [Header("Car Data")]
        public string AutoName;
        public string ItemCode;
        public int Tier;
        public int Price;
        public Sprite Icon;
        public float CompPriceMultiplier;
        public AutoLevelData[] autoLevelData;
        public AutoClass Class;
        [Space]
        public Vector3 CameraPositionInMenu;
        public Quaternion CameraRotationInMenu;
        public float InMenuFOV;
        public Vector3 CameraPositionInGame;
        public Quaternion CameraRotationInGame;
        [Space]
        public Vector3 rearviewOffset;
        [Space]
        public float scrapeRayRange;
        public float nearMissProxy;
        [Space]
        public GameObject LivePrefab;

        [Space]
        [Header("Ability")]
        public AutoAbility Ability;
        public string AbilityInfo;

        public float AccelerationMS(int GearboxLevel)
        {
            return 2000f / autoLevelData[GearboxLevel].Torque;
        }

        public float SteerSpeed(int HandlingLevel)
        {
            return autoLevelData[HandlingLevel].Handling;
        }

        public float RotateAmount(int HandlingLevel)
        {
            return 1000f / autoLevelData[HandlingLevel].Handling;
        }

        public float IdleRPM(int GearboxLevel)
        {
            return autoLevelData[GearboxLevel].MaxRPM / 10f;
        }
    }

    [Serializable]
    public class AutoLevelData
    {
        public float TopSpeed;
        public float Torque;
        public float MaxRPM;
        public int MaxGear;
        public float Handling;
        public int MaxHealth;
        public int Power;

    }

    public enum AutoClass
    {
        Muscle,
        Sports,
        Super,
    }
}
