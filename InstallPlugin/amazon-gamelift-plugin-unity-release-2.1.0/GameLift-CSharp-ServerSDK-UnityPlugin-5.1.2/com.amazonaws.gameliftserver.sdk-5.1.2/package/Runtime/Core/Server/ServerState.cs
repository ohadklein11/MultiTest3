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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aws.GameLift.Server.Model;
using log4net;
using WebSocketSharp;

namespace Aws.GameLift.Server
{
#pragma warning disable S1200
    public sealed class ServerState : IWebSocketMessageHandler
#pragma warning restore S1200
    {
        // When within 15 minutes of expiration we retrieve new instance role credentials
        public static readonly TimeSpan InstanceRoleCredentialTtlMin = TimeSpan.FromMinutes(15);

        private const string EnvironmentVariableWebsocketUrl = "GAMELIFT_SDK_WEBSOCKET_URL";
        private const string EnvironmentVariableProcessId = "GAMELIFT_SDK_PROCESS_ID";
        private const string EnvironmentVariableHostId = "GAMELIFT_SDK_HOST_ID";
        private const string EnvironmentVariableFleetId = "GAMELIFT_SDK_FLEET_ID";
        private const string EnvironmentVariableAuthToken = "GAMELIFT_SDK_AUTH_TOKEN";

        private const int RoleSessionNameMaxLength = 64;
        private const double HealthcheckIntervalSeconds = 60;
        private const double HealthcheckMaxJitterSeconds = 10;
        private const double HealthcheckTimeoutSeconds = HealthcheckIntervalSeconds - HealthcheckMaxJitterSeconds;
        private const string SdkLanguage = "CSharp";

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        private static readonly Random Random = new Random();

        private readonly IGameLiftWebSocket gameLiftWebSocket;
        private readonly GameLiftWebSocketRequestHandler webSocketRequestHandler;

        // Map of RoleArn -> Credentials for that role
        private readonly IDictionary<string, GetFleetRoleCredentialsResult> instanceRoleResultCache = new Dictionary<string, GetFleetRoleCredentialsResult>();

        private ProcessParameters processParameters;
        private volatile bool processReady;
        private string gameSessionId;
        private DateTime terminationTime = DateTime.MinValue; // init to 1/1/0001 12:00:00 AM
        private string fleetId;
        private string hostId;
        private string processId;
        // Assume we're on managed EC2, if GetFleetRoleCredentials fails we know to set this to false
        private bool onManagedEc2 = true;

        public static ServerState Instance { get; } = new ServerState();

        public static ILog Log { get; } = LogManager.GetLogger(typeof(ServerState));

        public ServerState(IGameLiftWebSocket webSocket, GameLiftWebSocketRequestHandler requestHandler)
        {
            gameLiftWebSocket = webSocket;
            webSocketRequestHandler = requestHandler;
        }

        private ServerState()
        {
            gameLiftWebSocket = new GameLiftWebSocket(this);
            webSocketRequestHandler = new GameLiftWebSocketRequestHandler(gameLiftWebSocket);
        }

        public GenericOutcome ProcessReady(ProcessParameters procParameters)
        {
            processReady = true;
            processParameters = procParameters;

            GenericOutcome result = gameLiftWebSocket.SendMessage(new ActivateServerProcessRequest(
                GameLiftServerAPI.GetSdkVersion().Result, SdkLanguage, processParameters.Port, processParameters.LogParameters.LogPaths));

            Task.Run(() => StartHealthCheck());

            return result;
        }

        public GenericOutcome ProcessEnding()
        {
            processReady = false;

            GenericOutcome result = gameLiftWebSocket.SendMessage(
                new TerminateServerProcessRequest());

            return result;
        }

        public GenericOutcome ActivateGameSession()
        {
            if (string.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }

            return gameLiftWebSocket.SendMessage(new ActivateGameSessionRequest(gameSessionId));
        }

        public AwsStringOutcome GetGameSessionId()
        {
            if (string.IsNullOrEmpty(gameSessionId))
            {
                return new AwsStringOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }

            return new AwsStringOutcome(gameSessionId);
        }

        public AwsDateTimeOutcome GetTerminationTime()
        {
            if (terminationTime == DateTime.MinValue)
            {
                return new AwsDateTimeOutcome(new GameLiftError(GameLiftErrorType.TERMINATION_TIME_NOT_SET));
            }

            return new AwsDateTimeOutcome(terminationTime);
        }

        public GenericOutcome UpdatePlayerSessionCreationPolicy(PlayerSessionCreationPolicy playerSessionPolicy)
        {
            if (string.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }

            return gameLiftWebSocket.SendMessage(new UpdatePlayerSessionCreationPolicyRequest(gameSessionId, playerSessionPolicy));
        }

        public GenericOutcome AcceptPlayerSession(string playerSessionId)
        {
            if (string.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }

            return gameLiftWebSocket.SendMessage(new AcceptPlayerSessionRequest(gameSessionId, playerSessionId));
        }

        public GenericOutcome RemovePlayerSession(string playerSessionId)
        {
            if (string.IsNullOrEmpty(gameSessionId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.GAMESESSION_ID_NOT_SET));
            }

            return gameLiftWebSocket.SendMessage(new RemovePlayerSessionRequest(gameSessionId, playerSessionId));
        }

#pragma warning disable S3242
        public DescribePlayerSessionsOutcome DescribePlayerSessions(DescribePlayerSessionsRequest request)
#pragma warning restore S3242
        {
            if (request == null)
            {
                return new DescribePlayerSessionsOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "DescribePlayerSessionsRequest is required"));
            }

            // must have player session id or game session id or player id
            if (string.IsNullOrEmpty(request.PlayerSessionId) && string.IsNullOrEmpty(request.GameSessionId) && string.IsNullOrEmpty(request.PlayerId))
            {
                return new DescribePlayerSessionsOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "At least one of PlayerSessionId, GameSessionId and PlayerId is required."));
            }

            GenericOutcome outcome = webSocketRequestHandler.SendRequest(request);
            if (!outcome.Success)
            {
                return new DescribePlayerSessionsOutcome(outcome.Error);
            }

            return (DescribePlayerSessionsOutcome)outcome;
        }

