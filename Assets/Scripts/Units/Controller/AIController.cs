﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Library;
using UI;
using UnityEngine;
using Event = Define.Event;
using Random = UnityEngine.Random;

namespace Units.Controller
{
    public class AIController : MonoSingleton<AIController>, IController
    {
        [SerializeField]
        private List<Vector3> m_SpawnPoints;
        [SerializeField]
        private List<GameObject> m_EnemyBases;
        [SerializeField]
        private GameObject m_GoblinMagePrefab;
        [SerializeField]
        private GameObject m_GoblinPrefab;
        [SerializeField]
        private List<IStats> m_Enemies;
        private int m_WaveCounter;
        [SerializeField]
        private int m_MaxWaveCount = 2;
        private List<IControllable> m_Controllables; 
        [SerializeField]
        private GameObject m_ManaPickupPrefab;
        [SerializeField]
        private GameObject m_HealthPickupPrefab;
        [SerializeField]
        private float m_MaxCountdownTimer;
        private float m_CurrentCountdownTimer;
        private bool m_ApplicationIsQuitting;
        private Player m_Player;
        protected override void Awake()
        {
            base.Awake();

            m_Controllables = new List<IControllable>();
            m_Enemies = new List<IStats>();
            m_EnemyBases = new List<GameObject>();

            m_CurrentCountdownTimer = m_MaxCountdownTimer;

            Publisher.self.Subscribe(Event.SpawnWaveClicked, SpawnWaves);
            Publisher.self.Subscribe(Event.UnitDied, OnUnitDied);
            Publisher.self.Subscribe(Event.UnitCanUpgradeSkill, OnCanUpgradeSkill);
        }

        // Use this for initialization
        void Start()
        {
            m_EnemyBases = GameObject.FindGameObjectsWithTag("EnemySpawn").ToList();
            m_Player = FindObjectOfType<Player>();
            StartCoroutine(AutoSpawn());
        }

        // Update is called once per frame
        void Update()
        {
            m_CurrentCountdownTimer -= Time.deltaTime;
            UIManager.self.AutoSpawnTimer.text = "Spawn Wave(" + Mathf.Ceil(m_CurrentCountdownTimer) + ")";
            if(m_CurrentCountdownTimer <= 0)
                UIManager.self.AutoSpawnTimer.text = "Enemies: " + m_Enemies.Count;
            Search();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Publisher.self.UnSubscribe(Event.SpawnWaveClicked, SpawnWaves);
            Publisher.self.UnSubscribe(Event.UnitDied, OnUnitDied);
            Publisher.self.UnSubscribe(Event.UnitCanUpgradeSkill, OnCanUpgradeSkill);
        }

        private void OnApplicationQuit()
        {
            m_ApplicationIsQuitting = true;
        }

        private void Search()
        {
            foreach (IControllable controlable in m_Controllables)
            {
                if (controlable.following == null)
                    SetFollowing(controlable);

                if (controlable.following != null)
                {
                    IUsesSkills skillUser = controlable as IUsesSkills;
                    controlable.navMashAgent.speed = controlable.speed;
                    controlable.navMashAgent.SetDestination(controlable.following.transform.position);

                    float distanceFromEnemyToTarget = 
                        Vector3.Distance(controlable.following.transform.position, controlable.transform.position);

                    if ((distanceFromEnemyToTarget < 7 && controlable.controllerType == ControllerType.GoblinMage) ||
                        (distanceFromEnemyToTarget < 2 && controlable.controllerType == ControllerType.Goblin))
                    {
                        Publisher.self.Broadcast(Event.UseSkill, skillUser, 0);
                    }
                }
            }
        }

        public void Register(IControllable a_Controllable)
        {
            switch (a_Controllable.controllerType)
            {
                case ControllerType.GoblinMage:
                    SetFollowing(a_Controllable);
                    m_Controllables.Add(a_Controllable);
                    break;

                case ControllerType.Goblin:
                    SetFollowing(a_Controllable);
                    m_Controllables.Add(a_Controllable);
                    break;
            }
        }
        public void Register(Player a_Player)
        {
            //m_Player = a_Player;

            //Register(m_Player.unit);
        }

        public void UnRegister(IControllable a_Controllable)
        {
            m_Controllables.Remove(a_Controllable);
        }
        public void UnRegister(Player a_Player)
        {
            //m_Player = null;
        }

