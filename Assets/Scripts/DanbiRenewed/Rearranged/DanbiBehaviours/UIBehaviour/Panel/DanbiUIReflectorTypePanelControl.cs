﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIReflectorTypePanelControl : DanbiUIPanelControl
    {
        [SerializeField]
        List<string> ReflectorTypeContents = new List<string>();

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            ReflectorTypeContents.Add("Cone");
            ReflectorTypeContents.Add("Halfsphere");
            ReflectorTypeContents.Add("Paraboloid");

            var panel = Panel.transform;
            var reflectorTypeDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            reflectorTypeDropdown.AddOptions(ReflectorTypeContents);
            reflectorTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    DanbiUIReflectorShapePanelControl.Call_OnTypeChanged?.Invoke(option);
                }
            );
        }
    };
};