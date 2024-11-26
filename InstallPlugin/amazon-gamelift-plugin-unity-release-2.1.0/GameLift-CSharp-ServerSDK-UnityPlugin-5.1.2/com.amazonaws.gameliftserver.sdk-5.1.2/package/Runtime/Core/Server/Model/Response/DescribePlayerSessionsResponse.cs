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

namespace Aws.GameLift.Server.Model
{
    /// <summary>
    /// This data type describes player session retrieved by <c>DescribePlayerSessions</c>. It can
    /// contain information about any of the following:
    /// <list type="bullet">
    /// <item>A specific player sessions</item>
    /// <item>All players session in a specific game</item>
    /// <item>All player sessions for a specific player</item>
    /// </list>
    /// For large collections of player sessions, use the pagination parameters to retrieve results as sequential pages.
    /// </summary>
    public class DescribePlayerSessionsResponse : Message
    {
        /// <summary>
        /// A pagination character used to view results as sequential pages.
        /// </summary>
        public string NextToken { get; set; }

        /// <summary>
        /// A list of <see cref="PlayerSession"/> objects containing information about the specific player sessions.
        /// </summary>
        public IList<PlayerSession> PlayerSessions { get; set; }
    }
}
