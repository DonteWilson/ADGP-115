﻿using UnityEngine;
using System.Collections.Generic;
using Library;
using Units.Skills;

using Event = Define.Event;

namespace Units
{
    public class Unit : MonoBehaviour, IUsesSkills, IControlable
    {
        protected NavMeshAgent m_NavMeshAgent;
        protected GameObject m_Following;
        protected IController m_Controller;
        protected ControllerType m_ControllerType;
        protected FiniteStateMachine<DamageState> m_DamageFSM;
        protected FiniteStateMachine<MovementState> m_MovementFSM;
        protected List<Skill> m_Skills;
        protected string m_UnitName;
        protected string m_UnitNickname;
        [SerializeField]
        protected float m_MaxMana;
        protected float m_Mana;
        [SerializeField]
        protected float m_MaxDefense;
        protected float m_Defense;
        [SerializeField]
        protected float m_MaxHealth;
        protected float m_Health;

        protected float m_Experience;
        protected int m_Level;

        protected Vector3 m_TotalVelocity;
        protected Vector3 m_Velocity;

        protected float m_Speed;
        protected Moving m_IsMoving;

        protected bool m_CanMoveWithInput;


        public NavMeshAgent navMashAgent
        {
            get { return m_NavMeshAgent;}
            set { m_NavMeshAgent = value; }  
        }

        public GameObject following
        {
            get { return m_Following;}
            set { m_Following = value; }
        }

        public IController controller
        {
            get { return m_Controller; }
            set { m_Controller = value; }
        }

        public ControllerType controllerType
        {
            get { return m_ControllerType; }
            set { m_ControllerType = value; }
        }

        public FiniteStateMachine<DamageState> damageFSM
        {
            get { return m_DamageFSM; }
            set { m_DamageFSM = value; }
        }

        public FiniteStateMachine<MovementState> movementFSM
        {
            get { return m_MovementFSM; }
            set { m_MovementFSM = value; }
        }

        public List<Skill> skills
        {
            get { return m_Skills; }
            set { m_Skills = value; }
        }

        //string name property
        public string unitName
        {
            get { return m_UnitName; }
            set { m_UnitName = value; }
        }
        //string nickname property
        public string unitNickname
        {
            get { return m_UnitNickname; }
            set { m_UnitNickname = value; }
        }
        //max mana int property
        public float maxMana
        {
            get { return m_MaxMana; }
            set { m_MaxMana = value; }
        }

        //mana int property
        public float mana
        {
            get { return m_Mana; }
            set {m_Mana = value; Publisher.self.DelayedBroadcast(Event.UnitManaChanged, this); }
        }

        //Max defense int property
        public float maxDefense
        {
            get { return m_MaxDefense; }
            set { m_MaxDefense = value; }
        }
        //Defense int property
        public float defense
        {
            get { return m_Defense; }
            set { m_Defense = value; }
        }

        //maxhealth int property
        public float maxHealth
        {
            get { return m_MaxHealth; }
            set { m_MaxHealth = value; }
        }

        //health int property
        public float health
        {
            get { return m_Health; }
            set { m_Health = value; }
        }
        //Experience int property
        public float experience
        {
            get { return m_Experience; }
            set { m_Experience = value; }
        }

        //Level int property
        public int level
        {
            get { return m_Level; }
            set { m_Level = value; Publisher.self.DelayedBroadcast(Event.UnitLevelChanged, this);}
        }

        //totalVelecotiy Vector3 property 
        public Vector3 totalVelocity
        {
            get { return m_TotalVelocity; }
            set { m_TotalVelocity = value; }
        }
        //Vector
        public Vector3 velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; }
        }


        //Speed int property
        public float speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        public Moving isMoving
        {
            get { return m_IsMoving; }
            set { m_IsMoving = value; }
        }

        //canMoveWithInput bool property
        public bool canMoveWithInput
        {
            get { return m_CanMoveWithInput; }
            set { m_CanMoveWithInput = value; }
        }
    }
}
