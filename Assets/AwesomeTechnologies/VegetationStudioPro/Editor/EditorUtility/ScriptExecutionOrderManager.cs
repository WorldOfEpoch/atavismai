using System;
using UnityEditor;

namespace AwesomeTechnologies.Utility
{
    [InitializeOnLoad]
    public class ScriptExecutionOrderManager
    {
        static ScriptExecutionOrderManager()
        {
            foreach (MonoScript monoScript in MonoImporter.GetAllRuntimeMonoScripts())
                if (monoScript.GetClass() != null)
                    foreach (var a in Attribute.GetCustomAttributes(monoScript.GetClass(), typeof(ScriptExecutionOrder)))
                    {
                        var currentOrder = MonoImporter.GetExecutionOrder(monoScript);
                        var newOrder = ((ScriptExecutionOrder)a).Order;
                        if (currentOrder != newOrder)
                            MonoImporter.SetExecutionOrder(monoScript, newOrder);
                    }
        }
    }
}