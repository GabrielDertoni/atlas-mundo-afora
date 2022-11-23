using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Stats : MonoBehaviour
{
    [SerializeField] private TMP_Text m_TimeMeter;
    [SerializeField] private TMP_Text m_PointsMeter;
    [SerializeField] private TMP_Text m_CollectablesMeter;
    [SerializeField] private TMP_Text m_DistanceMeter;

    // The time since the start of the level, measured in seconds.
    private float m_RunningTime = 0;

    private void Update()
    {
        m_RunningTime += Time.deltaTime;

        GameController gameController = GameController.GetInstance();

        if (m_TimeMeter)
            m_TimeMeter.text = Mathf.FloorToInt(m_RunningTime).ToString("0#") + ":" + Mathf.FloorToInt((m_RunningTime * 100) % 100).ToString("0#");

        if (m_PointsMeter)
            m_PointsMeter.text = gameController.GetPoints().ToString();

        if (m_CollectablesMeter)
            m_CollectablesMeter.text = gameController.GetStampCount().ToString() + "/3";

        if (m_DistanceMeter)
            m_DistanceMeter.text = gameController.GetPoints().ToString() + "m percorridos";
    }
}
