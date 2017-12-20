using System.Collections;
using UnityEngine;
using UnityEngine.UI;
 
public class ColliderImage : Image {
    private Collider2D _collider;

    private static Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, 90.0f));
	
    void Awake() {
        _collider = GetComponent<Collider2D>();
    }
 
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
        return _collider.OverlapPoint(screenPoint);
    } 
}