#pragma warning disable S3242
        public StartMatchBackfillOutcome StartMatchBackfill(StartMatchBackfillRequest request)
#pragma warning restore S3242
        {
            if (request == null)
            {
                return new StartMatchBackfillOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "StartMatchBackfillRequest is required"));
            }

            if (string.IsNullOrEmpty(request.GameSessionArn))
            {
                return new StartMatchBackfillOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "GameSessionArn is required in StartMatchBackfillRequest"));
            }

            if (string.IsNullOrEmpty(request.MatchmakingConfigurationArn))
            {
                return new StartMatchBackfillOutcome(new GameLiftError(
                    GameLiftErrorType.BAD_REQUEST_EXCEPTION,
                    "Invalid MatchmakingConfigurationArn request was passed to StopMatchBackfillRequest"));
            }

            if (request.Players == null || request.Players.Length == 0)
            {
                return new StartMatchBackfillOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "At least 1 Player is required in StartMatchBackfillRequest"));
            }

            GenericOutcome outcome = webSocketRequestHandler.SendRequest(request);
            if (!outcome.Success)
            {
                return new StartMatchBackfillOutcome(outcome.Error);
            }

            return (StartMatchBackfillOutcome)outcome;
        }

#pragma warning disable S3242
        public GenericOutcome StopMatchBackfill(StopMatchBackfillRequest request)
