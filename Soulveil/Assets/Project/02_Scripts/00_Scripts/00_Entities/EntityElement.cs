using UnityEngine;

public class EntityElement : MonoBehaviour
{
    [SerializeField] private Element currentElement;

    public Element CurrentElement => currentElement;

    public void SetElement ( Element newElement )
    {
        currentElement = newElement;
    }
}