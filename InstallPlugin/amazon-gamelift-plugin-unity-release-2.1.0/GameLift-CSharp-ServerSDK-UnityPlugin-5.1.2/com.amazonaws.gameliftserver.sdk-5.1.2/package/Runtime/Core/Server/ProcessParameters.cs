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

using Aws.GameLift.Server.Model;

namespace Aws.GameLift.Server
{
    /// <summary>
    /// This data type contains the set of parameters sent to GameLift in a ProcessReady() method.
    /// </summary>
    public class ProcessParameters
    {
        public delegate void OnStartGameSessionDelegate(GameSession gameSession);

        public delegate void OnUpdateGameSessionDelegate(UpdateGameSession updateGameSession);

        public delegate void OnProcessTerminateDelegate();

        public delegate bool OnHealthCheckDelegate();

        /// <summary>
        /// Name of callback function that GameLift invokes to activate a new game
        /// session. GameLift calls this function in response to the client request CreateGameSession.
        /// The callback function takes a GameSession object defined in GameLift API Reference.
        /// </summary>
        public OnStartGameSessionDelegate OnStartGameSession { get; set; }

        /// <summary>
        /// Name of callback function that GameLift invokes to pass an updated
        /// game session object to the server process. GameLift calls this function when a
        /// match backfill request has been processed in order to provide updated matchmaker
        /// data. It passes a GameSession object, a status update (updateReason), and the
        /// match backfill ticket ID.
        /// </summary>
        ///
        public OnUpdateGameSessionDelegate OnUpdateGameSession { get; set; }

        /// <summary>
        /// Name of callback function that GameLift invokes to force the server
        /// process to shut down. After calling this function, GameLift waits five minutes for
        /// the server process to shut down and respond with a ProcessEnding() call before it
        /// shuts down the server process.
        /// </summary>
        ///
        public OnProcessTerminateDelegate OnProcessTerminate { get; set; }

        /// <summary>
        /// Name of callback function that GameLift invokes to request a health
        /// status report from the server process. GameLift calls this function every 60 seconds.
        /// After calling this function GameLift waits 60 seconds for a response, and if none is
        /// received. records the server process as unhealthy.
        /// </summary>
        public OnHealthCheckDelegate OnHealthCheck { get; set; }

        /// <summary>
        /// Port number the server process will listen on for new player connections.
        /// The value must fall into the port range configured for any fleet deploying
        /// this game server build. This port number is included in game session and player
        /// session objects, which game sessions use when connecting to a server process.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Object with a list of directory paths to game session log files.
        /// </summary>
        public LogParameters LogParameters { get; set; }

        public ProcessParameters()
        {
            OnStartGameSession = gameSession => { };
            OnUpdateGameSession = updateGameSession => { };
            OnProcessTerminate = () => { };
            OnHealthCheck = () => true;
            Port = -1;
            LogParameters = new LogParameters();
        }

        public ProcessParameters(
            OnStartGameSessionDelegate onStartGameSession,
            OnUpdateGameSessionDelegate onUpdateGameSession,
            OnProcessTerminateDelegate onProcessTerminate,
            OnHealthCheckDelegate onHealthCheck,
            int port,
            LogParameters logParameters)
        {
            OnStartGameSession = onStartGameSession;
            OnUpdateGameSession = onUpdateGameSession;
            OnProcessTerminate = onProcessTerminate;
            OnHealthCheck = onHealthCheck;
            Port = port;
            LogParameters = logParameters;
        }
    }
}
