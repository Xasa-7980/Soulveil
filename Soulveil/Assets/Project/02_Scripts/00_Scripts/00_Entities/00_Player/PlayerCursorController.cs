using UnityEngine;

public class PlayerCursorController : MonoBehaviour
{
    private void Start ( )
    {
        LockCursor();
    }

    public void LockCursor ( )
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor ( )
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}