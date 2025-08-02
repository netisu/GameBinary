using Godot;
using System;
using Netisu.Datamodels;
using System.Linq;
using Netisu;
using Netisu.Client;
using System.Collections.Generic;

namespace Netisu.Game.Map
{
	public partial class Importer : Node
	{
		public bool InitializedSpawnPoint = false;

		// This property will hold the parsed map data when running on a server.
		public Godot.Collections.Dictionary ParsedMapData { get; private set; }

		private static Instance InstanceFromPackedScene(string _path) => GD.Load<PackedScene>(_path).Instantiate<Instance>();

		private static void DeserializeCommonTransform(ActiveInstance _object, Godot.Collections.Array _p, Godot.Collections.Array _s, Godot.Collections.Array _r)
		{
			try
			{
				if (_p != null && _p.Count != 0)
					_object.Position = new(_p[0].As<float>(), _p[1].As<float>(), _p[2].As<float>());

				if (_s != null && _s.Count != 0)
					_object.Scale = new(_s[0].As<float>(), _s[1].As<float>(), _s[2].As<float>());

				if (_r != null && _r.Count != 0)
					_object.Rotation = new(_r[0].As<float>(), _r[1].As<float>(), _r[2].As<float>());
			}
			catch (Exception err)
			{
				GD.PrintErr($"Error during transform deserialization: {err.Message}");
			}
		}

		private void LoopAddChildren(Godot.Collections.Array _d, Node _inode)
		{
			if (_d == null) return;
			foreach (Variant item in _d)
			{
				var CluaChildPackedObject = (Godot.Collections.Dictionary)item;
				SwitchCluaPackedObject(CluaChildPackedObject, _inode);
			}
		}

		private Node DeserializePart(Godot.Collections.Dictionary _d)
		{
			Node NodePart = CluaObjectInstance(3);
			Part part = NodePart as Part;
			Godot.Collections.Array ColorArray = _d["color"].As<Godot.Collections.Array>();
			Godot.Collections.Array PositionArray = _d["position"].As<Godot.Collections.Array>();
			Godot.Collections.Array ScaleArray = _d["scale"].As<Godot.Collections.Array>();
			Godot.Collections.Array RotationArray = _d["rotation"].As<Godot.Collections.Array>();

			NodePart.Name = _d["name"].ToString();

			part.Color = new PreservedGlobalClasses.Col3(ColorArray[0].As<float>(), ColorArray[1].As<float>(), ColorArray[2].As<float>());
			CallDeferred(nameof(DeserializeCommonTransform), part, PositionArray, ScaleArray, RotationArray);

			Godot.Collections.Array _c = _d["children"].As<Godot.Collections.Array>();
			LoopAddChildren(_c, NodePart);

			return NodePart;
		}

		private Sun DeserializeSun(Godot.Collections.Dictionary _d)
		{
			Node NodeSun = CluaObjectInstance(1);
			Sun sun = NodeSun as Sun;
			Godot.Collections.Array PositionArray = _d["position"].As<Godot.Collections.Array>();
			Godot.Collections.Array RotationArray = _d["rotation"].As<Godot.Collections.Array>();
			Godot.Collections.Array _c = _d["children"].As<Godot.Collections.Array>();
			LoopAddChildren(_c, NodeSun);
			return sun;
		}

		private PointLight DeserializePointLight(Godot.Collections.Dictionary _d)
		{
			PointLight pointLight = CluaObjectInstance(8) as PointLight;
			Godot.Collections.Array PositionArray = _d["position"].As<Godot.Collections.Array>();
			Godot.Collections.Array RotationArray = _d["rotation"].As<Godot.Collections.Array>();
			Godot.Collections.Array _c = _d["children"].As<Godot.Collections.Array>();
			LoopAddChildren(_c, pointLight);

			pointLight.Intensity = _d["Intensity"].As<float>();
			pointLight.Range = (float)_d["Range"];
			pointLight.Shadows = (bool)_d["Shadows"];

			CallDeferred(nameof(DeserializeCommonTransform), pointLight, PositionArray, default(Godot.Collections.Array), RotationArray);
			return pointLight;
		}

		private Instance DeserializeFolder(Godot.Collections.Dictionary _d)
		{
			Instance NodeFolder = CluaObjectInstance(6);
			Godot.Collections.Array _c = _d["children"].As<Godot.Collections.Array>();
			Godot.Collections.Array PositionArray = _d["position"].As<Godot.Collections.Array>();
			NodeFolder.Name = _d["name"].ToString();
			CallDeferred(nameof(DeserializeCommonTransform), NodeFolder as ActiveInstance, PositionArray, default(Godot.Collections.Array), default(Godot.Collections.Array));
			LoopAddChildren(_c, NodeFolder);
			return NodeFolder;
		}

		private Instance DeserializeSeat(Godot.Collections.Dictionary _d)
		{
			Instance seat = CluaObjectInstance(2);
			seat.Name = _d["name"].ToString();
			Godot.Collections.Array PositionArray = _d["position"].As<Godot.Collections.Array>();
			Godot.Collections.Array RotationArray = _d["rotation"].As<Godot.Collections.Array>();
			CallDeferred(nameof(DeserializeCommonTransform), seat as ActiveInstance, PositionArray, default(Godot.Collections.Array), RotationArray);
			return seat;
		}

