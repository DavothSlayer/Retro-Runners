using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using V3CTOR;
using Random = UnityEngine.Random;

namespace RetroCode
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("Core Functionality")]
        [SerializeField]
        private GameManager gameManager;
        public bool spawn;

        [Header("Data")]
        public RoadLane[] roadLanes;        
        public float minDespawnDistance;
        [SerializeField]
        private float maxDespawnDistance;

        // NPC POOL //
        [Header("Pooling")]
        public Transform NPCPoolParent;
        public Transform COPPoolParent;
        public Transform PickupPoolParent;
        [Space]
        public List<GameObject> NPCPool = new List<GameObject>();
        public List<GameObject> HeliPool = new List<GameObject>();
        public List<GameObject> COPPool = new List<GameObject>();
        public List<GameObject> PickupPool = new List<GameObject>();
        public List<GameObject> PickupList = new List<GameObject>();

        // RIGHT LANE NPCS //
        [HideInInspector]
        public List<GameObject> activeNPCsRL = new List<GameObject>();
        // LEFT LANE NPCS //
        [HideInInspector]
        public List<GameObject> activeNPCsLL = new List<GameObject>();
        // COPS //
        [HideInInspector]
        public List<GameObject> activeCOPs = new List<GameObject>();
        [HideInInspector]
        public List<GameObject> activeHelis = new List<GameObject>();
        // OBSTACLES //
        [HideInInspector]
        public List<GameObject> activePickups = new List<GameObject>();

        private RoadVariation roadVar;
        private HeatVariation heatVar;

        private void Start()
        {
            HeatLevelCheck();
            InitializeNPCs();
        }

        private void Update()
        {
            HandleNPCs();
        }

        #region NPCs
        private void HandleNPCs()
        {
            roadVar = gameManager.roadVariations[gameManager.activeRoadVar];

            spawn = GameManager.gameState == GameState.InGame || GameManager.gameState == GameState.InMenu;
          
            if (DistanceToLastNPCRL() < roadVar.NPCSpawnDistance)
            {
                if (activeNPCsRL.Count < roadVar.maxNPCCountRL) { SpawnNPCRL(roadVar); }
            }

            if (DistanceToLastNPCLL() < roadVar.NPCSpawnDistance)
            {
                if (activeNPCsRL.Count < roadVar.maxNPCCountLL) { SpawnNPCLL(roadVar); }
            }

            DespawnUnwantedNPCs();
        }

        public void InitializeNPCs()
        {
            // ASSUME STAGE TO BE 0 //
            RoadVariation roadVar;
            roadVar = gameManager.roadVariations[gameManager.activeRoadVar];

            SpawnNPCRL(roadVar);
            SpawnNPCLL(roadVar);
        }

        // SPAWN NPC //
        private void SpawnNPCRL(RoadVariation road)
        {
            // CALCULATE ZPOS //
            float zPos;
            if (activeNPCsRL.Count == 0 || activeNPCsRL[activeNPCsRL.Count - 1].transform.position.z < gameManager.playerTransform.position.z + 100f)
            {
                zPos = gameManager.playerTransform.position.z + road.NPCSpawnDistance;
            }
            else
            {
                GameObject lastNPC = activeNPCsRL[activeNPCsRL.Count - 1];
                zPos = lastNPC.transform.position.z + road.distanceBetweenNPC;
            }

            // CALCULATE XPOS //
            float xPos = 0f;
            for (int i = 0; i < gameManager.activeTiles.Count; i++)
            {
                GameObject tile = gameManager.activeTiles[i];

                if (zPos > tile.transform.position.z && zPos < tile.transform.position.z + 400f)
                {
                    if (tile.name.Contains("Start")) { return; }

                    if (tile.name.Contains("Level1"))
                        xPos = SpawnXPositionRL1();
                    else
                        xPos = SpawnXPositionRL2();
                }
            }

            if(xPos == 0f) { return; }

            Vector3 spawnPos = new Vector3(xPos, 0.1f, zPos);
            Quaternion spawnRot = Quaternion.identity;

            if (gameManager.heat && 
                Random.Range(0f, 100f) <= heatVar.copSpawnChance &&
                activeCOPs.Count != heatVar.maxCOPCount &&
                GameManager.gameState == GameState.InGame &&
                COPPool.Count > 0)
            {
                Vector3 posOffset = new Vector3(0f, 0f, 45f);

                GameObject spawnable = COPPool[Random.Range(0, COPPool.Count)];
                spawnable.transform.SetPositionAndRotation(spawnPos + posOffset, spawnRot);
                EXMET.AddSpawnable(spawnable, activeCOPs, COPPool);
            }
            else
            {
                GameObject spawnable = NPCPool[Random.Range(0, NPCPool.Count)];
                spawnable.transform.SetPositionAndRotation(spawnPos, spawnRot);
                EXMET.AddSpawnable(spawnable, activeNPCsRL, NPCPool);
            }
        }

        private void SpawnNPCLL(RoadVariation road)
        {
            // CALCULATE ZPOS //
            float zPos;
            if (activeNPCsLL.Count == 0 || activeNPCsLL[activeNPCsLL.Count - 1].transform.position.z < gameManager.playerTransform.position.z + 100f)
            {
                zPos = gameManager.playerTransform.position.z + road.NPCSpawnDistance;
            }
            else
            {
                GameObject lastNPC = activeNPCsLL[activeNPCsLL.Count - 1];
                zPos = lastNPC.transform.position.z + road.distanceBetweenNPC;
            }

            // CALCULATE XPOS //
            float xPos = 0f;
            for (int i = 0; i < gameManager.activeTiles.Count; i++)
            {
                GameObject tile = gameManager.activeTiles[i];

                if (zPos > tile.transform.position.z && zPos < tile.transform.position.z + 400f)
                {
                    if (tile.name.Contains("Start")) { return; }

                    if (tile.name.Contains("Level1"))
                        xPos = SpawnXPositionLL1();
                    else
                        xPos = SpawnXPositionLL2();
                }
            }

            if (xPos == 0f) { return; }

            Vector3 spawnPos = new Vector3(xPos, 0.1f, zPos);
            Quaternion spawnRot = Quaternion.Euler(0f, 180f, 0f);

            GameObject spawnable = NPCPool[Random.Range(0, NPCPool.Count)];
            spawnable.transform.SetPositionAndRotation(spawnPos, spawnRot);
            EXMET.AddSpawnable(spawnable, activeNPCsLL, NPCPool);
        }

        private float DistanceToLastNPCRL()
        {
            Vector3 playerPos = gameManager.playerTransform.position;

            if(activeNPCsRL.Count == 0)
            {
                return 0f;
            }
            else
            {
                GameObject lastNPC = activeNPCsRL[activeNPCsRL.Count - 1];

                return lastNPC.transform.position.z - playerPos.z;
            }
        }
         
        private float DistanceToLastNPCLL()
        {
            Vector3 playerPos = gameManager.playerTransform.position;

            if (activeNPCsLL.Count == 0)
            {
                return 0f;
            }
            else
            {
                GameObject lastNPC = activeNPCsLL[activeNPCsLL.Count - 1];

                return lastNPC.transform.position.z - playerPos.z;
            }
        }

        public void HeatLevelCheck()
        {
            if (gameManager.heat)
                heatVar = gameManager.heatVariations[gameManager.activeHeatLevel];

            if(activeHelis.Count < heatVar.maxHeliCount)
            {
                for (int i = activeHelis.Count; i < heatVar.maxHeliCount; i++)
                {
                    GameObject go = HeliPool[0];
                    EXMET.AddSpawnable(go, activeHelis, HeliPool);
                }                    
            }
            /*else if(activeHelis.Count > heatVar.maxHeliCount)
            {
                for (int i = 0; i < heatVar.maxHeliCount; i++)
                {
                    GameObject go = activeHelis[0];
                    StartCoroutine(go.GetComponent<HELICOPTER>().HideAway());
                }
            }*/
        }

        // DESPAWN NPC //
        private void DespawnUnwantedNPCs()
        {
            Vector3 playerPos = gameManager.playerTransform.position;

            if (activeNPCsRL.Count != 0)
                for (int i = 0; i < activeNPCsRL.Count; i++)
                {
                    GameObject npc = activeNPCsRL[i];
                    if (npc == null) continue;

                    if (npc.transform.position.z < playerPos.z - minDespawnDistance || npc.transform.position.z > playerPos.z + maxDespawnDistance)
                        EXMET.RemoveSpawnable(npc, activeNPCsRL, NPCPool);
                }

            if (activeNPCsLL.Count != 0)
                for (int i = 0; i < activeNPCsLL.Count; i++)
                {
                    GameObject npc = activeNPCsLL[i];
                    if (npc == null) continue;

                    if (npc.transform.position.z < playerPos.z - minDespawnDistance || npc.transform.position.z > playerPos.z + maxDespawnDistance)
                        EXMET.RemoveSpawnable(npc, activeNPCsLL, NPCPool);
                }

            if(activeCOPs.Count != 0)
                for (int i = 0; i < activeCOPs.Count; i++)
                {
                    GameObject cop = activeCOPs[i];
                    if (cop == null) continue;

                    if (cop.transform.position.z < playerPos.z - minDespawnDistance || cop.transform.position.z > playerPos.z + maxDespawnDistance)
                        EXMET.RemoveSpawnable(cop, activeCOPs, COPPool);
                }
        }

        // RESET NPC SPAWNS & OBSTACLES //
        public void KillEmAll()
        {
            for (int i = 0; i < activeNPCsRL.Count; i++)
            {
                EXMET.RemoveSpawnable(activeNPCsRL[i], activeNPCsRL, NPCPool); 
            }

            for (int i = 0; i < activeNPCsLL.Count; i++)
            {
                EXMET.RemoveSpawnable(activeNPCsLL[i], activeNPCsLL, NPCPool);
            }

            for (int i = 0; i < activeCOPs.Count; i++)
            {
                EXMET.RemoveSpawnable(activeCOPs[i], activeCOPs, COPPool);
            }

            for (int i = 0; i < activePickups.Count; i++)
            {
                EXMET.RemoveSpawnable(activePickups[i], activePickups, PickupPool);
            }

            for (int i = 0; i < activeHelis.Count; i++)
                EXMET.RemoveSpawnable(activeHelis[i], activeHelis, HeliPool);

            StopAllCoroutines();
        }
        #endregion NPCs

        #region Pickups
        public void SpawnPickUp(float zPos)
        {
            float xPos;
            GameObject tile = gameManager.activeTiles[0];

            if (tile.name.Contains("Start")) { return; }

            if (tile.name.Contains("Level1"))
                xPos = SpawnXPositionAll1();
            else
                xPos = SpawnXPositionAll2();

            if (xPos == 0f) { return; }

            Vector3 spawnPos = new Vector3(xPos, 0f, zPos);

            if (Random.Range(0f, 100f) <= roadVar.pickupSpawnChance && activePickups.Count <= roadVar.maxPickupCount)
            {
                float randomChance = Random.Range(0f, 1f);
                GameObject pickup;

                if (randomChance <= 0.25f)
                {
                    // BOOST //
                    pickup = PickupPool[0];
                }
                else if (randomChance <= 0.75f)
                {
                    // FIXER //
                    pickup = PickupPool[1];
                }
                else
                {
                    // TOKEN //
                    pickup = PickupPool[2];
                }

                pickup.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
                EXMET.AddSpawnable(pickup, activePickups, PickupPool);
            }
            else { return; }
        }

        public void RemovePickup(GameObject pickup)
        {
            EXMET.RemoveSpawnable(pickup, activePickups, PickupPool);
        }
        #endregion

        #region Spawn Positions
        private float SpawnXPositionRL1()
        {
            List<float> potentialX = new List<float>();
            int activelaneCount = 0;

            for (int i = 1; i < roadLanes.Length - 1; i++)
            {
                if (roadLanes[i].xPos < 0f) { continue; }
                if (!roadLanes[i].active) { continue; }
                else { activelaneCount++; }

                potentialX.Add(roadLanes[i].xPos);
            }

            if (potentialX.Count == 0) { return 0f; }

            return potentialX[Random.Range(0, potentialX.Count)];
        }

        private float SpawnXPositionRL2()
        {
            List<float> potentialX = new List<float>();
            int activelaneCount = 0;

            for (int i = 0; i < roadLanes.Length; i++)
            {
                if (roadLanes[i].xPos < 0f) { continue; }
                if (!roadLanes[i].active) { continue; }
                else { activelaneCount++; }

                potentialX.Add(roadLanes[i].xPos);
            }

            if (potentialX.Count == 0) { return 0f; }

            return potentialX[Random.Range(0, potentialX.Count)];
        }

        private float SpawnXPositionLL1()
        {
            List<float> potentialX = new List<float>();
            int activelaneCount = 0;

            for (int i = 1; i < roadLanes.Length - 1; i++)
            {
                if (roadLanes[i].xPos > 0f) { continue; }
                if (!roadLanes[i].active) { continue; }
                else { activelaneCount++; }

                potentialX.Add(roadLanes[i].xPos);
            }

            if (potentialX.Count == 0) { return 0f; }

            return potentialX[Random.Range(0, potentialX.Count)];
        }

        private float SpawnXPositionLL2()
        {
            List<float> potentialX = new List<float>();
            int activelaneCount = 0;

            for (int i = 0; i < roadLanes.Length; i++)
            {
                if (roadLanes[i].xPos > 0f) { continue; }
                if (!roadLanes[i].active) { continue; }
                else { activelaneCount++; }

                potentialX.Add(roadLanes[i].xPos);
            }

            if (potentialX.Count == 0) { return 0f; }

            return potentialX[Random.Range(0, potentialX.Count)];
        }

        private float SpawnXPositionAll1()
        {
            List<float> potentialX = new List<float>();
            int activelaneCount = 0;

            for (int i = 1; i < roadLanes.Length - 1; i++)
            {
                if (!roadLanes[i].active) { continue; }
                else { activelaneCount++; }

                potentialX.Add(roadLanes[i].xPos);
            }

            if (potentialX.Count == 0) { return 0f; }

            return potentialX[Random.Range(0, potentialX.Count)];
        }

        private float SpawnXPositionAll2()
        {
            List<float> potentialX = new List<float>();
            int activelaneCount = 0;

            for (int i = 0; i < roadLanes.Length; i++)
            {
                if (!roadLanes[i].active) { continue; }
                else { activelaneCount++; }

                potentialX.Add(roadLanes[i].xPos);
            }

            if (potentialX.Count == 0) { return 0f; }

            return potentialX[Random.Range(0, potentialX.Count)];
        }
        #endregion

        public void ActivateLane(float xPos)
        {
            for(int i = 0; i < roadLanes.Length; i++)
            {
                if (roadLanes[i].xPos == xPos) { roadLanes[i].active = true; }
            }
        }

        public void DeactivateLane(float xPos)
        {
            for (int i = 0; i < roadLanes.Length; i++)
            {
                if (roadLanes[i].xPos == xPos) { roadLanes[i].active = false; }
            }
        }

        public void ResetGame()
        {
            for (int i = 1; i < roadLanes.Length - 1; i++)
            {
                if (roadLanes[i].xPos != 3f) { ActivateLane(roadLanes[i].xPos); }
                else { DeactivateLane(roadLanes[i].xPos); }
            }

            KillEmAll();
        }
    }

    [Serializable]
    public class RoadLane
    {
        public float xPos;
        public bool active;
    }
}
