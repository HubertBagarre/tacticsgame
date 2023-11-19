using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using Battle;
using UnityEngine;

[CustomEditor(typeof(TimelineManager))]
public class TimelineManagerEditor : Editor
{
    private SerializedProperty entitiesInTimeline;

    private ReorderableList list;

    private void OnEnable()
    {
        entitiesInTimeline = serializedObject.FindProperty("timelineEntityShowers");
        
        list = new ReorderableList(serializedObject, entitiesInTimeline, true, true, false, false)
            {
                drawElementCallback = DrawListItems, // Delegate to draw the elements on the list
                drawHeaderCallback = DrawHeader // Skip this line if you set displayHeader to 'false' in your ReorderableList constructor.
            };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update(); // Update the array property's representation in the inspector
        
        list.DoLayoutList(); // Have the ReorderableList do its work

        // We need to call this so that changes on the Inspector are saved by Unity.
        serializedObject.ApplyModifiedProperties();
    }

    // Draws the elements on the list
    private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        var rectX = rect.x;
        
        var element = list.serializedProperty.GetArrayElementAtIndex(index); // The element in the list
        
        var entityName = element.FindPropertyRelative("<Name>k__BackingField").stringValue;
        EditorGUI.LabelField(new Rect(rectX, rect.y, 100, EditorGUIUtility.singleLineHeight), entityName);

        DrawLabel("Speed","Speed",90,40,100,29);
        DrawLabel("Distance","DistanceFromStart",35,55,100,29);
        
        DrawLabel("Rate","DecayRate",35,30,100,31);
        DrawLabel("Order","TurnOrder",35,35,100,31);

        return;

        void DrawLabel(string label,string field,float xLabel,float xField,float wLabel, float wField)
        {
            rectX += xLabel;
            GUI.enabled = true;
            EditorGUI.LabelField(new Rect(rectX, rect.y, wLabel, EditorGUIUtility.singleLineHeight), label);
            
            GUI.enabled = false;
            rectX += xField;
            EditorGUI.PropertyField(
                new Rect(rectX, rect.y, wField, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative($"<{field}>k__BackingField"),
                GUIContent.none
            );
        }
    }

    //Draws the header
    private void DrawHeader(Rect rect)
    {
        const string label = "Entity";
        EditorGUI.LabelField(rect, label);
    }
}
