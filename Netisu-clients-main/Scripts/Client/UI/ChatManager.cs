using System;
using System.Collections.Generic;
using Godot;

namespace Netisu.Client.UI
{
	public partial class ChatManager : Control
	{
		[Export]
		RichTextLabel MessagesContainer = null!;

		[Export]
		Control ConsoleContainer = null!;

		[Export]
		LineEdit InputEdit = null!;

		public bool Busy = false;

		public static ChatManager Instance { get; private set; } = null!;

		private static readonly Dictionary<string, Action> CommandHandlers =
		new()
		{
			["/console"]  = () =>
			{
				Instance.ConsoleContainer.Visible = true;
				Instance.OnMessageEditorFocusEntered();
			},
		};

		ChatManager()
		{
			Instance = this;
		}

		public override void _Ready()
		{
			InputEdit.TextSubmitted += OnMessageSubmitted;
			InputEdit.FocusEntered += OnMessageEditorFocusEntered;
		}

		public void MessageRecieved(string username, string content)
		{
			MessagesContainer.Text += $"[{username}]: {content}\n";
		}

		private void OnMessageEditorFocusEntered()
		{
			Busy = true;
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (Input.IsActionJustPressed("slash"))
			{
				InputEdit.GrabFocus();
			}
		}

		private void OnMessageSubmitted(string content)
		{
			InputEdit.Text = string.Empty;
			InputEdit.ReleaseFocus();

			if (content.StartsWith('/'))
			{
				if (CommandHandlers.TryGetValue(content.ToLower(), out Action action))
					action.Invoke();
				else
					MessageRecieved("Client", $@"Unknown command ""{content}""");
				return;
			}

			Client.Instance.RpcId(1, "ChatMessageRecieved", content);
			Busy = false;
		}
	}
}
