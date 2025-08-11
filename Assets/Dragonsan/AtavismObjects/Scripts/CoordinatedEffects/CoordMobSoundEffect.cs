using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Atavism
{

    public class CoordMobSoundEffect : CoordinatedEffect
    {

        public MobSoundEvent soundEvent;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (activationTime != 0 && Time.time > activationTime)
            {
                Run();
            }
        }

        public override void Execute(Dictionary<string, object> props)
        {
            if (!enabled)
                return;
            base.props = props;
            AtavismLogger.LogDebugMessage("Executing CoordAnimationEffect with num props: " + props.Count);
            if (props.ContainsKey("castingMod"))
            {
                castingMod = 1f/((int) props["castingMod"] / 1000f);
            }
            if (activationDelay == 0)
            {
                Run();
            }
            else
            {
                activationTime = Time.time + (useCastingModToActivationDelayMod ? activationDelay * castingMod : activationDelay);
            }
        }

        public override void CancelCoordEffect()
        {
          DestroyImmediate(gameObject);
        }

        public void Run()
        {
            AtavismObjectNode node;
            if (target == CoordinatedEffectTarget.Caster)
            {
                node = ClientAPI.WorldManager.GetObjectNode((OID)props["sourceOID"]);
            }
            else
            {
                node = ClientAPI.WorldManager.GetObjectNode((OID)props["targetOID"]);
            }

            if (node != null && node.GameObject != null && node.GameObject.GetComponent<MobSoundSet>() != null)
                node.GameObject.GetComponent<MobSoundSet>().PlaySoundEvent(soundEvent);

            // Now destroy this object
            if (destroyWhenFinished)
                Destroy(gameObject, duration);
        }
    }
}