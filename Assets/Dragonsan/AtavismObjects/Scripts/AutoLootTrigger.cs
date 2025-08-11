using System;
using UnityEngine;

namespace Atavism
{
    public class AutoLootTrigger : MonoBehaviour
    {
        private AtavismNode node;

        private void Start()
        {
            node = GetComponent<AtavismNode>();
        }


        void OnTriggerEnter(Collider other)
        {
            if (node != null)
            {
                if (node.Oid == ClientAPI.GetPlayerOid())
                {
                    GroundItemDisplay gid = other.gameObject.GetComponent<GroundItemDisplay>();
                    if (gid != null)
                    {
                        ItemPrefabData ipd = AtavismPrefabManager.Instance.GetItemTemplateByID(gid.TemplateId);
                        if (ipd.quality >= AtavismSettings.Instance.GetGeneralSettings().autoLootGroundMinQuality)
                        {
                            gid.Loot();
                        }
                    }
                }
            }
        }
    }
}