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

using System.Collections.Generic;
using Aws.GameLift.Server.Model;

namespace Aws.GameLift.Server
{
    public interface IWebSocketMessageHandler
    {
        void OnErrorResponse(string requestId, int statusCode, string errorMessage);

        void OnSuccessResponse(string requestId);

        void OnStartGameSession(GameSession gameSession);

        void OnUpdateGameSession(GameSession gameSession, UpdateReason updateReason, string backfillTicketId);

        void OnTerminateProcess(long terminationTime);

        void OnStartMatchBackfillResponse(string requestId, string ticketId);

        void OnDescribePlayerSessionsResponse(string requestId, IList<PlayerSession> playerSessions, string nextToken);

        void OnGetComputeCertificateResponse(string requestId, string certificatePath, string computeName);

        void OnGetFleetRoleCredentialsResponse(
            string requestId,
            string assumedRoleUserArn,
            string assumedRoleId,
            string accessKeyId,
            string secretAccessKey,
            string sessionToken,
            long expiration);

        void OnRefreshConnection(string refreshConnectionEndpoint, string authToken);
    }
}
