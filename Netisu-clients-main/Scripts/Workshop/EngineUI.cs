using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Netisu.Datamodels;
using Netisu.Properties;
using System.Diagnostics;

namespace Netisu.Workshop
{
	public partial class EngineUI : Control
	{
		[Export]
		public Netisu.Datamodels.Game GameDataModel = null!;
		public Dictionary<string, PanelContainer> ExistingTabs = [];
		public BaseScript CurrentScriptLoaded { get; private set; } = null;
		public Godot.Collections.Dictionary CurrentPropertiesLoaded = [];

		public static EngineUI Instance { get; private set; } = null!;

		[Export] public CodeEdit CodeEditor = null;
		[Export] public AnimationPlayer AnimationPlayer;
		[Export] public PanelContainer ActiveTab = null;
		[Export] public HBoxContainer TabsContainer;
		[Export] public VBoxContainer PropertiesContainer;

		[Export] public UserInterface userInterface;

		private CodeHighlighter _highlighter = null!;
		private readonly Dictionary<string, string> _preserved_icons = new() {
		{"Map", "res://Assets/ProFontsicons/map.svg"},
	};
		private readonly List<string> _preserved_lua = [
			"and",
		"do",
		"else",
		"elseif",
		"end",
		"false",
		"for",
		"function",
		"if",
		"in",
		"local",
		"nil",
		"not",
		"or",
		"repeat",
		"return",
		"then",
		"true",
		"until",
		"while",
	];

		private readonly Dictionary<string, string> _preserved_global_functions = new() {
		{"printl", "printl("},
		{"wait", "wait("},
	};

		private readonly List<string> _preserved_global_keywords = [
			"__SCRIPT__",
	];

		private readonly List<string> _preserved_global_classes = [
			"GlobalState",
		"Game",
		"Map",
	];

		public override void _Ready()
		{
			Instance = this;
			PhysicsServer3D.SetActive(false);
			BuildTab("Game", false, true);
			SetUpCodeEditor();
		}

		private void SetUpCodeEditor()
		{
			_highlighter = (CodeHighlighter)CodeEditor.SyntaxHighlighter;
			foreach (string item in _preserved_lua)
			{
				_highlighter.AddKeywordColor(item, Color.FromString("D77C79", Colors.White));
			}
			foreach (string item in _preserved_global_classes)
			{
				_highlighter.AddKeywordColor(item, Color.FromString("8fffdb", Colors.White));
			}
			_highlighter.AddColorRegion("--[[", "]]", Color.FromString("6a9955", Colors.White));
			_highlighter.AddColorRegion("--", "", Color.FromString("6a9955", Colors.White));
			_highlighter.AddColorRegion("\"", "\"", Color.FromString("E6A472", Colors.White));
			_highlighter.AddColorRegion("'", "'", Color.FromString("E6A472", Colors.White));
		}

		public static PackedScene InstanceFromPackedScene(string _u) => GD.Load<PackedScene>(_u);

		public void BuildTab(string name, bool forScript = false, bool isGame = false)
		{
			if (ExistingTabs.ContainsKey(name))
				return;
			if (ExistingTabs.Count == 11)
			{
				return;
			}
			PackedScene _i = InstanceFromPackedScene("res://Prefabs/UIScenes/Tab.tscn");
			PanelContainer _pc = (PanelContainer)_i.Instantiate();
			ImageTexture _icon = new();

			if (ActiveTab != null)
			{
				ActiveTab.GetNode<Button>("HBoxContainer/Exit").Visible = false;
				ActiveTab.GetNode<Panel>("Active").Visible = false;
			}

			ActiveTab = _pc;

			_pc.GetNode<Panel>("Active").Visible = true;
			_pc.GetNode<Label>("HBoxContainer/Label").Text = name;

			if (forScript)
			{
				_icon = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Assets/Icons/ScriptExtend.svg"));
			}
			else
			{
				_icon = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Assets/Icons/Script.svg"));
			}
			if (isGame)
			{
				_icon = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Assets/Icons/Environment.svg"));
			}

			if (name != "Game") { ActiveTab.GetNode<Button>("HBoxContainer/Exit").Visible = true; } else { ActiveTab.GetNode<Button>("HBoxContainer/Exit").Visible = false; }
			_pc.GetNode<TextureRect>("HBoxContainer/TextureRect").Texture = _icon;
			TabsContainer.AddChild(_pc);
			ExistingTabs.Add(name, _pc);
			_pc.GetNode<Button>("BodyButton").Pressed += () => OnBodyButtonPressed(_pc);
			_pc.GetNode<Button>("HBoxContainer/Exit").Pressed += () => CloseTab(name);
		}

		public void OnBodyButtonPressed(PanelContainer dd)
		{
			string scriptName = dd.GetNode<Label>("HBoxContainer/Label").Text;
			if (scriptName == "Game")
			{
				SwitchToTab("Game");
				return;
			}
			LoadIntoCurrentScript((BaseScript)GetNode("/root/Root/EngineGUI/SubViewportContainer/SubViewport/Game").FindChild(scriptName, true, false));
		}

		public void SwitchToTab(string name)
		{
			if (name == ActiveTab.GetNode<Label>("HBoxContainer/Label").Text)
				return;
			if (name == "Game")
				ExitCurrentScript();
			ActiveTab.GetNode<Panel>("Active").Visible = false;
			ActiveTab.GetNode<Button>("HBoxContainer/Exit").Visible = false;
			ActiveTab = ExistingTabs[name];
			if (name != "Game") { ActiveTab.GetNode<Button>("HBoxContainer/Exit").Visible = true; }
			ActiveTab.GetNode<Panel>("Active").Visible = true;
		}

