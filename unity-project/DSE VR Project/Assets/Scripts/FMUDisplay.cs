using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Author: Ammar Hasan 150454388 2018
/// Class Purpose: This class reads a JSON file to 
/// </summary>
public class FMUDisplay : MonoBehaviour {

    public string jsonPath;

    // String for JSON reading
    private const string FMU_TITLE = "fmus";

    // Used at initialization
    void Start () {
        if (System.IO.File.Exists(jsonPath)) // check if path exists
        {
            // get json text
            string json = File.ReadAllText(jsonPath);

            // Read json using JSON.NET
            JToken jsonToken = JToken.Parse(json);

            // get JSON tokens into a list
            IList<JToken> fmuConfigs = jsonToken[FMU_TITLE].Children().ToList();

            string fmuConfigsText = "";

            // serialize JSON results into .NET objects
            foreach (JToken fmuConfig in fmuConfigs)
            {
                // Add strings from JSON to fmuConfigString to set to label
                string fmuConfigString = fmuConfig.ToString();
                fmuConfigsText = fmuConfigsText + fmuConfigString + "\n" + "\n";
            }
            this.gameObject.GetComponent<UnityEngine.UI.Text>().text = fmuConfigsText;
        }

        else
        {
            Debug.Log("JSON File not found");
        }
    }
}
