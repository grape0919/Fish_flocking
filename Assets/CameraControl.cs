using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [Serializable]
    public class Settings
    {
        public Vector3 speed = new Vector3(-2.4f, 5.0f, 2.0f);
        public Vector3 rotation;
        public float keyFactor = 1.0f;

        public float distance = 2.0f;
        public float minDistance = 0.2f;

        public float xMinLimit = -90;
        public float xMaxLimit = 90;
        public bool rotateWithTarget = false;

        public bool isEnabled = true;
        public Vector3 position = Vector3.zero;
        public bool isAttached = true;
    }

    [SerializeField]
    private Settings settings;
    
    static private Settings globalSettings;

    public bool Enabled { get { return settings.isEnabled; } set { settings.isEnabled = value; } }
    public bool Attached { get { return settings.isAttached; } set { settings.isAttached = value; } }
    public Transform Target { get { return target; } set { target = value; } }

    void Start()
    {
        if (globalSettings == null)
        {
            settings.rotation = transform.eulerAngles;

            if (target)
            {
                settings.distance = (transform.position - target.transform.position).magnitude;
                settings.position = target.transform.position;
                settings.isAttached = true;
            }

            globalSettings = settings;
        }
        else
            settings = globalSettings;
        
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    public void ResetStoredSettings()
    {
        globalSettings = null;
    }

    public void CheckForNewTarget(Vector3 mousePos)
    {
        if (settings.isEnabled)
        {
            var ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit[] hits = Physics.RaycastAll(ray, Camera.main.farClipPlane);

            foreach (var hit in hits)
            {
                if (hit.collider is BoxCollider || hit.collider is SphereCollider)
                {
                    target = hit.collider.gameObject.transform;
                    settings.isAttached = true;
                    break;
                }
            }
        }
    }

    private bool IsAttached()
    {
        return target && settings.isAttached;
    }

    private bool IsRotateWithTarget()
    {
        return IsAttached() && settings.rotateWithTarget;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            settings.isAttached = false;

        if (IsAttached())
            settings.position = target.transform.position;

        if (settings.isEnabled)
        {
            if (!Input.GetMouseButton(1))
            {
                settings.rotation.x += Input.GetAxis("Mouse Y") * settings.speed.x;
                settings.rotation.y += Input.GetAxis("Mouse X") * settings.speed.y;
                settings.rotation.x = MathTools.ClampAngle(settings.rotation.x, settings.xMinLimit, settings.xMaxLimit);
            }

            var distRaw = -Input.GetAxis("Mouse ScrollWheel") * settings.speed.z;

            settings.distance = Mathf.Max(settings.distance + distRaw, settings.minDistance);
        }

        if (IsRotateWithTarget())
        {
            settings.rotation = target.rotation.eulerAngles;
            settings.rotation.z = 0;
            settings.rotation.x += 5;
        }

        var quatRot = Quaternion.Euler(settings.rotation);

        transform.rotation = quatRot;
        transform.position = quatRot * new Vector3(0.0f, 0.0f, -settings.distance) + settings.position;
    }
}
