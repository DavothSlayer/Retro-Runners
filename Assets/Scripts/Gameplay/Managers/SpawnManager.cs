using System;
using System.Collections.Generic;
using System.Linq;
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

        [Header("Tiles")]
        public RoadVariation[] roadVariations;
        [SerializeField]
        private float tileSafeZone;
        public UnityEvent<float> RoadTileSpawned;
        [SerializeField]
        private Transform background;
        private float tileZSpawn = 0f;
        private float railZSpawn;
        private float tileLength = 400f;
        private float tileLineForPlayer = 1200f;
        
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
        // OBSTACLES //
        [HideInInspector]
        public List<GameObject> activePickups = new List<GameObject>();

        private RoadVariation roadVar;
        private HeatVariation heatVar;

        private void Start()
        {
            ResetGame();
            //HeatLevelCheck();
            //InitializeNPCs();
        }

        private void Update()
        {
            //HandleNPCs();

            TileHandler();
            //GuardRailHandler();
        }

        #region NPCs
        [BurstCompile]
        private void HandleNPCs()
        {
            roadVar = roadVariations[nextRoadVar];

            //spawn = GameManager.gameState == GameState.InGame || GameManager.gameState == GameState.InMenu;

            SpawnNPCs(roadVar);
            DespawnUnwantedNPCs();
        }

        public void InitializeNPCs()
        {
            RoadVariation roadVar = roadVariations[nextRoadVar];

            SpawnNPCs(roadVar);
        }

        // SPAWN NPC //
        private void SpawnNPCs(RoadVariation roadVar)
        {
            if(activeNPCsRL.Count < roadVar.maxNPCCountRL)
            {
                if (ProxyToLastNPCGoingLane() < roadVar.NPCSpawnDistance)
                {
                    print("Spawning NPC in going lane.");

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

                        if (!tile.name.Contains("Start"))
                            potentialX = PotentialXSpawnPos(Lanes.Right, tile.name.Contains("Level1"));

                        /*if (potentialZ > tile.transform.position.z && potentialZ < tile.transform.position.z + 400f)
                        {
                            if (!tile.name.Contains("Start"))
                                potentialX = PotentialXSpawnPos(Lanes.Right, tile.name.Contains("Level1"));
                        }*/
                    }

                    if (potentialX != 0f)
                    {
                        Vector3 spawnPos = new Vector3(potentialX, 0.1f, potentialZ);
                        Quaternion spawnRot = Quaternion.identity;

                        GameObject spawnable = NPCPool[Random.Range(0, NPCPool.Count)];
                        spawnable.transform.SetPositionAndRotation(spawnPos, spawnRot);
                        EXMET.AddSpawnable(spawnable, activeNPCsRL, NPCPool);

                        /*
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
                        */
                    }
                }
            }

            if(activeNPCsLL.Count < roadVar.maxNPCCountLL)
            {
                if (ProxyToLastNPCWrongLane() < roadVar.NPCSpawnDistance)
                {
                    print("Spawning NPC in wrong lane.");

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

                        if (!tile.name.Contains("Start"))
                            potentialX = PotentialXSpawnPos(Lanes.Left, tile.name.Contains("Level1"));

                        /*if (potentialZ > tile.transform.position.z && potentialZ < tile.transform.position.z + 400f)
                        {
                            if (!tile.name.Contains("Start"))
                                potentialX = PotentialXSpawnPos(Lanes.Left, tile.name.Contains("Level1"));
                        }*/
                    }

                    if (potentialX != 0f)
                    {
                        Vector3 spawnPos = new Vector3(potentialX, 0.1f, potentialZ);
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
            if (gameManager.playerCar == null) { return; }

            background.position = new Vector3(0f, 0f, gameManager.playerTransform.position.z + 1300f);

            if (gameManager.playerTransform.position.z >= tileLineForPlayer)
            {
                nextRoadVar = NextRoadVar();

                SpawnRoadTile();

                tileLineForPlayer += tileLength;
            }

            if (activeTiles.Count <= 5) { return; }

            if (gameManager.playerTransform.position.z >= activeTiles[0].transform.position.z + tileSafeZone)
            {
                RemoveRoadTile(0);
            }
        }

        [BurstCompile]
        private void SpawnRoadTile()
        {
            // SPAWN NEXT TILE //
            var tileArray = roadVariations[activeRoadVar].roadTiles;
            GameObject nextTile = NextTile();

            if (activeRoadVar != nextRoadVar)
            {
                activeRoadVar = nextRoadVar;
            }
            else
            {
                if (activeTiles.Contains(nextTile))
                {
                    int duplicateInt = activeTiles.IndexOf(nextTile);
                    RemoveRoadTile(duplicateInt);
                }
            }

            nextTile.transform.SetPositionAndRotation(transform.forward * tileZSpawn, transform.rotation);
            EXMET.AddSpawnable(nextTile, activeTiles, tileArray);

            roadLanes[0].active = nextTile.name.Contains("Level2");
            roadLanes[5].active = nextTile.name.Contains("Level2");

            tileZSpawn += tileLength;

            if (GameManager.gameState == GameState.InGame)
                RoadTileSpawned?.Invoke(tileZSpawn);
        }

        private GameObject NextTile()
        {
            // GET NEXT TILE //
            var tileArray = roadVariations[activeRoadVar].roadTiles;
            GameObject tile;

            if (activeRoadVar != nextRoadVar)
            {
                tile = roadVariations[nextRoadVar].transitionTile;
            }
            else
            {
                if (tileArray.Count == 0)
                    tile = activeTiles[0];
                else
                    tile = tileArray[0];
            }

            return tile;
        }

        [BurstCompile]
        private int NextRoadVar()
        {
            for (int i = 0; i < activeTiles.Count; i++)
                if (activeTiles[i].name.Contains("Start")) return activeRoadVar;

            int nextVar = Random.Range(0f, 1f) > 0.5f ? 1 : 0;

            return nextVar;
        }

        [BurstCompile]
        private void RemoveRoadTile(int integer)
        {
            if (activeTiles.Count == 0) { return; }

            if (!activeTiles[integer].name.Contains("Start"))
            {
                GameObject obj = activeTiles[integer];
                int index = obj.name.Contains("Level1") ? 0 : 1;

                EXMET.RemoveSpawnable(obj, activeTiles, roadVariations[index].roadTiles);
            }
            else if (activeTiles[integer].name.Contains("Start"))
            {
                GameObject obj = activeTiles[integer];

                obj.SetActive(false);
                activeTiles.Remove(obj);
            }
        }
        // ROAD TILE METHODS //

        // GUARD RAIL METHODS //
        [BurstCompile]
        private void GuardRailHandler()
        {
            if (gameManager.playerCar == null) { return; }

            if (gameManager.playerTransform.position.z > railZSpawn - 800f)
                SpawnGuardRail();

            if (activeRails.Count <= 1) return;
            if (gameManager.playerTransform.position.z > activeRails[0].transform.position.z + 800f)
                RemoveRail(0);
        }

        private void SpawnGuardRail()
        {
            // SPAWN NEXT RAIL //
            var railArray = roadVariations[activeRoadVar].rails;
            GameObject nextRail = NextRail();

            nextRail.transform.SetPositionAndRotation(transform.forward * railZSpawn, transform.rotation);
            EXMET.AddSpawnable(nextRail, activeRails, railArray);

            railZSpawn += 200f;
        }

        private GameObject NextRail()
        {
            if (roadVariations[activeRoadVar].rails.Count == 0)
                return activeRails[0];
            else
                return roadVariations[activeRoadVar].rails[0];
        }

        private void RemoveRail(int index)
        {
            GameObject obj = activeRails[index];

            int railIndex = obj.name.Contains("Rail1") ? 0 : 1;
            EXMET.RemoveSpawnable(obj, activeRails, roadVariations[railIndex].rails);
        }

        public void RemoveDeadRail(GameObject obj)
        {
            int railIndex = obj.name.Contains("Rail1") ? 0 : 1;
            EXMET.RemoveSpawnable(obj, activeRails, roadVariations[railIndex].rails);
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
            tileZSpawn = 0f;
            tileLineForPlayer = 1000f;
            railZSpawn = 0f;
            activeRoadVar = 0;
            nextRoadVar = 0;

            // DESPAWN ACTIVE TILES //
            while (activeTiles.Count > 0)
                RemoveRoadTile(0);

            // SPAWN IN NEW ROAD TILES //
            for (int i = 0; i < 5; i++)
                SpawnRoadTile();

            // DESPAWN ACTIVE RAILS //
            while (activeRails.Count > 0)
                RemoveRail(0);

            // SPAWN IN NEW RAILS //
            for (int i = 0; i < 6; i++)
                SpawnGuardRail();

            roadLanes[0].active = false;
            roadLanes[5].active = false;

            for (int i = 1; i < roadLanes.Length - 1; i++)
            {
                if (roadLanes[i].xPos != 3f) { ActivateLane(roadLanes[i].xPos); }
                else { DeactivateLane(roadLanes[i].xPos); }
            }

            KillEmAll();
            //InitializeNPCs();
        }
    }

    [Serializable]
    public class RoadLane
    {
        public float xPos;
        public bool active;
    }
}
