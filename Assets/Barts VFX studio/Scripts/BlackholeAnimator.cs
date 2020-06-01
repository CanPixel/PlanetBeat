using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackholeAnimator : MonoBehaviour
{
    Animator _a;
    

    public void Blackholeopen()
    {
        _a.SetBool("open", true);
        _a.SetBool("close", false);
    }
    public void Blackholeclose()
    {
        _a.SetBool("open", false);
        _a.SetBool("close", true);
    }
}
