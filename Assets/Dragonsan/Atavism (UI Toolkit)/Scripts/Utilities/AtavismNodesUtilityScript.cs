using UnityEngine;
using System.Collections;
using Atavism;
using System.Collections.Generic;

namespace Atavism
{
    public class AtavismNodesUtilityScript : MonoBehaviour
    {
        #region Structs
        [System.Serializable]
        public struct TAtavismPlayer
        {
            [Header("Dir update")]
            public bool dirUpdateDirty;
            public float timestamp;
            public Vector3 direction;
            public Vector3 position;

            [Header("Orient update")]
            public bool orientDirty;
            public Quaternion orientation;

            [Header("Others")]
            public float lastDirSent;
            public float lastOrientSent;
            public float lastUpdateSent;
            public float lastUpdateMovmentSent;

            public AtavismInputController InputController;
            public Quaternion Orientation;
            public Vector3 Direction;
            public GameObject GameObject;
            public GameObject Parent;
        }

        [System.Serializable]
        public struct TAtavismMobNode
        {
            public Vector3 Direction;
            public bool positionalSoundEmitter;
            public long Facing;
            public MovementState MovementState;
            public float FallingSpeed;
            public bool IsFalling;
            public Vector3 LastDirection;
            public bool CanMove;
            public bool CanTurn;
        }

        [System.Serializable]
        public struct TAtavismObjectNode
        {
            public bool createdGo;
            public Vector3 rubChangeToPos;
            public AtavismMobController MobController;
            public bool FollowTerrain;
            public bool Targetable;
            public Quaternion Orientation;
            public Vector3 Position;
            public ObjectNodeType ObjectType;
            public string Name;
            public GameObject Parent;
            public GameObject GameObject;
            public long Oid;
            public Transform controllingTransform;
        }
        #endregion

        [SerializeField] private TAtavismPlayer atavismPlayer;
        [SerializeField] private TAtavismMobNode atavismMobNode;
        [SerializeField] private TAtavismObjectNode atavismObjectNode;

        private AtavismPlayer m_player;
        private AtavismMobController m_mobController;
        private AtavismMobNode m_mobNode;
        private AtavismObjectNode m_objectNode;
        private AtavismNode m_node;
        public AtavismObjectNode ObjectNode => m_objectNode;

        protected void ObjectNodeReady()
        {
            InitData();
        }

        private void Start()
        {
            InitData();
        }

        private void Update()
        {
            if (m_player != null)
                atavismPlayer = PopulateAtavismPlayer(m_player, atavismPlayer);
            if (m_mobNode != null)
                atavismMobNode = PopulateAtavismMobNode(m_mobNode, atavismMobNode);
            if (m_node != null)
                atavismObjectNode = PopulateAtavismObjectNode(m_objectNode, atavismObjectNode);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitData()
        {
            m_node = GetComponent<AtavismNode>();
            if (m_node != null)
            {
                m_objectNode = (AtavismObjectNode)AtavismClient.Instance.WorldManager.GetObjectNode(m_node.Oid);
                m_mobNode = (AtavismMobNode)AtavismClient.Instance.WorldManager.GetObjectNode(m_node.Oid);
                if (m_mobNode != null)
                    m_mobController = m_mobNode.MobController;
                if (m_mobController != null && m_mobController.isPlayer)
                    m_player = (AtavismPlayer)AtavismClient.Instance.WorldManager.GetObjectNode(m_node.Oid);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="atavismPlayer"></param>
        /// <returns></returns>
        public static TAtavismPlayer PopulateAtavismPlayer(AtavismPlayer serializedController, TAtavismPlayer node)
        {
            if (serializedController == null)
                return node;

            node.dirUpdateDirty = serializedController.dirUpdate.dirty;
            node.timestamp = serializedController.dirUpdate.timestamp;
            node.direction = serializedController.dirUpdate.direction;
            node.position = serializedController.dirUpdate.position;
            node.orientDirty = serializedController.orientUpdate.dirty;
            node.orientation = serializedController.orientUpdate.orientation;

            node.Direction = serializedController.Direction;
            node.GameObject = serializedController.GameObject;
            node.InputController = serializedController.InputController;
            node.lastDirSent = serializedController.lastDirSent;
            node.lastOrientSent = serializedController.lastOrientSent;
            node.lastUpdateMovmentSent = serializedController.lastUpdateMovmentSent;
            node.lastUpdateSent = serializedController.lastUpdateSent;
            node.Orientation = serializedController.Orientation;
            node.Parent = serializedController.Parent;

            return node;
        }

        public static TAtavismMobNode PopulateAtavismMobNode(AtavismMobNode node, TAtavismMobNode serializedNode)
        {
            if (node == null)
                return serializedNode;

            serializedNode.Direction = node.Direction;
            serializedNode.positionalSoundEmitter = node.PositionalSoundEmitter;
            serializedNode.Facing = node.Facing;
            serializedNode.MovementState = node.MovementState;
            serializedNode.FallingSpeed = node.FallingSpeed;
            serializedNode.IsFalling = node.IsFalling;
            serializedNode.LastDirection = node.LastDirection;

            serializedNode.CanMove = node.CanMove();
            serializedNode.CanTurn = node.CanTurn();

            return serializedNode;
        }

        public static TAtavismObjectNode PopulateAtavismObjectNode(AtavismObjectNode node, TAtavismObjectNode serializedNode)
        {
            if (node == null)
                return serializedNode;

            serializedNode.createdGo = node.createdGo;
            serializedNode.rubChangeToPos = node.rubChangeToPos;
            serializedNode.MobController = node.MobController;

            serializedNode.FollowTerrain = node.FollowTerrain;
            serializedNode.Targetable = node.Targetable;
            serializedNode.Orientation = node.Orientation;
            serializedNode.Position = node.Position;
            serializedNode.ObjectType = node.ObjectType;
            serializedNode.Name = node.Name;
            serializedNode.Parent = node.Parent;
            serializedNode.GameObject = node.GameObject;
            serializedNode.Oid = node.Oid;
            serializedNode.controllingTransform = node.GetControllingTransform();

            return serializedNode;
        }
    }
}