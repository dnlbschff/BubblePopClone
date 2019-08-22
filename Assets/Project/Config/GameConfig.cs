using System;
using System.Collections.Generic;
using System.Linq;
using BPC.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace BPC.Config
{
    [CreateAssetMenu(menuName = "BPC/Configs/GameConfig", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Serializable]
        public class BoardSettings
        {
            public GameObject BoardViewPrefab;

            public Vector2Int Min;
            public Vector2Int Max;
            public int MinNumberOfBubbles;
            public int InitialRows;
            public int InitialMaxSpawnExponent;
            public int ExplosionExponent;
        }

        public BoardSettings BoardSetting;

        [Serializable]
        public class BubbleSettings
        {
            public bool ShowDebugText;
            public GameObject BubbleViewPrefab;
            public GameObject NextBubblesViewPrefab;
            public GameObject BubbleBurstViewPrefab;
            public float BubbleSpeed = 1f;
            public float MergeTime = 0.2f;

            [SerializeField] private List<Color> _exponentColors;
            [SerializeField] private List<Sprite> _bubbleSprites;

            public Color GetBubbleColor(int exponent)
            {
                Debug.Assert(exponent > 0, $"Exponent was {exponent}.");
                exponent--;
                exponent %= _exponentColors.Count;
                return _exponentColors[exponent];
            }

            public Sprite GetBubbleSprite(int exponent)
            {
                var index = exponent / 10;
                index = Mathf.Clamp(index, 0, _bubbleSprites.Count);
                return _bubbleSprites[index];
            }
        }

        [FormerlySerializedAs("_bubbleSetting")]
        public BubbleSettings BubbleSetting;

        [Serializable]
        public class InputSettings
        {
            public GameObject InputViewPrefab;
            public GameObject TargetViewPrefab;

            [Range(0f, 1f)]
            public float TargetAlpha;

            public float ShootInterval;
        }

        public InputSettings InputSetting;

        [Serializable]
        public class SfxSettings
        {
            [Serializable]
            public class SfxInfo
            {
                public SfxId SfxId;
                public AudioClip AudioClip;
                public AudioMixerGroup MixerGroup;
            }

            public GameObject AudioViewPrefab;

            [SerializeField]
            private List<SfxInfo> _sfxFiles;


            public SfxInfo GetSfxInfo(SfxId sfxId)
            {
                return _sfxFiles.First(x => x.SfxId == sfxId);
            }
        }

        public SfxSettings SfxSetting;
    }
}