using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LighthouseExtends.Language;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace LighthouseExtends.TextTable.Tests.EditMode
{
    public class TextTableServiceTest
    {
        LanguageService languageService;
        StubLoader loader;
        TextTableService textTableService;

        [SetUp]
        public void SetUp()
        {
            languageService = new LanguageService();
            loader = new StubLoader();
            textTableService = new TextTableService(loader, languageService);
        }

        [TearDown]
        public void TearDown()
        {
            textTableService.Dispose();
            languageService.Dispose();
        }

        [Test]
        public void GetText_BeforeLoad_ReturnsKey()
        {
            Assert.AreEqual("greeting", textTableService.GetText(new TextData("greeting")));
        }

        [UnityTest]
        public IEnumerator GetText_AfterLoad_ReturnsTableValue() => UniTask.ToCoroutine(async () =>
        {
            loader.SetTable(new Dictionary<string, string> { { "greeting", "Hello" } });

            await languageService.SetLanguage("en", CancellationToken.None);

            Assert.AreEqual("Hello", textTableService.GetText(new TextData("greeting")));
        });

        [UnityTest]
        public IEnumerator GetText_KeyNotFound_ReturnsKey() => UniTask.ToCoroutine(async () =>
        {
            loader.SetTable(new Dictionary<string, string> { { "other", "Other" } });

            await languageService.SetLanguage("en", CancellationToken.None);

            Assert.AreEqual("missing", textTableService.GetText(new TextData("missing")));
        });

        [UnityTest]
        public IEnumerator GetText_WithParam_SubstitutesPlaceholder() => UniTask.ToCoroutine(async () =>
        {
            loader.SetTable(new Dictionary<string, string> { { "greeting", "Hello {name}" } });

            await languageService.SetLanguage("en", CancellationToken.None);

            var data = new TextData("greeting", new Dictionary<string, object> { { "name", "World" } });
            Assert.AreEqual("Hello World", textTableService.GetText(data));
        });

        [UnityTest]
        public IEnumerator GetText_MultipleParams_SubstitutesAll() => UniTask.ToCoroutine(async () =>
        {
            loader.SetTable(new Dictionary<string, string> { { "msg", "{a} and {b}" } });

            await languageService.SetLanguage("en", CancellationToken.None);

            var data = new TextData("msg", new Dictionary<string, object> { { "a", "X" }, { "b", "Y" } });
            Assert.AreEqual("X and Y", textTableService.GetText(data));
        });

        class StubLoader : ITextTableLoader
        {
            IReadOnlyDictionary<string, string> table = new Dictionary<string, string>();

            public void SetTable(IReadOnlyDictionary<string, string> t) => table = t;

            public UniTask<IReadOnlyDictionary<string, string>> LoadAsync(string languageCode, CancellationToken ct)
                => UniTask.FromResult(table);
        }
    }
}