		public void CloseTab(string name)
		{
			SwitchToTab("Game");
			if (IsInstanceValid(ExistingTabs[name]))
				ExistingTabs[name].QueueFree();
			ExistingTabs.Remove(name);
		}

		public async void BuildInstance(string type)
		{
			if (type == "Script")
				type = "ServerScript";

			var path = $"res://Prefabs/{type}.tscn";
			PackedScene packedScene = InstanceFromPackedScene(path);

			if (packedScene == null)
			{
				GD.PrintErr($"Failed to load scene at path: {path}");
				return;
			}

			Node instanced = packedScene.Instantiate();
			instanced.Name = type;

			var gameNode = GetNode<Node>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/Game");
			Gizmo3DPlugin.Gizmo3D.Instance.ClearSelection();

			//  Determine the correct parent and add the new node to the scene tree
			Node parentNode;
			switch (type)
			{
				case "LocalScript":
					parentNode = gameNode.GetNode<LocalScripts>("LocalScripts");
					break;
				case "ServerScript":
					parentNode = gameNode.GetNode<Scripts>("Scripts");
					break;
				default:
					if (instanced is UiInstance)
					{
						parentNode = userInterface;
					}
					else
					{
						parentNode = gameNode.GetNode<Map>("Map");
					}
					break;
			}
			parentNode.AddChild(instanced, true);

			//  Wait for one frame so Godot can fully initialize the new node
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			if (!IsInstanceValid(instanced)) // Safety check in case it was deleted during the wait
				return;

			GameExplorer.Instance.AddObjectToGameExplorer(instanced as Instance);

			if (instanced is ActiveInstance activeInstance && !(instanced is UiInstance))
			{
				Gizmo3DPlugin.Gizmo3D.Instance.Select(activeInstance);
				PropertiesManager.Load(activeInstance);
			}
			else if (instanced is UiInstance uiInstance)
			{
				PropertiesManager.Load(uiInstance);
			}
			else if (instanced is BaseScript baseScript)
			{
				BuildTab(instanced.Name, false, true);
				SwitchToTab(instanced.Name);
				LoadIntoCurrentScript(baseScript);
			}
		}

		public static void OpenDocs()
		{
			OS.ShellOpen("https://client-docs.netisu.com/");
		}

		public static void GizmoModeUpdate(int to)
		{
			EngineCamera.Instance.inColorMode = false;
			switch (to)
			{
				case 0: Gizmo3DPlugin.Gizmo3D.Instance.Mode = Gizmo3DPlugin.Gizmo3D.ToolMode.Select; break;
				case 1: Gizmo3DPlugin.Gizmo3D.Instance.Mode = Gizmo3DPlugin.Gizmo3D.ToolMode.Move; break;
				case 2: Gizmo3DPlugin.Gizmo3D.Instance.Mode = Gizmo3DPlugin.Gizmo3D.ToolMode.Scale; break;
				case 3: Gizmo3DPlugin.Gizmo3D.Instance.Mode = Gizmo3DPlugin.Gizmo3D.ToolMode.Rotate; break;
			}
		}


		public void LoadIntoCurrentScript(BaseScript script)
		{
			BuildTab(script.GetName());
			ActivityManager.Instance?.discordRpcClient.UpdateState($"Editing: {script.GetName()}.lua");
			if (ExistingTabs.ContainsKey(script.GetName()))
				SwitchToTab(script.GetName());
			if (CurrentScriptLoaded == null)
			{
				CodeEditor.GetParent().GetParent().GetNode<SubViewportContainer>("SubViewportContainer").Visible = false;
				CodeEditor.GetParent<Control>().Visible = true;
				CodeEditor.Text = script.Source;
				CurrentScriptLoaded = script;
			}
			else if (CurrentScriptLoaded != script)
			{
				WriteIntoScript(CurrentScriptLoaded, CodeEditor.Text);
				CurrentScriptLoaded = null;
				LoadIntoCurrentScript(script);
			}
		}

		public void ExitCurrentScript()
		{
			if (CurrentScriptLoaded != null)
			{
				CurrentScriptLoaded.Source = CodeEditor.Text;
				CodeEditor.GetParent().GetParent().GetNode<SubViewportContainer>("SubViewportContainer").Visible = true;
				CodeEditor.GetParent<Control>().Visible = false;
				CurrentScriptLoaded = null;
				ActivityManager.Instance.discordRpcClient.UpdateState($"Editing workspace");
				EngineCamera.Instance.Disabled = false;
			}
		}

		public void Quit()
		{
			Engine3D.Instance.ShutPlaytest();
			GetTree().Quit();
		}

		public void Collapse(string what)
		{
			var control = GetNode<Control>(what);
			control.Visible = !control.Visible;
		}

		public static void WriteIntoScript(BaseScript script, string content) => script.Source = content;

