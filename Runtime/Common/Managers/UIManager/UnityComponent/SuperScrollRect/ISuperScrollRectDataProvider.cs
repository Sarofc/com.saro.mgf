using UnityEngine;

namespace Saro.UI
{
    public interface ISuperScrollRectDataProvider
    {
        int GetCellCount();

        void SetCell(GameObject cell, int index);
    }
}
