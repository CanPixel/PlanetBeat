// For short bursts of force (such as an explosion) use ForceMode2D.Impulse
// This adds an instant force impulse to the Rigidbody2D, using mass

using System.Collections;
using UnityEngine;

public class ExampleOne : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private float thrust = 10.0f;

    private void Start()
    {
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        transform.position = new Vector3(0.0f, -2.0f, 0.0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb2D.AddForce(transform.up * thrust, ForceMode2D.Impulse);
        }
    }
}