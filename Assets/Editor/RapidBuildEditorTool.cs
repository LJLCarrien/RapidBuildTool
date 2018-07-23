﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    private bool lblFoldout;
    private UIAtlas comAtlas;
    void OnGUI()
    {
        EditorGUILayout.HelpBox("创建对象:", MessageType.Info, true);
        DrawVertical(() =>
        {
            DrawButton("创建按钮", CreateButton, 100);
            isNeedScale = GUILayout.Toggle(isNeedScale, "点击需要缩放效果");
        }, "Box");
        //常用图集
        ShowComAtlas();
        EditorGUILayout.HelpBox("针对此对象的子物体修改:", MessageType.Info);
        DrawVertical(() =>
        {
            if (mTarget == null) mTarget = Selection.activeGameObject;
            mTarget = EditorGUILayout.ObjectField("修改对象：", mTarget, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
            if (mTarget != null)
            {
                DrawHorizontal(() =>
                {
                    isNeedChangeName = GUILayout.Toggle(isNeedChangeName, "修改名字");
                    isNeedChangeTs = GUILayout.Toggle(isNeedChangeTs, "修改位置");
                });
                isOnlyShowDepthBelowTen = GUILayout.Toggle(isOnlyShowDepthBelowTen, "只显示层级低于10");

                DrawButton("一键修正缩放", OneKeyResetScale, 100);
                if (resultStringBuilder != null && resultStringBuilder.Length > 0)
                    GUILayout.TextArea(resultStringBuilder.ToString());

                DrawHorizontal(() =>
                {
                    DrawButton("获取Label", GetLabels, 100);
                    DrawButton("获取所有Sprite", GetSprites, 100);
                    DrawButton("获取所有Texture", GetTextures, 100);
                });

                //labels
                ShowLabels();
            }
        }, "Box");

    }


    #region 创建
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
            btnLbl.depth = LabelDefaultDepth;

            btnLbl.transform.SetParent(btn.transform);
        }

    }
    /// <summary>
    /// 是否需要按钮点击缩放
    /// </summary>
    private bool isNeedScale = true;
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
    #endregion
    #region 保存
    [SerializeField]//必须要加
    protected List<UIAtlas> AtlasList = new List<UIAtlas>();

    //序列化对象
    protected SerializedObject _serializedObject;

    //序列化属性
    protected SerializedProperty _assetLstProperty;

    protected void OnEnable()
    {
        //使用当前类初始化
        _serializedObject = new SerializedObject(this);
        //获取当前类中可序列话的属性
        _assetLstProperty = _serializedObject.FindProperty("AtlasList");
    }


    private bool commonAtlasFoldout;
    //private List<UIAtlas> commonAtlasList;
    //private int totalAtlasNum = 3;

    private void ShowComAtlas()
    {
        //commonAtlasList = new List<UIAtlas>(totalAtlasNum);

        //commonAtlasFoldout = EditorGUILayout.Foldout(commonAtlasFoldout, "Atlas");
        //if (commonAtlasFoldout)
        //{
        //    DrawVertical(() =>
        //    {
        //        for (int i = commonAtlasList.Count; i < totalAtlasNum; i++)
        //        {
        //            var atlasitem = EditorGUILayout.ObjectField("常用：" + i, commonAtlasList[i], typeof(UIAtlas), true, GUILayout.ExpandWidth(true)) as UIAtlas;
        //            commonAtlasList.Add(atlasitem);
        //        }
        //    }, "Box");
        //}
        //更新
        _serializedObject.Update();

        //开始检查是否有修改
        EditorGUI.BeginChangeCheck();

        //显示属性
        //第二个参数必须为true，否则无法显示子节点即List内容
        EditorGUILayout.PropertyField(_assetLstProperty, true);

        //结束检查是否有修改
        if (EditorGUI.EndChangeCheck())
        {
            //提交修改
            _serializedObject.ApplyModifiedProperties();
        }
    }

    //private void AddComAtlas()
    //{
    //    totalAtlasNum++;
    //}
    #endregion
    #region 修改
    /// <summary>
    /// 是否需要提供修改名字
    /// </summary>
    private bool isNeedChangeName = true;
    /// <summary>
    /// 是否需要提供修改位置
    /// </summary>
    private bool isNeedChangeTs = true;
    /// <summary>
    /// 是否只显示层级低于10的
    /// </summary>
    private bool isOnlyShowDepthBelowTen;

    #region Labels
    private readonly int LabelDefaultDepth = 10;
    private List<UILabel> childLblList;
    /// <summary>
    /// 获得指定物品下的所有带Label组件的子物体
    /// </summary>
    private void GetLabels()
    {
        childLblList = new List<UILabel>();
        var tmpList = GetChildsWithThis<UILabel>();
        if (tmpList != null)
        {
            if (isOnlyShowDepthBelowTen)
            {
                var tmpDepthBelowTenList = new List<UILabel>();
                foreach (var uiLabel in tmpList)
                {
                    if (uiLabel.depth < LabelDefaultDepth)
                    {
                        tmpDepthBelowTenList.Add(uiLabel);
                    }

                }
                childLblList.AddRange(tmpDepthBelowTenList);

            }
            else
                childLblList.AddRange(tmpList);
        }

        Debug.LogError(childLblList.Count);
    }
    /// <summary>
    /// 显示带Label组件的子物体
    /// </summary>
    private void ShowLabels()
    {
        if (childLblList != null && childLblList.Count != 0)
        {
            lblFoldout = EditorGUILayout.Foldout(lblFoldout, "Labels");
            if (lblFoldout)
            {
                DrawVertical(() =>
                {
                    foreach (var itemLbl in childLblList)
                    {
                        DrawToggleItem(itemLbl, itemLbl.gameObject.activeSelf);
                    }
                }, "Box");
            }
        }
    }
    #endregion
    #region Sprites
    private List<UISprite> childSpList;
    private void GetSprites()
    {
        childSpList = new List<UISprite>();
        var tmpList = GetChildsWithThis<UISprite>();
        if (tmpList != null)
            childSpList.AddRange(tmpList);
        Debug.LogError(childSpList.Count);
    }
    #endregion
    #region Textures
    private List<UITexture> childTxList;
    private void GetTextures()
    {
        childTxList = new List<UITexture>();
        var tmpList = GetChildsWithThis<UITexture>();
        if (tmpList != null)
            childTxList.AddRange(tmpList);
        Debug.LogError(childTxList.Count);
    }
    #endregion
    #region 功能辅助
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
    #region 其他功能
    private StringBuilder resultStringBuilder;
    private readonly Vector3 needResetToThisScale = Vector3.one;
    /// <summary>
    /// 还原所有子物体的缩放为1
    /// </summary>
    private void OneKeyResetScale()
    {
        if (mTarget != null)
        {
            resultStringBuilder = new StringBuilder();
            for (int i = 0; i < mTarget.transform.childCount; i++)
            {
                var childTs = mTarget.transform.GetChild(i);
                if (childTs.localScale != needResetToThisScale)
                {
                    var oldLocalScale = childTs.localScale;
                    childTs.localScale = needResetToThisScale;
                    resultStringBuilder.AppendLine(childTs.name + " 旧:" + oldLocalScale + " 新:" + childTs.localScale);
                }
            }
        }
    }
    #endregion
    #endregion

    #region 辅助
    void DrawButton(string cName, Action cClickAction, int cWidth, int cHeight = 50)
    {
        if (GUILayout.Button(cName, GUILayout.Width(cWidth), GUILayout.Height(cHeight)))
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

    //原始名字字典
    Dictionary<UIWidget, string> originalNameDic = new Dictionary<UIWidget, string>();
    //原始位置字典
    Dictionary<UIWidget, Vector2> originalTsDic = new Dictionary<UIWidget, Vector2>();
    /// <summary>
    /// 绘单个Item
    /// </summary>
    /// <param name="widget"></param>
    /// <param name="toggle"></param>
    void DrawToggleItem(UIWidget widget, bool toggle)
    {
        var bIsToggle = EditorGUILayout.BeginToggleGroup(widget.name, toggle);
        widget.gameObject.SetActive(bIsToggle);
        DrawHorizontal(() =>
        {
            if (isNeedChangeName)
            {
                var widgetName = EditorGUILayout.TextField("name:", widget.name);
                widget.name = widgetName;
                if (!originalNameDic.ContainsKey(widget)) originalNameDic.Add(widget, widget.name);
                DrawButton("还原", () =>
                {
                    if (originalNameDic.ContainsKey(widget))
                        widget.name = originalNameDic[widget];
                }, 50, 20);
            }
        });
        DrawHorizontal(() =>
        {
            if (isNeedChangeTs)
            {
                var widgetTs = EditorGUILayout.Vector2Field("Transform:", new Vector2(widget.transform.localPosition.x, widget.transform.localPosition.y));
                widget.transform.localPosition = widgetTs;
                if (!originalTsDic.ContainsKey(widget)) originalTsDic.Add(widget, widgetTs);
                DrawButton("还原", () =>
                {
                    if (originalTsDic.ContainsKey(widget))
                    {
                        widget.transform.localPosition = originalTsDic[widget];
                    }
                }, 50, 20);
            }
        });
        EditorGUILayout.EndToggleGroup();
    }
    #endregion

    protected virtual Rect BeginVertical(params GUILayoutOption[] options)
    {
        return EditorGUILayout.BeginVertical("ProgressBarBack", options);
    }

    protected virtual Rect BeginVertical(string style = "", params GUILayoutOption[] options)
    {
        return EditorGUILayout.BeginVertical(style, options);
    }
}

