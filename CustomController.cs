using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class CustomController : MonoBehaviour {
    public static SerialPort sp;
    private string receivedstring;
    public string port;

    public bool useCustomControls = false;

    [Range(2, 100)]
    public int sensitivity = 20;

    void Start() {
       sp = new SerialPort(port, 9600);
    }

    void Update() {
        if (sp.IsOpen == false) sp.Open();

        var readData = readFromArduino();
        if (readData.Length > 0) {
            receivedstring = readData;
            Debug.Log(readData);
        }
    }

    public string readFromArduino() {
        int timeOut = 1;
        sp.ReadTimeout = timeOut;
        try {
            return sp.ReadLine();
        } catch (System.Exception) { return ""; } 
    }

    public int GetData() {
        return int.Parse(receivedstring);
    }
}