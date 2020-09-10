﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorShapePanelControl : DanbiUIPanelControl
    {
        DanbiUIReflectorCone Cone = new DanbiUIReflectorCone();
        DanbiUIReflectorHalfsphere Halfsphere = new DanbiUIReflectorHalfsphere();

        int selectedPanel = 0;
        new GameObject[] Panel = new GameObject[2];

        public delegate void OnTypeChanged(int selectedPanel);
        public static OnTypeChanged Call_OnTypeChanged;

        new void Start()
        {
            // i == 0 => Cube
            // i == 1 => Cylinder
            for (int i = 0; i < 2; ++i)
            {
                Panel[i] = transform.GetChild(1).GetChild(i).gameObject;
                if (!Panel[i].name.Contains("Panel"))
                {
                    Panel[i] = null;
                }
                else
                {
                    Panel[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            }

            Panel[0].gameObject.SetActive(true);
            Panel[1].gameObject.SetActive(false);

            BindPanelFields();
            DanbiUIControl.Call_OnPanelDataUpdated(this);

            Call_OnTypeChanged += Caller_OnTypeChanged;
        }

        void Caller_OnTypeChanged(int selectedPanel)
        {
            bool isChanged = this.selectedPanel != selectedPanel;
            this.selectedPanel = selectedPanel;

            if (isChanged)
            {
                if (this.selectedPanel == 0)
                {
                    Panel[0].gameObject.SetActive(true);
                    Panel[1].gameObject.SetActive(false);
                }

                if (this.selectedPanel == 1)
                {
                    Panel[0].gameObject.SetActive(false);
                    Panel[1].gameObject.SetActive(true);
                }

                BindPanelFields();
            }
        }

        protected override void BindPanelFields()
        {
            if (Panel[selectedPanel].Null())
            {
                return;
            }

            var panel = Panel[selectedPanel].transform;
            switch (selectedPanel)
            {
                case 0:
                    Cone.BindInput(panel);
                    break;

                case 1:
                    Halfsphere.BindInput(panel);
                    break;
            }
        } // BindPanelField()
    };
};