		public void LoadIntoProperties(Godot.Collections.Dictionary properties)
		{

			CurrentPropertiesLoaded = properties;
			ClearOutExistingPropertyTabs();
			PropertiesContainer.Visible = true;
			

			string type = properties["type"].ToString();

			if (!properties.ContainsKey("type"))
				return;

			if (properties.ContainsKey("name"))
			{
				PropertiesContainer.GetNode<LineEdit>("EngineAttr/Name").Text = properties["name"].ToString();
			}
			else
			{
				// If there's no name, leave it blank.
				PropertiesContainer.GetNode<LineEdit>("EngineAttr/Name").Text = "";
			}
			
			PropertiesContainer.GetNode<Label>("EngineAttr/Type").Text = type;

			if (type == "Folder")
			{
				if (properties.ContainsKey("position")) AddVec3Property("Position", (Godot.Collections.Array)properties["position"], "POSITION_GENERAL_PART");
			}

			if (type == "Part")
			{
				AddOptionsProperty("Shape", "OPTIONS_SHAPE_PART", "Cube");
				if (properties.ContainsKey("position")) AddVec3Property("Position", (Godot.Collections.Array)properties["position"], "POSITION_GENERAL_PART");
				if (properties.ContainsKey("scale")) AddVec3Property("Scale", (Godot.Collections.Array)properties["scale"], "SCALE_GENERAL_PART");
				if (properties.ContainsKey("rotation")) AddVec3Property("Rotation", (Godot.Collections.Array)properties["rotation"], "ROTATION_GENERAL_PART");
				if (properties.ContainsKey("collisions")) AddBoolProperty("Collisions Enabled", properties["collisions"], "COLLISIONS_PART");
				if (properties.ContainsKey("anchored")) AddBoolProperty("Anchored", properties["anchored"], "BOOL_ANCHORED_PART");
				if (properties.ContainsKey("color")) AddCol3Property("Color", (Godot.Collections.Array)properties["color"], "COLOR_PART");
				if (properties.ContainsKey("transparency")) AddSliderProperty("Transparency", properties["transparency"].As<float>(), "SLIDER_TRANSPARENCY_PART");
				AddOptionsProperty("Studs type", "OPTIONS_STUDS_TYPE", "None", "Outlet", "Inlet");
				return;
			}

			if (type == "Sun")
			{
				if (properties.ContainsKey("position")) AddVec3Property("Position", (Godot.Collections.Array)properties["position"], "POSITION_GENERAL_SUN");
				if (properties.ContainsKey("rotation")) AddVec3Property("Rotation", (Godot.Collections.Array)properties["rotation"], "ROTATION_GENERAL_SUN");
				if (properties.ContainsKey("intensity")) AddFloatProperty("Intensity", properties["intensity"], "FLOAT_INTENSITY_SUN");
				return;
			}

			if (type == "PointLight")
			{
				if (properties.ContainsKey("position")) AddVec3Property("Position", (Godot.Collections.Array)properties["position"], "POSITION_GENERAL_LIGHT");
				if (properties.ContainsKey("rotation")) AddVec3Property("Rotation", (Godot.Collections.Array)properties["rotation"], "ROTATION_GENERAL_LIGHT");
				if (properties.ContainsKey("Intensity")) AddFloatProperty("Intensity", properties["Intensity"], "FLOAT_INTENSITY_POINT_LIGHT");
				if (properties.ContainsKey("Range")) AddFloatProperty("Range", properties["Range"], "FLOAT_RANGE_POINT_LIGHT");
				if (properties.ContainsKey("Shadows")) AddBoolProperty("Shadows Enabled", properties["Shadows"], "BOOL_SHADOW_POINT_LIGHT");
			}

			if (type == "Spawnpoint")
			{
				if (properties.ContainsKey("position")) AddVec3Property("Position", (Godot.Collections.Array)properties["position"], "POSITION_SPAWNPOINT");
			}

			 if (type == "Environment")
			{
				if (properties.ContainsKey("Brightness")) AddFloatProperty("Brightness", properties["Brightness"], "FLOAT_BRIGHTNESS_ENV");
				if (properties.ContainsKey("ManualTimeControl")) AddBoolProperty("Manual Time Control", properties["ManualTimeControl"], "BOOL_MANUAL_TIME_ENV");
				if (properties.ContainsKey("DayTime")) AddSliderProperty("Time of Day (24h)", properties["DayTime"].As<float>(), "SLIDER_DAY_TIME_ENV", 0, 24);
				if (properties.ContainsKey("VolumetricFogEnabled")) AddBoolProperty("Use Volumetric Fog", properties["VolumetricFogEnabled"], "BOOL_VOL_FOG_ENABLED_ENV");
				if (properties.ContainsKey("SSREnabled")) AddBoolProperty("Use Screen Space Reflections", properties["SSREnabled"], "BOOL_SSR_ENABLED_ENV");
				if (properties.ContainsKey("SSAOEnabled")) AddBoolProperty("Use SSAO", properties["SSAOEnabled"], "BOOL_SSAO_ENABLED_ENV");
				return;
			}

			if (type == "UiLabel")
			{
				if (properties.ContainsKey("position")) AddVec2Property("Position", (Godot.Collections.Array)properties["position"], "UI_POSITION_GENERAL_LABEL");
				if (properties.ContainsKey("size")) AddVec2Property("Size", (Godot.Collections.Array)properties["size"], "UI_SIZE_GENERAL_LABEL");
				if (properties.ContainsKey("text")) AddStringProperty("Label Text", properties["text"].ToString(), "STRING_UI_LABEL_TEXT");
				return;
			}

			if (type == "UIHorizontalLayout")
			{
				if (properties.ContainsKey("position")) AddVec2Property("Position", (Godot.Collections.Array)properties["position"], "UI_POSITION_GENERAL_UIHL");
				if (properties.ContainsKey("size")) AddVec2Property("Size", (Godot.Collections.Array)properties["size"], "UI_SIZE_GENERAL_UIHL");
				if (properties.ContainsKey("separation")) AddFloatProperty("Separation", properties["separation"], "FLOAT_SEPARATON_HBOX");
				return;
			}

			if (type == "UiPanel")
			{
				if (properties.ContainsKey("position")) AddVec2Property("Position", (Godot.Collections.Array)properties["position"], "UI_POSITION_GENERAL_UIP");
				if (properties.ContainsKey("size")) AddVec2Property("Size", (Godot.Collections.Array)properties["size"], "UI_SIZE_GENERAL_UIP");
				if (properties.ContainsKey("color")) AddCol3Property("Color", (Godot.Collections.Array)properties["color"], "COLOR_UIP");
				if (properties.ContainsKey("border_corner_radius")) AddFloatProperty("Border Corner Radius", properties["border_corner_radius"], "FLOAT_CORNER_RADIUS_BORDER_UIP");
				return;
			}

			if (type == "UiButton")
			{
				if (properties.ContainsKey("position")) AddVec2Property("Position", (Godot.Collections.Array)properties["position"], "UI_POSITION_GENERAL_UIB");
				if (properties.ContainsKey("size")) AddVec2Property("Size", (Godot.Collections.Array)properties["size"], "UI_SIZE_GENERAL_UIB");
				if (properties.ContainsKey("bg_color")) AddCol3Property("Background Color", (Godot.Collections.Array)properties["bg_color"], "COLOR_BG_UIB");
				if (properties.ContainsKey("text_color")) AddCol3Property("Text Color", (Godot.Collections.Array)properties["text_color"], "COLOR_TEXT_UIB");
				return;
			}

			if (type == "MeshPart")
			{
				List<string> dataList = ["[REST]"];
				foreach ((Variant _key, Variant _value) in properties)
				{
					if (_key.ToString().StartsWith("=>Animation"))
					{
						dataList.Add(_value.ToString());
					}
				}
				string[] data = [.. dataList];
				if (properties.ContainsKey("let_engine_make_collisions")) AddBoolProperty("Let engine generate collisions", properties["let_engine_make_collisions"], "BOOL_LET_ENGINE_MAKE_COLLISIONS");
				AddOptionsProperty("Animation To Play On Start", "OPTIONS_ANIM_ON_START_MESH_PART", data);
				AddOptionsProperty("Preview Animations", "OPTIONS_ANIM_PREV_MESH_PART", data);
			}

			if (type == "LocalScript")
			{
				if (properties.ContainsKey("content")) AddCodeProperty("Script Preview", properties["content"].ToString());
			}

			if (type == "ServerScript")
			{
				if (properties.ContainsKey("content")) AddCodeProperty("Script Preview", properties["content"].ToString());
			}
		}

