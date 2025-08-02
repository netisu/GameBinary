using Godot;
using Netisu.Datamodels;

namespace Netisu.Game
{
	public static class CluaObjectList
	{
		public static Godot.Collections.Dictionary GetObjectInformation(Instance instance, bool forExport = false)
		{
			if (!GodotObject.IsInstanceValid(instance) || !instance.IsInsideTree())
			{
				// If the object is invalid or not in the tree, return an empty dictionary.
				return new Godot.Collections.Dictionary();
			}
			Godot.Collections.Array children = [];
			foreach (Node child in instance.GetChildren())
			{
				if (child is Instance childAsInstance)
				{
					children.Add(GetObjectInformation(childAsInstance, forExport));
				}
			}
			Godot.Collections.Dictionary _d = new()
			{
				["type"] = instance.GetType().Name
			};

			if (!forExport)
				_d["ngine_node_path"] = instance.GetPath();

			_d["name"] = instance.Name;
			switch (instance.GetType().Name)
			{
				case "Part":
					Part part = instance as Part;
					_d["position"] = new Godot.Collections.Array { part.Position.x, part.Position.y, part.Position.z };
					_d["rotation"] = new Godot.Collections.Array { part.Rotation.x, part.Rotation.y, part.Rotation.z };
					_d["scale"] = new Godot.Collections.Array { part.Scale.x, part.Scale.y, part.Scale.z };
					_d["color"] = new Godot.Collections.Array { part.Color.r, part.Color.g, part.Color.b };
					_d["anchored"] = part.Anchored;
					_d["collisions"] = part.Collisions;
					_d["transparency"] = part.Transparency;
					_d["shape"] = part.Shape;
					_d["children"] = children;
					return _d;
				case "Sun":
					Sun sun = instance as Sun;
					_d["position"] = new Godot.Collections.Array { sun.Position.x, sun.Position.y, sun.Position.z };
					_d["rotation"] = new Godot.Collections.Array { sun.Rotation.x, sun.Rotation.y, sun.Rotation.z };
					_d["intensity"] = 1.0f;
					_d["MaximumDistance"] = 100f;
					_d["children"] = children;
					return _d;
				case "Folder":
					Folder folder = instance as Folder;
					_d["position"] = new Godot.Collections.Array { folder.Position.x, folder.Position.y, folder.Position.z };
					_d["children"] = children;
					return _d;
				case "PointLight":
					PointLight pointlight = instance as PointLight;
					_d["position"] = new Godot.Collections.Array { pointlight.Position.x, pointlight.Position.y, pointlight.Position.z };
					_d["rotation"] = new Godot.Collections.Array { pointlight.Rotation.x, pointlight.Rotation.y, pointlight.Rotation.z };
					_d["Intensity"] = pointlight.Intensity;
					_d["Range"] = pointlight.Range;
					_d["Shadows"] = pointlight.Shadows;
					_d["children"] = children;
					return _d;
				case "Spawnpoint":
					Spawnpoint spawnpoint = instance as Spawnpoint;
					_d["position"] = new Godot.Collections.Array { spawnpoint.Position.x, spawnpoint.Position.y, spawnpoint.Position.z };
					_d["rotation"] = new Godot.Collections.Array { spawnpoint.Rotation.x, spawnpoint.Rotation.y, spawnpoint.Rotation.z };
					_d["children"] = children;

					return _d;
				case "LocalScript":
					BaseScript local_script = instance as BaseScript;
					_d["content"] = local_script.Source;
					return _d;
				case "Script":
					BaseScript _script = instance as BaseScript;
					_d["content"] = _script.Source;
					return _d;

				// ui components
				case "UiLabel":
					UiLabel uiLabel = instance as UiLabel;
					_d["position"] = new Godot.Collections.Array { uiLabel.Position.x, uiLabel.Position.y };
					_d["size"] = new Godot.Collections.Array { uiLabel.Size.x, uiLabel.Size.y };
					_d["text"] = uiLabel.Text;
					return _d;
				case "UIHorizontalLayout":
					UIHorizontalLayout uIHorizontalLayout = instance as UIHorizontalLayout;
					_d["position"] = new Godot.Collections.Array { uIHorizontalLayout.Position.x, uIHorizontalLayout.Position.y };
					_d["size"] = new Godot.Collections.Array { uIHorizontalLayout.Size.x, uIHorizontalLayout.Size.y };
					_d["separation"] = uIHorizontalLayout.Separation;
					return _d;

				case "UiPanel":
					UiPanel uiPanel = instance as UiPanel;
					_d["color"] = new Godot.Collections.Array { uiPanel.BackgroundColor.r, uiPanel.BackgroundColor.g, uiPanel.BackgroundColor.b, uiPanel.BackgroundColor.a };
					_d["position"] = new Godot.Collections.Array { uiPanel.Position.x, uiPanel.Position.y };
					_d["size"] = new Godot.Collections.Array { uiPanel.Size.x, uiPanel.Size.y };
					_d["border_corner_radius"] = uiPanel.BorderCornerRadius;
					return _d;
				case "UiButton":
					UiButton uiButton = instance as UiButton;
					_d["bg_color"] = new Godot.Collections.Array { uiButton.BackgroundColor.r, uiButton.BackgroundColor.g, uiButton.BackgroundColor.b, uiButton.BackgroundColor.a };
					_d["text_color"] = new Godot.Collections.Array { uiButton.TextColor.r, uiButton.TextColor.g, uiButton.TextColor.b, uiButton.TextColor.a };
					_d["position"] = new Godot.Collections.Array { uiButton.Position.x, uiButton.Position.y };
					_d["size"] = new Godot.Collections.Array { uiButton.Size.x, uiButton.Size.y };
					return _d;
				case "Environment":
					Datamodels.Environment _environment = instance as Datamodels.Environment;
					_d["Brightness"] = _environment.Brightness;
					_d["DayTime"] = _environment.DayTime;
					// _d["ManualTimeControl"] = _environment.ManualTimeControl;
					return _d;
			}

			if (forExport) return null;

			switch (instance.GetType().Name)
			{
				case "Map":
					return _d;
				case "LocalScripts":
					return _d;
				default:
					return null;
			}
		}
	}
}
