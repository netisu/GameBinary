using Godot;
using System;

public partial class GizmosMenu : PopupMenu
{

	public void OnSelection(int id) {
		string _text = GetItemText(id);

		if (_text == "View Grid") {
			SetItemChecked(id, !IsItemChecked(id));
			///GizmoMixin.MakeGrid(IsItemChecked(id));
		}
	}
}