#pragma warning restore S3242
        {
            if (request == null)
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "StopMatchBackfillRequest is required"));
            }

            if (string.IsNullOrEmpty(request.GameSessionArn))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "GameSessionArn is required in StopMatchBackfillRequest"));
            }

            if (string.IsNullOrEmpty(request.MatchmakingConfigurationArn))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "MatchmakingConfigurationArn is required in StopMatchBackfillRequest"));
            }

            if (string.IsNullOrEmpty(request.TicketId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "TicketId is required in StopMatchBackfillRequest"));
            }

            return webSocketRequestHandler.SendRequest(request);
        }

        private void StartHealthCheck()
        {
            Log.InfoFormat(
                "Starting HealthCheck thread. GameLift Server SDK will report process health status to GameLift every: {0} seconds (plus random jitter of up to {1} seconds).",
                HealthcheckIntervalSeconds,
                HealthcheckMaxJitterSeconds);
            while (processReady)
            {
                Task.Run(() => HeartbeatServerProcess());
                Thread.Sleep(TimeSpan.FromSeconds(GetNextHealthCheckIntervalSeconds()));
            }
        }

        private static double GetNextHealthCheckIntervalSeconds()
        {
            // Jitter the healthCheck interval +/- a random value between [-MAX_JITTER_SECONDS, MAX_JITTER_SECONDS]
            double jitter = HealthcheckMaxJitterSeconds * (2 * Random.NextDouble() - 1);
            return HealthcheckIntervalSeconds + jitter;
        }

        private async Task HeartbeatServerProcess()
        {
            // duplicate ProcessReady check here right before invoking
            if (!processReady)
            {
                Log.Debug("Reporting Health on an inactive process. Ignoring.");
                return;
            }

            Log.Debug("Reporting health using the OnHealthCheck callback.");

            bool healthCheckResult;
            try
            {
                var healthCheckResultTask = Task.Run(() => processParameters.OnHealthCheck.Invoke());
                var onHealthCheckCompleted = healthCheckResultTask.Wait(TimeSpan.FromSeconds(HealthcheckTimeoutSeconds));
                if (!onHealthCheckCompleted)
                {
                    Log.Warn("Timed out waiting for onHealthCheck callback to respond. Reporting process as unhealthy.");
                    healthCheckResult = false;
                }
                else
                {
                    healthCheckResult = healthCheckResultTask.Result;
                    if (healthCheckResult)
                    {
                        Log.Info("Received TRUE from the onHealthCheck callback. Reporting process as healthy.");
                    }
                    else
                    {
                        Log.Warn("Received FALSE from the onHealthCheck callback. Reporting process as unhealthy.");
                    }
                }
            }
            catch (AggregateException aex)
            {
                if (aex.InnerExceptions.Any(ix => ix is TaskCanceledException))
                {
                    Log.Warn("Healthcheck task cancelled. Reporting process as unhealthy.");
                }
                else
                {
                    Log.Error("Encountered unexpected error when calling onHealthCheck callback. Reporting process as unhealthy.", aex);
                }

                healthCheckResult = false;
            }

            HeartbeatServerProcessRequest request = new HeartbeatServerProcessRequest(healthCheckResult);
            GenericOutcome outcome = webSocketRequestHandler.SendRequest(request);
            if (!outcome.Success)
            {
                Log.WarnFormat("Failed to report health status to GameLift service. Error: {0}", outcome.Error);
            }
        }

        public GenericOutcome InitializeNetworking(ServerParameters serverParameters)
        {
            var websocketUrl = Environment.GetEnvironmentVariable(EnvironmentVariableWebsocketUrl) ?? serverParameters.WebSocketUrl;
            processId = Environment.GetEnvironmentVariable(EnvironmentVariableProcessId) ?? serverParameters.ProcessId;
            hostId = Environment.GetEnvironmentVariable(EnvironmentVariableHostId) ?? serverParameters.HostId;
            fleetId = Environment.GetEnvironmentVariable(EnvironmentVariableFleetId) ?? serverParameters.FleetId;
            var authToken = Environment.GetEnvironmentVariable(EnvironmentVariableAuthToken) ?? serverParameters.AuthToken;
            
            if (string.IsNullOrEmpty(websocketUrl))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "websocketUrl is required in InitSDK ServerParameters"));
            }
            if (string.IsNullOrEmpty(processId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "processId is required in InitSDK ServerParameters"));
            }
            if (string.IsNullOrEmpty(hostId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "hostId is required in InitSDK ServerParameters"));
            }
            if (string.IsNullOrEmpty(fleetId))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "fleetId is required in InitSDK ServerParameters"));
            }
            if (string.IsNullOrEmpty(authToken))
            {
                return new GenericOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION, "authToken is required in InitSDK ServerParameters"));
            }
            
            return EstablishNetworking(websocketUrl, authToken);
        }

        private GenericOutcome EstablishNetworking(string webSocketUrl, string authToken)
        {
            return gameLiftWebSocket.Connect(webSocketUrl, processId, hostId, fleetId, authToken);
        }

        public GetComputeCertificateOutcome GetComputeCertificate()
        {
            Log.DebugFormat("Calling GetComputeCertificate");

            GetComputeCertificateRequest webSocketRequest = new GetComputeCertificateRequest();
            GenericOutcome outcome = webSocketRequestHandler.SendRequest(webSocketRequest);
            if (!outcome.Success)
            {
                return new GetComputeCertificateOutcome(outcome.Error);
            }

            return (GetComputeCertificateOutcome)outcome;
        }

        public GetFleetRoleCredentialsOutcome GetFleetRoleCredentials(GetFleetRoleCredentialsRequest request)
        {
            Log.DebugFormat("Calling GetFleetRoleCredentials: {0}", request);

            // If we've decided we're not on managed EC2, fail without making an APIGW call
            if (!onManagedEc2)
            {
                Log.DebugFormat("SDK is not running on managed EC2, fast-failing the request");
                return new GetFleetRoleCredentialsOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION));
            }

            // Check if we're cached credentials recently that still have at least 15 minutes before expiration
            if (instanceRoleResultCache.ContainsKey(request.RoleArn))
            {
                var previousResult = instanceRoleResultCache[request.RoleArn];
                if (previousResult.Expiration.Subtract(InstanceRoleCredentialTtlMin) > DateTime.UtcNow)
                {
                    Log.DebugFormat("Returning cached credentials which expire in {0} seconds", (previousResult.Expiration - DateTime.UtcNow).Seconds);
                    return new GetFleetRoleCredentialsOutcome(previousResult);
                }

                instanceRoleResultCache.Remove(request.RoleArn);
            }

            // If role session name was not provided, default to fleetId-hostId
            if (request.RoleSessionName.IsNullOrEmpty())
            {
                var generatedRoleSessionName = $"{fleetId}-{hostId}";
                if (generatedRoleSessionName.Length > RoleSessionNameMaxLength)
                {
                    generatedRoleSessionName = generatedRoleSessionName.Substring(0, RoleSessionNameMaxLength);
                }

                request.RoleSessionName = generatedRoleSessionName;
            }

            // Role session name cannot be over 64 chars (enforced by IAM's AssumeRole API)
            if (request.RoleSessionName.Length > RoleSessionNameMaxLength)
            {
                return new GetFleetRoleCredentialsOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION));
            }

            var rawOutcome = webSocketRequestHandler.SendRequest(request);
            if (!rawOutcome.Success)
            {
                return new GetFleetRoleCredentialsOutcome(rawOutcome.Error);
            }

            var outcome = (GetFleetRoleCredentialsOutcome)rawOutcome;
            var result = outcome.Result;

            // If we get a success response from APIGW with empty fields we're not on managed EC2
            if (result.AccessKeyId == string.Empty)
            {
                onManagedEc2 = false;
                return new GetFleetRoleCredentialsOutcome(new GameLiftError(GameLiftErrorType.BAD_REQUEST_EXCEPTION));
            }

            instanceRoleResultCache[request.RoleArn] = result;
            return outcome;
        }

        public void OnErrorResponse(string requestId, int statusCode, string errorMessage)
        {
            webSocketRequestHandler.HandleResponse(requestId, new GenericOutcome(new GameLiftError(statusCode, errorMessage)));
        }

        public void OnSuccessResponse(string requestId)
        {
            if (requestId != null)
            {
                webSocketRequestHandler.HandleResponse(requestId, new GenericOutcome());
            }
            else
            {
                Log.Info("RequestId was null");
            }
        }

        public void OnStartGameSession(GameSession gameSession)
        {
            // Inject data that already exists on the server
            gameSession.FleetId = fleetId;

            Log.DebugFormat("ServerState got the startGameSession signal. GameSession : {0}", gameSession);

            if (!processReady)
            {
                Log.Debug("Got a game session on inactive process. Ignoring.");
                return;
            }

            gameSessionId = gameSession.GameSessionId;

            Task.Run(() =>
            {
                processParameters.OnStartGameSession(gameSession);
            });
        }

        public void OnUpdateGameSession(GameSession gameSession, UpdateReason updateReason, string backfillTicketId)
        {
            Log.DebugFormat("ServerState got the updateGameSession signal. GameSession : {0}", gameSession);

            if (!processReady)
            {
                Log.Warn("Got an updated game session on inactive process.");
                return;
            }

            Task.Run(() =>
            {
                processParameters.OnUpdateGameSession(new UpdateGameSession(gameSession, updateReason, backfillTicketId));
            });
        }

        public void OnTerminateProcess(long terminationTime)
        {
            // TerminationTime is milliseconds that have elapsed since Unix epoch time begins (00:00:00 UTC Jan 1 1970).
            this.terminationTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(terminationTime);

            Log.DebugFormat("ServerState got the terminateProcess signal. termination time : {0}", this.terminationTime);

            Task.Run(() =>
            {
                processParameters.OnProcessTerminate();
            });
        }

        public void OnStartMatchBackfillResponse(string requestId, string ticketId)
        {
            StartMatchBackfillResult result = new StartMatchBackfillResult(ticketId);
            webSocketRequestHandler.HandleResponse(requestId, new StartMatchBackfillOutcome(result));
        }

        public void OnDescribePlayerSessionsResponse(string requestId, IList<PlayerSession> playerSessions, string nextToken)
        {
            DescribePlayerSessionsResult result = new DescribePlayerSessionsResult(playerSessions, nextToken);
            webSocketRequestHandler.HandleResponse(requestId, new DescribePlayerSessionsOutcome(result));
        }

        public void OnGetComputeCertificateResponse(string requestId, string certificatePath, string computeName)
        {
            GetComputeCertificateResult result = new GetComputeCertificateResult(certificatePath, computeName);
            webSocketRequestHandler.HandleResponse(requestId, new GetComputeCertificateOutcome(result));
        }

        public void OnGetFleetRoleCredentialsResponse(
            string requestId,
            string assumedRoleUserArn,
            string assumedRoleId,
            string accessKeyId,
            string secretAccessKey,
            string sessionToken,
            long expiration)
        {
            var result = new GetFleetRoleCredentialsResult(
                assumedRoleUserArn,
                assumedRoleId,
                accessKeyId,
                secretAccessKey,
                sessionToken,
                Epoch.AddMilliseconds(expiration));
            webSocketRequestHandler.HandleResponse(requestId, new GetFleetRoleCredentialsOutcome(result));
        }

        public void OnRefreshConnection(string refreshConnectionEndpoint, string authToken)
        {
            var outcome = EstablishNetworking(refreshConnectionEndpoint, authToken);

            if (!outcome.Success)
            {
                Log.ErrorFormat(
                    "Failed to refresh websocket connection. The GameLift SDK will try again each minute until the refresh succeeds, or the websocket is forcibly closed. {0}",
                    outcome.Error);
            }
        }

        public void Shutdown()
        {
            processReady = false;

            // Sleep thread for 1 sec.
            // This is to help deal with race conditions related to processReady flag being turned off (i.e. HeartbeatServerProcess)
            Thread.Sleep(TimeSpan.FromSeconds(1).Milliseconds);

            gameLiftWebSocket.Disconnect();
        }
    }
}
