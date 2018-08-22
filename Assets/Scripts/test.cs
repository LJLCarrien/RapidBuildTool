using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class test : MonoBehaviour
{
    List<int> intList = new List<int>();
    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ContextMenu("test2")]
    private void test2()
    {
        for (int i = 0; i < 5; i++)
        {
            intList.Add(i);
        }
        foreach (var i in intList)
        {
            Debug.LogError(i);
        }
    }
}
