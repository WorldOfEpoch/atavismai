using System;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace Atavism
{

    public class UGUIKeySettingsEntry : MonoBehaviour
    {
        public Text label;
        public TextMeshProUGUI TMPLabel;
        public Button button;
        public Text buttonText;
        public TextMeshProUGUI buttonTMPText;
        public Button altButton;
        public Text altButtonText;
        public TextMeshProUGUI altButtonTMPText;
        public AtavismKeyDefinition def;
    }
}