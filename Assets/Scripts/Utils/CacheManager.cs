using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CacheManager : Singleton<CacheManager> {

    private Dictionary<string, object> _objectDict = new Dictionary<string, object>();

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public bool HasObject(string name) {
        return _objectDict.ContainsKey(name);
    }

    public T GetObject<T>(string name) {
        if (_objectDict.ContainsKey(name)) {
            return (T)_objectDict[name];
        }
        return default(T);
    }

    public void SetObject(string name, object obj) {
        if (!_objectDict.ContainsKey(name)) {
            _objectDict.Add(name, obj);
        }
    }

    public void RemoveObject(string name) {
        if (!_objectDict.ContainsKey(name)) {
            _objectDict.Remove(name);
        }
    }
}
