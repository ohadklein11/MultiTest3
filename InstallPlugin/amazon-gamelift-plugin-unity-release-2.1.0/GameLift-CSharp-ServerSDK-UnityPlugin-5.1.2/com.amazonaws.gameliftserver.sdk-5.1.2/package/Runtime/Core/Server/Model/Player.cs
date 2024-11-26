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
    /// Represents a player in matchmaking. When starting a matchmaking request, a player
    /// has a player ID, attributes, and may have latency data. Team information is added
    /// after a match has been successfully completed.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// A unique identifier for a player.
        /// </summary>
        public string PlayerId { get; set; }

        /// <summary>
        /// A collection of key:value pairs containing player information for use in matchmaking.
        /// Player attribute keys must match the playerAttributes used in a matchmaking rule set.<br/>
        /// Example: <c>"PlayerAttributes": {"skill": {"N": "23"}, "gameMode": {"S": "deathmatch"}}</c>.
        /// </summary>
        public Dictionary<string, AttributeValue> PlayerAttributes { get; set; }

        /// <summary>
        /// Name of the team that the player is assigned to in a match. Team names are defined in a matchmaking rule set.
        /// </summary>
        public string Team { get; set; }

        /// <summary>
        /// A set of values, expressed in milliseconds, that indicates the amount of
        /// latency that a player experiences when connected to a location. If this property
        /// is present, FlexMatch considers placing the match only in Regions for which latency
        /// is reported.<br/>
        /// If a matchmaker has a rule that evaluates player latency, players must report
        /// latency in order to be matched.If no latency is reported in this scenario,
        /// FlexMatch assumes that no locations are available to the player and the ticket
        /// is not matchable.
        /// </summary>
        public Dictionary<string, int> LatencyInMS { get; set; }
    }
}
