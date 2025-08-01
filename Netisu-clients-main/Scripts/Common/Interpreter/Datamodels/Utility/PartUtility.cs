using Godot;
using Netisu;

namespace Netisu.Datamodels.Utilities
{
    public partial class PartUtility : RigidBody3D
    {
        [Export]
        public Part part = null!;

        private Transform3D _lastSentTransform;

        public override void _IntegrateForces(PhysicsDirectBodyState3D state)
        {
            base._IntegrateForces(state);

            if (part.BatchRenderId == -1)
                return;

            if (!_lastSentTransform.IsEqualApprox(state.Transform))
            {
                BatchRenderPart.Instance.UpdateTransform(part.BatchRenderId, state.Transform, part.Shape);
                _lastSentTransform = state.Transform;
            }
        }
        
        [MoonSharp.Interpreter.MoonSharpHidden]
		public override void _Notification(int what)
		{
			switch (what)
			{
				case (int)NotificationTransformChanged:
					{
						if (part.BatchRenderId != -1)
						{
							BatchRenderPart.Instance.UpdateTransform(part.BatchRenderId, Transform, part.Shape);
						}
						break;
					}

				default: break;
			}
		}
    }
}