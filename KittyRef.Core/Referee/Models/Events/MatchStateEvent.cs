// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using KittyRef.Core.Models;

namespace KittyRef.Core.Referee.Models.Events
{
    /// <summary>
    /// Contains the current state of the room.
    /// </summary>
    [PublicAPI]
    public class MatchStateEvent : MatchState;
}
