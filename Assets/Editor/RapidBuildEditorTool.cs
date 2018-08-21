using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;

public class RapidBuildEditorTool : EditorWindow
{
    private static RapidBuildEditorTool cWin;

    [MenuItem("Tools/拼UI神器 &#Q")]
    static void ShowWindow()
    {
        cWin = CreateInstance<RapidBuildEditorTool>();
        cWin.Show();
        cWin.Init();
    }

    private void Init()
    {
        InitAlwaysUseAtlas();
    }
    void OnDestroy()
    {
        SaveAltasSetting();
    }
    /// <summary>
    /// 修改对象
    /// </summary>
    private GameObject mTarget;
    private bool lblFoldout;
    private bool SpriteFoldout;
    //选中图集下标
    private int UseChoiceAtlasIndex;
    void OnGUI()
    {
        EditorGUILayout.HelpBox("创建对象:", MessageType.Info, true);
        DrawVertical(() =>
        {
            DrawButton("创建按钮", CreateButton);
            isNeedScale = GUILayout.Toggle(isNeedScale, "点击需要缩放效果");
        }, "Box");

        //常用图集
        ShowComAtlas();
        if (totalAtlasNum > 0)
            //DrawHorizontal(() =>
            //{
            //    DrawButton(allSelectState ? "取消全选" : "全部选中", CancleAllAtlasSelect);
            //    DrawButton("选中&&最后的", UseLastSelectAltas);
            //});

            EditorGUILayout.HelpBox("针对此对象的子物体修改:", MessageType.Info);
        DrawVertical(() =>
        {
            if (mTarget == null) mTarget = Selection.activeGameObject;
            mTarget = EditorGUILayout.ObjectField("修改对象：", mTarget, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
            DrawButton("保存修改", ApplyChangeAndSave);

            if (mTarget != null)
            {
                DrawHorizontal(() =>
                {
                    isNeedChangeName = GUILayout.Toggle(isNeedChangeName, "修改名字");
                    isNeedChangeTs = GUILayout.Toggle(isNeedChangeTs, "修改位置");
                });
                isOnlyShowDepthBelowTen = GUILayout.Toggle(isOnlyShowDepthBelowTen, "只显示层级低于10");

                DrawButton("一键修正缩放", OneKeyResetScale);
                if (resultStringBuilder != null && resultStringBuilder.Length > 0)
                    GUILayout.TextArea(resultStringBuilder.ToString());

                DrawHorizontal(() =>
                {
                    DrawButton("获取Label", GetLabels);
                    DrawButton("获取所有Sprite", GetSprites);
                    DrawButton("获取所有Texture", GetTextures);
                });

                //labels
                ShowLabels();
                //Sprites
                ShowSprites();
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
    #region 常用图集
    private bool commonAtlasFoldout;
    private List<UIAtlas> commonAtlasList = new List<UIAtlas>();
    private int totalAtlasNum = 0;
    private UIAtlas selectAtlas;
    private void ShowComAtlas()
    {
        DrawHorizontal(() =>
        {
            commonAtlasFoldout = EditorGUILayout.Foldout(commonAtlasFoldout, "常用图集：");
            EditorGUILayout.LabelField("Size:", GUILayout.Width(90));
            totalAtlasNum = EditorGUILayout.IntField(totalAtlasNum);
        });

        commonAtlasFoldout = totalAtlasNum > 0;

        if (commonAtlasFoldout)
        {
            DrawVertical(() =>
            {
                for (int i = 0; i < totalAtlasNum; i++)
                {
                    DrawAtlasItem(i);
                }
            });
        }
    }

    /// <summary>
    /// 初始化显示常用图集
    /// </summary>
    private void InitAlwaysUseAtlas()
    {
        var allAtlasStr = EditorPrefs.GetString(GetType().Name + "commonAtlasDicValue");
        var itemAtlasArr = allAtlasStr.Split(',');
        for (int i = 0; i < itemAtlasArr.Length; i++)
        {
            var atlasGo = AssetDatabase.LoadMainAssetAtPath(itemAtlasArr[i]) as GameObject;
            if (atlasGo != null)
            {
                var atlas = atlasGo.GetComponent<UIAtlas>();
                if (atlas != null) commonAtlasList.Add(atlas);
            }
        }
        totalAtlasNum = commonAtlasList.Count;
    }
    #region 保存常用图集配置
    private void SaveAltasSetting()
    {
        List<string> atlasList = new List<string>(commonAtlasList.Count);
        string itemAtlasPath;
        foreach (var item in commonAtlasList)
        {
            itemAtlasPath = AssetDatabase.GetAssetPath(item);
            if (!string.IsNullOrEmpty(itemAtlasPath))
                atlasList.Add(itemAtlasPath);
        }

        var atlasArr = atlasList.ToArray();
        var commonAtlasDicValue = string.Join(",", atlasArr);

        Debug.LogError(commonAtlasDicValue);
        EditorPrefs.SetString(GetType().Name + "commonAtlasDicValue", commonAtlasDicValue);
    }
    #endregion
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
                        DrawToggleLabelItem(itemLbl, itemLbl.gameObject.activeSelf);
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

    private void ShowSprites()
    {
        if (childSpList != null && childSpList.Count != 0)
        {
            DrawButton("一键修改使用图集", OneKeyChangeAtlas);

            SpriteFoldout = EditorGUILayout.Foldout(SpriteFoldout, "Sprites");
            if (SpriteFoldout)
            {
                DrawVertical(() =>
                {
                    foreach (var item in childSpList)
                    {
                        DrawToggleSpriteItem(item, item.gameObject.activeSelf);
                    }
                }, "Box");
            }
        }

    }

    private void OneKeyChangeAtlas()
    {
        if (selectAtlas != null)
        {
            foreach (var item in childSpList)
            {
                item.atlas = selectAtlas;
            }
        }
        else
        {
            ShowNotification(new GUIContent("当前未选中图集"));
        }
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
    void DrawButton(string cName, Action cClickAction, int cWidth = 100, int cHeight = 50)
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
    void DrawVertical(Action cAction, params GUILayoutOption[] cOptions)
    {
        if (cAction != null)
        {
            EditorGUILayout.BeginVertical(cOptions);
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
    /// 绘单个LabelItem
    /// </summary>
    /// <param name="widget"></param>
    /// <param name="toggle"></param>
    void DrawToggleLabelItem(UIWidget widget, bool toggle)
    {
        var bIsToggle = EditorGUILayout.BeginToggleGroup(widget.name, toggle);
        if (bIsToggle != widget.gameObject.activeSelf) Selection.activeGameObject = widget.gameObject;

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

    void DrawToggleSpriteItem(UISprite sprite, bool toggle)
    {
        var bIsToggle = EditorGUILayout.BeginToggleGroup(sprite.name, toggle);
        if (bIsToggle != sprite.gameObject.activeSelf) Selection.activeGameObject = sprite.gameObject;
        sprite.gameObject.SetActive(bIsToggle);
        var usingAtlas = EditorGUILayout.ObjectField("Atlas：", sprite.atlas, typeof(UIAtlas), true, GUILayout.ExpandWidth(true)) as UIAtlas;

        DrawButton("更改", () =>
        {
            sprite.atlas = selectAtlas;

        }, 50, 20);


        //DrawButton("输出颜色", () =>
        //{
        //    var RgbColor = sprite.color;
        //    var HSBColor = RGBConvertToHSV(RgbColor);
        //    Debug.LogError(string.Format("RGB:{0},HSV{1}", sprite.color, HSBColor));
        //}, 50, 20);

        EditorGUILayout.EndToggleGroup();

    }
    /// <summary>
    /// 绘单个常用图集
    /// </summary>
    /// <param name="index"></param>
    void DrawAtlasItem(int index)
    {
        UIAtlas atlasItem = null;
        DrawHorizontal(() =>
        {
            if (index < commonAtlasList.Count)
            {
                GUI.backgroundColor = selectAtlas == commonAtlasList[index] ? new Color(0.8f, 0.8f, 0.8f) : Color.white;
                commonAtlasList[index] = EditorGUILayout.ObjectField("常用：" + index, commonAtlasList[index], typeof(UIAtlas), true, GUILayout.ExpandWidth(true)) as UIAtlas;
                atlasItem = commonAtlasList[index];

            }
            else
            {
                atlasItem = EditorGUILayout.ObjectField("常用：" + index, atlasItem, typeof(UIAtlas), true, GUILayout.ExpandWidth(true)) as UIAtlas;
                commonAtlasList.Add(atlasItem);
            }

            if (atlasItem != null)
                DrawButton("选中", () =>
                {
                    selectAtlas = atlasItem;
                }, 50, 20);
            GUI.backgroundColor = Color.white;
        });

    }
    #endregion
    #region 颜色
    //RGB to HSV
    Vector3 RGBConvertToHSV(Color rgb)
    {
        float R = rgb.r;
        float G = rgb.g;
        float B = rgb.b;
        Vector3 hsv = Vector3.zero;
        float max1 = Mathf.Max(R, Mathf.Max(G, B));
        float min1 = Mathf.Min(R, Mathf.Min(G, B));
        if (Math.Abs(max1 - min1) > 0)
        {
            if (R == max1)
            {
                hsv.x = (G - B) / (max1 - min1);
            }
            if (G == max1)
            {
                hsv.x = 2 + (B - R) / (max1 - min1);
            }
            if (B == max1)
            {
                hsv.x = 4 + (R - G) / (max1 - min1);
            }
        }

        hsv.x = hsv.x * 60.0f;
        if (hsv.x < 0)
            hsv.x = hsv.x + 360;
        hsv.z = max1;
        hsv.y = (max1 - min1) / max1;
        return hsv;
    }
    #endregion

    #region 保存预设
    //[InitializeOnLoadMethod]
    //static void StartInitializeOnLoadMethod()
    //{
    //if (cWin == null) PrefabUtility.prefabInstanceUpdated = null;
    //PrefabUtility.prefabInstanceUpdated = delegate (GameObject instance)
    //{
    //        //prefab保存的路径
    //        Debug.Log(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(instance)));
    //};
    //}

    //参考自http://www.hiwrz.com/2016/06/06/unity/216/
    /// <summary>
    /// 应用预设且保存unity
    /// </summary>
    private void ApplyChangeAndSave()
    {
        if (mTarget == null) return;
        PrefabType pType = PrefabUtility.GetPrefabType(mTarget);
        if (pType != PrefabType.PrefabInstance) return;
        GameObject prefabGo = GetPrefabInstanceParent(mTarget);
        UnityEngine.Object prefabAsset = null;
        if (prefabGo != null)
        {
            prefabAsset = PrefabUtility.GetPrefabParent(prefabGo);
            if (prefabAsset != null)
                PrefabUtility.ReplacePrefab(prefabGo, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
        }
        AssetDatabase.SaveAssets();
    }
    /// <summary>
    /// 遍历获取prefab节点所在的根prefab节点
    /// </summary>
    static GameObject GetPrefabInstanceParent(GameObject go)
    {
        if (go == null)
        {
            return null;
        }
        PrefabType pType = EditorUtility.GetPrefabType(go);
        if (pType != PrefabType.PrefabInstance)
        {
            return null;
        }
        if (go.transform.parent == null)
        {
            return go;
        }
        pType = EditorUtility.GetPrefabType(go.transform.parent.gameObject);
        if (pType != PrefabType.PrefabInstance)
        {
            return go;
        }
        return GetPrefabInstanceParent(go.transform.parent.gameObject);
    }
    #endregion


}

