using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {
    public class Settings
    {
        public float SpeedMultipliyer = 12.0f;      //속도
        public float ViewRadius = 0.2f;             //시야
        public float OptDistance = 0.1f;            //보이드간의 거리
        public float MinSpeed { get { return 0.1f * SpeedMultipliyer; } }   //최소 속도
        public float InclineFactor { get { return 360f / SpeedMultipliyer; } }    //경사요소
        public float AlignmentForcePart = 0.0003f;    //정렬
        public float TotalForceMultipliyer = 1;
        public float Inertness = 0.1f;
        public float AttractrionForce = 0.2463f;       //인력

        public Trace Trace { get; set; }
    }

    private Settings sts = null;
    public Settings SettingsRef
    {
        get { return sts; }
        set { sts = value; }
    }

    private Vector3 velocity = Vector3.zero;
    public Vector3 Velocity { get { return velocity; } }

    void Start()
    {
        if (sts == null)
        {
            sts = Main.GetSettings(gameObject);

            if (sts == null)
                sts = new Settings();
        }
        
    }
    float ranum = 1;

    void FixedUpdate()
    {
        BoidTools.SeparationForce sepForce = new BoidTools.SeparationForce(sts);    //분산 값 계산
        BoidTools.CollisionAvoidanceForce collAvoid = new BoidTools.CollisionAvoidanceForce(sts, sepForce.Calc(sts.OptDistance));   //회피 계산

        if(Random.Range(0, 100f) > 99)
        {
            ranum = Mathf.Clamp(Random.Range(ranum - 0.2f, ranum + 0.2f), 0.2f, 1);
        }

        velocity = Vector3.zero;
        Vector3 centeroid = Vector3.zero;   //자신 근처 보이드들의 중심

        Vector3 collisionAvoidance = Vector3.zero;  //장애물 회피 백터값
        Vector3 avgSpeed = Vector3.zero;            //근처 보이드들의 평균 속도
        int neighbourCount = 0;                     //근처 보이드들의 수 카운팅

        Vector3 direction = transform.rotation * Vector3.forward;   //현재 바라보고 있는 방향(0,0,1)
        Vector3 curPos = transform.position; //현재 포지션            (0,0,0)

        //curPos를 중심으로 sts.ViewRadius크기의 반지름을 가진 구 안에 있는 (collider을 포함한)오브젝트를 감지합니다.
        foreach (var vis in Physics.OverlapSphere(curPos, sts.ViewRadius))  //가상의 트리거 구를 만들어서 그안에있는 오브젝트 검색
        {
            Vector3 visPos = vis.transform.position;    //검색한 오브젝트의 포지션 (0.3,0.3,0.3)
            Boid boid;                                  //보이드 객체

            if ((boid = vis.GetComponent<Boid>()) != null) //객체에 boid스크립트 검색
            {
                Vector3 separationForce;    //분산백터값

                if (!sepForce.Calc(curPos, visPos, out separationForce))//(0,0,0), (0.3,0.3,0.3)
                    continue;

                collisionAvoidance += separationForce;  //분리백터값 (-0.02,-0.02,-0.02) 
                ++neighbourCount;           //근처 보이드의 수       (1) 
                //centeroid += CalcCenteroid(sts, curPos, visPos);
                centeroid += visPos;        //근처 보이드의 중심값   (0.3,0.3,0.3)
                avgSpeed += boid.velocity;  //근처 보이드의 평균 속도(0,0,0)
            }
            else //발견한 객체에 boid스크립트가 없으면 장애물로 인식
            {
                BoidTools.CollisionAvoidanceForce.Force force;
                if (collAvoid.Calc(curPos, direction, vis, out force))
                {
                    collisionAvoidance += force.dir;     //회피백터값   ((-1.8,-1.8,-1.8)            
                }
            }
            
        }

        if (neighbourCount > 0)
        {
            //중심값
            centeroid = ranum * (centeroid / neighbourCount- curPos);   //(0.3,0.3,0.3) / 1 - (0,0,0) = (0.3,0.3,0.3)

            //근처 보이드들의 속도의 평균
            avgSpeed = avgSpeed / neighbourCount - velocity;    //(0,0,0) / 1 - (0,0,0)
        }

        Vector3 positionForce = (1.0f - sts.AlignmentForcePart) * sts.SpeedMultipliyer * (centeroid + collisionAvoidance);  //분산과 회피(1 - 0.01) * 12 * ((0.3,0.3,0.3) + (-0.02,-0.02,-0.02)) = 0.99 * 12 * (0.28,0.28,0.28) = (3.3,3.3,3.3)
                                                                                                                            // (1 - 0.01) * 12 * ((0,0,0) + (-1.8,-1.8,-1.8)) = (-28,-28,-28)
        Vector3 alignmentForce = sts.AlignmentForcePart * avgSpeed / Time.deltaTime;    //정렬과 응집 (0,0,0)
        Vector3 attractionForce = ranum * (CalculateAttractionForce(sts, curPos, velocity));      //목적지   (1.2,1.2,1.2)
        Vector3 totalForce = sts.TotalForceMultipliyer * (positionForce + alignmentForce + attractionForce);        //위 셋을 더함   (3.3,3.3,3.3) + (0,0,0) + (1.2,1.2,1.2) = (4.5,4.5,4.5)
        Vector3 newVelocity = (1 - sts.Inertness) * (totalForce * Time.deltaTime) + sts.Inertness * velocity;   //(1-0.5) * ((4.5,4.5,4.5)*0.02) +0.5 * (0,0,0) = (0.045,0.045,0.045)
        
        velocity = ranum * CalcNewVelocity(sts.MinSpeed, velocity, newVelocity, direction); //매개변수 최저속도, 크기값, 새로운 크기값, 방향   1.2, (0,0,0), (0.045,0.045,0.045), (0,0,1) = (3,3,3)
        
        Quaternion rotation = CalcRotation(sts.InclineFactor, velocity, totalForce);

        if (MathTools.IsValid(rotation))
            gameObject.transform.rotation = rotation;
            
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }

    static Vector3 CalcCenteroid(Settings sts, Vector3 curPos, Vector3 dsrPos)
    {
        Vector3 revDir = curPos - dsrPos;

        float dis = revDir.sqrMagnitude;
        float factor = 1 - dis / (sts.ViewRadius * sts.ViewRadius);
        factor = factor * factor + 0.4f;

        if (factor > 1.5f)
            factor = 1.5f;

        return revDir * factor;
    }

    static Vector3 CalculateAttractionForce(Settings sts, Vector3 curPos, Vector3 curVelocity)  //매개변수 셋팅값, 자기포지션, 자기이동방향및속도
    {
        if (!sts.Trace)
            return Vector3.zero;

        Vector3 attrPos = sts.Trace.GetAtractionPoint();    //목적지   (2,2,2)
        Vector3 direction = (attrPos - curPos).normalized;  //목적지 - 현재위치 = 방향   (2,2,2) - (0,0,0) = (0.6,0.6,0.6)
        
        float factor = sts.AttractrionForce * sts.SpeedMultipliyer * MathTools.AngleToFactor(direction, curVelocity);       //0.2 * 12 * 0.84 = 2
        
        return factor * direction; //(1.2,1.2,1.2)
    }

    static Vector3 CalcNewVelocity(float minSpeed, Vector3 curVel, Vector3 dsrVel, Vector3 defaultVelocity) //1.2, (0,0,0), (0.045,0.045,0.045), (0,0,1)
    {
        float curVelLen = curVel.magnitude; //0
        
        if (curVelLen > MathTools.sqrEpsilon)
            curVel /= curVelLen;        //(0,0,0)
        else
        {
            curVel = defaultVelocity; //(0,0,1)
            curVelLen = 1;              
        }

        float dsrVelLen = dsrVel.magnitude; //0.17    0.006
        float resultLen = minSpeed;     //1.2
        
        if (dsrVelLen > MathTools.sqrEpsilon)
        {
            dsrVel /= dsrVelLen;    //(7.5,7.5,7.5)
            
            float angleFactor = MathTools.AngleToFactor(dsrVel, curVel);   //0.3
            float rotReqLength = 2 * curVelLen * angleFactor;   //2 * 1 * 0.3 = 0.6
            
            float speedRest = dsrVelLen - rotReqLength; //1 - 0.6 = 0.4
            
            if (speedRest > 0)
            {
                curVel = dsrVel;    //(7.5,7.5,7.5)
                resultLen = speedRest;  //0.4
                
            }
            else
            {
                curVel = Vector3.Slerp(curVel, dsrVel, dsrVelLen / rotReqLength);   //선형보간 방향값에대한 보간
            }
            
            if (resultLen < minSpeed)
                resultLen = minSpeed;
        }
        
        return curVel * resultLen;  //(3,3,3)
    }

    static Quaternion CalcRotation(float inclineFactor, Vector3 velocity, Vector3 totalForce)   //경사요소, 
    {
        if (velocity.sqrMagnitude < MathTools.sqrEpsilon)
            return new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);

        Vector3 rightVec = MathTools.RightVectorXZProjected(velocity);      //y축을 기준으로 회전
        float inclineDeg = MathTools.VecProjectedLength(totalForce, rightVec) * -inclineFactor;
        return Quaternion.LookRotation(velocity) * Quaternion.AngleAxis(Mathf.Clamp(inclineDeg, -50, 50), Vector3.forward);   //z축을 기준으로 회전 기울어지는 효과
    }

    /*void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 0.8f);
        Debug.DrawRay(transform.position, velocity, Color.red);
    }*/
}
