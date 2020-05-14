
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    Animator m_Animator;

    void Start()
    {
        //Get the Animator attached to the GameObject you are intending to animate.
        m_Animator = gameObject.GetComponent<Animator>();
        m_Animator.SetBool("res-low", true);
    }

    void Update()
    {
        //Press the up arrow button to reset the trigger and set another one
        if (Input.GetKey(KeyCode.F1))
        {
            Debug.Log("Medium Resource");

            m_Animator.SetBool("res-low", false);
            m_Animator.SetBool("res-med", true);
        }
        if (Input.GetKey(KeyCode.F2))
        {
            Debug.Log("High Resource");

            m_Animator.SetBool("res-med", false);
            m_Animator.SetBool("res-high", true);
        }
        if (Input.GetKey(KeyCode.F3))
        {
            Debug.Log("High Resource");

            m_Animator.SetBool("res-high", false);
            m_Animator.SetBool("res-unstable", true);
        }
        if (Input.GetKey(KeyCode.F4))
        {
            Debug.Log("Exploding Resource");
            m_Animator.SetBool("res-unstable", false);
            m_Animator.SetBool("res-explode", true);
        }
    }
}