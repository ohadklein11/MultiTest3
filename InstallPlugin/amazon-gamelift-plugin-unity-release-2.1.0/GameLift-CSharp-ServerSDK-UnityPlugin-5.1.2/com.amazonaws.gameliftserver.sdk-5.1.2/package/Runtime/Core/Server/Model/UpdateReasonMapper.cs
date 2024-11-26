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
    public enum UpdateReason
    {
        MATCHMAKING_DATA_UPDATED,
        BACKFILL_FAILED,
        BACKFILL_TIMED_OUT,
        BACKFILL_CANCELLED,
        UNKNOWN,
    }

    public static class UpdateReasonMapper
    {
        private const string MatchmakingDataUpdatedReason = "MATCHMAKING_DATA_UPDATED";
        private const string BackfillFailedReason = "BACKFILL_FAILED";
        private const string BackfillTimedOutReason = "BACKFILL_TIMED_OUT";
        private const string BackfillCancelledReason = "BACKFILL_CANCELLED";
        private const string UnknownReason = "UNKNOWN";

        public static UpdateReason GetUpdateReasonForName(string name)
        {
            switch (name)
            {
                case MatchmakingDataUpdatedReason:
                    return UpdateReason.MATCHMAKING_DATA_UPDATED;
                case BackfillFailedReason:
                    return UpdateReason.BACKFILL_FAILED;
                case BackfillTimedOutReason:
                    return UpdateReason.BACKFILL_TIMED_OUT;
                case BackfillCancelledReason:
                    return UpdateReason.BACKFILL_CANCELLED;
                default:
                    return UpdateReason.UNKNOWN;
            }
        }

        public static string GetNameForUpdateReason(UpdateReason value)
        {
            switch (value)
            {
                case UpdateReason.MATCHMAKING_DATA_UPDATED:
                    return MatchmakingDataUpdatedReason;
                case UpdateReason.BACKFILL_FAILED:
                    return BackfillFailedReason;
                case UpdateReason.BACKFILL_TIMED_OUT:
                    return BackfillTimedOutReason;
                case UpdateReason.BACKFILL_CANCELLED:
                    return BackfillCancelledReason;
                default:
                    return UnknownReason;
            }
        }
    }
}
