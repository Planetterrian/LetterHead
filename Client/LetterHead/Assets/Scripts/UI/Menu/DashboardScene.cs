using System;
using System.Collections.Generic;
using System.Linq;
using UI.Pagination;
using UnityEngine;

public class DashboardScene : GuiScene
{
    public PagedRect pagination;

    public override void OnBeginShow()
    {
        base.OnBeginShow();

        pagination.SetCurrentPage(1);
    }
}