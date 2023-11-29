using System.Linq;
using Battle.ScriptableObjects;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NewAbilitySO))]
public class NewAbilitySOEditor : Editor
{
    NewAbilitySO abilitySO;
    private bool showRequirementsParameters;

    private void OnEnable()
    {
        abilitySO = (NewAbilitySO) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        showRequirementsParameters = EditorGUILayout.Foldout(showRequirementsParameters, "Possible Requirements Parameters");

        if (showRequirementsParameters)
        {
            var parameters = abilitySO.Requirements.SelectMany(condition => condition.SpecificParameters).Distinct().ToArray();
            
            GUI.enabled = false;
            foreach (var parameter in parameters)
            {
                EditorGUILayout.LabelField(parameter);
            }
            GUI.enabled = true;
        }
    }
}
