using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* #TODO: make optimal with useX, useY, useZ 
   #TODO: remove magic design */

/// <summary>
/// Author: Ammar Hasan 15045438
/// Class purpose: Laser pointer controller
/// </summary>
public class LaserPointer : MonoBehaviour
{
    // what it selects with
	private SteamVR_LaserPointer laser;
	private SteamVR_TrackedController controller;

    // things it can select
    private Point selectedPoint = null;
    private ScaleButton scaleButton = null;
    private AxisButton axisButton = null;
    private InvertAxisButton invertAxisButton = null;

    private string xLabelString;
    private string yLabelString;
    private string zLabelString;

    private bool inPointCollision;

	// UI elements effected by laser selection
	public GameObject xValueLabel;
	public GameObject yValueLabel;
	public GameObject zValueLabel;
	public GameObject designLabel;

    // plotter
    public Plotter plotter;

    public void SetXLabelString(string label)
    {
        xLabelString = label;
    }
    public void SetYLabelString(string label)
    {
        yLabelString = label;
    }

    public void SetZLabelString(string label)
    {
        zLabelString = label;
    }


    private void OnEnable()
	{
		laser = GetComponent<SteamVR_LaserPointer>();

		laser.PointerIn -= Collision;
		laser.PointerIn += Collision;
		laser.PointerOut -= CollisionEnded;
		laser.PointerOut += CollisionEnded;

		controller = GetComponent<SteamVR_TrackedController>();

		if (controller == null)
		{
			controller = GetComponentInParent<SteamVR_TrackedController>();
		}
        
		controller.TriggerClicked -= TriggerEvent;
		controller.TriggerClicked += TriggerEvent; 
	}

   
	private void TriggerEvent(object sender, ClickedEventArgs e)
	{
		if (selectedPoint != null && inPointCollision)
		{
            if (plotter.UseX())
            {
                xValueLabel.GetComponent<UnityEngine.UI.Text>().text =
                xLabelString + selectedPoint.GetAxis().x;
            }
            
            if (plotter.UseY())
            {
                yValueLabel.GetComponent<UnityEngine.UI.Text>().text =
                yLabelString + selectedPoint.GetAxis().y;
            }

            if (plotter.UseZ())
            {
                zValueLabel.GetComponent<UnityEngine.UI.Text>().text =
                zLabelString + selectedPoint.GetAxis().z;
            }

            string design = "";

            foreach(string designVal in selectedPoint.GetDesign())
            {
                design = design + designVal + "\n";
            }

            designLabel.GetComponent<UnityEngine.UI.Text>().text =
                design;
        }

        else if (selectedPoint != null && !inPointCollision)
        {
            selectedPoint = null;

            if (plotter.UseX())
            {
                xValueLabel.GetComponent<UnityEngine.UI.Text>().text =
                    xLabelString;

            }

            if (plotter.UseY())
            {

                yValueLabel.GetComponent<UnityEngine.UI.Text>().text =
                    yLabelString;
            }

            if (plotter.UseZ())
            {

                zValueLabel.GetComponent<UnityEngine.UI.Text>().text =
                    zLabelString;
            }

            designLabel.GetComponent<UnityEngine.UI.Text>().text =
                "";
        }
        
        else if(scaleButton != null)
        {
            scaleButton.UpdateScale();
        }

        else if (axisButton != null)
        {
            axisButton.UpdateVariable();
        }

        else if (invertAxisButton != null)
        {
            invertAxisButton.InvertVaraibles();
        }
    } 

	private void Collision(object sender, PointerEventArgs e)
	{
        if (e.target.GetComponent<Point>() != null)
        {
            inPointCollision = true;
            selectedPoint = e.target.GetComponent<Point>();
        }
        
        else if(e.target.GetComponent<ScaleButton>() != null)
        {
            scaleButton = e.target.GetComponent<ScaleButton>();
        }

        else if (e.target.GetComponent<AxisButton>() != null)
        {
            axisButton = e.target.GetComponent<AxisButton>();
        }

        else if (e.target.GetComponent<InvertAxisButton>() != null)
        {
            invertAxisButton = e.target.GetComponent<InvertAxisButton>();
        }
    }

	private void CollisionEnded(object sender, PointerEventArgs e)
	{

        if (e.target.GetComponent<Point>() != null)
        {
            inPointCollision = false;
        }

        else if (e.target.GetComponent<ScaleButton>() != null)
        {
            scaleButton = null;
        }

        else if (e.target.GetComponent<AxisButton>() != null)
        {
            axisButton = null;
        }

        else if (e.target.GetComponent<InvertAxisButton>() != null)
        {
            invertAxisButton = null;
        }
    }

}