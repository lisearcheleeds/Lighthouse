using System;
using LighthouseExtends.Font;
using LighthouseExtends.TextTable;
using R3;
using TMPro;
using UnityEngine;

namespace LighthouseExtends.TextMeshPro
{
    /// <summary>
    /// TextMeshProUGUI that automatically reflects language and font changes via TextTableService and FontService.
    /// Text is resolved from the Inspector textKey or from an ITextData set via SetTextData.
    /// </summary>
    public class LHTextMeshPro : TextMeshProUGUI
    {
        [SerializeField] string textKey;
        IDisposable fontSubscription;

        ITextData textData;
        IDisposable textSubscription;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!Application.isPlaying)
            {
                return;
            }

            ApplyText();
            ApplyFont(FontService.Instance?.CurrentFont.CurrentValue);
        }

        protected override void Start()
        {
            base.Start();

            if (!Application.isPlaying)
            {
                return;
            }

            var textTableService = TextTableService.Instance;
            if (textTableService != null)
            {
                textSubscription = textTableService.CurrentLanguage.Subscribe(_ => ApplyText());
            }

            var fontService = FontService.Instance;
            if (fontService != null)
            {
                fontSubscription = fontService.CurrentFont.Subscribe(ApplyFont);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            textSubscription?.Dispose();
            textSubscription = null;

            fontSubscription?.Dispose();
            fontSubscription = null;
        }

        public void SetTextData(ITextData textData)
        {
            this.textData = textData;
            if (isActiveAndEnabled)
            {
                ApplyText();
            }
        }

        void ApplyText()
        {
            var data = textData ?? (!string.IsNullOrEmpty(textKey) ? new TextData(textKey) : null);
            if (data == null)
            {
                return;
            }

            var service = TextTableService.Instance;
            var newText = service != null ? service.GetText(data) : data.TextKey;
            if (text == newText)
            {
                return;
            }

            text = newText;
        }

        void ApplyFont(TMP_FontAsset fontAsset)
        {
            if (fontAsset == null || font == fontAsset)
            {
                return;
            }

            font = fontAsset;
        }
    }
}
