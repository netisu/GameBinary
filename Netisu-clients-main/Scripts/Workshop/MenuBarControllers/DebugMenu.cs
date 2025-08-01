using Godot;
using System;
using System.Linq;

public partial class DebugMenu : PopupMenu
{
    public static int PlaytestSelectionInstances { get; private set; } = 2;

    private readonly int[] _blacklistedIndices = { 0, 1 };

    public override void _Ready()
    {
        IndexPressed += OnSelectionUpdate;
    }

    private void OnSelectionUpdate(long id)
    {
        int selectedId = (int)id;

        if (IsSameSelection(selectedId) || IsBlacklisted(selectedId))
            return;
		SetItemChecked(PlaytestSelectionInstances, false);
        PlaytestSelectionInstances = selectedId;
		
		SetItemChecked(selectedId, true);
    }

    private static bool IsSameSelection(int selection) => selection == PlaytestSelectionInstances;

    private bool IsBlacklisted(int selection) => _blacklistedIndices.Contains(selection);
}
