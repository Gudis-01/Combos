using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float speed = 10f;
    public float force = 10f;
    public KeyDirection[] keys;
    public KeyDirection jump;
    
    private Rigidbody _rigidbody;
    private Vector3 _torque =Vector3.zero;
    private Vector3 _jump;
    
    [System.Serializable]
    public struct KeyDirection
    {
        public KeyCode key;
        public Vector3 direction;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyDirection key in keys)
        {
            if (Input.GetKey(key.key))
            {
                _torque += key.direction;
            }
        }

        if (Input.GetKeyDown(jump.key))
        {
            _jump+=jump.direction;
        }
    }

    private void FixedUpdate()
    {
        _rigidbody.AddTorque(_torque * speed * Time.fixedDeltaTime);
        _torque = Vector3.zero;
        
        _rigidbody.AddForce(_jump * force);
        _jump = Vector3.zero;
    }
}
