using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class JobChangePageUITests
{
    private GameObject pageObject;
    private GameObject titleObject;
    private GameObject scrollObject;
    private GameObject listObject;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(listObject);
        Object.DestroyImmediate(scrollObject);
        Object.DestroyImmediate(titleObject);
        Object.DestroyImmediate(pageObject);
    }

    [Test]
    public void Initialize_AssignsListWithoutRecursiveCall()
    {
        pageObject = new GameObject("Job Change Page");
        JobChangePageUI page = pageObject.AddComponent<JobChangePageUI>();

        titleObject = new GameObject("Title", typeof(RectTransform), typeof(Text));
        Text title = titleObject.GetComponent<Text>();
        scrollObject = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        listObject = new GameObject("List", typeof(RectTransform));
        RectTransform listRoot = listObject.GetComponent<RectTransform>();

        page.Initialize(title, scrollRect, listRoot);

        Assert.That(page.ListRoot, Is.SameAs(listRoot));
        Assert.That(scrollRect.content, Is.SameAs(listRoot));
    }
}
