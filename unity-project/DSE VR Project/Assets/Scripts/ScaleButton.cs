using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ammar Hasan 15045438
/// Class Purpose: Controls Scale buttons behaviour
/// </summary>
public class ScaleButton : MonoBehaviour
{
	// stores resize factor and plotter reference
    public float factor;
    public Plotter plotter;

	/// <summary>
	/// Updates the scale of plotter by given rate
	/// </summary>
    public void UpdateScale()
    {
        plotter.RedrawScale(factor);
    }
}
