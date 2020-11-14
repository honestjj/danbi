﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUIProjectorCalibratedPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public bool useCalbiratedCamera;

        // TODO : add -1(Not Used) into the dropdown.
        [Readonly]
        public EDanbiCameraUndistortionMethod lensDistortionMode = EDanbiCameraUndistortionMethod.Direct;

        [Readonly]
        public float newtonThreshold;

        [Readonly]
        public float iterativeThreshold;

        [Readonly]
        public float iterativeSafetyCounter;

        public delegate void OnSetUseCalibratedCamera(bool use);
        public static OnSetUseCalibratedCamera onSetUseCalibratedCamera;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ProjectorCalibrationPanel-useCalibratedCamera", useCalbiratedCamera == true ? 1 : 0);
            PlayerPrefs.SetInt("ProjectorCalbirationpanel-lensDistortionMode", (int)lensDistortionMode);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-newtonThreshold", newtonThreshold);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-iterativeThreshold", iterativeThreshold);
            PlayerPrefs.SetFloat("ProjectorCalbirationpanel-iterativeSafetyCounter", iterativeSafetyCounter);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            bool prevUseCalibratedCamera = PlayerPrefs.GetInt("ProjectorCalibrationPanel-useCalibratedCamera", default) == 1;
            useCalbiratedCamera = prevUseCalibratedCamera;
            onSetUseCalibratedCamera?.Invoke(prevUseCalibratedCamera);
            (uiElements[0] as Toggle).isOn = prevUseCalibratedCamera;

            var prevLensDistortionMode = (EDanbiCameraUndistortionMethod)PlayerPrefs.GetInt("ProjectorCalibrationPanel-lensDistortionMode", -1);
            lensDistortionMode = prevLensDistortionMode;
            (uiElements[1] as Dropdown).value = (int)prevLensDistortionMode;

            var prevNewtonThreshold = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-newtonThreshold", default);
            newtonThreshold = prevNewtonThreshold;
            (uiElements[2] as InputField).text = prevNewtonThreshold.ToString();

            var prevIterativeThreshold = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-iterativeThreshold", default);
            iterativeThreshold = prevIterativeThreshold;
            (uiElements[3] as InputField).text = prevIterativeThreshold.ToString();

            var prevIterativeSafetyCounter = PlayerPrefs.GetFloat("ProjectorCalibrationPanel-iterativeSafetyCounter", default);
            iterativeSafetyCounter = prevIterativeSafetyCounter;
            (uiElements[4] as InputField).text = prevIterativeSafetyCounter.ToString();

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            Dropdown lensDistortionModeDropdown = default;
            GameObject newtonProperties = default;
            GameObject iterativeProperties = default;
            InputField newtonThresholdInputField = default;
            InputField iterativeThresholdInputField = default;
            InputField iterativeSafetyCounterInputField = default;

            // 1. bind the "Calibrated Camera" toggle.
            var useCalibratedCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            useCalibratedCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {                    
                    onSetUseCalibratedCamera?.Invoke(isOn);
                    useCalbiratedCamera = isOn;
                    lensDistortionModeDropdown.interactable = isOn;
                    newtonProperties.SetActive(isOn);
                    iterativeProperties.SetActive(isOn);
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // bind the undistortion Method dropdown.
            lensDistortionModeDropdown = panel.GetChild(1).GetComponent<Dropdown>();
            lensDistortionModeDropdown.AddOptions(new List<string> { "direct", "iterative", "newton" });
            lensDistortionModeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (option)
                    {
                        case 0: // direct
                            lensDistortionMode = EDanbiCameraUndistortionMethod.Direct;
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(false);
                            break;

                        case 1: // iterative
                            lensDistortionMode = EDanbiCameraUndistortionMethod.Iterative;
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(true);
                            break;

                        case 2: // newton
                            lensDistortionMode = EDanbiCameraUndistortionMethod.Newton;
                            newtonProperties.SetActive(true);
                            iterativeProperties.SetActive(false);
                            break;
                    }
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );
            lensDistortionModeDropdown.value = 0;

            // bind the newton properties and turn it off.
            newtonProperties = panel.GetChild(2).gameObject;

            // bind the newton threshold.
            newtonThresholdInputField = newtonProperties.transform.GetChild(0).GetComponent<InputField>();
            newtonThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        newtonThreshold = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            // newtonThresholdInputField.gameObject.SetActive(false);
            newtonProperties.SetActive(false);

            // bind the iterative properties and turn it off.
            iterativeProperties = panel.GetChild(3).gameObject;

            // bind the iterative threshold.
            iterativeThresholdInputField = iterativeProperties.transform.GetChild(0).GetComponent<InputField>();
            iterativeThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        iterativeThreshold = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );            

            // bind the iterative safety counter.
            iterativeSafetyCounterInputField = iterativeProperties.transform.GetChild(1).GetComponent<InputField>();
            iterativeSafetyCounterInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        iterativeSafetyCounter = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            iterativeProperties.SetActive(false);

            LoadPreviousValues(useCalibratedCameraToggle, lensDistortionModeDropdown, newtonThresholdInputField, iterativeThresholdInputField, iterativeSafetyCounterInputField);
        }
    };
};