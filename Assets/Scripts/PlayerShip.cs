using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShip : MonoBehaviour {
    [Range(1, 10)]
    public float maxVelocity = 5;
    [Range(0, 20)]
    public float acceleration = 0.1f;
    [Range(1, 20)]
    public float turningSpeed = 2.5f;
    [Range(1, 10)]
    public float exitVelocityReduction = 2;
    [Range(1, 5)]
    public float brakingSpeed = 1;

    private Rigidbody2D rb;
    private float velocity, turn;

    [HideInInspector]
    public List<GameObject> trailingObjects = new List<GameObject>();

    private ParticleSystem exhaust; 

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        exhaust = GetComponentInChildren<ParticleSystem>();
    }


    void Update() {
        var emit = exhaust.emission;
        emit.enabled = IsThrust();

        if(IsThrust()) velocity = Mathf.Lerp(velocity, maxVelocity, Time.deltaTime * acceleration);
        else velocity = Mathf.Lerp(velocity, 0, Time.deltaTime * acceleration * 2f);

        for(int i = 0; i < trailingObjects.Count; i++) {
            trailingObjects[i].transform.localScale = Vector3.Lerp(trailingObjects[i].transform.localScale, Vector3.one * 0.06f, Time.deltaTime * 2f);
            trailingObjects[i].transform.position = Vector3.Lerp(trailingObjects[i].transform.position, (transform.position - (transform.up * (i + 1) * 0.5f)), Time.deltaTime * 8f);
        }

        if(IsBrake()) {
            velocity = 0;
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * brakingSpeed);
        }

        if(IsTurningLeft()) turn += turningSpeed;
        else if(IsTurningRight()) turn -= turningSpeed;

        rb.AddForce(transform.up * velocity);
        rb.rotation = turn;
    }

    public void NeutralizeForce() {
        rb.velocity /= exitVelocityReduction;
    }

    public void AddAsteroid(GameObject obj) {
        trailingObjects.Add(obj);
    }

    public bool IsThrust() {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
    }

    public bool IsBrake() {
        return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
    }

    public bool IsTurningRight() {
        return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
    }

    public bool IsTurningLeft() {
        return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
    }
}
