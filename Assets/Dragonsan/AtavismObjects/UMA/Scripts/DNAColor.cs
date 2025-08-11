using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DNAColor : MonoBehaviour {

	public string sharedColor;
	public DNASliderPanel panel;
	
	public void ColorClicked()
	{
		Color buttonColor = gameObject.GetComponent<Image>().color;
		panel.AdjustColor(sharedColor,buttonColor);
	}
}
