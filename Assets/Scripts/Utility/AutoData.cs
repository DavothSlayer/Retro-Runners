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

        public float TopSpeed(int EngineLevel)
        {
            return autoLevelData[EngineLevel].TopSpeed;
        }

        public float Acceleration(int GearboxLevel)
        {
            return autoLevelData[GearboxLevel].AccelerationTime;
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
            return autoLevelData[GearboxLevel].MaxRPM / 6f;
        }

        [ContextMenu("Set Default Camera Data")]
        public void SetDefaultCamData()
        {
            CameraPositionInMenu = new(2.6f, 1.1f, 6.8f);
            CameraRotationInMenu = Quaternion.Euler(6.4f, 195f, 350f);
            InMenuFOV = 55f;
            CameraPositionInGame = new(0f, 3.5f, -9f);
            CameraRotationInGame = Quaternion.Euler(5f, 0f, 0f);
            rearviewOffset = new(0f, -1f, 7f);
        }
    }

    [Serializable]
    public class AutoLevelData
    {
        public float TopSpeed;
        public float AccelerationTime;
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
