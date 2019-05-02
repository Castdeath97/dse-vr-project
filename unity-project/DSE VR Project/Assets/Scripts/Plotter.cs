using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Author: Ammar Hasan 150454388 2018
/// Class Purpose: This class reads a CSV file to draw points
/// in a plot and JSON file to check which variables are in the pareto
/// </summary>
public class Plotter : MonoBehaviour 
{
	enum Axis 
	{
		X,
		Y,
		Z
	}

	private const char DEL = ','; // CSV file delimiter for reading
    private const char JSON_DEL = '\"'; // Used to remove qoutes from JSON
    private const int RANK_OFFSET = 2; // Offset between rank and variables

    // titles for JSON reading
    private const string RANKING_TITLE = "ranking";
    private const string PARETO_TITLE = "pareto";
    private const string PARETO_LABEL_TEXT = "Pareto: ";

    // title for other labels
    private const string X_LABEL_TEXT = "X: ";
    private const string Y_LABEL_TEXT = "Y: ";
    private const string Z_LABEL_TEXT = "Z: ";
    private const string LABEL_SEP = ": ";


    private int paramtersOffset;

	private Vector3 origin;

	// axes

	public GameObject xAxis;
	public GameObject nXAxis;
	public GameObject yAxis;
	public GameObject nYAxis;
	public GameObject zAxis;
	public GameObject nZAxis;

	// axis properties

	public Vector3 proportionOfAxis;

    public float maxScale;
    public float minScale;

	public GameObject pointPrefab; // prefab for plot points

    private Vector3 maxAxis;
    private Vector3 minAxis;

    private List<GameObject> points = new System.Collections.Generic.List<GameObject>();
	private List<string> variables = new System.Collections.Generic.List<string>();
	private List<string> parameters = new System.Collections.Generic.List<string>();

	// index that refers to select variables (defaults to 0,1,2)
	private Vector3Int variableIndex = new Vector3Int (0, 1, 2);

    // pareto status, used to deal with INTO-CPS results limitations
    private bool paretoStatus = true;

    // scale and position record for reseting

    private Vector3 xAxisOrgScale;
    private Vector3 nXAxisOrgScale;
    private Vector3 yAxisOrgScale;
    private Vector3 nYAxisOrgScale;
    private Vector3 zAxisOrgScale;
    private Vector3 nZAxisOrgScale;

    private Vector3 xAxisOrgPos;
    private Vector3 nXAxisOrgPos;
    private Vector3 yAxisOrgPos;
    private Vector3 nYAxisOrgPos;
    private Vector3 zAxisOrgPos;
    private Vector3 nZAxisOrgPos;

    // list of variables in pareto
    private List<string> paretoList = new List<string>();

    // results path
    public string dsePath;

    // path to json with pareto information
    public string jsonPath;

    // axes to use
    public bool useX;
	public bool useY;
	public bool useZ;

	// axis labels

	public GameObject xLabel;
	public GameObject yLabel;
	public GameObject zLabel;

	// Point materials for pareto 

	public Material rank1Mat;
	public Material rank2Mat;
	public Material rank3Mat;
    public Material noRankMat;

    // Laser pointers
    public LaserPointer leftPointer;
    public LaserPointer rightPointer;

    // label used to show which variables are used in pareto
    public GameObject paretoLabel;

    // getters and setters

	public Vector3Int GetVariableIndex()
	{
		return variableIndex;
	}

	public List<string> GetVariables()
	{
		return variables;
	}

	public List<string> GetParameters()
	{
		return parameters;
	}

	public bool UseX()
	{
		return useX;
	}

	public bool UseY()
	{
		return useY;
	}

	public bool UseZ()
	{
		return useZ;
	}

