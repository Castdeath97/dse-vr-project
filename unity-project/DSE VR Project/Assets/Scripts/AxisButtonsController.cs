
using UnityEngine;

/// <summary>
/// Author: Ammar Hasan 150454388
/// Class Purpose: this class controlls x, y and z Axis buttons
/// </summary>
public class AxisButtonsController : MonoBehaviour 
{
    // buttons
	public GameObject xButton;
	public GameObject yButton;
	public GameObject zButton;

    // labels
    public GameObject xButtonLabel;
    public GameObject yButtonLabel;
    public GameObject zButtonLabel;

    public Plotter plot;

    // Use this for initialization
    private void Start () 
	{
		// activate buttons and their labels based on whether axis is in use
		if (plot.UseX()) 
		{
			xButton.SetActive (true);
            xButtonLabel.SetActive(true);
        }

        if (plot.UseY()) 
		{
			yButton.SetActive (true);
            yButtonLabel.SetActive(true);
        }

        if (plot.UseZ()) 
		{
			zButton.SetActive (true);
            zButtonLabel.SetActive(true);
        }
    }
}
