using Godot;
using System;

namespace Netisu.Workshop.MenuBarControllers
{
	public partial class TreeMenuRMB : PopupMenu
	{
		public TreeItem ForTreeItem;

		public Datamodels.Instance RelatedObject;

		public void OnPressed(long id)
		{
			switch ((int)id)
			{
				case 0:   // rename
					Netisu.Workshop.GameExplorer.Instance.AttemptEdit();
					break;

				case 1:   // delete
					Netisu.Workshop.GameExplorer.Instance.RemoveObjectFromGameExplorer(RelatedObject, true);

					RelatedObject.Destroy();
					break;

				case 3:   // Open Documentation
					OS.ShellOpen($"https://client-docs.netisu.com/Classes/{RelatedObject.GetType().Name}");
					break;

				case 5:   // Cut
					break;

				case 6:   // Copy
					break;

				case 7:   // Paste
					break;

				case 8:   // Get Path
					break;

				case 10:  // Export as .glb
					break;

				case 11:  // collapse
					Netisu.Workshop.GameExplorer.Instance.CollapseCurrentSelectedBranch();
					break;
			}
		}
	}

}
