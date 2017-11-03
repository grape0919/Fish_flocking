using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public CameraControl cameraControl; //카메라
    public Object fishPrefab;           //물고기 오브젝트
    public Transform fishParent;        //보이드 위치
    private Transform cameraObj;        //카메라 오브젝트
    public Transform instancePoints;
    Boid[] boids;

    public class BoidSettingsEx : Boid.Settings
    {
        public float FishsCount = 200;  //보이드 객체 수
    }

    [System.Serializable]
    public class Settings
    {
        public List<BoidSettingsEx> boidSettings = new List<BoidSettingsEx>();
    }
    [SerializeField]
    private Settings settings = new Settings();
    static private Settings globalSettings;

    void Start()
    {
        if (globalSettings == null)
        {
            settings = LoadSettings();
            globalSettings = settings;
        }
        else
            settings = globalSettings;
        InstantiateFishs();
    }

    void InstantiateFishs()
    {
        int num = 0;
        Transform ip = instancePoints;
        BoidSettingsEx sts = settings.boidSettings[0];

        boids = new Boid[(int)sts.FishsCount];
        sts.Trace = ip.GetComponent<Trace>();

        const float size = 0.1f;

        //카메라에 잡히는 boid 생성
        cameraObj = InstantiateFish(ip.position, ip.rotation, sts, num).transform;
        cameraControl.Target = cameraObj;

        MathTools.FillSquareUniform(sts.FishsCount, delegate (int x, int y)
        {
            if (x != 0 || y != 0)
                InstantiateFish(
                  cameraObj.position + new Vector3(size * x, size * y, Random.Range(-size, size)),
                  MathTools.RandomYawPitchRotation(),
                  sts,
                  num
                );
        });
    }
  

    Settings LoadSettings()
    {
        Settings res;
        
        res = settings;
        
            res.boidSettings.Add(new BoidSettingsEx());

        return res;
    }

    public static Boid.Settings GetSettings(GameObject obj)
    {
        Main main = Camera.main.GetComponent<Main>();
        return main.settings.boidSettings[0];
    }

    private GameObject InstantiateFish(Vector3 position, Quaternion rotation, Boid.Settings boidSettings, int number)
    {
        GameObject obj = (GameObject)Instantiate(fishPrefab, position, rotation);
        boids[number] = obj.GetComponent<Boid>();

        obj.transform.parent = fishParent;
        boids[number].SettingsRef = boidSettings;

        number++;

        return obj;
    }
}