		private void AddVec3Property(string title, Godot.Collections.Array array = null, string friendlyPropertyName = null)
		{
			var dict = new Godot.Collections.Dictionary
		{
			{"title", title},
			{"x", Math.Round((double)array[0], 4).ToString()},
			{"y", Math.Round((double)array[1], 4).ToString()},
			{"z", Math.Round((double)array[2], 4).ToString()}
		};
			MakeAndAddProperty("vec_3", dict, friendlyPropertyName);
		}

		private void AddVec2Property(string title, Godot.Collections.Array array = null, string friendlyPropertyName = null)
		{
			var dict = new Godot.Collections.Dictionary
		{
			{"title", title},
			{"x", Math.Round((double)array[0], 4).ToString()},
			{"y", Math.Round((double)array[1], 4).ToString()},
		};
			MakeAndAddProperty("vec_2", dict, friendlyPropertyName);
		}

		private void AddBoolProperty(string title, Variant value, string friendlyPropertyName = null)
		{
			var dict = new Godot.Collections.Dictionary
		{
			{"title", title},
			{"Value", value}
		};
			MakeAndAddProperty("bool", dict, friendlyPropertyName);
		}

		private void AddOptionsProperty(string title, string friendlyPropertyName = null, params string[] _data)
		{
			var dict = new Godot.Collections.Dictionary
		{
			{"title", title},
		};
			int val = 0;
			foreach (string option in _data)
			{
				dict["Value" + val.ToString()] = option;
				val++;
			}

			MakeAndAddProperty("options", dict, friendlyPropertyName);
		}

		private void AddFloatProperty(string title, Variant Value, string friendlyPropertyName = null)
		{
			var dict = new Godot.Collections.Dictionary
		{
			{"title", title},
			{"Value", Value}
		};
			MakeAndAddProperty("float", dict, friendlyPropertyName);
		}

		private void AddCol3Property(string title, Godot.Collections.Array array, string friendlyPropertyName = null)
		{
			var dict = new Godot.Collections.Dictionary
		{
			{"title", title},
			{"r", array[0].ToString()},
			{"g", array[1].ToString()},
			{"b", array[2].ToString()},
			{"info_text", ""},
			{"ForPart", true},
		};
			MakeAndAddProperty("color", dict, friendlyPropertyName);
		}

		private void AddSliderProperty(string title, float value, string friendlyPropertyName = null, float min = 0, float max = 100)
		{
			var dict = new Godot.Collections.Dictionary { {"title", title}, {"Value", value}, {"Min", min}, {"Max", max} };
			MakeAndAddProperty("slider", dict, friendlyPropertyName);
		}


		private void AddCodeProperty(string title, string code, string friendlyPropertyName = null)
		{
			var dict = new Godot.Collections.Dictionary
		{
			{"title", title},
			{"code", code},
		};
			MakeAndAddProperty("code", dict, friendlyPropertyName);
		}

		private void AddStringProperty(string title, string text, string friendlyPropertyName = null)
		{
			var dict = new Godot.Collections.Dictionary
		{
			{"title", title},
			{"text", text},
		};
			MakeAndAddProperty("string", dict, friendlyPropertyName);
		}

		private void MakeAndAddProperty(string typeName, Godot.Collections.Dictionary data, string friendlyPropertyName = null)
		{
			Control property = MakeProperty(typeName, data, friendlyPropertyName);
			if (property != null)
			{
				PropertiesContainer.AddChild(property);
			}
		}

