using UnityEngine;

public class GravityManager : MonoBehaviour
{
    void Awake()
    {
        // disable global gravity, and use local gravity instead in each ball 
        Physics.gravity = Vector3.zero; 
    }
}
