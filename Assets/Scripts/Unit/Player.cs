﻿// Unit class used for storing Player and Enemy Data.

using System;
using System.Collections.Generic;
using Library;
using UnityEngine;

using Event = Define.Event;

namespace Unit
{
    // Public unit class that takes in IStats and IAttack
    public class Player : MonoBehaviour, IStats, IControlable
    {
        #region -- VARIABLES --
        // Private int and string memorable variables
        [SerializeField]
        private List<GameObject> m_SkillPrefabs;

        [SerializeField]
        private string m_UnitName;

        [SerializeField]
        private int m_Health;
        [SerializeField]
        private int m_Mana;
        [SerializeField]
        private int m_Defense;

        [SerializeField]
        private int m_Exp;
        [SerializeField]
        private int m_Level;

        [SerializeField]
        private int m_Speed;

        [SerializeField]
        private Moving m_IsMoving;
        [SerializeField]
        private Vector3 m_Velocity;

        [SerializeField]
        private bool m_CanMoveWithInput;

        [SerializeField]
        private Vector3 m_OriginalRotation;
        #endregion

        #region -- PROPERTIES --
        // Health int property
        public int health
        {
            get { return m_Health; }
            set { m_Health = value; }
        }

        // Defense int property
        public int defense
        {
            get { return m_Defense; }
            set { m_Defense = value; }
        }
        // Experience int property
        public int experience
        {
            get { return m_Exp; }
            set { m_Exp = value; }
        }
        // Speed int property
        public int speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }
        // Level int property
        public int level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        // Mana/Currency int property
        public int mana
        {
            get { return m_Mana; }
            set { m_Mana = value; }
        }

        // String name property
        public string unitName
        {
            get { return m_UnitName; }
            set { m_UnitName = value; }
        }

        public Moving isMoving
        {
            get { return m_IsMoving; }
            set { m_IsMoving = value; }
        }

        public Vector3 velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; }
        }

        public bool canMoveWithInput
        {
            get { return m_CanMoveWithInput; }
            set { m_CanMoveWithInput = value; }
        }
        #endregion

        // Unit class that stores Health, Defense, Exp, Level, Speed, Mana, Name
        private void Start()
        {
            if (m_SkillPrefabs == null)
                m_SkillPrefabs = new List<GameObject>();

            m_OriginalRotation = transform.eulerAngles;

            Publisher.self.Subscribe(Event.UseSkill, OnUseSkill);
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Update()
        {
            m_Velocity = Vector3.zero;

            if (m_CanMoveWithInput)
            {
                m_IsMoving.up = Input.GetKey(KeyCode.W);
                m_IsMoving.down = Input.GetKey(KeyCode.S);
                m_IsMoving.left = Input.GetKey(KeyCode.A);
                m_IsMoving.right = Input.GetKey(KeyCode.D);
            }

            if (m_IsMoving.up)
                m_Velocity += Vector3.up * m_Speed;
            if (m_IsMoving.down)
                m_Velocity += Vector3.down * m_Speed;
            if (m_IsMoving.left)
                m_Velocity += Vector3.left * m_Speed;
            if (m_IsMoving.right)
                m_Velocity += Vector3.right * m_Speed;
        }

        public void Move()
        {
            transform.position += m_Velocity * Time.deltaTime;
        }

        public void LateUpdate()
        {
            CalculateRotation();
        }

        private void CalculateRotation()
        {
            if (m_Velocity == Vector3.zero)
                return;

            float rotationX = Mathf.Atan(m_Velocity.x / m_Velocity.y) * (180.0f / Mathf.PI);

            if ((m_Velocity.x < 0.0f && m_Velocity.y < 0.0f) ||
                (m_Velocity.x > 0.0f && m_Velocity.y < 0.0f) ||
                (m_Velocity.x == 0.0f && m_Velocity.y < 0.0f))
                rotationX = rotationX + 180;

            transform.rotation = Quaternion.Euler(
                rotationX,
                m_OriginalRotation.y,
                m_OriginalRotation.z);
        }

        private void OnUseSkill(Event a_Event, params object[] a_Params)
        {
            int skillIndex = (int)a_Params[0];

            Debug.Log("Use Skill " + skillIndex);
            if (m_SkillPrefabs.Count >= skillIndex - 1)
            {
                GameObject newObject = Instantiate(m_SkillPrefabs[skillIndex - 1]);

                newObject.transform.position = transform.position;

                newObject.GetComponent<IParentable>().parent = gameObject;

                newObject.GetComponent<IControlable>().velocity = new Vector3(
                    -Mathf.Cos(transform.rotation.eulerAngles.x * (Mathf.PI / 180)),
                    0,
                    Mathf.Sin(transform.rotation.eulerAngles.x * (Mathf.PI / 180)));
            }
        }
    }
}

