//using Fantasy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[CustomEditor(typeof(PanelUI))]
public class PanelUIEditor : UnityEditor.Editor
{
    private PanelUI _panelUI;
    private readonly HashSet<int> _remove = new HashSet<int>();
    private static string[] m_UIComonentName;
    public static string[] UIComponentName
    {
        get 
        { 
            if(m_UIComonentName == null)
            {
                m_UIComonentName = new string[UIComponentType.Count + 1];
                m_UIComonentName[0] = "自定义";
                for(int i = 1; i < m_UIComonentName.Length; i++)
                {
                    m_UIComonentName[i] = UIComponentType[i - 1].Name;
                }
            }
            return m_UIComonentName; 
        }
    }

    public static List<Type> UIComponentType = new List<Type>()
    {
        typeof(Slider),
        typeof(Toggle),
        typeof(Dropdown),
        typeof(ScrollRect),
        typeof(InputField),
        typeof(TMP_Text),
        typeof(TMP_Dropdown),
        typeof(TMP_InputField),
        typeof(Button),
        typeof(Text),
        typeof(Image),
        typeof(RawImage),
        typeof(ListHandler),
    };

    public static List<Type> UIComponentTypeSort = new List<Type>()
    {
        typeof(Slider),
        typeof(Toggle),
        typeof(Dropdown),
        typeof(ScrollRect),
        typeof(InputField),
        typeof(Button),
        typeof(ListHandler),
    };

    private void OnEnable()
    {
        _panelUI = (PanelUI)target;
    }

