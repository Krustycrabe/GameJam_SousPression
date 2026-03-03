using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Verrouille le curseur au centre
        Cursor.visible = false;                   // Cache le curseur
    }
}