        private void SetFollowing(IControllable a_Controllable)
        {
            if (a_Controllable.controllerType == ControllerType.Goblin)
            {
                List<GameObject> players = GameObject.FindGameObjectsWithTag("Player").ToList();
                if (players.Count == 0)
                    return;

                players.Sort(
                    delegate (GameObject a, GameObject b)
                    {
                        float distanceA = Vector3.Distance(a.transform.position, a_Controllable.transform.position);
                        float distanceB = Vector3.Distance(b.transform.position, a_Controllable.transform.position);
                        
                        if (distanceA > distanceB)
                            return 1;
                        if (distanceA < distanceB)
                            return -1;

                        return 0;
                    });
                a_Controllable.following = players[0];
            }

            if (a_Controllable.controllerType == ControllerType.GoblinMage)
            {
                List<GameObject> fortresses = GameObject.FindGameObjectsWithTag("Fortress").ToList();
                if (fortresses.Count == 0)
                    return;

                fortresses.Sort(
                    delegate (GameObject a, GameObject b)
                    {
                        float distanceA = Vector3.Distance(a.transform.position, a_Controllable.transform.position);
                        Debug.Log(distanceA);

                        float distanceB = Vector3.Distance(b.transform.position, a_Controllable.transform.position);
                        Debug.Log(distanceB);
                        if (distanceA > distanceB)
                            return 1;
                        if (distanceA < distanceB)
                            return -1;

                        return 0;
                    });
                a_Controllable.following = fortresses[0];
            }
        }
        private void SpawnWaves(Event a_Event, params object[] a_Params)
        {
            if (m_Enemies.Count != 0)
                return;

            StopAllCoroutines();

            StartCoroutine(Spawn());

        }

        private void OnUnitDied(Event a_Event, params object[] a_Params)
        {
            IStats unit = a_Params[0] as IStats;

            if (unit == null || m_ApplicationIsQuitting)
                return;

            if (unit.gameObject.tag == "Player")
                Publisher.self.Broadcast(Event.GameOver);


            if (m_Enemies.Contains(unit))
            {
                Vector3 healthInstantPosition = 
                    new Vector3(
                        unit.gameObject.transform.position.x + 0.25f, 
                        unit.gameObject.transform.position.y + 0.5f, 
                        unit.gameObject.transform.position.z);
                Vector3 manaInstantPosition = new Vector3(
                    unit.gameObject.transform.position.x - 0.25f, 
                    unit.gameObject.transform.position.y + 0.5f, 
                    unit.gameObject.transform.position.z);

                GameObject newHealthPickup = 
                    Instantiate(m_HealthPickupPrefab, healthInstantPosition, Quaternion.identity) as GameObject;
                GameObject newManaPickup = 
                    Instantiate(m_ManaPickupPrefab, manaInstantPosition, Quaternion.identity) as GameObject;


                if (m_Player.unit.health >= m_Player.unit.maxHealth)
                    Destroy(newHealthPickup, 3.0f);

                if (m_Player.unit.mana >= m_Player.unit.maxMana)
                    Destroy(newManaPickup, 3.0f);

                newHealthPickup.GetComponent<Rigidbody>().
                    AddExplosionForce(250 + Random.value * 750, unit.gameObject.transform.position, 10);
                newManaPickup.GetComponent<Rigidbody>().
                    AddExplosionForce(250 + Random.value * 750, unit.gameObject.transform.position, 10);
            }

            m_Enemies.Remove(unit);

            if (m_Enemies.Count == 0 && m_WaveCounter != m_MaxWaveCount)
                StartCoroutine(AutoSpawn());

            if (m_WaveCounter == m_MaxWaveCount && m_Enemies.Count == 0)
                Publisher.self.Broadcast(Event.GameWin);
        }

        private IEnumerator Spawn()
        {
            
            m_CurrentCountdownTimer = 0;
            m_WaveCounter++;

            if (m_WaveCounter == m_MaxWaveCount)
                UIAnnouncer.self.Announce("FINAL WAVE!!");


            Publisher.self.Broadcast(Event.SpawnWave, m_WaveCounter);
            if (m_SpawnPoints.Count == 0)
            {
                for (int BasesIndex = 0; BasesIndex <= m_EnemyBases.Count - 1; BasesIndex++)
                {
                    Vector3 spawnPoints = m_EnemyBases[BasesIndex].transform.GetChild(0).position;
                    m_SpawnPoints.Add(spawnPoints);
                }
            }

            foreach (Vector3 spawnPoint in m_SpawnPoints)
            {
                for (int i = 0; i < m_WaveCounter; i++)
                {

                    GameObject goblin = Instantiate(m_GoblinPrefab);

                    goblin.transform.position = new Vector3(
                        spawnPoint.x,
                        spawnPoint.y,
                        spawnPoint.z);


                    GameObject goblinMage = Instantiate(m_GoblinMagePrefab);
                    goblinMage.transform.position = new Vector3(
                        spawnPoint.x,
                        spawnPoint.y,
                        spawnPoint.z);

                    m_Enemies.Add(goblinMage.GetComponent<IStats>());
                    m_Enemies.Add(goblin.GetComponent<IStats>());
                    yield return new WaitForSeconds(0.75f);
                }
            }

        }
        private IEnumerator AutoSpawn()
        {
            m_CurrentCountdownTimer = m_MaxCountdownTimer;
            yield return new WaitForSeconds(m_MaxCountdownTimer);

            yield return Spawn();
        }

        private void OnCanUpgradeSkill(Event a_Event, params object[] a_Params)
        {
            IUsesSkills unit = a_Params[0] as IUsesSkills;

            if (unit == null)
                return;

            if (m_Controllables.Contains(unit as IControllable))
            {
                int skillindex = Random.Range(0, unit.skills.Count - 1);

                Publisher.self.Broadcast(Event.UpgradeSkill, unit, skillindex);
            }
            
        }
    }
}