		private static void AssignTextSubmittedHandler(string prefix, Control _property_control, string friendlyPropertyName, LineEdit.TextSubmittedEventHandler handler, bool isUiInstance = false)
		{
			if (friendlyPropertyName.StartsWith(prefix))
			{
				if (isUiInstance)
				{
					_property_control.GetNode<LineEdit>("Vec2/XValue").TextSubmitted += handler;
					_property_control.GetNode<LineEdit>("Vec2/YValue").TextSubmitted += handler;
					return;
				}
				_property_control.GetNode<LineEdit>("Vec3/XValue").TextSubmitted += handler;
				_property_control.GetNode<LineEdit>("Vec3/YValue").TextSubmitted += handler;
				_property_control.GetNode<LineEdit>("Vec3/ZValue").TextSubmitted += handler;
			}
		}

		private void RegisterPropertyAlter(Control _property_control, string friendlyPropertyName = null)
		{
			AssignTextSubmittedHandler("POSITION", _property_control, friendlyPropertyName, d => OnPositionChangeOfVector3(_property_control, friendlyPropertyName));
			AssignTextSubmittedHandler("SCALE", _property_control, friendlyPropertyName, d => OnScaleChangeOfVector3(_property_control, friendlyPropertyName));
			AssignTextSubmittedHandler("ROTATION", _property_control, friendlyPropertyName, d => OnRotationChangeOfVector3(_property_control, friendlyPropertyName));

			AssignTextSubmittedHandler("UI_POSITION", _property_control, friendlyPropertyName, d => OnPositionChangeOfVector2(_property_control, friendlyPropertyName), true);
			AssignTextSubmittedHandler("UI_SIZE", _property_control, friendlyPropertyName, d => OnSizeChangeOfVector2(_property_control, friendlyPropertyName), true);

			if (friendlyPropertyName.StartsWith("COLOR"))
			{
				_property_control.GetNode<ColorPickerButton>("color/Button").ColorChanged += col => OnColorChangeOfColor(_property_control, friendlyPropertyName);
			}

			if (friendlyPropertyName.StartsWith("FLOAT"))
			{
				_property_control.GetNode<LineEdit>("float/Value").TextSubmitted += _t => OnFloatChange(_property_control, friendlyPropertyName);
			}

			if (friendlyPropertyName.StartsWith("BOOL"))
			{
				_property_control.GetNode<CheckBox>("bool/Value").Pressed += () => OnBoolPropertyChange(_property_control, friendlyPropertyName);
			}

			if (friendlyPropertyName.StartsWith("OPTIONS"))
			{
				_property_control.GetNode<OptionButton>("options/Value").ItemSelected += id => OnOptionChange(_property_control, id, friendlyPropertyName);
			}

			if (friendlyPropertyName.StartsWith("SLIDER"))
			{
				_property_control.GetNode<HSlider>("slider/slider").ValueChanged += id => OnSliderPropertyChange(_property_control, friendlyPropertyName);
			}

			if (friendlyPropertyName.StartsWith("STRING"))
			{
				_property_control.GetNode<LineEdit>("string/Value").TextSubmitted += _t => OnStringChange(_property_control, friendlyPropertyName);
			}
		}

		private void OnBoolPropertyChange(Control _property_control, string friendlyPropertyName = null)
		{
			if (friendlyPropertyName == "BOOL_ANCHORED_PART")
			{
				GetNode<Part>(CurrentPropertiesLoaded["ngine_node_path"].ToString()).Anchored = _property_control.GetNode<CheckBox>("bool/Value").ButtonPressed;
			}

			if (friendlyPropertyName == "BOOL_LET_ENGINE_MAKE_COLLISIONS")
			{
				GetNode<MeshPart>(CurrentPropertiesLoaded["ngine_node_path"].ToString()).let_engine_make_collisions = _property_control.GetNode<CheckBox>("bool/Value").ButtonPressed;
			}
		}

		private void OnSliderPropertyChange(Control _property_control, string friendlyPropertyName = null)
		{
			if (friendlyPropertyName == "SLIDER_TRANSPARENCY_PART")
			{
				Part _r_3d = GetNodeOrNull<Part>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
				if (_r_3d == null)
					return;
				_r_3d.Transparency = (float)_property_control.GetNode<HSlider>("slider/slider").Value / 100;
			}
		}

		private void OnFloatChange(Control _property_control, string friendlyPropertyName = null)
		{
			ReleaseFocusFromProperties(_property_control, "float/Value");
			if (friendlyPropertyName == "FLOAT_INTENSITY_POINT_LIGHT")
			{
				PointLight _r_3d = GetNodeOrNull<PointLight>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
				_r_3d.Intensity = float.Parse(_property_control.GetNode<LineEdit>("float/Value").Text);
				return;

			}
			if (friendlyPropertyName == "FLOAT_RANGE_POINT_LIGHT")
			{
				PointLight _r_3d = GetNodeOrNull<PointLight>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
				_r_3d.Range = float.Parse(_property_control.GetNode<LineEdit>("float/Value").Text);
			}

			if (friendlyPropertyName == "FLOAT_INTENSITY_SUN")
			{
				Sun _r_3d = GetNodeOrNull<Sun>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
				_r_3d.Intensity = float.Parse(_property_control.GetNode<LineEdit>("float/Value").Text);
				return;
			}

			if (friendlyPropertyName == "FLOAT_SEPARATON_HBOX")
			{
				UIHorizontalLayout uIHorizontalLayout = GetNodeOrNull<UIHorizontalLayout>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
				uIHorizontalLayout.Separation = int.Parse(_property_control.GetNode<LineEdit>("float/Value").Text);
			}

			if (friendlyPropertyName == "FLOAT_CORNER_RADIUS_BORDER_UIP")
			{
				UiPanel uiPanel = GetNodeOrNull<UiPanel>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
				uiPanel.BorderCornerRadius = int.Parse(_property_control.GetNode<LineEdit>("float/Value").Text);
			}
		}

