using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class RainingObject : MonoBehaviour
{
    [SerializeField] private RainObject prefab;
    [SerializeField] public float spawnRate = 0.5f;
    [SerializeField] private int poolSize = 3;

    public bool isActive { get; private set; } = false;

    private List<GameObject> objectPool = new List<GameObject>();
    private List<GameObject> objectInUse = new List<GameObject>();

    private Coroutine spawnCoroutine;

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        int newObj = poolSize - (objectInUse.Count + objectPool.Count);
        for (int i = 0; i < newObj; i++)
        {
            GameObject obj = Instantiate(prefab.gameObject, transform);
            obj.GetComponent<RainObject>().RainingObject = this;
            obj.SetActive(false);
            objectPool.Add(obj);
        }
    }

    private GameObject GetObjectFromPool()
    {
        if (objectPool.Count > 0)
        {
            GameObject obj = objectPool[0];
            objectPool.RemoveAt(0);
            obj.SetActive(true);
            objectInUse.Add(obj);
            return obj;
        }
        else if (objectInUse.Count > 0)
        {
            GameObject obj = objectInUse[0];
            objectInUse.RemoveAt(0);
            obj.SetActive(true);
            objectInUse.Add(obj);
            return obj;
        }
        return null;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        if (obj != null)
        {
            if (objectInUse.Contains(obj))
            {
                objectInUse.Remove(obj);
            }
            obj.SetActive(false);
            objectPool.Add(obj);
        }
    }

    public void StartRainingObjects(float minX = 0, float maxX = 0, float minY = 0, float maxY = 0)
    {
        isActive = true;
        spawnCoroutine = StartCoroutine(SpawnObjectsContinuously( minX, maxX, minY, maxY));
    }

    public void StopRainingObjects()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        isActive = false;
    }

    public void ClearFallenObjects()
    {
        while (objectInUse.Count > 0)
        {
            GameObject obj = objectInUse[0];
            objectInUse.RemoveAt(0);
            obj.SetActive(false);
            objectPool.Add(obj);
        }
    }

    public void setSpawnRate(float rate)
    {
        spawnRate = rate;
    }

    private IEnumerator SpawnObjectsContinuously(float minX = 0, float maxX = 0, float minY = 0, float maxY = 0)
    {
        SpawnObjectInArea(minX, maxX, minY, maxY);
        while (objectInUse.Count > 0)
        {
            SpawnObjectInArea(minX, maxX, minY, maxY);
            yield return new WaitForSeconds(spawnRate);
        }
        isActive = false;
    }

    public void RespawnObject(GameObject obj)
    {
        if (objectInUse.Contains(obj))
        {
            objectInUse.Remove(obj);
            obj.SetActive(false);
            SpawnObjectInArea();
        }
    }

    public void SetPoolSize(int size)
    {
        poolSize = size;
        InitializePool();
    }

    public void SpawnObjectInArea(float minX = 0 , float maxX = 0, float minY = 0, float maxY = 0)
    {
        GameObject obj = GetObjectFromPool();
        if (obj != null)
        {
            obj.transform.localPosition = GetRandomPosition(minX, maxX, minY, maxY);
            obj.GetComponent<RainObject>().OnSpawn();
        }
    }

    private Vector2 GetRandomPosition(float minX = 0, float maxX = 0, float minY = 0, float maxY = 0)
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        return new Vector2(x, y);
    }

}