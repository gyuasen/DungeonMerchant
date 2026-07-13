using NUnit.Framework;
using UnityEngine;

public sealed class UIPageRouterTests
{
    private GameObject routerObject;
    private GameObject firstPageObject;
    private GameObject secondPageObject;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(firstPageObject);
        Object.DestroyImmediate(secondPageObject);
        Object.DestroyImmediate(routerObject);
    }

    [Test]
    public void Register_HidesPagesUntilTheyAreSelected()
    {
        UIPageRouter router = CreateRouterAndPages(
            out RectTransform firstPage,
            out RectTransform secondPage);

        router.Register(firstPage);
        router.Register(secondPage);

        Assert.That(firstPage.gameObject.activeSelf, Is.False);
        Assert.That(secondPage.gameObject.activeSelf, Is.False);
    }

    [Test]
    public void Show_LeavesOnlySelectedPageVisible()
    {
        UIPageRouter router = CreateRouterAndPages(
            out RectTransform firstPage,
            out RectTransform secondPage);
        router.Register(firstPage);
        router.Register(secondPage);

        router.Show(firstPage);
        router.Show(secondPage);

        Assert.That(firstPage.gameObject.activeSelf, Is.False);
        Assert.That(secondPage.gameObject.activeSelf, Is.True);
        Assert.That(router.CurrentPage.gameObject, Is.SameAs(secondPageObject));
    }

    private UIPageRouter CreateRouterAndPages(
        out RectTransform firstPage,
        out RectTransform secondPage)
    {
        routerObject = new GameObject("Router");
        UIPageRouter router = routerObject.AddComponent<UIPageRouter>();

        firstPageObject = new GameObject("First Page", typeof(RectTransform));
        firstPage = firstPageObject.GetComponent<RectTransform>();
        firstPageObject.AddComponent<SimpleUIPage>();

        secondPageObject = new GameObject("Second Page", typeof(RectTransform));
        secondPage = secondPageObject.GetComponent<RectTransform>();
        secondPageObject.AddComponent<SimpleUIPage>();
        return router;
    }
}
