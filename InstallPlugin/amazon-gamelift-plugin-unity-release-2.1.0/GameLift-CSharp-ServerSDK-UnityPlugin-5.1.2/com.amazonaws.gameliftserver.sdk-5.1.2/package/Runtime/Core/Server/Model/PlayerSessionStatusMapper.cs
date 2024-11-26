/*
* All or portions of this file Copyright (c) Amazon.com, Inc. or its affiliates or
* its licensors.
*
* For complete copyright and license terms please see the LICENSE at the root of this
* distribution (the "License"). All use of this software is governed by the License,
* or, if provided, by the license below or the license accompanying this file. Do not
* remove or modify any license notices. This file is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*
*/

namespace Aws.GameLift.Server.Model
{
    /// <summary>
    /// The current status of player sessions in a game session.
    /// </summary>
    public enum PlayerSessionStatus
    {
        NOT_SET,
#pragma warning disable S4016
        RESERVED,
#pragma warning restore S4016
        ACTIVE,
        COMPLETED,
        TIMEDOUT,
    }

    public static class PlayerSessionStatusMapper
    {
        private const string Reserved = "RESERVED";
        private const string Active = "ACTIVE";
        private const string Completed = "COMPLETED";
        private const string Timedout = "TIMEDOUT";
        private const string NotSet = "NOT_SET";

        public static PlayerSessionStatus GetPlayerSessionStatusForName(string name)
        {
            switch (name)
            {
                case Reserved:
                    return PlayerSessionStatus.RESERVED;
                case Active:
                    return PlayerSessionStatus.ACTIVE;
                case Completed:
                    return PlayerSessionStatus.COMPLETED;
                case Timedout:
                    return PlayerSessionStatus.TIMEDOUT;
                default:
                    return PlayerSessionStatus.NOT_SET;
            }
        }

        public static string GetNameForPlayerSessionStatus(PlayerSessionStatus value)
        {
            switch (value)
            {
                case PlayerSessionStatus.RESERVED:
                    return Reserved;
                case PlayerSessionStatus.ACTIVE:
                    return Active;
                case PlayerSessionStatus.COMPLETED:
                    return Completed;
                case PlayerSessionStatus.TIMEDOUT:
                    return Timedout;
                default:
                    return NotSet;
            }
        }
    }
}
