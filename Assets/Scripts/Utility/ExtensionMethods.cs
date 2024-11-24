using UnityEngine;
using RetroCode;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.EventSystems;
using System;

namespace V3CTOR
{
    public static class EXMET
    {
        public static byte[] ToBytes(CloudData data)
        {
            using (MemoryStream m = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(m, data);
                return m.ToArray();
            }
        }

        public static CloudData FromBytes(byte[] bytes)
        {
            if (bytes.Length == 0) return null;

            using (MemoryStream m = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                m.Write(bytes, 0, bytes.Length);
                m.Seek(0, SeekOrigin.Begin);              
                CloudData data = bf.Deserialize(m) as CloudData;              
                return data;
            }
        }

        public static void PlaySystem(this ParticleSystem particles)
        {
            if (particles.isPlaying) { return; }

            particles.Play(true);
        }

        public static void StopSystem(this ParticleSystem particles)
        {
            if (particles.isStopped) { return; }

            particles.Stop(true);
        }

        public static void RemoveSpawnable(GameObject removable, List<GameObject> poolToRemove, List<GameObject> poolToAdd)
        {
            removable.SetActive(false);

            poolToRemove.Remove(removable);
            poolToAdd.Add(removable);
        }

        public static void AddSpawnable(GameObject objectToAdd, List<GameObject> poolToAdd, List<GameObject> poolToRemove)
        {
            objectToAdd.SetActive(true);

            poolToAdd.Add(objectToAdd);
            poolToRemove.Remove(objectToAdd);
        }

        public static void SaveJSON(string json, string fileName)
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(path, json);
            Debug.Log("JSON data saved to: " + path);
        }

        public static string LoadJSON(string fileName)
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                Debug.Log("JSON data loaded from: " + path);
                return json;
            }
            else
            {
                Debug.LogError("File not found: " + path);
                return null;
            }
        }

        public static readonly NumberFormatInfo NumForThou = new NumberFormatInfo()
        {
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 0
        };

        public static string IntToCompClass(int index)
        {
            switch (index)
            {
                case 0:
                    return "engine";

                case 1:
                    return "power";

                case 2:
                    return "handling";

                case 3:
                    return "health";
            }

            return "";
        }

        public static string LevelAsRoman(int level)
        {
            switch (level)
            {
                case 0:
                    return "I";

                case 1:
                    return "II";

                case 2:
                    return "III";

                case 3:
                    return "IV";

                case 4:
                    return "V";
            }

            return "";
        }

        public static void SetPointerDownEvent(this EventTrigger eventTrigger, Action action)
        {
            EventTrigger.Entry entry = eventTrigger.triggers.Find(e => e.eventID == EventTriggerType.PointerDown);

            entry.callback.RemoveAllListeners();
            entry.callback.AddListener((_) => action.Invoke());
        }

        public static void SmoothNumberText(int startNum, int targetNum)
        {
            LeanTween.value(startNum, targetNum, 1f);
        }
    }

    public interface Damageable
    {
        void Damage(int damage);
        int DamageToPlayer();
        int Health();
    }

    public interface PickUp
    {
        void PickupInit();
        PickupType GetPickupType();
        GameObject Hoverable();
    }

    public interface NearMissable
    {
        public float NearMissProxy();
    }
}
