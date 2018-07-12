using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class RapidBuildEditorTool : EditorWindow
{
    [MenuItem("Tools/拼UI神器 &#Q")]
    static void ShowWindow()
    {
        var cWin = CreateInstance<RapidBuildEditorTool>();
        cWin.Show();
    }

    private GameObject mTarget;
    void OnGUI()
    {
        EditorGUILayout.HelpBox("创建UI:", MessageType.Info);
        DrawVertical(() =>
        {
            DrawButton("创建按钮", CreateButton, 100);
            isNeedScale = GUILayout.Toggle(isNeedScale, "需要缩放");
        }, "Box");

        EditorGUILayout.HelpBox("修改UI:", MessageType.Info);
        DrawVertical(() =>
        {
            mTarget = Selection.activeGameObject;
            mTarget = EditorGUILayout.ObjectField("啥UI：", mTarget, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
            if (mTarget != null)
            {
                DrawHorizontal(() =>
                {
                    DrawButton("获取Label", GetLabels, 100);
                    DrawButton("获取所有Sprite", GetSprites, 100);
                    DrawButton("获取所有Texture", GetTextures, 100);
                });
                DrawHorizontal(() =>
                {
                    foreach (var itemLbl in childLblList)
                    {
                        if (componnent2ToggoleDic.ContainsKey(itemLbl))
                        {
                            DrawToggleGroup(itemLbl, componnent2ToggoleDic[itemLbl]);
                        }
                        else
                        {
                            
                            DrawToggleGroup(itemLbl, itemLbl.gameObject.activeSelf);

                        }
                    }
                });

            }
        }, "Box");
    }


    #region 功能

    /// <summary>
    /// 创建按钮
    /// </summary>
    private void CreateButton()
    {
        GameObject goParent = null;
        if (Selection.activeTransform != null)
            goParent = Selection.activeTransform.gameObject;
        else
        {
            goParent = NGUIEditorTools.SelectedRoot(true);
        }
        if (goParent != null)
        {
            var btnSprite = NGUISettings.AddSprite(goParent);
            Selection.activeGameObject = btnSprite.gameObject;
            var btn = btnSprite.gameObject; ;
            btn.name = "Btn";
            btn.AddComponent<UIButton>();
            BtnOtherSet(btn);
            btn.AddComponent<BoxCollider>();
            btnSprite.autoResizeBoxCollider = true;

            var btnLbl = NGUISettings.AddLabel(goParent);
            btnLbl.name = "BtnLbl";
            btnLbl.applyGradient = false;
            btnLbl.depth = 10;

            btnLbl.transform.SetParent(btn.transform);
        }

    }
    /// <summary>
    /// 是否需要按钮点击缩放
    /// </summary>
    private bool isNeedScale;
    /// <summary>
    /// 按钮所需的其他设置
    /// </summary>
    private void BtnOtherSet(GameObject btn)
    {
        if (isNeedScale)
        {
            var btnScale = btn.AddComponent<UIButtonScale>();
            btnScale.hover = Vector3.one;
            btnScale.pressed = new Vector3(0.9f, 0.9f, 0.9f);
            btnScale.duration = 0.1f;
        }

    }

    private List<UILabel> childLblList;
    /// <summary>
    /// 获得指定物品下的所有带Label组件的子物体
    /// </summary>
    private void GetLabels()
    {
        childLblList = new List<UILabel>();
        var tmpList = GetChildsWithThis<UILabel>();
        if (tmpList != null)
            childLblList.AddRange(tmpList);
        Debug.LogError(childLblList.Count);
    }

    private List<UISprite> childSpList;
    private void GetSprites()
    {
        childSpList = new List<UISprite>();
        var tmpList = GetChildsWithThis<UISprite>();
        if (tmpList != null)
            childSpList.AddRange(tmpList);
        Debug.LogError(childSpList.Count);
    }

    private List<UITexture> childTxList;
    private void GetTextures()
    {
        childTxList = new List<UITexture>();
        var tmpList = GetChildsWithThis<UITexture>();
        if (tmpList != null)
            childTxList.AddRange(tmpList);
        Debug.LogError(childTxList.Count);
    }

    private List<T> GetChildsWithThis<T>() where T : UIWidget
    {
        List<T> chilTList = new List<T>();
        var maxCount = mTarget.transform.childCount;
        for (int i = 0; i < maxCount; i++)
        {
            var componentTs = mTarget.transform.GetChild(i);
            var component = componentTs.GetComponent<T>();
            if (component != null)
                chilTList.Add(component);
        }
        if (chilTList.Count > 0) return chilTList;
        else return null;
    }
    #endregion

    #region 辅助
    void DrawButton(string cName, Action cClickAction, int cWidth)
    {
        if (GUILayout.Button(cName, GUILayout.Width(cWidth), GUILayout.Height(50)))
        {
            if (cClickAction != null) cClickAction();
        }
    }
    void DrawVertical(Action cAction, string cStyle, params GUILayoutOption[] cOptions)
    {
        if (cAction != null)
        {

            if (string.IsNullOrEmpty(cStyle)) EditorGUILayout.BeginVertical(cOptions);
            else EditorGUILayout.BeginVertical(cStyle, cOptions);
            cAction();
            EditorGUILayout.EndVertical();
        }
    }
    void DrawHorizontal(Action cAction, params GUILayoutOption[] cOptions)
    {
        if (cAction != null)
        {
            EditorGUILayout.BeginHorizontal(cOptions);
            cAction();
            EditorGUILayout.EndHorizontal();
        }
    }

    Dictionary<UIWidget, bool> componnent2ToggoleDic = new Dictionary<UIWidget, bool>();
    void DrawToggleGroup(UIWidget widget, bool toggle)
    {
        var bIsToggle = EditorGUILayout.BeginToggleGroup(widget.name, toggle);
        componnent2ToggoleDic.Add(widget, bIsToggle);
        //widget.gameObject.SetActive(toggle);
    }
    #endregion

}