    /// <summary>
    /// Redraws the axis with a new selection of variable indexes for every
    /// axis
    /// </summary>
    /// <param name="newVariableIndex">The new set of variable indexes are a vector 3</param>
    public void RedrawAxis(Vector3Int newVariableIndex)
    {
        // check index validity 
        if(((newVariableIndex.x + 1) > variables.Count || newVariableIndex.x < 0) ||
           ((newVariableIndex.y + 1) > variables.Count || newVariableIndex.y < 0) ||
           ((newVariableIndex.z + 1) > variables.Count || newVariableIndex.z < 0))
        {
            Debug.Log("invalid index");
        }

        else
        {
            variableIndex = newVariableIndex;
            // destory points to start again
            DestroyPoints();

            // disable all axis again and reset transforms to start again

            ResetAxisTransforms();
            xAxis.SetActive(false);
            nXAxis.SetActive(false);

            yAxis.SetActive(false);
            nYAxis.SetActive(false);

            zAxis.SetActive(false);
            nZAxis.SetActive(false);

            // change pareto status as only the initial setup (in pareto list) works due to tool limitations
            List<string> variablesInUse = new List<string>();
            if (useX) variablesInUse.Add(variables[variableIndex.x]);
            if (useY) variablesInUse.Add(variables[variableIndex.y]);
            if (useZ) variablesInUse.Add(variables[variableIndex.z]);

            if (!paretoList.Except(variablesInUse).Any())
            {
                paretoStatus = true;
            }
            
            else
            {
                paretoStatus = false;

            }

            UpdateLabels();
            UpdateMaxMins();
            CheckAxis(xAxis, yAxis, zAxis, nXAxis, nYAxis, nZAxis); // check which axes need to remain
            ReadCSVFile(); // read CSV file

        }   
    }

    /// <summary>
    /// Redraws Axes with a new set of variables indexes, but for inversion
    /// </summary>
    /// <param name="newVariableIndex">The new set of variable indexes are a vector 3</param>
    public void RedrawAxisInvert(Vector3Int newVariableIndex)
    {
        // check index validity 
        if (((newVariableIndex.x + 1) > variables.Count || newVariableIndex.x < 0) ||
           ((newVariableIndex.y + 1) > variables.Count || newVariableIndex.y < 0) ||
           ((newVariableIndex.z + 1) > variables.Count || newVariableIndex.z < 0))
        {
            Debug.Log("invalid index");
        }

        else
        {
            variableIndex = newVariableIndex;
            // destory points to start again
            DestroyPoints();

            // disable all axis and reset their transforms again to start again

            ResetAxisTransforms();
            xAxis.SetActive(false);
            nXAxis.SetActive(false);

            yAxis.SetActive(false);
            nYAxis.SetActive(false);

            zAxis.SetActive(false);
            nZAxis.SetActive(false);

            UpdateLabels();
            UpdateMaxMins();
            CheckAxis(xAxis, yAxis, zAxis, nXAxis, nYAxis, nZAxis); // check which axes need to remain
            ReadCSVFile(); // read CSV file

        }
    }

    /// <summary>
    /// Redraws the axes with a new scale
    /// </summary>
    /// <param name="newScale">new scale to redraw axes and graph with as a float</param>
    public void RedrawScale(float newScale)
    {
		// check if any new sizes violate limits
		if (newScale * proportionOfAxis.x > maxScale ||
		    newScale * proportionOfAxis.y > maxScale ||
		    newScale * proportionOfAxis.z > maxScale ||

		    newScale * proportionOfAxis.x < minScale ||
		    newScale * proportionOfAxis.y < minScale ||
		    newScale * proportionOfAxis.z < minScale) 
		{
			return;
		}

        DestroyPoints();

		// scale the axis by the newscale
		if (useX) 
		{
			proportionOfAxis.x *= newScale;

			if (maxAxis.x > 0.0f) 
			{ 
				ScaleAxis(xAxis, Axis.X, newScale);
			}

			if (minAxis.x < 0.0f) 
			{
				ScaleAxis(nXAxis, Axis.X, newScale);
			}
		}

		if (useY) 
		{
			proportionOfAxis.y *= newScale;

			if (maxAxis.y > 0.0f) 
			{ 
				ScaleAxis(yAxis, Axis.Y, newScale);
			}

			if (minAxis.y < 0.0f) 
			{
				ScaleAxis(nYAxis, Axis.Y, newScale);
			}
		}

		if (useZ) 
		{
			proportionOfAxis.z *= newScale;

			if (maxAxis.z > 0.0f) 
			{ 
				ScaleAxis(zAxis, Axis.Z, newScale);
			}

			if (minAxis.z < 0.0f) 
			{
				ScaleAxis(nZAxis, Axis.Z, newScale);
			}
		}

        ReadCSVFile();
    }