		private void OnStringChange(Control _property_control, string friendlyPropertyName = null)
		{
			ReleaseFocusFromProperties(_property_control, "string/Value");
			if (friendlyPropertyName == "STRING_UI_LABEL_TEXT")
			{
				UiLabel uiLabel = GetNode<UiLabel>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
				uiLabel.Text = _property_control.GetNode<LineEdit>("string/Value").Text;
			}
		}

		private void OnOptionChange(Control _property_control, long item_id, string friendlyPropertyName = null)
		{

			if (friendlyPropertyName == "OPTIONS_ANIM_PREV_MESH_PART")
			{
				string txt = _property_control.GetNode<OptionButton>("options/Value").GetItemText((int)item_id);
				GetNodeOrNull<MeshPart>(CurrentPropertiesLoaded["ngine_node_path"].ToString()).Play(txt);
			}

			if (friendlyPropertyName == "OPTIONS_ANIM_ON_START_MESH_PART")
			{
				string txt = _property_control.GetNode<OptionButton>("options/Value").GetItemText((int)item_id);
				AnimationPlayer _am_plr = GetNodeOrNull<AnimationPlayer>(CurrentPropertiesLoaded["ngine_node_path"] + "/GLTFRoot/AnimationPlayer");
				GetNodeOrNull<MeshPart>(CurrentPropertiesLoaded["ngine_node_path"].ToString()).play_anim_on_start = txt;
			}

			if (friendlyPropertyName == "OPTIONS_SHAPE_PART")
			{
				string txt = _property_control.GetNode<OptionButton>("options/Value").GetItemText((int)item_id);
				Part _r_3d = GetNodeOrNull<Part>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
				if (_r_3d == null)
					return;
				_r_3d.Shape = txt;
			}
		}

		private void OnColorChangeOfColor(Control _property_control, string friendlyPropertyName = null)
		{
			var colorpickerbutton = _property_control.GetNode<ColorPickerButton>("color/Button");
			colorpickerbutton.ReleaseFocus();

			Instance _r_3d = GetNodeOrNull<Instance>(CurrentPropertiesLoaded["ngine_node_path"].ToString());

			if (_r_3d is Part part)
			{
				part.Color = new(colorpickerbutton.Color.R, colorpickerbutton.Color.G, colorpickerbutton.Color.B);
				return;
			}

			if (_r_3d is UiPanel uiPanel)
			{
				uiPanel.BackgroundColor = new(colorpickerbutton.Color.R, colorpickerbutton.Color.G, colorpickerbutton.Color.B, colorpickerbutton.Color.A);
			}

			if (_r_3d is UiButton uiButton)
			{
				if (friendlyPropertyName == "COLOR_BG_UIB")
				{
					uiButton.BackgroundColor = new(colorpickerbutton.Color.R, colorpickerbutton.Color.G, colorpickerbutton.Color.B, colorpickerbutton.Color.A);
				}

				if (friendlyPropertyName == "COLOR_TEXT_UIB")
				{
					uiButton.TextColor = new(colorpickerbutton.Color.R, colorpickerbutton.Color.G, colorpickerbutton.Color.B, colorpickerbutton.Color.A);
				}
			}
		}

		private static void ReleaseFocusFromProperties(Control _property_control, params string[] nodePaths)
		{
			foreach (var path in nodePaths)
			{
				_property_control.GetNode<Control>(path).ReleaseFocus();
			}
		}

		private void OnPositionChangeOfVector3(Control _property_control, string friendlyPropertyName = null)
		{
			ReleaseFocusFromProperties(_property_control, "Vec3/XValue", "Vec3/YValue", "Vec3/ZValue");
			Node node = GetNodeOrNull(CurrentPropertiesLoaded["ngine_node_path"].ToString());
			if (node is ActiveInstance activeInstance)
			{
				activeInstance.Position = new(float.Parse(_property_control.GetNode<LineEdit>("Vec3/XValue").Text), float.Parse(_property_control.GetNode<LineEdit>("Vec3/YValue").Text), float.Parse(_property_control.GetNode<LineEdit>("Vec3/ZValue").Text));
			}
		}

		private void OnPositionChangeOfVector2(Control _property_control, string friendlyPropertyName = null)
		{
			ReleaseFocusFromProperties(_property_control, "Vec2/XValue", "Vec2/YValue");
			Node node = GetNodeOrNull(CurrentPropertiesLoaded["ngine_node_path"].ToString());
			if (node is UiInstance uiInstance)
			{
				uiInstance.Position = new(float.Parse(_property_control.GetNode<LineEdit>("Vec2/XValue").Text), float.Parse(_property_control.GetNode<LineEdit>("Vec2/YValue").Text));
			}
		}

		private void OnSizeChangeOfVector2(Control _property_control, string friendlyPropertyName = null)
		{
			ReleaseFocusFromProperties(_property_control, "Vec2/XValue", "Vec2/YValue");
			Node node = GetNodeOrNull(CurrentPropertiesLoaded["ngine_node_path"].ToString());
			if (node is UiInstance uiInstance)
			{
				uiInstance.Size = new(float.Parse(_property_control.GetNode<LineEdit>("Vec2/XValue").Text), float.Parse(_property_control.GetNode<LineEdit>("Vec2/YValue").Text));
			}
		}

