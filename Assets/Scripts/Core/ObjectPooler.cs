using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [HideInInspector] public List<IPooledObject> poolAvailable = new List<IPooledObject>();
    [SerializeField] GameObject pooledObject;
    [SerializeField] int poolStartingSize;

    void Awake()
    {
        for (int i = 0; i < poolStartingSize; i++)
        {
            IPooledObject newObject = Instantiate(pooledObject, transform).GetComponent<IPooledObject>();
            newObject.Pooler = this;
            newObject.Depool();
        }
    }

    public IPooledObject Pool(Vector3 position, Quaternion rotation)
    {
        IPooledObject newObject;
        if (poolAvailable.Count > 0)
        {
            poolAvailable.Remove(newObject = poolAvailable[0]);
        }
        else
        {
            newObject = Instantiate(pooledObject, transform).GetComponent<IPooledObject>();
            newObject.Pooler = this;
        }
        newObject.Pool(position, rotation);
        return newObject;
    }
    
    public interface IPooledObject
    {
        public ObjectPooler Pooler { get; set; }

        public abstract void Pool(Vector3 position, Quaternion rotation);

        public abstract void Depool();
    }
}