	// initialization
	private void Start () 
	{
		// get origin from z axis game object
		Transform zAxisTrans = zAxis.transform;

		origin = zAxisTrans.position;
		origin.z -= (zAxisTrans.lossyScale.z / 2);


        // Record orginal scales and positions for later reseting

        xAxisOrgScale = xAxis.transform.localScale;
        nXAxisOrgScale = nXAxis.transform.localScale;

        yAxisOrgScale = yAxis.transform.localScale;
        nYAxisOrgScale = nYAxis.transform.localScale;

        zAxisOrgScale = zAxis.transform.localScale;
        nZAxisOrgScale = nZAxis.transform.localScale;


        xAxisOrgPos = xAxis.transform.position;
        nXAxisOrgPos = nXAxis.transform.position;

        yAxisOrgPos = yAxis.transform.position;
        nYAxisOrgPos = nYAxis.transform.position;

        zAxisOrgPos = zAxis.transform.position;
        nZAxisOrgPos = nZAxis.transform.position;

        // if two axes are not used, enable one and alert user
        if(!useX && !useY || !useZ && !useY)
        {
            Debug.Log("Can't disable 2 axes, enabling a axis");

            if(!useX)
            {
                useX = true;
            }

            if (!useY)
            {
                useY = true;
            }
        }

        LoadJson();
        ReadHeaders();
        UpdateMaxMins();
        CheckAxis(xAxis, yAxis, zAxis, nXAxis, nYAxis, nZAxis); // check which axes need to remain
        ReadCSVFile(); // read CSV file
    }

    /// <summary>
    /// Used to load the JSON file with the pareto ranking information
    /// </summary>
    private void LoadJson()
    {
        if (System.IO.File.Exists(jsonPath)) // check if path exists
        {
            // get json text
            string json = File.ReadAllText(jsonPath);

            // Read json using JSON.NET
            JToken jsonToken = JToken.Parse(json);

            // get JSON tokens into a list
            IList<JToken> paretoVariables = jsonToken[RANKING_TITLE][PARETO_TITLE].Children().ToList();
            string paretoLabelText = PARETO_LABEL_TEXT;
            foreach (JToken variable in paretoVariables)
            {
                // split to get raw name (no qoutes or values)
                string variableString = variable.ToString();
                string[] variableSplit = variableString.Split(JSON_DEL);
                paretoList.Add(variableSplit[1]);
                paretoLabelText = paretoLabelText + variableSplit[1] + " ";
            }

            // set label to used pareto variables
            paretoLabel.GetComponent<UnityEngine.UI.Text>().text = paretoLabelText;
        }

        else
        {
            Debug.Log("JSON File not found");
        }
    }

    /// <summary>
    /// Removes all points for redrawing
    /// </summary>
    private void DestroyPoints()
    {
        foreach (GameObject point in points)
        {
            Destroy(point);
        }
        points.Clear();
    }

    /// <summary>
    /// Reads all headers of the CSV file
    /// </summary>
	private void ReadHeaders()
	{
		if (System.IO.File.Exists (dsePath)) // check if path exists
		{ 
			// read file using StreamReader
			using (StreamReader reader = new StreamReader (dsePath)) 
			{
				string headingRow = reader.ReadLine (); 
				string[] headings = headingRow.Split (DEL);

				// ignore 0 as its the rank
				int i = 2;

				while (headings.Length > i && headings [i] != "") 
				{
					variables.Add (headings[i]);
					i++;				
				}

				// check number of axis in use
				int AxisNum = 0;

				if (useX) AxisNum++;
				if (useY) AxisNum++;
				if (useZ) AxisNum++;

				if (variables.Capacity < AxisNum) 
				{
					Debug.Log ("Not enough variables");
				}

                else
                {
                    UpdateLabels();
                }

                i++; // skip empty string
                paramtersOffset = i; // keep ofset to parameters

                while (headings.Length > i) 
				{
					parameters.Add (headings [i]);
					i++;
				}
			}
		}
			
		else 
		{
			Debug.Log ("CSV File not found");
		}
	}

    /// <summary>
    /// Updates variables minimums and maximums based on values in CSV
    /// </summary>
    private void UpdateMaxMins()
    {
        if (System.IO.File.Exists(dsePath)) // check if path exists
        {
            // read file using StreamReader
            using (StreamReader reader = new StreamReader(dsePath))
            {
                reader.ReadLine(); // skip headers to get to min/max vals

                /// get min and max rows and split
                string maximumRow = reader.ReadLine();
                string[] maximums = maximumRow.Split(DEL);

                string minimumRow = reader.ReadLine();
                string[] minimums = minimumRow.Split(DEL);

                // find mins and maxes for axes in use

                if (useX) maxAxis.x = float.Parse(maximums[variableIndex.x + RANK_OFFSET]);
                if (useY) maxAxis.y = float.Parse(maximums[variableIndex.y + RANK_OFFSET]);
                if (useZ) maxAxis.z = float.Parse(maximums[variableIndex.z + RANK_OFFSET]);

                if (useX) minAxis.x = float.Parse(minimums[variableIndex.x + RANK_OFFSET]);
                if (useY) minAxis.y = float.Parse(minimums[variableIndex.y + RANK_OFFSET]);
                if (useZ) minAxis.z = float.Parse(minimums[variableIndex.z + RANK_OFFSET]);
            }
        }

        else
        {
            Debug.Log("CSV File not found");
        }
    }

