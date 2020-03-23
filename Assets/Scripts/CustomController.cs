using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class CustomController : MonoBehaviour {

    public enum SerialAddressPort {
        BARTMAC, WINDOWS
    }
    public SerialAddressPort serialAddress = SerialAddressPort.BARTMAC;
    public static SerialPort sp;
    public string receivedstring;

    void Start() {
        string add = "";
        switch(serialAddress) {
                case SerialAddressPort.BARTMAC:
                    add = "/dev/cu.usbmodem144101";
                    break;
                default: case SerialAddressPort.WINDOWS:
                    add = "COM5";
                    break; 
        }
       sp = new SerialPort(add, 9600);
    }

    void Update() {
        if (sp.IsOpen == false) sp.Open(); //Open the Serial Stream.

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
}