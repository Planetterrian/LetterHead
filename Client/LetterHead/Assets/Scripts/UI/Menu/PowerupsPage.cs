using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LetterHeadShared;
using UI.Pagination;

public class PowerupsPage : Page
{
    public PowerupRow[] powerupRows;

    public void Refresh()
    {
        for (int index = 0; index < powerupRows.Length; index++)
        {
            var powerupRow = powerupRows[index];
            powerupRow.SetCount(ClientManager.Instance.PowerupCount((Powerup.Type)index));
        }
    }
}