    /// <summary>
    /// Updates the axes labels
    /// </summary>
    private void UpdateLabels()
    {
        // set labels based on used axes for selection and axes
        if (useX)
        {
            xLabel.GetComponent<UnityEngine.UI.Text>().text =
                X_LABEL_TEXT + variables[variableIndex.x];


            leftPointer.SetXLabelString(variables[variableIndex.x] + LABEL_SEP);
            rightPointer.SetXLabelString(variables[variableIndex.x] + LABEL_SEP);
        }

        if (useY)
        {
            yLabel.GetComponent<UnityEngine.UI.Text>().text =
                Y_LABEL_TEXT + variables[variableIndex.y];

            leftPointer.SetYLabelString(variables[variableIndex.y] + LABEL_SEP);
            rightPointer.SetYLabelString(variables[variableIndex.y] + LABEL_SEP);
        }

        if (useZ)
        {
            zLabel.GetComponent<UnityEngine.UI.Text>().text =
                 Z_LABEL_TEXT + variables[variableIndex.z];

            leftPointer.SetZLabelString(variables[variableIndex.z] + LABEL_SEP);
            rightPointer.SetZLabelString(variables[variableIndex.z] + LABEL_SEP);
        }
    }

    /// <summary>
    /// Reset the transforms used axes back to original value
    /// </summary>
    private void ResetAxisTransforms()
    {
        if(useX)
        {
            xAxis.transform.localScale = xAxisOrgScale;
            xAxis.transform.position = xAxisOrgPos;

            nXAxis.transform.localScale = nXAxisOrgScale;
            nXAxis.transform.position = nXAxisOrgPos;

        }

        if (useY)
        {
            yAxis.transform.localScale = yAxisOrgScale;
            yAxis.transform.position = yAxisOrgPos;

            nYAxis.transform.localScale = nYAxisOrgScale;
            nYAxis.transform.position = nYAxisOrgPos;
        }

        if (useZ)
        {
            zAxis.transform.localScale = zAxisOrgScale;
            zAxis.transform.position = zAxisOrgPos;

            nZAxis.transform.localScale = nZAxisOrgScale;
            nZAxis.transform.position = nZAxisOrgPos;
        }
    }




    /// <summary>
    /// Used to check which axes are needed to be setup and activates them
    /// </summary>
    /// <param name="x">X axis Game Object</param>
    /// <param name="y">Y axis Game Object</param>
    /// <param name="z">Z axis Game Object</param>
    /// <param name="negX">-X axis Game Object</param>
    /// <param name="negY">-Y axis Game Object</param>
    /// <param name="negZ">-Z axis Game Object</param>
    private void CheckAxis(GameObject x, GameObject y, GameObject z
				   , GameObject negX, GameObject negY, GameObject negZ)
	{
       
		// check which axes need to be enabled and setup
		if (useX) 
		{
			xLabel.SetActive (true);
			if(minAxis.x < 0.0f) negX.SetActive(true);
			if(maxAxis.x > 0.0f) x.SetActive(true);

            SetUpAxisProp (x, negX, minAxis.x, maxAxis.x, Axis.X);
        }

		if (useY) 
		{
			yLabel.SetActive (true);
			if(minAxis.y < 0.0f) negY.SetActive(true);
			if(maxAxis.y > 0.0f) y.SetActive(true);

            SetUpAxisProp (y, negY, minAxis.y, maxAxis.y, Axis.Y);		
		}

		if (useZ) 
		{
			zLabel.SetActive (true);
			if(minAxis.z < 0.0f) negZ.SetActive(true);
			if(maxAxis.z > 0.0f) z.SetActive(true);

            SetUpAxisProp (z, negZ, minAxis.z, maxAxis.y, Axis.Z);
		}
    }


