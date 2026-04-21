using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LighthouseExtends.Language.Tests.EditMode
{
    public class LanguageServiceTest
    {
        LanguageService service;

        [SetUp]
        public void SetUp() => service = new LanguageService();

        [TearDown]
        public void TearDown() => service.Dispose();

        [UnityTest]
        public IEnumerator SetLanguage_NoHandlers_UpdatesCurrentLanguage() => UniTask.ToCoroutine(async () =>
        {
            await service.SetLanguage("en", CancellationToken.None);

            Assert.AreEqual("en", service.CurrentLanguage.CurrentValue);
        });

        [UnityTest]
        public IEnumerator SetLanguage_WithHandler_HandlerCalledWithCorrectCode() => UniTask.ToCoroutine(async () =>
        {
            string captured = null;
            service.RegisterChangeHandler((code, ct) => { captured = code; return UniTask.CompletedTask; });

            await service.SetLanguage("ja", CancellationToken.None);

            Assert.AreEqual("ja", captured);
        });

        [UnityTest]
        public IEnumerator SetLanguage_MultipleHandlers_AllInvoked() => UniTask.ToCoroutine(async () =>
        {
            var calls = new List<string>();
            service.RegisterChangeHandler((code, ct) => { calls.Add("A"); return UniTask.CompletedTask; });
            service.RegisterChangeHandler((code, ct) => { calls.Add("B"); return UniTask.CompletedTask; });

            await service.SetLanguage("en", CancellationToken.None);

            Assert.Contains("A", calls);
            Assert.Contains("B", calls);
        });

        [UnityTest]
        public IEnumerator SetLanguage_HandlerRunsBeforeCurrentLanguageIsUpdated() => UniTask.ToCoroutine(async () =>
        {
            string languageDuringHandler = "unset";
            service.RegisterChangeHandler((code, ct) =>
            {
                languageDuringHandler = service.CurrentLanguage.CurrentValue;
                return UniTask.CompletedTask;
            });

            await service.SetLanguage("en", CancellationToken.None);

            // Handler fires before CurrentLanguage is updated, so it still sees the old value
            Assert.AreNotEqual("en", languageDuringHandler);
            Assert.AreEqual("en", service.CurrentLanguage.CurrentValue);
        });
    }
}
