using Godot;
using System;

public partial class InsertMenu : PopupMenu
{
	[Export]
	private PackedScene _default_scene_popup;

	private PopupMenu Parts = null;
	private PopupMenu Scripting = null;
	private PopupMenu UI = null;
	private PopupMenu Lighting = null;
	private PopupMenu GameFeatures = null;

	public override void _Ready() => Initialize();
	
	private void Initialize() {
		Parts = (PopupMenu)_default_scene_popup.Instantiate();
		Parts.Name = "Parts";
		Parts.AddItem("Cube");
		Parts.AddItem("Cylinder");
		Parts.AddItem("Cone");
		Parts.AddItem("Sphere");
		Parts.AddItem("Wedge");
		Parts.AddItem("Seat");
		Parts.AddItem("MeshPart");
		AddChild(Parts);
        SetItemSubmenuNode(0, Parts);
		Parts.IndexPressed += PartsInstanceReq;

		Scripting = (PopupMenu)_default_scene_popup.Instantiate();
		Scripting.Name = "Scripting";
		Scripting.AddItem("LocalScript");
		Scripting.AddItem("Script");
		AddChild(Scripting);
        SetItemSubmenuNode(1, Scripting);
		Scripting.IndexPressed += ScriptingInstanceReq;

		UI = (PopupMenu)_default_scene_popup.Instantiate();
		UI.Name = "UI";
		UI.AddItem("UiPanel");
		UI.AddItem("UiLabel");
		UI.AddItem("UiButton");
		UI.AddItem("UiHorizontalLayout");
		AddChild(UI);
        SetItemSubmenuNode(2, UI);
		UI.IndexPressed += UIInstanceReq;

		Lighting = (PopupMenu)_default_scene_popup.Instantiate();
		Lighting.Name = "Lighting";
		Lighting.AddItem("Sun");
		Lighting.AddItem("PointLight");
		AddChild(Lighting);
        SetItemSubmenuNode(3, Lighting);
		Lighting.IndexPressed += LightingInstanceReq;

		GameFeatures = (PopupMenu)_default_scene_popup.Instantiate();
		GameFeatures.Name = "Game Features";
		GameFeatures.AddItem("Folder");
		GameFeatures.AddItem("Spawnpoint");
		AddChild(GameFeatures);
        SetItemSubmenuNode(4, GameFeatures);
		GameFeatures.IndexPressed += GameFeatReq;
	}

	private void UIInstanceReq(long id)
	{
		string item = UI.GetItemText((int)id);
		Netisu.Workshop.EngineUI.Instance.BuildInstance(item);
	}

	private void PartsInstanceReq(long id) 
	{
		string item = Parts.GetItemText((int)id);
		Netisu.Workshop.EngineUI.Instance.BuildInstance(item);
	}

	private void GameFeatReq(long id) 
	{
		string item = GameFeatures.GetItemText((int)id);
		Netisu.Workshop.EngineUI.Instance.BuildInstance(item);
	}

	private void LightingInstanceReq(long id) 
	{
		string item = Lighting.GetItemText((int)id);
		Netisu.Workshop.EngineUI.Instance.BuildInstance(item);
	}

	private void ScriptingInstanceReq(long id) 
	{
		string item = Scripting.GetItemText((int)id);
		Netisu.Workshop.EngineUI.Instance.BuildInstance(item);
	}
}