    /// <summary>
    /// Activates axes and scales them to proportion
    /// </summary>
    /// <param name="axis">Positive Game Object Axis to set up</param>
    /// <param name="negAxis">Negative Axis Game Object to set up</param>
    /// <param name="min">Axis minimum as a float</param>
    /// <param name="max">Axis maximum as a float</param>
    /// <param name="axisName">Axis type (enum) </param>
    private void SetUpAxisProp(GameObject axis, GameObject negAxis, 
		float min, float max, Axis axisName)
	{
	    if (max > 0.0f) 
		{ // if max is higher than 0, then we have a positive axis
			
			// fix to proportion based on axis name
			if (axisName == Axis.X) ScaleAxis(axis, axisName, proportionOfAxis.x);
			else if (axisName == Axis.Y) ScaleAxis(axis, axisName, proportionOfAxis.y);
			else ScaleAxis(axis, axisName, proportionOfAxis.z);
		}

		if (min < 0.0f) 
		{ // if max is less than 0, then we have a negative axis
			
			// fix to proportion
			if (axisName == Axis.X) ScaleAxis(negAxis, axisName, proportionOfAxis.x);
			else if (axisName == Axis.Y) ScaleAxis(negAxis, axisName, proportionOfAxis.y);
			else ScaleAxis(negAxis, axisName, proportionOfAxis.z);

			// Use to resize axes based on relative size

			float negToPosRatio = Mathf.Abs (min) / Mathf.Abs (max);

			if (negToPosRatio > 1) // if negative axis is larger
			{
				// shrink positive axis
				ScaleAxis(axis, axisName, (1 / negToPosRatio));
			} 

			else if (negToPosRatio < 1) // if positive axis is larger
			{
				// shrink negative axis 
				ScaleAxis(negAxis, axisName, negToPosRatio);		
			}
		}

        // else do nothing, 1 to 1 ratio
    }


    /// <summary>
    /// Scales axis GameObject by given factor in its component(x for x axis, y for y axis, ...)
    /// </summary>
    /// <param name="axis"> Game Object Axis to scale</param>
    /// <param name="axisName">Axis type (enum) </param>
    /// <param name="factor">The factor to scale it by as a float</param>
    private void ScaleAxis(GameObject axis, Axis axisName, float factor) 
	{

        if (factor == 1.0) return; // nothing to do

		if (axisName == Axis.X)
		{
			// record old position (x) and scale (x) for later
			float oldScale = axis.transform.localScale.x;
			float oldPos = axis.transform.position.x;
           
			// scale over x by given factor
			axis.transform.localScale = 
				Vector3.Scale (new Vector3 (factor, 1, 1), axis.transform.localScale);

			// move back to origin based on difference in scale
			float scaleDiff = axis.transform.localScale.x - oldScale;

			if (oldPos < origin.x) 
			{	// if the axis is positive move backwards
				axis.transform.Translate (new Vector3 ((-scaleDiff / 2), 0, 0));
			} 

			else 
			{	// if the axis is negative move forward
				axis.transform.Translate (new Vector3 ((scaleDiff / 2), 0, 0));
			} 
		} 

		else if (axisName == Axis.Y) 
		{
			// record old position (y) and scale (y) for later
			float oldScale = axis.transform.localScale.y;
			float oldPos = axis.transform.position.y;

			// scale over y by given factor
			axis.transform.localScale = 
				Vector3.Scale (new Vector3 (1, factor, 1), axis.transform.localScale);

			// move back to origin based on difference in scale
			float scaleDiff = axis.transform.localScale.y - oldScale;

			if (oldPos < origin.y) 
			{	// if the axis is positive move backwards
				axis.transform.Translate (new Vector3 (0, (-scaleDiff / 2), 0));
			} 

			else 
			{	// if the axis is negative move forward
				axis.transform.Translate (new Vector3 (0, (scaleDiff / 2), 0));
			} 
		} 

		else 
		{	// if z
			
			// record old position (z) and scale (z) for later
			float oldScale = axis.transform.localScale.z;
			float oldPos = axis.transform.position.z;

			// scale over z by given factor
			axis.transform.localScale = 
				Vector3.Scale (new Vector3 (1, 1, factor), axis.transform.localScale);

			// move back to origin based on difference in scale
			float scaleDiff = axis.transform.localScale.z - oldScale;

			if (oldPos < origin.z) 
			{	// if the axis is positive move backwards
				axis.transform.Translate (new Vector3 (0, 0, (-scaleDiff / 2)));
			} 

			else 
			{	// if the axis is negative move forward
				axis.transform.Translate (new Vector3 (0, 0, (scaleDiff / 2)));
			} 
		}
    }

