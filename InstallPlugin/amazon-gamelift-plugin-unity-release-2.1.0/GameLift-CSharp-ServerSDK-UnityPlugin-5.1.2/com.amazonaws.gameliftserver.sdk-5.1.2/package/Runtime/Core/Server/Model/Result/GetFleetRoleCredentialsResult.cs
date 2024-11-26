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

namespace Aws.GameLift.Server.Model
{
    /// <summary>
    /// The service role credentials provided to GameLift that allow the compute
    /// access to other AWS services in your account.
    /// </summary>
    public class GetFleetRoleCredentialsResult
    {
        public string AssumedRoleUserArn { get; }

        public string AssumedRoleId { get; }

        public string AccessKeyId { get; }

        public string SecretAccessKey { get; }

        public string SessionToken { get; }

        public DateTime Expiration { get; }

        public GetFleetRoleCredentialsResult(
            string assumedRoleUserArn,
            string assumedRoleId,
            string accessKeyId,
            string secretAccessKey,
            string sessionToken,
            DateTime expiration)
        {
            AssumedRoleUserArn = assumedRoleUserArn;
            AssumedRoleId = assumedRoleId;
            AccessKeyId = accessKeyId;
            SecretAccessKey = secretAccessKey;
            SessionToken = sessionToken;
            Expiration = expiration;
        }
    }
}
