using UnityEngine;

namespace Cadi.Scripts.UI.GraphicSystems.Selective
{
    public interface ISelectix
    {
        int RuntimeID { get; }
        bool IsLocked { get; }
        void Lock(bool disableVis);
        void UnLock();
        void TryDeselect();
        void SetGroup(int runtimeId, SelectixGroup group);
        void EditorBind(SelectixGroup group);
        void Init();
        RectTransform CachedRectTransform { get; }

        void UpdateVisuals(bool selected);
    }
}