		private Instance DeserializeSpawnPoint(Godot.Collections.Dictionary _d)
		{
			if (InitializedSpawnPoint) return null;
			InitializedSpawnPoint = true;
			Instance spawnpoint = CluaObjectInstance(7);
			spawnpoint.Name = _d["name"].ToString();
			Godot.Collections.Array PositionArray = _d["position"].As<Godot.Collections.Array>();
			Godot.Collections.Array RotationArray = _d["rotation"].As<Godot.Collections.Array>();
			CallDeferred(nameof(DeserializeCommonTransform), (ActiveInstance)spawnpoint, PositionArray, default(Godot.Collections.Array), RotationArray);
			return spawnpoint;
		}

		private static Instance DeserializeScript(Godot.Collections.Dictionary _d, bool localscript = false)
		{
			Instance script = localscript ? CluaObjectInstance(4) : CluaObjectInstance(5);
			if (script is BaseScript skriptObject)
			{
				skriptObject.Source = _d["content"].ToString();
			}
			return script;
		}

		private static void SetColor(Part part, Godot.Collections.Array colorArray)
		{
			part.Color = new PreservedGlobalClasses.Col3(colorArray[0].As<float>(), colorArray[1].As<float>(), colorArray[2].As<float>());
		}

		public Instance SwitchCluaPackedObject(Godot.Collections.Dictionary CluaPackedObject, Node _Parent)
		{
			switch (CluaPackedObject["type"].ToString())
			{
				case "Part":
					Node partNode = DeserializePart(CluaPackedObject);
					Part part = partNode as Part;
					part.Anchored = (bool)CluaPackedObject["anchored"];
					_Parent.AddChild(partNode, true);
					part.Shape = CluaPackedObject["shape"].As<string>();
					return part;
				case "Sun":
					Instance sun = DeserializeSun(CluaPackedObject);
					_Parent.AddChild(sun, true);
					return sun;
				case "Folder":
					Instance folder = DeserializeFolder(CluaPackedObject);
					_Parent.AddChild(folder, true);
					return folder;
				case "Seat":
					Instance seat = DeserializeSeat(CluaPackedObject);
					_Parent.AddChild(seat, true);
					return seat;
				case "Spawnpoint":
					Instance spawnpoint = DeserializeSpawnPoint(CluaPackedObject);
					if (spawnpoint != null) _Parent.AddChild(spawnpoint, true);
					return spawnpoint;
				case "PointLight":
					Instance pointlight = DeserializePointLight(CluaPackedObject);
					_Parent.AddChild(pointlight, true);
					return pointlight;
			}
			return null;
		}

		public void Import(string MapJson, Node _Parent, Workshop.GameExplorer _GameExplorer = null)
		{
			Json _j_client = new();
			if (_j_client.Parse(MapJson) != Error.Ok)
			{
				GD.PrintErr("Failed to parse map JSON.");
				return;
			}

			var data = _j_client.Data.As<Godot.Collections.Dictionary>();
			if (data == null) return;


			if (data.ContainsKey("Environment"))
			{
				var environmentNode = _Parent.GetNode<Datamodels.Environment>("Environment");
				if (environmentNode != null)
				{
					var envData = data["Environment"].As<Godot.Collections.Dictionary>();
					if (envData.ContainsKey("Brightness"))
						environmentNode.Brightness = envData["Brightness"].As<float>();
					if (envData.ContainsKey("DayTime"))
						environmentNode.DayTime = envData["DayTime"].As<float>();
					/*
				if (envData.ContainsKey("ManualTimeControl")) 
					environmentNode.ManualTimeControl = envData["ManualTimeControl"].As<bool>();
					*/
				}
			}

			Node mapNode = _Parent.GetNodeOrNull("Map");
			if (mapNode != null && data.ContainsKey("Map"))
			{
				var mapArray = data["Map"].As<Godot.Collections.Array>();
				if (mapArray != null)
				{
					foreach (Variant item in mapArray)
					{
						Instance newObj = SwitchCluaPackedObject((Godot.Collections.Dictionary)item, mapNode);
						_GameExplorer?.AddObjectToGameExplorer(newObj);
					}
				}
			}
		}

		public static Instance CluaObjectInstance(int _iid)
		{
			return _iid switch
			{
				1 => InstanceFromPackedScene("res://Prefabs/Sun.tscn"),
				2 => InstanceFromPackedScene("res://Prefabs/Seat.tscn"),
				3 => InstanceFromPackedScene("res://Prefabs/Cube.tscn"),
				4 => InstanceFromPackedScene("res://Prefabs/LocalScript.tscn"),
				5 => InstanceFromPackedScene("res://Prefabs/ServerScript.tscn"),
				6 => InstanceFromPackedScene("res://Prefabs/Folder.tscn"),
				7 => InstanceFromPackedScene("res://Prefabs/SpawnPoint.tscn"),
				8 => InstanceFromPackedScene("res://Prefabs/PointLight.tscn"),
				_ => null,
			};
		}
	}
}
