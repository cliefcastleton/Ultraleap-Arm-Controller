using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Leap;
using Leap.Unity;
using System.IO;
using System.IO.Ports;
using TMPro;
using System.Threading;

public class GetHandPos : MonoBehaviour
{
    public LeapServiceProvider provider;
    public GameObject rightHand;
    public GameObject leftHand;
    public Cobot3DIK cobotIK;
    public Transform simulationReference;

    [Header("Tuning")]
    public float inputMultiplier;
    public float xySensitivity;
    public float zSensitivity;

    private Vector3 handPosOffset;
    private Vector3 handRotOffset;

    [Header("Output")]
    public TextMeshProUGUI outputLabel;
    public string currentPose = "None";

    public ContactCobot contactCobot;

    private Vector3 handPosition = Vector3.zero;
    private Vector3 handRotation = Vector3.zero;
    private Vector3 lastHandPosition = Vector3.zero;

    private Transform handTransform;


    public void FistPose()
    {
        currentPose = "Fist";
    }

    public void OpenPalmPose()
    {
        currentPose = "OpenPalm";
    }

    public void OKPose()
    {
        currentPose = "OK";
    }

    public void PointPose()
    {
        currentPose = "Point";
    }



    public Vector3 GetHandPosition()
    {
        return handPosition;
    }

    public string GetHandPose()
    {
        return currentPose;
    }



    void Update()
    {
        //Prioritize the right hand, but if only the left hand exists, use that:
        if (rightHand.transform.parent.gameObject.activeSelf == false)
        {
            handTransform = leftHand.transform;
        }
        else
        {
            handTransform = rightHand.transform;
        }


        //Zero the output for tuning:
        if (Input.GetKeyDown(KeyCode.Space))
        {
            handPosOffset = handTransform.position;
            /*
            handPosOffset = Vector3.zero;
            handRotOffset = Vector3.zero;
            // reset handPosition and handRotation
            handPosition = Vector3.zero;
            handRotation = Vector3.zero;
            */
        }


        handRotation = handRotation - handRotOffset;

        if (rightHand.transform.parent.gameObject.activeSelf == false && leftHand.transform.parent.gameObject.activeSelf == false) {
            handPosition = lastHandPosition;
        } else {
            handPosition = new Vector3(
                (handTransform.position.x - handPosOffset.x) * xySensitivity * inputMultiplier,
                (handTransform.position.y) * zSensitivity * inputMultiplier,
                (handTransform.position.z - handPosOffset.z) * xySensitivity * inputMultiplier
            );
        }


        handRotation = new Quaternion(
            (int)Mathf.Round(handTransform.rotation.x * inputMultiplier),
            (int)Mathf.Round(handTransform.rotation.y * inputMultiplier),
            (int)Mathf.Round(handTransform.rotation.z * inputMultiplier),
            (int)Mathf.Round(handTransform.rotation.w * inputMultiplier)
        ).eulerAngles + handRotOffset;

        lastHandPosition = handPosition;


        simulationReference.position = new Vector3(handPosition.x * 1, Mathf.Clamp(handPosition.y * 1, 0.35f, .5f), handPosition.z * 1);

        /*
        if (rightHand.transform.parent.gameObject.activeSelf == false && leftHand.transform.parent.gameObject.activeSelf == false)
        {
            simulationReference.localPosition = new Vector3(0, 15.5f, 5);
        }
        else
        {
            simulationReference.position = new Vector3(handPosition.x * 1, Mathf.Clamp(handPosition.y * 1, 0.35f, .5f), handPosition.z * 1);
        }*/

        contactCobot.SendData();
    }
}