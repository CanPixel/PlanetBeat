using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet_Motion : MonoBehaviour
{
    public GameObject PLANET1;
    public GameObject PLANET2;
    public GameObject PLANET3;
    public GameObject PLANET4;
    public GameObject PLANET5;
    public GameObject PLANET6;
    
    [SerializeField]
    public Transform rotationCenter;
    Vector2 EllipsePos;
    
    [SerializeField]
    float rotationRadius = 2f, angularSpeed = 2f;
    
    float posX1, posY1 = 0f;
    float posX2, posY2 = 0f;
    float posX3, posY3 = 0f;
    float posX4, posY4 = 0f;
    float posX5, posY5 = 0f;
    float posX6, posY6 = 0f;
    
    float angle1 = 0f;
    float angle2 = 1f;
    float angle3 = 2f;
    float angle4 = 3f;
    float angle5 = 4f;
    float angle6 = 5f;
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
        posX1 = rotationCenter.position.x + Mathf.Cos (angle1) * rotationRadius;
        posY1 = rotationCenter.position.y + Mathf.Sin (angle1) * rotationRadius / 2;
        
        posX2 = rotationCenter.position.x + Mathf.Cos (angle2) * rotationRadius;
        posY2 = rotationCenter.position.y + Mathf.Sin (angle2) * rotationRadius / 2;
        
        posX3 = rotationCenter.position.x + Mathf.Cos (angle3) * rotationRadius;
        posY3 = rotationCenter.position.y + Mathf.Sin (angle3) * rotationRadius / 2;
        
        posX4 = rotationCenter.position.x + Mathf.Cos (angle4) * rotationRadius;
        posY4 = rotationCenter.position.y + Mathf.Sin (angle4) * rotationRadius / 2;

        posX5 = rotationCenter.position.x + Mathf.Cos (angle5) * rotationRadius;
        posY5 = rotationCenter.position.y + Mathf.Sin (angle5) * rotationRadius / 2;
        
        posX6 = rotationCenter.position.x + Mathf.Cos (angle6) * rotationRadius;
        posY6 = rotationCenter.position.y + Mathf.Sin (angle6) * rotationRadius / 2;
        
// ----------
        
        PLANET1.transform.position = new Vector2 (posX1, posY1);
        PLANET2.transform.position = new Vector2 (posX2, posY2);
        PLANET3.transform.position = new Vector2 (posX3, posY3);
        PLANET4.transform.position = new Vector2 (posX4, posY4);
        PLANET5.transform.position = new Vector2 (posX5, posY5);
        PLANET6.transform.position = new Vector2 (posX6, posY6);
        
        angle1 = angle1 + Time.deltaTime * angularSpeed;
        angle2 = angle2 + Time.deltaTime * angularSpeed;
        angle3 = angle3 + Time.deltaTime * angularSpeed;
        angle4 = angle4 + Time.deltaTime * angularSpeed;
        angle5 = angle5 + Time.deltaTime * angularSpeed;
        angle6 = angle6 + Time.deltaTime * angularSpeed;
        
        if(angle1 >= 360f)
            angle1 = 0f;
        if(angle2 >= 360f)
            angle2 = 0f;            
        if(angle3 >= 360f)
            angle3 = 0f;
        if(angle4 >= 360f)
            angle4 = 0f;
        if(angle5 >= 360f)
            angle5 = 0f;
        if(angle6 >= 360f)
            angle6 = 0f;
        //Debug.Log(posX);
        //Debug.Log(posY);
    }
}
