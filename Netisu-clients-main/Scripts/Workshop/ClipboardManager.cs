using Netisu.Game;
using Netisu.Game.Map;
using Godot;
using Netisu.Datamodels;

namespace Netisu.Workshop
{
	public partial class ClipboardManager : Node
	{
		public static ClipboardManager Instance { get; private set; } = null!;
		private Instance CurrentInstanceToCopy = null!;
		private Importer importer = new();

		[Export]
		public Button CopyButton, PasteButton, CloneButton, CutButton;
		

		ClipboardManager()
		{
			Instance = this;
		}

		public void Copy()
		{
			Instance CurrentSelectedInstance = Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance;
			if (CurrentSelectedInstance == null)
			{
				return;
			}

			CurrentInstanceToCopy = CurrentSelectedInstance;
		}

		public void Paste()
		{
			if (CurrentInstanceToCopy == null)
			{
				return;
			}

			Godot.Collections.Dictionary serializedData = CluaObjectList.GetObjectInformation(CurrentInstanceToCopy);
			Node newlyInstanced = importer.SwitchCluaPackedObject(serializedData, CurrentInstanceToCopy.GetParent());
			ActiveInstance activeInstance = newlyInstanced as ActiveInstance;
			Gizmo3DPlugin.Gizmo3D.Instance.Select(activeInstance);
			GameExplorer.Instance.AddObjectToGameExplorer(activeInstance);
			GameExplorer.Instance.SelectedFromEngineCamera(activeInstance);

			CurrentInstanceToCopy = null;
		}

		public static void Duplicate()
		{
			EngineCamera.DuplicateSelection();
		}
	}
}
