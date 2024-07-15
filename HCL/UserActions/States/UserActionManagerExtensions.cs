// Copyright © 2018 by Hilti Corporation – all rights reserved

using System;

namespace HCL_ODA_TestPAD.HCL.UserActions.States;

public static class UserActionManagerExtensions
{
    public static void DoIdle(this IUserActionManager @this, UserInteraction userAction)
    {
        ArgumentNullException.ThrowIfNull(@this);
    }

    public static IUserActionState CreateState(this IUserActionManager @this, UserInteraction userAction)
        => userAction switch
        {
            UserInteraction.Idle => new ActionIdle(@this),
            UserInteraction.Panning => new ActionPanning(@this),
            UserInteraction.Orbiting => new ActionOrbiting(@this),
            UserInteraction.ZoomToArea => new ActionZoomToArea(@this),
            UserInteraction.ZoomToScale => new ActionZoomToScale(@this),
            UserInteraction.Wheeling => new ActionWheeling(@this),
            _ => throw new ArgumentOutOfRangeException(nameof(userAction), userAction, null)
        };
}