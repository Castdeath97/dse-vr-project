using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ammar Hasan 150454388
/// Class Purpose: this class is used to invert axis variable 
/// when button is engaged
/// </summary>
public class InvertAxisButton : MonoBehaviour 
{

	public Plotter plotter;

	/// <summary>
	/// Used to invert variable in use
	/// </summary>
	public void InvertVaraibles()
    {
        Vector3Int variableIndex = plotter.GetVariableIndex(); // get variables index in used

        // swap them round based on which axes are in used

        if (plotter.UseX() && plotter.UseY() && plotter.UseZ())
        {
            variableIndex = new Vector3Int(variableIndex.y, variableIndex.z, variableIndex.x);
            plotter.RedrawAxisInvert(variableIndex);
        }

        else if (!plotter.UseX() && plotter.UseY() && plotter.UseZ())
        {
            variableIndex = new Vector3Int(0, variableIndex.z, variableIndex.y);
            plotter.RedrawAxisInvert(variableIndex);
        }

        else if (plotter.UseX() && !plotter.UseY() && plotter.UseZ())
        {
            variableIndex = new Vector3Int(variableIndex.z, 0, variableIndex.x);
            plotter.RedrawAxisInvert(variableIndex);
        }

        else if (plotter.UseX() && plotter.UseY() && !plotter.UseZ())
        {
            variableIndex = new Vector3Int(variableIndex.y, variableIndex.x, 0);
            plotter.RedrawAxisInvert(variableIndex);
        }

    }
}
