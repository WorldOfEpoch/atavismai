using UnityEditor;

public static class ExcaliburAIAtavismMenu
{
    [MenuItem("ExcaliburAI/Atavism/Auto-tailor Mapping")]
    public static async void AutoTailor()
    {
        try
        {
            var result = await AtavismIntrospector.BuildAndWriteMapping();
            EditorUtility.DisplayDialog("ExcaliburAI", result, "OK");
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("ExcaliburAI", "Error: " + ex.Message, "OK");
        }
    }
}
