using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathTools
{ 
    public const float sqrEpsilon = 1e-10f; //부동소수점 오류로 인해 사용

    //카메라 앵글 전환 때 사용
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -180)
            angle += 360;

        if (angle > 180)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    //boid자신의 위치에서 이웃하는 boid의 위치 계산
    public static Vector3 CalcPointOnBounds(Collider cld, Vector3 cur)  //매개변수 장애물의 충돌체, 근처 보이드
    {
        SphereCollider sphc = cld as SphereCollider;    //장애물이 구형태의 충돌체가 있는지 확인

        if (!sphc)
        {
            return cld.ClosestPointOnBounds(cur);       //장애물이 구 형태가 아닐 때 (0.1,0.1,0.1)
        }
        else
        {
            Vector3 realPos = sphc.transform.position + sphc.center;    //
            Vector3 dir = cur - realPos;
            Vector3 realScale = sphc.transform.lossyScale;
            float realRadius = sphc.radius * Mathf.Max(realScale.x, realScale.y, realScale.z);
            float dirLength = dir.magnitude;

            //Debug.Log(sphc.transform.position);

            if (dirLength < realRadius)
                return cur;

            float dirFraction = realRadius / dirLength;
            return realPos + dirFraction * dir;
        }
    }

    public static Vector3 RightVectorXZProjected(Vector3 vec)
    {
        //행렬변환, y축으로 회전

        const float sin = 1.0f;
        const float cos = 0.0f;

        return new Vector3(cos * vec.x + sin * vec.z, 0, -sin * vec.x + cos * vec.z);
    }

    public static float VecProjectedLength(Vector3 vec, Vector3 vecNormal)
    {
        Vector3 proj = Vector3.Project(vec, vecNormal); //vec를 cecNormal에 투영한다
        return proj.magnitude * Mathf.Sign(Vector3.Dot(proj, vecNormal));   //sign = sin값(양수 or 0 일때=1 음수일때=0, dot = 스칼라곱
    }


    //두백터의 스칼라곱
    public static float AngleToFactor(Vector3 a, Vector3 b)
    {
        //plot((1-cos(x))/2, x = 0..Pi);
        return (1 - Vector3.Dot(a, b)) / 2;
    }

    //유효함
    public static bool IsValid(Quaternion q)
    {
        #pragma warning disable 1718
        return q == q; //Comparisons to NaN always return false, no matter what the value of the float is.
        #pragma warning restore 1718
    }


    //보이드들을 생성할때 랜덤한 회전값
    public static Quaternion RandomYawPitchRotation()
    {
        return Quaternion.Euler(0, Random.Range(-180, 180), 0);
    }

    public delegate void PlacePoint(int x, int y);

    public static void FillSquareUniform(float totalNumber, PlacePoint dlg) //매개변수 보이드카운트 = 200
    {
        int mainSize = (int)Mathf.Sqrt(totalNumber);        //sqrt(200) = 14

        int lbrd = -mainSize / 2;                           //-7
        int rbrd = lbrd + mainSize;                         //7

        for (int x = lbrd; x < rbrd; ++x)       
            for (int y = lbrd; y < rbrd; ++y)
                dlg(x, y);

        int restOfItems = (int)(totalNumber + 0.5f) - mainSize * mainSize;
        --lbrd;
        ++rbrd;

        for (int y = rbrd - 1; y > lbrd && restOfItems > 0; --y, --restOfItems)
            dlg(rbrd - 1, y);

        for (int x = lbrd + 1; restOfItems > 0; ++x, --restOfItems)
            dlg(x, rbrd - 1);
    }
}
