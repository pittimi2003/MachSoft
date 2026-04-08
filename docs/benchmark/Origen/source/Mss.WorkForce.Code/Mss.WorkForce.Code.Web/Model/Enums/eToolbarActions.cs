using System.ComponentModel;

namespace Mss.WorkForce.Code.Web.Model.Enums
{
	public enum eToolbarActions
	{
		[Description("blocked")]
		LockTask,
		[Description("")]
		UnlockTask,
		[Description("cancelled")]
		CancelTask,
		[Description("reprioritized")]
		ChangePriority,
	}
}
