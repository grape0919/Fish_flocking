using UnityEngine;

public class BoidTools
{
    //분산에 필요한 힘
    public struct SeparationForce
    {
        public SeparationForce(Boid.Settings sts)
        {
            optFactor = sts.OptDistance * sts.OptDistance / 2;  //지정해둔 보이드 간의 거리 최대거리^2/2 (0.02)
        }

        public bool Calc(Vector3 cur, Vector3 other, out Vector3 force)
        {
            Vector3 revDir = cur - other;           //자신의 포지션과 상대의 포지션 차(-0.3,-0.3,-0.3)
            float sqrDist = revDir.sqrMagnitude;    //sqrMagnitude = 벡터의 길이를 제곱한 값, 두점간의 거리 비교(0.27)

            force = Vector3.zero;                   //리턴받을 값

            if (sqrDist < MathTools.sqrEpsilon)     //부동소수점 오류로 인해 사용
                return false;
            
            force = revDir * (optFactor / sqrDist); //(-0.3,-0.3,-0.3) * (0.02 / 0.27) = (-0.02,-0.02,-0.02)

            return true;
        }

        public float Calc(float dist)
        {
            return optFactor / dist;
        }

        readonly float optFactor;
    };

    //장애물 피하기 위한 힘
    public struct CollisionAvoidanceForce
    {
        public CollisionAvoidanceForce(Boid.Settings sts, float sepForceAtOptDistance)
        {
            optDistance = sts.OptDistance;  //0.2

            factor1 = sts.ViewRadius;       //0.8
            factor2 = -2 * sts.SpeedMultipliyer * sepForceAtOptDistance * sts.OptDistance / (sts.OptDistance - sts.ViewRadius); //-2 * 12 * 0.1 * 0.2 / (0.2-0.8) = 0.8
        }
        public struct Force
        {
            public Vector3 dir; //방향
            public Vector3 pos; //속력
        };

        public bool Calc(Vector3 cur, Vector3 fishDir, Collider cld, out Force force)   //(0,0,0), (0,0,1), (0.3,0.3,0.3)
        {
            Vector3 pointOnBounds = MathTools.CalcPointOnBounds(cld, cur);  //장애물을 기준으로 자신의 위치(0.1,0.1,0.1)
            Vector3 revDir = cur - pointOnBounds;   //장애물과 자신의 거리 (-0.1,-0.1,-0.1)
            float dist = revDir.magnitude;  //(0,0,0)과 revDir 과의 거리 0.17

            if (dist <= MathTools.sqrEpsilon)
            {
                revDir = (pointOnBounds - cld.transform.position).normalized; //방향을 나타내기 위함
                
                dist = 0.1f * optDistance;
            }
            else
                revDir /= dist; //(-0.6,-0.6,-0.6)
            force.dir = revDir * (CalcImpl(dist) * MathTools.AngleToFactor(revDir, fishDir));   //(-0.6,-0.6,-0.6) * 3 * 1 = (-1.8,-1.8,-1.8)
            force.pos = pointOnBounds;  //(0.1,0.1,0.1)
            return true;
        }

 
        float CalcImpl(float dist)
        {
            return factor2 * (factor1 / dist - 1);  //0.8 * (0.8 / 0.17 -1) = 3
        }

        delegate float ForceDlg(float dist);
        readonly float factor1;
        readonly float factor2;
        readonly float optDistance;
    };
}