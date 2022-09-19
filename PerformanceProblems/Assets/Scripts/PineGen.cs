using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PineGen : MonoBehaviour
{
    public Vector3 center;
    public GameObject pinePrefab;
    [SerializeField] private int nPines = 5000; // 5000
    private GameObject[] pine;
    [SerializeField] private bool staticBatching = false;

    [ContextMenu("Kill trees")]
    private void DeleteTrees()
    {
        GameObject[] allChildren = new GameObject[transform.childCount];

        int i = 0;
        //Find all child obj and store to that array
        foreach (Transform child in transform)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }

        foreach (GameObject child in allChildren)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    [ContextMenu("Generate trees")]
    private void GenerateTrees()
    {
        DeleteTrees();

        float tx, ty;

        pine = new GameObject[nPines];

        for (int i = 0; i < nPines; i++)
        {
            tx = Random.Range(-4.9f, 4.9f);
            ty = Random.Range(-4.9f, 4.9f);
            pine[i] = Instantiate(pinePrefab, new Vector3(tx, 0, ty) + center, Quaternion.identity, transform);
            pine[i].transform.localScale *= Random.Range(.5f, 1.5f);
        }
        if (staticBatching)
        {
            StaticBatchingUtility.Combine(pine, gameObject);
        }
    }
}