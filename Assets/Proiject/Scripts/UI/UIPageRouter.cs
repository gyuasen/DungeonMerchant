using System.Collections.Generic;
using UnityEngine;

public sealed class UIPageRouter : MonoBehaviour
{
    private readonly Dictionary<RectTransform, UIPageBase> pages =
        new Dictionary<RectTransform, UIPageBase>();

    public UIPageBase CurrentPage { get; private set; }

    public void Register(RectTransform pageRoot)
    {
        if (pageRoot == null)
        {
            return;
        }

        UIPageBase page = null;
        foreach (UIPageBase candidate in
                 pageRoot.GetComponents<UIPageBase>())
        {
            if (!(candidate is SimpleUIPage))
            {
                page = candidate;
                break;
            }
            page = candidate;
        }
        if (page == null)
        {
            page = pageRoot.gameObject.AddComponent<SimpleUIPage>();
        }
        pages[pageRoot] = page;
    }

    public void Show(RectTransform pageRoot)
    {
        HideAll();
        if (pageRoot != null && pages.TryGetValue(pageRoot, out UIPageBase page))
        {
            CurrentPage = page;
            page.Show();
        }
    }

    public void HideAll()
    {
        foreach (UIPageBase page in pages.Values)
        {
            page.Hide();
        }
        CurrentPage = null;
    }
}
