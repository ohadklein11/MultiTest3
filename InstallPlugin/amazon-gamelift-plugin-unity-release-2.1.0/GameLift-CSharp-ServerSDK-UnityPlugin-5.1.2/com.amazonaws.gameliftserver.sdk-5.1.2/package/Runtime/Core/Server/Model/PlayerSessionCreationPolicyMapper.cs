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
    /// Describes the game session's ability to accept new player sessions.
    /// </summary>
    public enum PlayerSessionCreationPolicy
    {
        NOT_SET,
        ACCEPT_ALL,
        DENY_ALL,
    }

    public static class PlayerSessionCreationPolicyMapper
    {
        private const string AcceptAll = "ACCEPT_ALL";
        private const string DenyAll = "DENY_ALL";
        private const string NotSet = "NOT_SET";

        public static PlayerSessionCreationPolicy GetPlayerSessionCreationPolicyForName(string name)
        {
            switch (name)
            {
                case AcceptAll:
                    return PlayerSessionCreationPolicy.ACCEPT_ALL;
                case DenyAll:
                    return PlayerSessionCreationPolicy.DENY_ALL;
                default:
                    return PlayerSessionCreationPolicy.NOT_SET;
            }
        }

        public static string GetNameForPlayerSessionCreationPolicy(PlayerSessionCreationPolicy value)
        {
            switch (value)
            {
                case PlayerSessionCreationPolicy.ACCEPT_ALL:
                    return AcceptAll;
                case PlayerSessionCreationPolicy.DENY_ALL:
                    return DenyAll;
                default:
                    return NotSet;
            }
        }
    }
}
