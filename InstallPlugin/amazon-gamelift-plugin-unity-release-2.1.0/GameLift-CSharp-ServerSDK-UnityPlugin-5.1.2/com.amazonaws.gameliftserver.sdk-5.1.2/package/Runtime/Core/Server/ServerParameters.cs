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

using System;

namespace Aws.GameLift.Server
{
    /// <summary>
    /// Connection information and methods for maintaining the connection between GameLift
    /// and your game server.
    /// </summary>
    public struct ServerParameters : IEquatable<ServerParameters>
    {
        public string WebSocketUrl { get; set; }

        public string ProcessId { get; set; }

        public string HostId { get; set; }

        public string FleetId { get; set; }

        public string AuthToken { get; set; }

        public ServerParameters(string webSocketUrl, string processId, string hostId, string fleetId, string authToken)
        {
            WebSocketUrl = webSocketUrl;
            ProcessId = processId;
            HostId = hostId;
            FleetId = fleetId;
            AuthToken = authToken;
        }

        public bool Equals(ServerParameters other)
        {
            return WebSocketUrl == other.WebSocketUrl && ProcessId == other.ProcessId && HostId == other.HostId &&
                   FleetId == other.FleetId && AuthToken == other.AuthToken;
        }
    }
}
