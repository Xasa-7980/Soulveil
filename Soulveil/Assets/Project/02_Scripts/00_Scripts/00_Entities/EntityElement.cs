using UnityEngine;

public class EntityElement : MonoBehaviour
{
    [SerializeField] private Element element;

    public Element CurrentElement => element;

    public void SetElement ( Element newElement )
    {
        element = newElement;
    }
}