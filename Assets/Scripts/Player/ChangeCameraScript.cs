using System;
using Unity.Cinemachine;
using UnityEngine;

public class ChangeCameraScript : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInChildren<CinemachineConfiner2D>().BoundingShape2D = gameObject.GetComponent<BoxCollider2D>();
        }
    }
}
