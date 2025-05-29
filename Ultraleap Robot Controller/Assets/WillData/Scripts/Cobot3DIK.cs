using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cobot3DIK : MonoBehaviour
{
    private float A23 = 136; //136
    private float A34 = 99; //99

    public float J1Angle;
    public float J2Angle;
    public float J3Angle;
    public float J4Angle;

    public Transform J1Object;
    public Transform J2Object;
    public Transform J3Object;
    public Transform J4Object;

    public Transform reference;


    // Update is called once per frame
    void Update()
    {
        float xDistance = new Vector3(reference.position.x - J1Object.position.x, 0, reference.position.z - J1Object.position.z).magnitude;
        float zDistance = reference.position.y - J1Object.position.y;

        Vector3 referencePosition = new Vector3(0, zDistance, xDistance);

        CalculateJoint1Angle(J1Object.position, reference.position);

        float[] angles = CalculateJoint24Angles(referencePosition);
        //Debug.Log("J1: " + GetJoint1Angle() + ", J2: " + (angles[0] - 180) + ", J3: " + angles[1] + ", J4: " + angles[2]);

        J1Angle = GetJoint1Angle();
        J2Angle = angles[0] - 180;
        J3Angle = angles[1];
        J4Angle = angles[2];


        J2Object.localRotation = Quaternion.Euler(new Vector3(angles[0], 0, 0));
        J3Object.localRotation = Quaternion.Euler(new Vector3(angles[1], 0, 0));
        J4Object.localRotation = Quaternion.Euler(new Vector3(angles[2], 0, 0));
    }

    float CalculateJoint1Angle(Vector3 origin, Vector3 targetPosition)
    {
        Vector3 lookPos = targetPosition - origin;
        lookPos.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(lookPos) * Quaternion.Euler(0, 180, 0);
        J1Object.rotation = lookRotation;
        return lookRotation.eulerAngles.y;

    }

    public float GetJoint1Angle()
    {
        float angle = CalculateJoint1Angle(J1Object.position, reference.position);
        
        angle = (angle > 180) ? angle - 360 : angle;
        angle = Mathf.Clamp(angle, -165, 165);

        return angle;
    }

    float[] CalculateJoint24Angles(Vector3 position)
    {
        float Hx = position.z * 500;
        float Hz = position.y * 500;

        float H = Mathf.Sqrt(Hx * Hx + Hz * Hz);

        float alphaH = Mathf.Atan2(Hz, Hx) * Mathf.Rad2Deg;


        float a = Mathf.Acos(Mathf.Clamp((A34 * A34 - A23 * A23 - H * H) / (-2 * A23 * H), -1, 1)) * Mathf.Rad2Deg;
        float b = Mathf.Acos(Mathf.Clamp((H * H - A23 * A23 - A34 * A34) / (-2 * A23 * A34), -1, 1)) * Mathf.Rad2Deg;
        float c = 180 - a - b;

        float delta = 90 - alphaH;

        float J2 = (a + (90 - alphaH));
        float J3 = b - 180;
        float J4 = 180 - delta + c;

        float[] angles = new float[] { J2, J3, J4 };

        return angles;
    }
}
