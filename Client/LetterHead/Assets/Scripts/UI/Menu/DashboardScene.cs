using System;
using System.Collections.Generic;
using System.Linq;
using UI.Pagination;
using UnityEngine;

public class DashboardScene : GuiScene
{
    public PagedRect pagination;
    public HomePage homePage;

    public static bool showReview;

    public override void OnBeginShow()
    {
        base.OnBeginShow();

        pagination.DefaultPage = PersistManager.Instance.initialDashPage;
        pagination.SetCurrentPage(PersistManager.Instance.initialDashPage, true);

        homePage.ClearMatches();

        if (showReview)
        {
            showReview = false;
            NPBinding.Utility.RateMyApp.AskForReviewAttempt();
        }
    }
}