    public override void OnInspectorGUI()
    {
        var dataProperty = serializedObject.FindProperty("list");
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();

        if (string.IsNullOrEmpty(_panelUI.componentName))
        {
            _panelUI.componentName = _panelUI.gameObject.name;
        }

        _panelUI.componentName = EditorGUILayout.TextField("Component Name", _panelUI.componentName);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        _panelUI.assetName = EditorGUILayout.TextField("AssetName", _panelUI.assetName);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        _panelUI.bundleName = EditorGUILayout.TextField("BundleName", _panelUI.bundleName);
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Add"))
        {
            AddReference(dataProperty, Guid.NewGuid().GetHashCode().ToString(), null);
        }

        if (GUILayout.Button("Clear"))
        {
            dataProperty.ClearArray();
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        if (GUILayout.Button("Delete Empty"))
        {
            for (var i = dataProperty.arraySize - 1; i >= 0; i--)
            {
                var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("obj");

                if (gameObjectProperty.objectReferenceValue != null)
                {
                    continue;
                }

                dataProperty.DeleteArrayElementAtIndex(i);
                EditorUtility.SetDirty(_panelUI);
                serializedObject.ApplyModifiedProperties();
                serializedObject.UpdateIfRequiredOrScript();
            }
        }

        if (GUILayout.Button("Sort"))
        {
            _panelUI.list.Sort(new PanelUIDataComparer());
            EditorUtility.SetDirty(this);
            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }

        GUILayout.EndHorizontal();


        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("注册面板", GUILayout.ExpandWidth(true)))
        {
            ResMgr.Instance.Load<PanelList_SO>("PanelList_SO").RigisterPanel(_panelUI.gameObject);
        }


        if (GUILayout.Button(new GUIContent("Generate Code")))
        {
            Generate(dataProperty);
        }

        GUILayout.EndVertical();

        for (var i = _panelUI.list.Count - 1; i >= 0; i--)
        {
            GUILayout.BeginHorizontal();

            var property = dataProperty.GetArrayElementAtIndex(i);
            var key = property.FindPropertyRelative("key");
            key.stringValue = EditorGUILayout.TextField(key.stringValue, GUILayout.Width(150));
            var objProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("obj");

            Object obj = objProperty.objectReferenceValue;

            var indexproperty = property.FindPropertyRelative("index");
            int index = EditorGUILayout.Popup(indexproperty.intValue, UIComponentName, GUILayout.Width(150));
            if (indexproperty.intValue != index)
            {
                if(obj is GameObject gb)
                {
                    if(index != 0)
                    {
                        Component component = gb.GetComponent(UIComponentType[index - 1]);
                        if(component != null)
                        {
                            objProperty.objectReferenceValue = component;
                            indexproperty.intValue = index;
                        }
                    }
                }else if(obj is Component c)
                {
                    if(index == 0)
                    {
                        objProperty.objectReferenceValue = c.gameObject;
                        indexproperty.intValue = index;
                    }
                    else
                    {
                        Component component = c.gameObject.GetComponent(UIComponentType[index - 1]);
                        if (component != null)
                        {
                            objProperty.objectReferenceValue = component;
                            indexproperty.intValue = index;
                        }
                    }
                }
            }
            obj = objProperty.objectReferenceValue;
            objProperty.objectReferenceValue = EditorGUILayout.ObjectField(obj, typeof(Object), true);

            if (GUILayout.Button("X"))
            {
                _remove.Add(i);
            }

            GUILayout.EndHorizontal();
        }

        var eventType = Event.current.type;

        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var o in DragAndDrop.objectReferences)
                {
                    AddReference(dataProperty,o.name,o);
                }
            }

            Event.current.Use();
        }

        if (_remove.Count > 0)
        {
            foreach (var removeIndex in _remove)
            {
                dataProperty.DeleteArrayElementAtIndex(removeIndex);
            }

            _remove.Clear();
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
    }

    private void AddReference(SerializedProperty dataProperty ,string key, Object obj)
    {
        var index = dataProperty.arraySize;
        dataProperty.InsertArrayElementAtIndex(index);
        var element = dataProperty.GetArrayElementAtIndex(index);



        if (obj is GameObject o)
        {
            if (o.name.Contains("[") && o.name.Contains("]"))
            {
                int i = o.name.IndexOf("]") + 1;
                string objName = o.name.Substring(i, o.name.Length - i);
                element.FindPropertyRelative("index").intValue = 0;
                element.FindPropertyRelative("key").stringValue = objName;
                element.FindPropertyRelative("obj").objectReferenceValue = obj;
            }
            else
            {
                Component c = null;
                int i = 99;
                foreach (var com in o.GetComponents<Component>())
                {
                    Type type = com.GetType();
                    for(int j = 0; j < UIComponentType.Count; j++)
                    {
                        if (UIComponentType[j].IsAssignableFrom(type)) 
                        {
                            if (j < i)
                            {
                                if (i < UIComponentType.Count && UIComponentTypeSort.Exists(ui => ui == UIComponentType[i])) 
                                {
                                    if (UIComponentTypeSort.Exists(ui => ui == UIComponentType[j])) 
                                    {
                                        if(UIComponentTypeSort.FindIndex(ui => ui == UIComponentType[j]) >= UIComponentTypeSort.FindIndex(ui => ui == UIComponentType[i]))
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                i = j;
                                c = com;
                                break;
                            }
                        }
                    }
                }
                if (c != null)
                {
                    element.FindPropertyRelative("index").intValue = i + 1;
                    element.FindPropertyRelative("key").stringValue = key;
                    element.FindPropertyRelative("obj").objectReferenceValue = c;
                }
                else
                {
                    element.FindPropertyRelative("index").intValue = 0;
                    element.FindPropertyRelative("key").stringValue = key;
                    element.FindPropertyRelative("obj").objectReferenceValue = obj;
                }
            }
        }
        else
        {
            element.FindPropertyRelative("index").intValue = 0;
            element.FindPropertyRelative("key").stringValue = key;
            element.FindPropertyRelative("obj").objectReferenceValue = obj;
        }
    }

    private void Generate(SerializedProperty dataProperty)
    {
        var generatePath = Application.dataPath+"/Scripts/UI/Entity/";

        if (!Directory.Exists(generatePath))
        {
            Directory.CreateDirectory(generatePath);
            return;
        }

        // if (string.IsNullOrEmpty(generatePath))
        // {
        //     EditorUtility.DisplayDialog("Generate Code", "Please enter the path in the menu first Fantasy/ReferencePreferences Set GeneratePath", "OK");
        //     return;
        // }
        //
        // if (!Directory.Exists(generatePath))
        // {
        //     EditorUtility.DisplayDialog("Generate Code", $"{generatePath} is not a valid path", "OK");
        //     return;
        // }

        //if (string.IsNullOrEmpty(_fantasyUI.assetName))
        //{
        //    EditorUtility.DisplayDialog("Generate Code", $"assetName is null", "OK");
        //    return;
        //}

        //if (string.IsNullOrEmpty(_fantasyUI.bundleName))
        //{
        //    EditorUtility.DisplayDialog("Generate Code", $"bundleName is null", "OK");
        //    return;
        //}

        //if (_fantasyUI.uiLayer == UILayer.None)
        //{
        //    EditorUtility.DisplayDialog("Generate Code", $"UILayer is None", "OK");
        //    return;
        //}

        var createStr = new List<string>();
        var propertyStr = new List<string>();
        var bindStr = new List<string>();

        for (var i = _panelUI.list.Count - 1; i >= 0; i--)
        {
            var keyProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
            var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("obj");
            string refName = "";
            Object obj = gameObjectProperty.objectReferenceValue;
            var gameObjectName = Regex.Replace(keyProperty.stringValue, @"\s+", "");



            if (obj is GameObject gb)
            {
                int index = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue;
                if(index == 0 && gb.name.Contains("[") && gb.name.Contains("]"))
                {
                    refName = gb.name.Substring(1, gb.name.IndexOf("]") - 1);
                    if(refName != "GameObject")
                    {
                        createStr.Add($"\t\t{gameObjectName} = ui.GetReferenceComponent<{refName}>(\"{keyProperty.stringValue}\");");
                    }
                    else
                    {
                        refName = gb.GetType().FullName;
                        createStr.Add($"\t\t{gameObjectName} = ui.GetReference<{refName}>(\"{keyProperty.stringValue}\");");
                    }
                }
                else
                {
                    refName = gb.GetType().FullName;
                    createStr.Add($"\t\t{gameObjectName} = ui.GetReference<{refName}>(\"{keyProperty.stringValue}\");");
                }

            }else if(obj is Component c)
            {
                refName = c.GetType().FullName;
                createStr.Add($"\t\t{gameObjectName} = ui.GetReference<{refName}>(\"{keyProperty.stringValue}\");");
            }
            else
            {
                refName = obj.GetType().FullName;
                createStr.Add($"\t\t{gameObjectName} = ui.GetReference<{refName}>(\"{keyProperty.stringValue}\");");
            }


            propertyStr.Add($"\tpublic {refName} {gameObjectName};");

            if(refName == typeof(Button).FullName)
            {
                bindStr.Add($"\t\tstring  {gameObjectName}ButtonName = \"{gameObjectName}\";");
                bindStr.Add("\t\t" + gameObjectName + ".onClick.AddListener(()=>{ OnClick(" + gameObjectName + "ButtonName); });");
            }else if(refName == typeof(Toggle).FullName)
            {
                bindStr.Add($"\t\tstring  {gameObjectName}ToggleName = \"{gameObjectName}\";");
                bindStr.Add("\t\t" + gameObjectName + ".onValueChanged.AddListener((o)=>{ OnToggleValueChanged(" + gameObjectName + "ToggleName, o); });");
            }
            else if (refName == typeof(Dropdown).FullName)
            {
                bindStr.Add($"\t\tstring  {gameObjectName}DropdownName = \"{gameObjectName}\";");
                bindStr.Add("\t\t" + gameObjectName + ".onValueChanged.AddListener((o)=>{ OnDropdownValueChanged(" + gameObjectName + "DropdownName, o); });");
            }
            else if (refName == typeof(InputField).FullName)
            {
                bindStr.Add($"\t\tstring  {gameObjectName}InputFieldName = \"{gameObjectName}\";");
                bindStr.Add("\t\t" + gameObjectName + ".onValueChanged.AddListener((o)=>{ OnInputFieldValueChanged(" + gameObjectName + "InputFieldName, o); });");
            }
            else if (refName == typeof(TMP_Dropdown).FullName)
            {
                bindStr.Add($"\t\tstring  {gameObjectName}DropdownName = \"{gameObjectName}\";");
                bindStr.Add("\t\t" + gameObjectName + ".onValueChanged.AddListener((o)=>{ OnDropdownValueChanged(" + gameObjectName + "DropdownName, o); });");
            }
            else if (refName == typeof(TMP_InputField).FullName)
            {
                bindStr.Add($"\t\tstring  {gameObjectName}InputFieldName = \"{gameObjectName}\";");
                bindStr.Add("\t\t" + gameObjectName + ".onValueChanged.AddListener((o)=>{ OnInputFieldValueChanged(" + gameObjectName + "InputFieldName, o); });");
            }
        }

        var sb = new StringBuilder();

        sb.AppendLine("using UnityEngine;");
        sb.AppendLine($"public partial class {_panelUI.componentName} : BasePanel\n{{");

        foreach (var property in propertyStr)
        {
            sb.AppendLine(property);
        }

        sb.AppendLine("\n\tpublic override void Init(PanelUI ui)\n\t{");

        foreach (var str in createStr)
        {
            sb.AppendLine(str);
        }

        foreach(var str in bindStr)
        {
            sb.AppendLine(str);
        }

        sb.AppendLine("\t}");
        sb.AppendLine("}");

        var combinePath = Path.Combine(generatePath, $"{_panelUI.componentName}.cs");
        using var entityStreamWriter = new StreamWriter(combinePath);
        entityStreamWriter.Write(sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"代码生成位置:{combinePath}");
    }
}
