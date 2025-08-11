using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DNASlider : MonoBehaviour
{
	public TextMeshProUGUI label;
	public Slider slider;
	public string dna;
	public DNASliderPanel panel;
	
	public void ValueChanged(float val)
	{
		panel.AdjustDNA(dna,val);
	}
}
