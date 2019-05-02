using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(RectTransform))]

/// <summary>
/// Author: Ammar Hasan 15045438
/// Class Purpose: Stores fields to be kept by drawn points and 
/// provides methods to manipulate them.
/// </summary>
public class Point : MonoBehaviour
{
    // design values
    private List<string> design;

    // axis values
    public Vector3 axis;

    // getters and setters


    public void SetDesign(List<string> design)
	{
        this.design = design;
	}

	public List<string> GetDesign()
	{
		return design;
	}
		

	public void SetAxis(Vector3 vec)
	{
		this.axis = vec;
	}

	public Vector3 GetAxis()
	{
		return axis;
	}
}