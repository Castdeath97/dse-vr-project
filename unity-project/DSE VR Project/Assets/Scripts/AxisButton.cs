using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: Ammar Hasan 150454388
/// Class Purpose: this class is used to change a variable 
/// when button is engaged
/// </summary>
public class AxisButton : MonoBehaviour 
{
	public enum Axis 
	{
		X,
		Y,
		Z
	}

	public Plotter plot;
	public Axis axisName;

	/// <summary>
	/// Used to update variable in use
	/// </summary>
	public void UpdateVariable()
	{
        List<string> variables = plot.GetVariables(); // get the list of variables
        List<int> variableIndexesInUse = new List<int>();

        // check variable indexes used by axis by counting
        int varsInUse = 0;

        if (plot.UseX())
        {
            varsInUse++;
            variableIndexesInUse.Add(plot.GetVariableIndex().x);
        }

        if (plot.UseY())
        {
            varsInUse++;
            variableIndexesInUse.Add(plot.GetVariableIndex().y);
        }

        if (plot.UseZ())
        {
            varsInUse++;
            variableIndexesInUse.Add(plot.GetVariableIndex().z);
        }

        // if there is other variables to use allow switch
        if (varsInUse < variables.Count)
        {

            // check which axis we are dealing with
            Vector3Int newVariableIndex = new Vector3Int();
            int i;
            switch (axisName)
            {
                // update the variable in use for that axis, making sure it is not used by something else
                case Axis.X:
                    newVariableIndex = plot.GetVariableIndex();
                    i = newVariableIndex.x;
                    
                    while (variableIndexesInUse.Contains(i)) // while i is in use
                    {
                        i = (i + 1) % (variables.Count); // look for a new index in range
                    }

                    // once you find a new index in range, set it and redraw
                    newVariableIndex.x = i;
                    plot.RedrawAxis(newVariableIndex);
                     
                    break;

                case Axis.Y:

                    newVariableIndex = plot.GetVariableIndex();
                    i = newVariableIndex.y;

                    while (variableIndexesInUse.Contains(i)) // while i is in use
                    {
                        i = (i + 1) % (variables.Count); // look for a new index in range
                    }

                    // once you find a new index in range, set it and redraw
                    newVariableIndex.y = i;
                    plot.RedrawAxis(newVariableIndex);

                    break;

                case Axis.Z:

                    newVariableIndex = plot.GetVariableIndex();
                    i = newVariableIndex.z;

                    while (variableIndexesInUse.Contains(i)) // while i is in use
                    {
                        i = (i + 1) % (variables.Count); // look for a new index in range
                    }

                    // once you find a new index in range, set it and redraw
                    newVariableIndex.z = i;
                    plot.RedrawAxis(newVariableIndex);

                    break;
            }
		}

	}
}
