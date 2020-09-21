﻿using UnityEngine;

namespace Danbi
{
    [System.Serializable]
    public class DanbiScreen : MonoBehaviour
    {
        #region Exposed
        // TODO: just display the values from the inputFields
        [SerializeField, Header("16:9 or 16:10")]
        EDanbiScreenAspects TargetScreenAspect = EDanbiScreenAspects.E_16_9;

        // TODO: just display the values from the inputFields
        [SerializeField, Header("2K(2560 x 1440), 4K(3840 x 2160) or 8K(7680 x 4320)")]
        EDanbiScreenResolutions TargetScreenResolution = EDanbiScreenResolutions.E_4K;
        
        // TODO: just display the values from the inputFields
        [SerializeField, Readonly, Header("Current Resolution of the target distorted image")]
        // TODO: just display the values from the inputFields
        Vector2Int ScreenResolution = new Vector2Int();

        // TODO: just display the values from the inputFields
        [SerializeField, Header("Resolution Scaler")]
        int ResolutionScaler = 1;

        #endregion Exposed


        #region Internal
        public EDanbiScreenAspects targetScreenAspect { get => TargetScreenAspect; }

        public EDanbiScreenResolutions targetScreenResolution { get => TargetScreenResolution; }

        public Vector2Int screenResolution { get => ScreenResolution; }

        public int resolutionScaler { get => ResolutionScaler; }

        #endregion Internal    

        void Reset()
        {
            TargetScreenAspect = EDanbiScreenAspects.E_16_9;
            TargetScreenResolution = EDanbiScreenResolutions.E_4K;
            ResolutionScaler = 1;
        }

        void OnValidate()
        {
            CalcScreenResolution();
        }

        void Start()
        {
            CalcScreenResolution();
        }

        void CalcScreenResolution()
        {
            ScreenResolution = GetScreenResolution(eScreenAspects: TargetScreenAspect, eScreenResolution: TargetScreenResolution);
            ScreenResolution *= ResolutionScaler;
        }

        /// <summary>
        /// Calculate the actual screen resolution by the screen aspects and the target resolutions.
        /// </summary>
        /// <param name="eScreenAspects"></param>
        /// <param name="eScreenResolution"></param>
        /// <returns></returns>
        public static Vector2Int GetScreenResolution(EDanbiScreenAspects eScreenAspects,
                                                  EDanbiScreenResolutions eScreenResolution)
        {
            var result = default(Vector2Int);
            switch (eScreenResolution)
            {
                case EDanbiScreenResolutions.E_1K:
                    result = new Vector2Int(1920, 1920);
                    break;

                case EDanbiScreenResolutions.E_2K:
                    result = new Vector2Int(2560, 2560);
                    break;

                case EDanbiScreenResolutions.E_4K:
                    result = new Vector2Int(3840, 3840);
                    break;

                case EDanbiScreenResolutions.E_8K:
                    result = new Vector2Int(7680, 7680);
                    break;
            }

            switch (eScreenAspects)
            {
                case EDanbiScreenAspects.E_16_9:
                    result.y = Mathf.FloorToInt(result.y * 9 / 16);
                    break;

                case EDanbiScreenAspects.E_16_10:
                    result.y = Mathf.FloorToInt(result.y * 10 / 16);
                    break;
            }
            return result;
        }
    };
};
