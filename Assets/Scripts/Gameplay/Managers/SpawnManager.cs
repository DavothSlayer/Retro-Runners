using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Events;
using V3CTOR;
using Random = UnityEngine.Random;

namespace RetroCode
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("Core Functionality")]
        [SerializeField]
        private GameManager gameManager;
        public bool spawnNPC;
        public bool spawnRoad;

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
        public List<GameObject> DeadNPCPool = new List<GameObject>();
        public List<GameObject> HeliPool = new List<GameObject>();
        public List<GameObject> COPPool = new List<GameObject>();
        public List<GameObject> RailsPool = new List<GameObject>();
        public List<GameObject> PickupPool = new List<GameObject>();
        public List<GameObject> PickupList = new List<GameObject>();

        [Header("Tiles")]
        public RoadVariation[] roadVariations;
        [SerializeField]
        private float tileSafeZone;
        public UnityEvent<float> RoadTileSpawned;
        [SerializeField]
        private Transform background;
        private float tileZSpawn = 0f;
        private float tileLength = 400f;
        
        private int nextRoadVar = 0;
        private int activeRoadVar = 0;
        private List<GameObject> activeTiles = new List<GameObject>();
        private List<GameObject> activeRails = new List<GameObject>();

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
        [HideInInspector]
        public List<GameObject> activeDeadNPCs = new List<GameObject>();
        // OBSTACLES //
        [HideInInspector]
        public List<GameObject> activePickups = new List<GameObject>();

        private RoadVariation roadVar;
        private HeatVariation heatVar;

        private void Start()
        {
            ResetGame();
            //HeatLevelCheck();
            InitializeNPCs();
        }

        [BurstCompile]
        private void Update()
        {
            if (gameManager.playerTransform == null) return;

            HandleNPCs();

            TileHandler();
        }

        #region NPCs
        [BurstCompile]
        private void HandleNPCs()
        {
            if (!spawnNPC) return;
            
            roadVar = roadVariations[nextRoadVar];

            SpawnNPCs(roadVar);
            DespawnUnwantedNPCs();
        }

        public void InitializeNPCs()
        {
            while (gameManager.playerTransform == null) return;

            if (!spawnNPC) return;

            RoadVariation roadVar = roadVariations[nextRoadVar];

            SpawnNPCs(roadVar);
        }

        // SPAWN NPC //
        [BurstCompile]
        private void SpawnNPCs(RoadVariation roadVar)
        {
            if(activeNPCsRL.Count < roadVar.maxNPCCountRL)
            {
                if (ProxyToLastNPCGoingLane() < roadVar.NPCSpawnDistance)
                {
                    // CALCULATE ZPOS //
                    float potentialZ;
                    if (activeNPCsRL.Count == 0)
                    {
                        potentialZ = gameManager.playerTransform.position.z + roadVar.NPCSpawnDistance;
                    }
                    else
                    {
                        if (activeNPCsRL[activeNPCsRL.Count - 1].transform.position.z < gameManager.playerTransform.position.z + roadVar.distanceBetweenNPC)
                        {
                            potentialZ = gameManager.playerTransform.position.z + roadVar.NPCSpawnDistance;
                        }
                        else
                        {
                            GameObject lastNPC = activeNPCsRL[activeNPCsRL.Count - 1];
                            potentialZ = lastNPC.transform.position.z + roadVar.distanceBetweenNPC;
                        }
                    }

                    // CALCULATE XPOS //
                    float potentialX = 0f;
                    for (int i = 0; i < activeTiles.Count; i++)
                    {
                        GameObject tile = activeTiles[i];

                        if (potentialZ > tile.transform.position.z && potentialZ < tile.transform.position.z + 400f)
                        {
                            if (!tile.name.Contains("Last"))
                                potentialX = PotentialXSpawnPos(Lanes.Right, tile.name.Contains("Level1"));
                        }
                    }

                    if (potentialX != 0f)
                    {
                        Vector3 spawnPos = new Vector3(potentialX, 0f, potentialZ);
                        Quaternion spawnRot = Quaternion.identity;

                        GameObject spawnable = NPCPool[Random.Range(0, NPCPool.Count)];
                        spawnable.transform.SetPositionAndRotation(spawnPos, spawnRot);
                        EXMET.AddSpawnable(spawnable, activeNPCsRL, NPCPool);

                        if (gameManager.heat &&
                            Random.Range(0f, 100f) <= heatVar.copSpawnChance &&
                            activeCOPs.Count <= heatVar.maxCOPCount &&
                            GameManager.gameState == GameState.InGame &&
                            COPPool.Count > 0)
                        {
                            Vector3 posOffset = new Vector3(0f, 0f, 45f);

                            GameObject cop = COPPool[Random.Range(0, COPPool.Count)];
                            cop.transform.SetPositionAndRotation(spawnPos + posOffset, spawnRot);
                            EXMET.AddSpawnable(cop, activeCOPs, COPPool);
                        }
                    }
                }
            }

            if(activeNPCsLL.Count < roadVar.maxNPCCountLL)
            {
                if (ProxyToLastNPCWrongLane() < roadVar.NPCSpawnDistance)
                {
                    // CALCULATE ZPOS //
                    float potentialZ;
                    if (activeNPCsLL.Count == 0)
                    {
                        potentialZ = gameManager.playerTransform.position.z + roadVar.NPCSpawnDistance;
                    }
                    else
                    {
                        if (activeNPCsLL[activeNPCsLL.Count - 1].transform.position.z < gameManager.playerTransform.position.z + roadVar.distanceBetweenNPC)
                        {
                            potentialZ = gameManager.playerTransform.position.z + roadVar.NPCSpawnDistance;
                        }
                        else
                        {
                            GameObject lastNPC = activeNPCsLL[activeNPCsLL.Count - 1];
                            potentialZ = lastNPC.transform.position.z + roadVar.distanceBetweenNPC;
                        }
                    }

                    // CALCULATE XPOS //
                    float potentialX = 0f;
                    for (int i = 0; i < activeTiles.Count; i++)
                    {
                        GameObject tile = activeTiles[i];

                        if (potentialZ > tile.transform.position.z && potentialZ < tile.transform.position.z + 400f)
                        {
                            if (!tile.name.Contains("Last"))
                                potentialX = PotentialXSpawnPos(Lanes.Left, tile.name.Contains("Level1"));
                        }
                    }

                    if (potentialX != 0f)
                    {
                        Vector3 spawnPos = new Vector3(potentialX, 0f, potentialZ);
                        Quaternion spawnRot = Quaternion.Euler(0f, 180f, 0f);

                        GameObject spawnable = NPCPool[Random.Range(0, NPCPool.Count)];
                        spawnable.transform.SetPositionAndRotation(spawnPos, spawnRot);
                        EXMET.AddSpawnable(spawnable, activeNPCsLL, NPCPool);
                    }
                }
            }
        }

        private float ProxyToLastNPCGoingLane()
        {
            Vector3 playerPos = gameManager.playerTransform.position;

            if(activeNPCsRL.Count == 0) return 0f;

            GameObject lastNPC = activeNPCsRL[activeNPCsRL.Count - 1];

            return lastNPC.transform.position.z - playerPos.z;
        }
         
        private float ProxyToLastNPCWrongLane()
        {
            Vector3 playerPos = gameManager.playerTransform.position;

            if (activeNPCsLL.Count == 0) return 0f;

            GameObject lastNPC = activeNPCsLL[activeNPCsLL.Count - 1];

            return lastNPC.transform.position.z - playerPos.z;
        }

        [BurstCompile]
        public void HandleDeadNPC(NPC npc)
        {
            EXMET.RemoveSpawnable(npc.gameObject, npc.transform.position.x < 0f ? activeNPCsLL : activeNPCsRL, NPCPool);

            DeadNPC deadnpc = null;
            for(int i = 0; i < DeadNPCPool.Count; i++)
            {
                DeadNPC dNPC = DeadNPCPool[i].GetComponent<DeadNPC>();

                if (dNPC.NPCType == npc.NPCType)
                {
                    deadnpc = dNPC;
                    break;
                }
            }

            if (deadnpc == null) return;

            deadnpc.transform.SetPositionAndRotation(npc.transform.position, npc.transform.rotation);
            EXMET.AddSpawnable(deadnpc.gameObject, activeDeadNPCs, DeadNPCPool);
            deadnpc.ActivateDead(npc.transform.forward * Random.Range(15f, 25f) + npc.transform.up * Random.Range(3f, 7f));
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

            if(activeCOPs.Count != 0)
                for (int i = 0; i < activeCOPs.Count; i++)
                {
                    GameObject cop = activeCOPs[i];
                    if (cop == null) continue;

                    if (cop.transform.position.z < playerPos.z - minDespawnDistance || cop.transform.position.z > playerPos.z + maxDespawnDistance)
                        EXMET.RemoveSpawnable(cop, activeCOPs, COPPool);
                }

            if (activeNPCsLL.Count != 0)
                for (int i = 0; i < activeNPCsLL.Count; i++)
                {
                    GameObject npc = activeNPCsLL[i];
                    if (npc == null) continue;

                    if (npc.transform.position.z < playerPos.z - minDespawnDistance || npc.transform.position.z > playerPos.z + maxDespawnDistance)
                        EXMET.RemoveSpawnable(npc, activeNPCsLL, NPCPool);
                }

            if (activeNPCsRL.Count != 0)
                for (int i = 0; i < activeNPCsRL.Count; i++)
                {
                    GameObject npc = activeNPCsRL[i];
                    if (npc == null) continue;

                    if (npc.transform.position.z < playerPos.z - minDespawnDistance || npc.transform.position.z > playerPos.z + maxDespawnDistance)
                        EXMET.RemoveSpawnable(npc, activeNPCsRL, NPCPool);
                }

            if(activeDeadNPCs.Count != 0)
                for (int i = 0; i < activeDeadNPCs.Count; i++)
                {
                    DeadNPC deadNPC = activeDeadNPCs[i].GetComponent<DeadNPC>();
                    if (deadNPC == null) continue;

                    if (deadNPC.referenceRigidBody.position.z < playerPos.z - minDespawnDistance || deadNPC.referenceRigidBody.position.z > playerPos.z + maxDespawnDistance)
                    {
                        EXMET.RemoveSpawnable(deadNPC.gameObject, activeDeadNPCs, DeadNPCPool);
                        deadNPC.ResetDead();
                    }
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
            {
                EXMET.RemoveSpawnable(activeHelis[i], activeHelis, HeliPool);
            }
        }
        #endregion NPCs

        #region Pickups
        public void SpawnPickUp(float zPos)
        {
            float potentialX;
            GameObject tile = activeTiles[0];

            if (tile.name.Contains("Start")) { return; }

            potentialX = PotentialXSpawnPos(Lanes.Both, tile.name.Contains("Level1"));

            if (potentialX == 0f) { return; }

            Vector3 spawnPos = new Vector3(potentialX, 0f, zPos);

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

        #region Road Generation
        // ROAD TILE METHODS //
        [BurstCompile]
        private void TileHandler()
        {
            if (gameManager.playerCar == null) return;

            background.position = new(0f, 0f, gameManager.playerTransform.position.z + 800f);

            if (gameManager.playerTransform.position.z + tileSafeZone >= tileZSpawn && spawnRoad)
            {
                SpawnRoadTile();
                RemoveRoadTile(0);
            }
        }

        [BurstCompile]
        private void SpawnRoadTile()
        {
            // SPAWN NEXT TILE //
            GameObject nextTile = NextTile();
            activeRoadVar = nextRoadVar;
            var tileArray = roadVariations[activeRoadVar].roadTilePool;

            nextTile.transform.SetPositionAndRotation(transform.forward * tileZSpawn, transform.rotation);
            EXMET.AddSpawnable(nextTile, activeTiles, tileArray);

            roadLanes[0].active = nextTile.name.Contains("Level2");
            roadLanes[5].active = nextTile.name.Contains("Level2");

            for (int i = 0; i < 2; i++) SpawnGuardRail(tileZSpawn + i * 200);

            tileZSpawn += tileLength;
            nextRoadVar = NextRoadVar();

            if (GameManager.gameState == GameState.InGame)
                RoadTileSpawned?.Invoke(tileZSpawn);
        }

        [BurstCompile]
        private GameObject NextTile()
        {
            // GET NEXT TILE //
            var tileArray = roadVariations[activeRoadVar].roadTilePool;
            GameObject tile = null;

            if (activeRoadVar != nextRoadVar)
            {
                for (int i = 0; i < tileArray.Count; i++)
                    if (tileArray[i].name.Contains("Last"))
                    {
                        tile = tileArray[i];
                    }
            }
            else
            {
                for (int i = 0; i < tileArray.Count; i++)
                    if (!tileArray[i].name.Contains("Last"))
                    {
                        tile = tileArray[i];
                    }
            }

            return tile;
        }

        [BurstCompile]
        private int NextRoadVar()
        {
            for (int i = 0; i < activeTiles.Count; i++)
                if (activeTiles[i].name.Contains("Last"))
                    return activeTiles[i].name.Contains("Level1Last") ? 1 : 0;

            int nextVar = Random.Range(0f, 1f) > 0.5f ? 1 : 0;
            return nextVar;
        }

        [BurstCompile]
        private void RemoveRoadTile(int integer)
        {
            if (activeTiles[integer] == null) return;

            GameObject obj = activeTiles[integer];
            int index = obj.name.Contains("Level1") ? 0 : 1;

            EXMET.RemoveSpawnable(obj, activeTiles, roadVariations[index].roadTilePool);

            for (int i = 0; i < 2; i++) RemoveRail(0);
        }
        // ROAD TILE METHODS //

        // GUARD RAIL METHODS //
        [BurstCompile]
        private void SpawnGuardRail(float zPos)
        {
            // SPAWN NEXT RAIL //
            GameObject nextRail = NextRail();

            nextRail.GetComponent<GuardRail>().ResetRail();
            nextRail.transform.SetPositionAndRotation(transform.forward * zPos, transform.rotation);
            EXMET.AddSpawnable(nextRail, activeRails, RailsPool);
        }

        private GameObject NextRail()
        {
            if (RailsPool.Count == 0)
                return activeRails[0];
            else
                return RailsPool[0];
        }

        [BurstCompile]
        private void RemoveRail(int index)
        {
            if (activeRails.Count == 0) return;
            if (activeRails[index] == null) return;

            GameObject obj = activeRails[index];
            EXMET.RemoveSpawnable(obj, activeRails, RailsPool);
        }
        // GUARD RAIL METHODS //
        #endregion

        #region Spawn Positions
        private enum Lanes
        {
            Both,
            Left,
            Right,
        }

        private float PotentialXSpawnPos(Lanes lanes, bool fourlanes)
        {
            List<float> potentialX = new List<float>();

            switch (lanes)
            {
                case Lanes.Both:
                    if (fourlanes)
                    {
                        for (int i = 1; i < roadLanes.Length - 1; i++)
                        {
                            if (!roadLanes[i].active) { continue; }

                            potentialX.Add(roadLanes[i].xPos);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < roadLanes.Length; i++)
                        {
                            if (!roadLanes[i].active) { continue; }

                            potentialX.Add(roadLanes[i].xPos);
                        }
                    }
                    break;
                case Lanes.Left:
                    if (fourlanes)
                    {
                        for (int i = 1; i < roadLanes.Length - 1; i++)
                        {
                            if (roadLanes[i].xPos > 0f) { continue; }
                            if (!roadLanes[i].active) { continue; }

                            potentialX.Add(roadLanes[i].xPos);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < roadLanes.Length; i++)
                        {
                            if (roadLanes[i].xPos > 0f) { continue; }
                            if (!roadLanes[i].active) { continue; }

                            potentialX.Add(roadLanes[i].xPos);
                        }
                    }
                    break;
                case Lanes.Right:
                    if (fourlanes)
                    {
                        for (int i = 1; i < roadLanes.Length - 1; i++)
                        {
                            if (roadLanes[i].xPos < 0f) { continue; }
                            if (!roadLanes[i].active) { continue; }

                            potentialX.Add(roadLanes[i].xPos);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < roadLanes.Length; i++)
                        {
                            if (roadLanes[i].xPos < 0f) { continue; }
                            if (!roadLanes[i].active) { continue; }

                            potentialX.Add(roadLanes[i].xPos);
                        }
                    }
                    break;
                default:
                    return 0f;
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

        [BurstCompile]
        public void ResetGame()
        {
            // DESPAWN ACTIVE TILES //
            spawnNPC = false;
            spawnRoad = false;

            for (int i = activeTiles.Count; i > 0; i--)
                RemoveRoadTile(activeTiles.Count - 1);

            tileZSpawn = 0f;
            activeRoadVar = 0;
            nextRoadVar = 0;

            for (int i = 0; i < 5; i++)
                SpawnRoadTile();

            spawnRoad = true;

            roadLanes[0].active = false;
            roadLanes[5].active = false;

            for (int i = 1; i < roadLanes.Length - 1; i++)
            {
                if (roadLanes[i].xPos != 3f) { ActivateLane(roadLanes[i].xPos); }
                else { DeactivateLane(roadLanes[i].xPos); }
            }

            KillEmAll();

            spawnNPC = true;

            InitializeNPCs();
        }
    }

    public enum NPCType
    {
        Sedan,
        Flatbed,
        Oilrig,
    }

    [Serializable]
    public class RoadLane
    {
        public float xPos;
        public bool active;
    }

    [Serializable]
    public class RoadVariation
    {
        public List<GameObject> roadTilePool = new List<GameObject>();
        [Space]
        public int maxNPCCountRL;
        public int maxNPCCountLL;
        [Space]
        public float NPCSpawnDistance;
        public float distanceBetweenNPC;
        [Space]
        public int maxPickupCount;
        [Range(0, 100f)]
        public float pickupSpawnChance;
    }
}
