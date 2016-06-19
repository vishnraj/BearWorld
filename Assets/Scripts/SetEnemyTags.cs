using UnityEngine;
using System.Collections;

public class SetEnemyTags : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        SetChildrenTags(transform);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetChildrenTags(Transform parent)
    {
        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform child = parent.GetChild(i);
            child.tag = "Enemy";

            if (child.childCount > 0)
            {
                SetChildrenTags(child);
            }
        }
    }
}
