using UnityEngine;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance;

    public GameObject draggedObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}