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
    public static class MessageActions
    {
#pragma warning disable S2339
        public const string AcceptPlayerSession = "AcceptPlayerSession";
        public const string ActivateGameSession = "ActivateGameSession";
        public const string TerminateServerProcess = "TerminateServerProcess";
        public const string ActivateServerProcess = "ActivateServerProcess";
        public const string UpdatePlayerSessionCreationPolicy = "UpdatePlayerSessionCreationPolicy";
        public const string CreateGameSession = "CreateGameSession";
        public const string UpdateGameSession = "UpdateGameSession";
        public const string StartMatchBackfill = "StartMatchBackfill";
        public const string TerminateProcess = "TerminateProcess";
        public const string DescribePlayerSessions = "DescribePlayerSessions";
        public const string StopMatchBackfill = "StopMatchBackfill";
        public const string HeartbeatServerProcess = "HeartbeatServerProcess";
        public const string GetComputeCertificate = "GetComputeCertificate";
        public const string GetFleetRoleCredentials = "GetFleetRoleCredentials";
        public const string RefreshConnection = "RefreshConnection";
        public const string RemovePlayerSession = "RemovePlayerSession";
#pragma warning restore S2339
    }
}