		private void OnRotationChangeOfVector3(Control _property_control, string friendlyPropertyName = null)
		{
			if (friendlyPropertyName == null)
				return;

			ReleaseFocusFromProperties(_property_control, "Vec3/XValue", "Vec3/YValue", "Vec3/ZValue");

			ActiveInstance _r_3d = GetNodeOrNull<ActiveInstance>(CurrentPropertiesLoaded["ngine_node_path"].ToString());

			try
			{
				_r_3d.Rotation = new(float.Parse(_property_control.GetNode<LineEdit>("Vec3/XValue").Text), float.Parse(_property_control.GetNode<LineEdit>("Vec3/YValue").Text), float.Parse(_property_control.GetNode<LineEdit>("Vec3/ZValue").Text));
			}
			catch
			{
				return;
			}
		}

		private void OnScaleChangeOfVector3(Control _property_control, string friendlyPropertyName = null)
		{
			ActiveInstance _r_3d = GetNodeOrNull<ActiveInstance>(CurrentPropertiesLoaded["ngine_node_path"].ToString());
			if (_r_3d == null)
				return;

			ReleaseFocusFromProperties(_property_control, "Vec3/XValue", "Vec3/YValue", "Vec3/ZValue");
			_r_3d.Scale = new(float.Parse(_property_control.GetNode<LineEdit>("Vec3/XValue").Text), float.Parse(_property_control.GetNode<LineEdit>("Vec3/YValue").Text), float.Parse(_property_control.GetNode<LineEdit>("Vec3/ZValue").Text));
		}

		public void ClearOutExistingPropertyTabs()
		{
			foreach (Control obj in PropertiesContainer.GetChildren().Cast<Control>())
			{
				if (obj.Name != "EngineAttr")
				{
					obj.GetParent().RemoveChild(obj);
					obj.QueueFree();
				}
			}
			ClearIntoProperties();
		}

		private Control MakeProperty(string typeName, Godot.Collections.Dictionary data, string friendlyPropertyName = null)
		{
			Control res = (Control)InstanceFromPackedScene($"res://Prefabs/UIScenes/{typeName}.tscn").Instantiate();
			string title = data["title"].ToString();
			switch (typeName)
			{
				case "vec_3":
					res.GetNode<Label>("Vec3/Title").Text = title;
					res.GetNode<LineEdit>("Vec3/XValue").Text = data["x"].ToString();
					res.GetNode<LineEdit>("Vec3/YValue").Text = data["y"].ToString();
					res.GetNode<LineEdit>("Vec3/ZValue").Text = data["z"].ToString();
					RegisterPropertyAlter(res, friendlyPropertyName);
					break;
				case "vec_2":
					res.GetNode<Label>("Vec2/Title").Text = title;
					res.GetNode<LineEdit>("Vec2/XValue").Text = data["x"].ToString();
					res.GetNode<LineEdit>("Vec2/YValue").Text = data["y"].ToString();
					RegisterPropertyAlter(res, friendlyPropertyName);
					break;
				case "bool":
					RegisterPropertyAlter(res, friendlyPropertyName);
					res.GetNode<Label>("bool/title").Text = title;
					bool value = (bool)data["Value"];
					var checkBox = res.GetNode<CheckBox>("bool/Value");
					checkBox.ButtonPressed = value;
					checkBox.Text = value ? "Enabled" : "Disabled";
					break;
				case "color":
					RegisterPropertyAlter(res, friendlyPropertyName);
					Color _value_as_color = new((float)data["r"], (float)data["g"], (float)data["b"], 1.0f);
					res.GetNode<Label>("color/title").Text = title;
					res.GetNode<ColorPickerButton>("color/Button").Color = _value_as_color;
					res.GetNode<Label>("color/Label").Text = data["info_text"].ToString();
					break;
				case "float":
					RegisterPropertyAlter(res, friendlyPropertyName);
					res.GetNode<Label>("float/float").Text = title;
					res.GetNode<LineEdit>("float/Value").Text = data["Value"].ToString();
					break;
				case "code":
					res.GetNode<Label>("Code/Code").Text = title;
					res.GetNode<CodeEdit>("Code/CodeEdit").Text = data["code"].ToString();
					break;
				case "string":
					RegisterPropertyAlter(res, friendlyPropertyName);
					res.GetNode<Label>("string/title").Text = title;
					res.GetNode<LineEdit>("string/Value").Text = data["text"].ToString();
					break;
				case "slider":
					RegisterPropertyAlter(res, friendlyPropertyName);
					res.GetNode<Label>("slider/title").Text = title;
					res.GetNode<HSlider>("slider/slider").Value = (float)data["Value"] * 100;
					break;
				case "options":
					RegisterPropertyAlter(res, friendlyPropertyName);
					res.GetNode<Label>("options/title").Text = title;
					foreach (KeyValuePair<Variant, Variant> kvp in data)
					{
						if (kvp.Key.ToString() != "title")
						{
							res.GetNode<OptionButton>("options/Value").AddItem(kvp.Value.ToString());
						}
					}
					break;
			}

			return res;
		}

		public void ClearIntoProperties()
		{
			PropertiesContainer.Visible = false;
		}


		private void _OnCodeEditCodeCompletionRequested()
		{
			UpdateCodeCompletionOptions();
		}

		private void UpdateCodeCompletionOptions()
		{
			foreach (KeyValuePair<string, string> entry in _preserved_global_functions)
			{
				CodeEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, entry.Key, entry.Value);
			}

			foreach (string className in _preserved_global_classes)
			{
				CodeEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, className, className, new Color(1, 1, 1, 1));
			}