    /// <summary>
    /// Used to read CSV value and instantiate points for plot
    /// </summary>
	private void ReadCSVFile ()
	{

		if (System.IO.File.Exists (dsePath)) // check if path exists
		{ 
			// read file using StreamReader
			using (StreamReader reader = new StreamReader (dsePath)) 
			{

                // Skip axis headings, min and max rows
                reader.ReadLine();
                reader.ReadLine();
                reader.ReadLine();

                // get the first row
                string row = reader.ReadLine ();

                // Stop if there are no more rows
                while (row != null)
                {
                    string[] cells = row.Split(DEL); // seperate row into cells

                    // get the x, y and z coords and rank from csv
                    int rank = int.Parse(cells[0]);

                    // take enabled axis into account when getting values,
                    // use findCoord to get proportion correct value
                    float x, y, z;
                    float xCSV, yCSV, zCSV;
                    if (useX)
                    {
                        xCSV = float.Parse(cells[variableIndex.x + RANK_OFFSET]);
                        x = FindCoord(minAxis.x, maxAxis.x, xCSV, origin.x, proportionOfAxis.x);

                    }

                    else
                    {
                        xCSV = 0;
                        x = origin.x;
                    }

                    if(useY)
                    {
                        yCSV = float.Parse(cells[variableIndex.y + RANK_OFFSET]);
                        y = FindCoord(minAxis.y, maxAxis.y, yCSV, origin.y, proportionOfAxis.y);
                    }

                    else
                    {
                        yCSV = 0;
                        y = origin.y;
                    }

                    if (useZ)
                    {
                        zCSV = float.Parse(cells[variableIndex.z + RANK_OFFSET]);
                        z = FindCoord(minAxis.z, maxAxis.z, zCSV, origin.z, proportionOfAxis.z);
                    }

                    else
                    {
                        zCSV = 0;
                        z = origin.z;
                    }


                    // create point and add to point list
                    GameObject point = Instantiate(pointPrefab, origin, Quaternion.identity);

                    // set axis to move point to position

                    point.transform.position = new Vector3(x, y, z); 


                    // get renderer to change materials based on rank
                    Renderer rend = point.GetComponent<Renderer>();


                    if (rend != null)
                    {
                        if (paretoStatus)
                        { 
                            if(rank == 1)
                            {
                                rend.material = rank1Mat;
                            }

                            else if (rank == 2)
                            {
                                rend.material = rank2Mat;
                            }

                            else if (rank > 2)
                            {
                                rend.material = rank3Mat;
                            }

                            else
                            {
                                rend.material = noRankMat; // default to no rank if value is otherwise
                            }

                        }

                        else
                        {
                            rend.material = noRankMat; // no pareto
                        }
                    }

                    else
                    {
                        Debug.Log("No renderer found!");
                    }



                    // add fields to point for selection
                    Point pointController = point.GetComponent<Point>();
                    pointController.SetAxis(new Vector3(xCSV, yCSV, zCSV));

                    // Add design value
                    List<string> designs = new List<string>();
                    int i = 0;
                    foreach(string param in parameters)
                    {
                        designs.Add(param + ": "
                            + float.Parse(cells[i + paramtersOffset]));
                        i++;
                    }

                    pointController.SetDesign(designs);
                    points.Add(point);

                    row = reader.ReadLine(); // update row
                }
            }
		} 

		else 
		{
			Debug.Log ("CSV File not found");
		}
    }

    /// <summary>
    /// Used to convert CSV coords to proportion correct values in plot
    /// </summary>
    /// <param name="min">Minimum value for its axis as a float</param>
    /// <param name="max">Maxiumum value for its axis as a float</param>
    /// <param name="csvCoord">Raw coordinate from the CSV</param>
    /// <param name="originVal">Original coordinate at that axis as a float</param>
    /// <param name="prop">Proportion to scale to s</param>
    /// <returns></returns>
    private float FindCoord(float min, float max, float csvCoord, float originVal, float prop)
	{
		if (csvCoord > 0.0f) 
		{
            return ((csvCoord / max) * prop) + originVal;
		} 
			
		else if (csvCoord < 0.0f) 
		{
			return originVal - ((Mathf.Abs(csvCoord) / Mathf.Abs(min)) * prop);
		} 

		else 
		{
			return originVal;
		}
	}
}
