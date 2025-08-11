using UnityEngine;
using UnityEngine.UIElements;

public class LoadUIWindow : MonoBehaviour
{
    [Tooltip("Path to the UXML layout asset in the Resources folder")]
    public string uxmlPath = "path_to_your_uxml_file"; // Replace with the path to your UXML file inside the Resources folder

    private void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("No UIDocument component found on this GameObject!");
            return;
        }

        // Load the UXML asset
        var visualTreeAsset = Resources.Load<VisualTreeAsset>(uxmlPath);
        if (visualTreeAsset == null)
        {
            Debug.LogError($"Failed to load UXML asset from path: {uxmlPath}");
            return;
        }

        // Clone the UXML layout and attach it to the panel
        VisualElement root = uiDocument.rootVisualElement;
        VisualElement window = visualTreeAsset.CloneTree();
        root.Add(window);
    }
}
