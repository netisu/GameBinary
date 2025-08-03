using Godot;
using System.Collections.Generic;
using Netisu.Datamodels;
using System.Diagnostics;
using Netisu.Workshop.MenuBarControllers;

namespace Netisu.Workshop
{
	public partial class GameExplorer : Tree
	{
		[Export]
		public PopupMenu TreePopUpMenu;

		[Export]
		public UserInterface userInterface = null!;

		[Export]
		public Texture2D HiddenIcon, VisibleIcon, LockedIcon;

		public Dictionary<TreeItem, Instance> cachedItems = [];
		public Dictionary<Instance, TreeItem> cachedItemsReverse = [];

		public static GameExplorer Instance { get; private set; } = null!;

		private bool isUpdatingSelection = false;

		public override void _Ready()
		{
			Instance = this;
			CallDeferred("CreateTreeItems", GetNode<Instance>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/Game"), default);
		}

		public void AttemptEdit() => EditSelected(true);

		public void CollapseCurrentSelectedBranch() => GetSelected()?.SetCollapsedRecursive(true);

		public void CreateTreeItems(Instance node, TreeItem item = null)
		{
			if (node is null)
			{
				return;
			}

			TreeItem current_item = CreateItem(item);

			if (node is UiInstance uiInstance and not UserInterface)
			{
				if (!uiInstance.Visible)
				{
					current_item.AddButton(0, HiddenIcon);
				}
				else
				{
					current_item.AddButton(0, VisibleIcon);
				}
			}

			current_item.SetText(0, node.Name);
			cachedItems.Add(current_item, node);
			cachedItemsReverse.Add(node, current_item);

			if (node is not Datamodels.Game)
			{
				current_item.SetEditable(0, true);
			}

			try
				{
					string iconName = node.GetGroups()[0];
					if (iconName == "RESTRICTED_MAX")
					{
						iconName = node.GetGroups()[1];
					}
					current_item.SetIcon(0, GD.Load<Texture2D>($"res://Assets/Icons/{iconName}.svg"));
				}
				catch
				{
					cachedItems.Remove(current_item);
					current_item.Free();
					return;
				}

			var stopwatch = Stopwatch.StartNew();
			var _dkdkd = node.GetChildren();
			foreach (Node node1 in _dkdkd)
			{
				if (node1 is Instance instance)
				{
					CreateTreeItems(instance, current_item);
				}
			}

			stopwatch.Stop();
			GD.Print($"Elapsed time: {stopwatch.ElapsedMilliseconds} ms for {node.Name}");
		}

		public void UpdateIcon(Instance _node)
		{
			cachedItemsReverse[_node]?.SetIcon(0, GD.Load<Texture2D>("res://Assets/Icons/" + _node.GetGroups()[0] + ".svg"));
		}

		public void AddObjectToGameExplorer(Instance instance)
		{
			Instance parentNode = instance.GetParent<Instance>();
			TreeItem parentTreeItem = cachedItemsReverse.TryGetValue(parentNode, out TreeItem value) ? value : null;
			if (IsInstanceValid(parentTreeItem))
				CreateTreeItems(instance, parentTreeItem);
		}

		public bool RemoveObjectFromGameExplorer(Instance item, bool setGizmo = false)
		{
			try
			{
				if (setGizmo)
				{
					Gizmo3DPlugin.Gizmo3D.Instance.ClearSelection();
				}

				item.Destroy();
			}
			catch { return false; }

			TreeItem treeItem = cachedItemsReverse.TryGetValue(item, out TreeItem value) ? value : null;

			if (IsInstanceValid(treeItem))
			{
				foreach (TreeItem ChildItem in treeItem.GetChildren())
				{
					cachedItems.Remove(ChildItem);
				}
				treeItem.Free();
			}

			cachedItems.Remove(treeItem);
			return true;
		}

		public void SelectedFromEngineCamera(Node _object)
		{
			if (isUpdatingSelection) return;
			isUpdatingSelection = true;

			TreeItem _object_tree_item = cachedItemsReverse.TryGetValue(_object as Instance, out TreeItem value) ? value : null;
			_object_tree_item?.Select(0);
			
			Gizmo3DPlugin.Gizmo3D.Instance.ClearSelection();
			if (_object is ActiveInstance activeInstance)
			{
				Gizmo3DPlugin.Gizmo3D.Instance.Select(activeInstance);
				EngineCamera.OnUpdateCurrentSelected();
			}

			isUpdatingSelection = false;
		}

		public void DeselectedFromEngineCamera(Instance _object)
		{
			if (isUpdatingSelection) return;
			isUpdatingSelection = true;
			TreeItem _object_tree_item = cachedItemsReverse[_object];
			_object_tree_item?.Deselect(0);

			isUpdatingSelection = false;
		}

		public void OnTreeItemClicked()
		{
			if (isUpdatingSelection) return;

			Instance ObjectClicked = cachedItems[GetSelected()];
			if (ObjectClicked != null)
			{
				if (ObjectClicked is ActiveInstance activeInstance)
				{
					Gizmo3DPlugin.Gizmo3D.Instance.Select(activeInstance);
					EngineCamera.OnUpdateCurrentSelected();
				}
				else
				{
					Gizmo3DPlugin.Gizmo3D.Instance.ClearSelection();
					Netisu.Properties.PropertiesManager.Empty();
					Netisu.Properties.PropertiesManager.Load(ObjectClicked);
				}
			}
		}

		public override Variant _GetDragData(Vector2 position)
		{
			if (GetSelected() != null)
			{
				bool RESTRICTED_MAX = cachedItems[GetSelected()].IsInGroup("RESTRICTED_MAX");
				if (RESTRICTED_MAX)
					return default;

				FontFile f = ResourceLoader.Load<FontFile>("res://Assets/Fonts/Montserrat-Bold.ttf");
				Label prev = new()
				{
					Text = GetSelected().GetText(0)
				};
				prev.AddThemeFontOverride("font", f);
				prev.AddThemeFontSizeOverride("font_size", 13);
				SetDragPreview(prev);
				return GetSelected();
			}

			return default;
		}

		public override bool _CanDropData(Vector2 atPosition, Variant data)
		{
			try
			{
				data = data.As<TreeItem>();

				var dropped_on = GetItemAtPosition(atPosition);
				Instance associatedNode = cachedItems[dropped_on];
				Instance beingDraggedNode = cachedItems[data.As<TreeItem>()];

				if (associatedNode is LocalScripts || associatedNode is Scripts || associatedNode is BaseScript || associatedNode is Players || associatedNode is Netisu.Datamodels.Environment || associatedNode is UserInterface)
				{
					return false;
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		public override void _DropData(Vector2 at, Variant item)
		{
			try
			{
				TreeItem treeItem = item.As<TreeItem>();
				var toItem = GetItemAtPosition(at);
				if (toItem == null)
				{
					return;
				}

				if (!cachedItems.TryGetValue(treeItem, out Instance value) || !cachedItems.TryGetValue(toItem, out Instance toitemValue))
				{
					return;
				}

				var movingObject = value;
				var movingObjectTo = toitemValue;

				if (movingObjectTo.Name == "Game" || movingObjectTo.IsInGroup("Script"))
				{
					return;
				}

				if (!IsInstanceValid(movingObject) || !IsInstanceValid(movingObjectTo))
				{
					return;
				}
				Node currentParent = movingObjectTo;
				while (currentParent != null)
				{
					if (currentParent == movingObject)
					{
						return;
					}
					currentParent = currentParent.GetParent();
				}
				if (movingObject != movingObjectTo)
				{
					RefreshNodes(movingObject, toItem, treeItem, movingObjectTo);
				}
			}
			catch
			{
			}
		}

		public void RefreshNodes(Node movingObject, TreeItem toItem, TreeItem shiftingItem, Node movingObjectTo)
		{
			if (IsInstanceValid(movingObject) && IsInstanceValid(movingObjectTo))
			{
				TreeItem parentTreeItem = shiftingItem.GetParent();
				parentTreeItem?.RemoveChild(shiftingItem);

				toItem.AddChild(shiftingItem);

				movingObject.GetParent().RemoveChild(movingObject);
				movingObjectTo.AddChild(movingObject, true);

				cachedItems.Remove(shiftingItem);
				cachedItems[shiftingItem] = movingObject as Instance;
				shiftingItem.SetText(0, movingObject.Name);
				shiftingItem.Select(0);
			}
		}

		private void _on_item_edited()
		{
			if (GetSelected() == null) return;
			if (cachedItems[GetSelected()].GetParent() == null) return;

			Node RenamedObject = cachedItems[GetSelected()];
			string newerName = GetSelected().GetText(0);

			(RenamedObject as Instance)?.Rename(newerName);
			GetSelected().SetText(0, RenamedObject.Name);
		}

		public void RefreshTextChange(string newername, Instance instance = null)
		{
			instance.Rename(newername);
			cachedItemsReverse[instance].SetText(0, instance.Name);
		}

		public void OnQuickButton(TreeItem treeItem, int column, int id, int mouse_button_index)
		{

			if (mouse_button_index != 1)
				return;

			Instance objectHappenedOn = cachedItems[treeItem];
			if (objectHappenedOn == null)
				return;
			if (id == 0)
			{
				if (objectHappenedOn is UiInstance uiInstance)
				{
					var UiInstanceTree = cachedItemsReverse[uiInstance];
					uiInstance.Visible = !uiInstance.Visible;
					if (!uiInstance.Visible)
					{
						UiInstanceTree.EraseButton(0, 0);
						UiInstanceTree.AddButton(0, HiddenIcon, 0);
					}
					else
					{
						UiInstanceTree.EraseButton(0, 0);
						UiInstanceTree.AddButton(0, VisibleIcon, 0);
					}
					return;
				}

				if (objectHappenedOn is ActiveInstance activeInstance)
				{
					EngineUI.LockSelected(activeInstance);
				}
				return;
			}
		}


		private void DeleteIfScript(BaseScript script)
		{
			if (script != null)
			{
				EngineUI.Instance.CloseTab(script?.GetName());
				TreeItem searchRes = cachedItemsReverse[script];
				if (searchRes != null)
				{
					cachedItems.Remove(searchRes);
					searchRes.Free();
					script.Destroy();
				}
			}
		}

		public override void _Input(InputEvent @event)
		{
			if (Input.IsActionJustPressed("Delete"))
			{
				if (GetSelected() != null)
				{
					Node _as_obj = cachedItems[GetSelected()];
					if (_as_obj is BaseScript script)
					{
						DeleteIfScript(script);
						EngineUI.Instance.ClearOutExistingPropertyTabs();
					}
				}
			}

			if (Input.IsActionJustPressed("Rename_object"))
			{
				if (GetSelected() != null)
				{
					AttemptEdit();
				}
			}
		}

		public override void _GuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton ev)
			{
				if (ev.DoubleClick)
				{
					Vector2 mousePos = GetLocalMousePosition();
					TreeItem target = GetItemAtPosition(mousePos);
					if (target != null)
					{
						Instance apparentSelect = cachedItems[target];

						if (apparentSelect is BaseScript baseScript)
						{
							EngineUI.Instance.LoadIntoCurrentScript(baseScript);
							Netisu.Properties.PropertiesManager.Load(baseScript);
						}
					}
				}

				if ((int)ev.ButtonIndex == 2)
				{
					Vector2 _pos = GetLocalMousePosition();
					Vector2I MousePosAsVec2I = DisplayServer.MouseGetPosition();
					TreeItem tree_item = GetItemAtPosition(_pos);
					if (tree_item != null && GetSelected() != null)
					{
						TreePopUpMenu.Position = MousePosAsVec2I;
						TreePopUpMenu.Popup();
						(TreePopUpMenu as TreeMenuRMB).ForTreeItem = tree_item;
						(TreePopUpMenu as TreeMenuRMB).RelatedObject = cachedItems[tree_item];
					}
				}
			}
		}
	}
}
