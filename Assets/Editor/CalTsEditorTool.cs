using UnityEngine;
using System.Collections;
using UnityEditor;

public class CalTsEditorTool : EditorWindow
{

    [MenuItem("Tools/局部位置计算器 &%C")]
    public static void ShowWindow()
    {
        CalTsEditorTool callTsTool = (CalTsEditorTool)GetWindow(typeof(CalTsEditorTool));
        callTsTool.Show();
    }




    void OnGUI()
    {
        ParenTf = EditorGUILayout.ObjectField("父对象：", ParenTf, typeof(Transform), true, GUILayout.ExpandWidth(true)) as Transform;
        if (ParenTf != null)
        {
            EditorGUILayout.Vector3Field("世界坐标：", ParenTf.position);
            EditorGUILayout.Vector3Field("局部坐标：", ParenTf.localPosition);
        }

        ChildWroldTf = EditorGUILayout.ObjectField("子对象：", ChildWroldTf, typeof(Transform), true, GUILayout.ExpandWidth(true)) as Transform;
        if (ChildWroldTf != null)
        {
            EditorGUILayout.Vector3Field("世界坐标：", ChildWroldTf.position);
            EditorGUILayout.Vector3Field("局部坐标：", ChildWroldTf.localPosition);
        }

        DrawGuiHelper.DrawButton("子->父（局标）：", () =>
        {
            GetLocalPos();

        });
        childLocalParentPos = EditorGUILayout.Vector3Field("子转父局部坐标：", childLocalParentPos);

    }


    public Transform ParenTf;
    public Transform ChildWroldTf;
    private Vector3 childLocalParentPos;
    private void GetLocalPos()
    {
        if (ParenTf == null) return;
        if (ChildWroldTf == null) return;
        //InverseTransformPoint 获得ChildWroldTf 作为 ParenTf 子物体时的局部坐标
        childLocalParentPos = ParenTf.InverseTransformPoint(ChildWroldTf.position);
        //Debug.LogError(string.Format("world:{0}\n ,curLocal:{1}\n,as Child local:{2}", ChildWroldTf.position, ChildWroldTf.localPosition, childLocalParentPos));

    }


}