			foreach (string className in _preserved_lua)
			{
				CodeEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Member, className, className);
			}

			foreach (string className in _preserved_global_keywords)
			{
				CodeEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Variable, className, className);
			}

			CodeEditor.UpdateCodeCompletionOptions(true);
		}

		private void _OnNewerTextInput()
		{
			foreach (KeyValuePair<string, string> entry in _preserved_global_functions)
			{
				CodeEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, entry.Key, entry.Value);
			}

			foreach (string className in _preserved_global_classes)
			{
				CodeEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function, className, className, new Color(1, 1, 1, 1));
			}

			foreach (string className in _preserved_lua)
			{
				CodeEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Member, className, className);
			}

			foreach (string className in _preserved_global_keywords)
			{
				CodeEditor.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Variable, className, className);
			}

			CodeEditor.UpdateCodeCompletionOptions(true);
		}

		private void MenuGLTF() => GetNode<FileDialog>("/root/Root/Windows/Meshimporter/MeshFileDialog").Visible = !GetNode<FileDialog>("/root/Root/Windows/Meshimporter/MeshFileDialog").Visible;

		private void OnGLTFImported(string path)
		{
			GltfDocument gltf_doc_load = new();
			GltfState gltf_state_load = new();
			var error = gltf_doc_load.AppendFromFile(path, gltf_state_load);
			Node3D gltf_scene_root_node = (Node3D)gltf_doc_load.GenerateScene(gltf_state_load);
			Node _mesh_part = InstanceFromPackedScene("res://Prefabs/MeshPart.tscn").Instantiate();
			MeshPart _mesh_as_mesh = (MeshPart)_mesh_part;
			_mesh_part.AddChild(gltf_scene_root_node, true);
			_mesh_part.Name = gltf_scene_root_node.Name;
			gltf_scene_root_node.Name = "GLTFRoot";
			GetNode("/root/Root/Game/Map").AddChild(_mesh_part, true);
			GameExplorer.Instance.AddObjectToGameExplorer(_mesh_part as Instance);
			_mesh_as_mesh.CreateTrimeshShapesForGLTFSceneRootMeshes(gltf_scene_root_node);
		}

		public void AddToOutput(string _t)
		{
			GetNode<RichTextLabel>("Output/Container/main/VBoxContainer/Console").Text += "[Engine:Workshop] " + _t;
		}

		public void MakeOutputVisible()
		{
			GetNode<Control>("Output").Visible = !GetNode<Control>("Output").Visible;
		}

		public void ExportRequested()
		{
			GD.Print(Netisu.Game.Exporter.SerializeTheGame(GameDataModel.GetNode<Datamodels.Environment>("Environment"), GameDataModel));
		}

		public void UpdateSnapping(string _text, int mode_)
		{
			GetNode<LineEdit>($"/root/Root/EngineGUI/Ribbon/Panel/HBoxContainer2/ScrollContainer/VBoxContainer/{mode_.ToString()}/Panel/LineEdit").ReleaseFocus();
			try
			{
				float.Parse(_text);
			}
			catch
			{
				GetNode<LineEdit>($"/root/Root/EngineGUI/Ribbon/Panel/HBoxContainer2/ScrollContainer/VBoxContainer/{mode_}/Panel/LineEdit").Text = "0";
				GetNode<LineEdit>($"/root/Root/EngineGUI/Ribbon/Panel/HBoxContainer2/ScrollContainer/VBoxContainer/{mode_}/Panel/LineEdit").ReleaseFocus();
				return;
			}
			switch (mode_)
			{
				case 1:
					//Engine_camera.GizmoMixin.MoveSnap = float.Parse(_text);
					break;
				case 2:
					//Engine_camera.GizmoMixin.RotationSnap = Mathf.DegToRad(float.Parse(_text));
					break;
				default:
					break;
			}
		}

		public void UpdateNameChange(string newerName)
		{
			if (Gizmo3DPlugin.Gizmo3D.Instance.Selection != null)
			{
				GameExplorer.Instance.RefreshTextChange(newerName, Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance);
			}
			GetNode<LineEdit>("/root/Root/EngineGUI/Sidebar/VSplitContainer/Properties/ScrollContainer/VBoxContainer/EngineAttr/Name").ReleaseFocus();
		}

		public static void AnchorCurrentSelected()
		{
			ActiveInstance _selected = Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance;
			if (_selected != null)
				_selected.Anchored = !_selected.Anchored;
		}

		public void HideUI()
		{
			foreach (Node child in userInterface.GetChildren())
			{
				if (child is UiInstance uiInstance)
				{
					uiInstance.Visible = !uiInstance.Visible;
					if (!uiInstance.Visible)
					{
						GameExplorer.Instance.cachedItemsReverse[uiInstance].EraseButton(0, 0);
						GameExplorer.Instance.cachedItemsReverse[uiInstance].AddButton(0, GameExplorer.Instance.HiddenIcon, 0);
					}
					else
					{
						GameExplorer.Instance.cachedItemsReverse[uiInstance].EraseButton(0, 0);
						GameExplorer.Instance.cachedItemsReverse[uiInstance].AddButton(0, GameExplorer.Instance.VisibleIcon, 0);
					}
				}
			}
		}

		public static void LockSelected() => LockSelected(Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance);

		public static void LockSelected(ActiveInstance activeInstance)
		{
			if (activeInstance != null)
			{
				activeInstance.Locked = !activeInstance.Locked;
				if (activeInstance.Locked)
				{
					EngineCamera.Instance.ExcludedItems.Add(activeInstance.Container.GetRid());
					GameExplorer.Instance.cachedItemsReverse[activeInstance].AddButton(0, GameExplorer.Instance.LockedIcon, 0);
					return;
				}
				EngineCamera.Instance.ExcludedItems.Remove(activeInstance.Container.GetRid());
				GameExplorer.Instance.cachedItemsReverse[activeInstance].EraseButton(0, 0);
			}
		}
	}

}
