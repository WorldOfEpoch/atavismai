using UnityEngine;
using System.Collections;

namespace Atavism
{

    public enum TargetType
    {
        Ground,
        All,
        AoE,
        Group,
        Location,
        Single_Target,
    }
    public enum TargetSubType
    {
        Enemy,
        Friendly,
        Friendly_Or_Enemy,
        Self,
        All,
    }
    /// <summary>
    /// Abstract class used by classes such as Ability and Item that are activatable by the player.
    /// </summary>
    public abstract class Activatable //: MonoBehaviour
    {

        public string name;
       // public Sprite icon;
       float cooldownDuration;
        float cooldownStart;
        public string tooltip;

        // Use this for initialization
        void Start()
        {

        }

        public abstract bool Activate();

       // public Sprite Icon;

        public abstract void DrawTooltip(float x, float y);

        public virtual Cooldown GetLongestActiveCooldown()
        {
            return null;
        }

        public void StartCooldown(float duration)
        {
            cooldownStart = Time.time;
            cooldownDuration = duration;
        }

        public bool IsOnCooldown()
        {
            if (cooldownStart == -1)
                return false;
            else
                return true;
        }

        public float CooldownTimeLeft()
        {
            if (cooldownStart == -1)
                return 0;

            return Time.time - (cooldownStart + cooldownDuration);
        }
    }
}