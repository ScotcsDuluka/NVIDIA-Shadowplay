/* Copyright (c) 2015-2023, NVIDIA CORPORATION.  All rights reserved.
 *
 * NVIDIA CORPORATION and its licensors retain all intellectual property
 * and proprietary rights in and to this software, related documentation
 * and any modifications thereto.  Any use, reproduction, disclosure or
 * distribution of this software and related documentation without an express
 * license agreement from NVIDIA CORPORATION is strictly prohibited.
 */

'use strict'

////////////////////////////////////////////////////////////////////////////////
// Common utility and checks                                                  //
////////////////////////////////////////////////////////////////////////////////

if (process.version !== 'v11.13.0') {
    var err = 'nodejs version 11.13.0 is required, you are using ' + process.version;
    throw err;
}

process.env.UV_THREADPOOL_SIZE = 64;

var nvUtil = require('./NvUtil.node');
nvUtil.ClaimSingleInstance();

var fs = require('fs');
var logger = require('./Logger.js')(nvUtil, GetNvNodeAppdataDirectoryPath() + '\\nvnode.log');

var openSslConfigPath = GetNvNodeAppdataDirectoryPath() + '\\openssl.cfg';
process.env.OPENSSL_CONF = openSslConfigPath;

function SendNodeJSExceptionFeedback(message) {
    if (NvBackendAPI && typeof NvBackendAPI.addNodeJSCrashFeedbackSync === "function") {
        var err = NvBackendAPI.addNodeJSCrashFeedbackSync(message);
        if (err) {
            logger.error("Failed to send automatic feedback about NodeJS exception, reason: " + err);
        } else {
            logger.info("Successfully sent automatic feedback about NodeJS exception");
        }
    }
    else {
        logger.error("Failed to send automatic feedback about NodeJS exception, NvBackend plugin is not loaded or does not have AddFeedback functionality.");
    }
}

function OnUnhandledError(err) {
    try {
        if (err) {
            if (err.stack) {
                logger.error(err.stack);
            }
            else {
                logger.error(err);
            }
            SendNodeJSExceptionFeedback(err.toString());
        } else {
            SendNodeJSExceptionFeedback('undefined error');
        }
    } catch (e) {
        logger.error(e);
    }
    Shutdown(1);
}

process.on('uncaughtException', function (err) {
    logger.error("uncaughtException handler triggered!");
    OnUnhandledError(err);
})

const localSystemContainerServiceName = 'NvContainerLocalSystem';

function GfeIsInStandbyMode() {

    const SERVICE_AUTO_START = 2;
    const SERVICE_STOPPED = 1;

    let status = nvUtil.GetSystemServiceStatus(localSystemContainerServiceName);

    if (status.state !== SERVICE_STOPPED) {
        return false;
    }

    if (status.startupType === SERVICE_AUTO_START) {
        return false;
    }

    var path = GetNvidiaAppdataDirectoryPath() + '\\NVIDIA GeForce Experience\\CefCache';
    try {
        var st = fs.statSync(path);
        return !st.isDirectory();
    }
    catch (err) {
        return (err && err.code === 'ENOENT');
    }
}

if (GfeIsInStandbyMode()) {
    logger.infoSync('nodejs is exiting as GFE is in standby mode');
    Shutdown();
}

logger.info('Starting NvContainer Local System container...');
try {
    nvUtil.StartSystemService(localSystemContainerServiceName, 30000);
}
catch (err) {
    logger.error(err);
    Shutdown();
}

function GetNvidiaAppdataDirectoryPath() {
    return nvUtil.GetLocalAppdataPath() + '\\NVIDIA Corporation';
}

function GetSelfUpdateStatusFilePath() {
    var path = nvUtil.GetProgramDataPath() + '\\NVIDIA Corporation\\Downloader';

    try {
        fs.mkdirSync(path, { recursive: true });
    } catch (e) {
        return null;
    }

    return path;
}

function GetNvNodeAppdataDirectoryPath() {
    var path = GetNvidiaAppdataDirectoryPath();

    try {
        fs.mkdirSync(path);
    } catch (e) {
        if (e.code !== 'EEXIST') throw e;
    }

    path = path + '\\NvNode';
    try {
        fs.mkdirSync(path);
    } catch (e) {
        if (e.code !== 'EEXIST') throw e;
    }

    return path;
}

function GetNvNodeProgramDataDirectoryPath() {
    var path = nvUtil.GetProgramDataPath() + '\\NVIDIA Corporation\\NvNode';

    try {
        fs.mkdirSync(path, { recursive: true });
    } catch (e) {
        return null;
    }

    return path;
}

logger.info('Loading fast-boot dependency...');
var fastboot = require("fast-boot");
fastboot.start({ cacheFile: './module-locations-cache.json' });
logger.info('fast-boot ready');

logger.info('Loading ExpressJS dependency...');
var app = require('./node_modules/express/index.js')();
logger.info('ExpressJS ready');


logger.info('Loading HTTP dependency...');
var http = require('http');
logger.info('Creating HTTP server...');
var httpServer = http.createServer(app);
logger.info('HTTP ready');


logger.info('Loading Socket.IO dependency...');
var io = require('./node_modules/socket.io/lib/index.js')(httpServer);
logger.info('Socket.IO ready');

logger.info('Loading "on-finished" dependency...');
var onFinished = require('on-finished');

//
// Process each request:
// 1) Enable CORS and check security cookie.
// 2) Log requests and responses for debug purposes.
//

const securityCheckEnabled = nvUtil.IsSecurityCheckEnabled();
const securityCookie = nvUtil.GenerateRandom(16);

var nextRequestId = 1;

app.use(function (req, res, next) {
    res.header('Access-Control-Allow-Origin', '*');
    res.header('Access-Control-Allow-Methods', 'GET,POST');
    res.header('Access-Control-Allow-Headers', 'X_LOCAL_SECURITY_COOKIE, Content-Type, Content-Length');

    if (req.method !== 'OPTIONS' && securityCheckEnabled && req.headers.x_local_security_cookie != securityCookie) {
        logger.error('Rejecting request with invalid security cookie: ' + req.method + ' ' + req.originalUrl);
        res.writeHead(403, { 'Content-Type': 'text/html;charset=utf-8' });
        res.end('Security token is invalid');
        return;
    }

    var requestId = nextRequestId;
    nextRequestId++;

    logger.debug('Incoming request  #' + requestId + ': ' + req.method + ' ' + req.originalUrl);
    onFinished(res, function (err, res) {
        logger.debug('Response finished #' + requestId + ': ' + req.method + ' ' + req.originalUrl + ' with status ' + res.statusCode);
    });
    next();
});

io.use(function (socket, next) {
    if (securityCheckEnabled && socket.handshake.query.X_LOCAL_SECURITY_COOKIE != securityCookie) {
        next(new Error('Security token is invalid'));
    }
    else {
        next();
    }
});

//
// Logging socket.io connections.
//
io.on('connection', function (socket) {
    logger.info('Socket ' + socket.id + ' connected');

    socket.on('error', function (error) {
        logger.info('Socket ' + socket.id + ' error: ' + error);
    });

    socket.on('reconnect', function () {
        logger.info('Socket ' + socket.id + ' reconnected');
    });

    socket.on('reconnecting', function () {
        logger.info('Socket ' + socket.id + ' reconnecting');
    });

    socket.on('reconnect_attempt', function () {
        logger.info('Socket ' + socket.id + ' reconnect attempt');
    });

    socket.on('reconnect_error', function () {
        logger.error('Socket ' + socket.id + ' reconnect error');
    });

    socket.on('reconnect_failed', function () {
        logger.error('Socket ' + socket.id + ' reconnect failed');
    });

    socket.on('disconnect', function () {
        logger.error('Socket ' + socket.id + ' disconnected');
    });
});


//
// NvPiplConfig is a pre-requisite of NvBackend, load it first.
//

const gfeVersion = nvUtil.GetGFEVersionSync();
const nvNodeProgramDataPath = GetNvNodeProgramDataDirectoryPath();
logger.info('Loading NvPiplConfig module...');
const NvPiplConfig = require('./NvPiplConfig')(app, io, logger, gfeVersion, nvNodeProgramDataPath);
logger.info('NvPiplConfig module loaded');

//
// NvBackend module is mandatory, preload it before waiting for LS container.
//

var NvBackendAPI;
{
    if (securityCheckEnabled) {
        nvUtil.VerifyFileSignatureSync(__dirname + '\\NvBackendAPINode.node');
    }
    logger.info('Loading NvBackendAPI module...');
    NvBackendAPI = require('./NvBackendAPI.js')(httpServer, app, io, logger, NvPiplConfig);
    logger.info('NvBackendAPI module loaded');
}

//
// Some modules that don't depend on NvContainer could be loaded while LS container is starting.
//

var NvAccountAPI;
var DriverInstallAPI;
var downloaderAPI;

let modulesPreloadError = undefined;
try {
    if (securityCheckEnabled) {
        nvUtil.VerifyFileSignatureSync(__dirname + '\\NvAccountAPINode.node');
    }
    logger.info('Loading AccountAPI module...');
    NvAccountAPI = require('./NvAccountAPI.js')(app, io, logger, SendNodeJSExceptionFeedback, NvBackendAPI, NvPiplConfig);
    logger.info('AccountAPI module loaded');

    if (securityCheckEnabled) {
        nvUtil.VerifyFileSignatureSync(__dirname + '\\DriverInstall.node');
    }
    logger.info('Loading DriverInstallAPI module...');
    DriverInstallAPI = require('./DriverInstallAPI.js')(app, io, logger);
    logger.info('DriverInstallAPI module loaded');

    if (securityCheckEnabled) {
        nvUtil.VerifyFileSignatureSync(__dirname + '\\Downloader.node');
    }
    logger.info('Loading downloaderAPI module...');
    downloaderAPI = require('./downloader.js')(app, io, logger);
    logger.info('downloaderAPI module loaded');
} catch (e) {
    // Do not throw error right away, try to keep it until reporting is up.
    logger.error(e);
    modulesPreloadError = e;
}

function FileExists(path) {
    try {
        var st = fs.statSync(path);
        return st.isFile();
    }
    catch (err) {
        return !(err && err.code === 'ENOENT');
    }
}

//
// Check signature of native modules before waiting for NvContainerLS.
//

function VerifySignatureIfFileExists(path) {
    if (FileExists(path)) {
        nvUtil.VerifyFileSignatureSync(path);
    } else {
        logger.info("Skipping signature verification as file doesn't exist: ", path);
    }
}

let signatureVerificationError = undefined;

if (securityCheckEnabled) {
    logger.info('Verifying native module signatures...');

    try {
        VerifySignatureIfFileExists(__dirname + '\\NvCameraAPINode.node');
        VerifySignatureIfFileExists(__dirname + '\\NVGalleryAPINode.node');
        VerifySignatureIfFileExists(__dirname + '\\NvGameStreamAPINode.node');
        VerifySignatureIfFileExists(__dirname + '\\NvShadowPlayAPINode.node');
        VerifySignatureIfFileExists(__dirname + '\\NvSDKAPINode.node');
        VerifySignatureIfFileExists(__dirname + '\\NvABHubAPI.node');
    } catch (e) {
        // Do not throw error right away, try to keep it until reporting is up.
        logger.error(e);
        signatureVerificationError = e;
    }
}


//
// Wait for LocalSystem container if it is not yet running.
//

try {
    logger.info('Waiting for NvContainer Local System container to start...');
    nvUtil.WaitSystemService(localSystemContainerServiceName);
    logger.info('NvContainer Local System container started');
}
catch (err) {
    logger.error(err);
    Shutdown();
}


////////////////////////////////////////////////////////////////////////////////
// Module initialization chain                                                //
////////////////////////////////////////////////////////////////////////////////

NvBackendAPI.initialize().catch(function (err) {
    NvBackendAPI = undefined;
    OnUnhandledError(err);
}).then(LoadNVIDIAModules).then(StartHTTPServer).then(RegisterShutdownCallbacks).catch(OnUnhandledError);

////////////////////////////////////////////////////////////////////////////////
// Other NVIDIA modules                                                             //
////////////////////////////////////////////////////////////////////////////////

var NvCommonTasks;
var NvAutoDriverDownload;
var NvAutoGFEDownload;

var NvGameStreamAPI;
var ShadowPlayAPI;
var NvGalleryAPI;
var NvCameraAPI;
var NvSDKAPI;
var NvAbHubAPI;

function ReportOptionalModuleLoadError(err) {
    logger.error("Optional module load error!");
    if (err.stack) {
        logger.error(err.stack);
    }
    else {
        logger.error(err);
    }

    SendNodeJSExceptionFeedback("Optional module load error: " + err.toString());
}

function LoadNVIDIAModules() {

    if (signatureVerificationError) {
        throw signatureVerificationError;
    }

    if (modulesPreloadError) {
        throw modulesPreloadError;
    }

    let promises = [];

    // NvPiplConfig is already loaded, initialize it.
    promises.push(NvPiplConfig.initialize());

    try {
        logger.info('NvAbHubAPI: Loading module...');
        NvAbHubAPI = require('./NvAbHubAPI.js')(app, io, logger);
        promises.push(NvAbHubAPI.initialize().catch(function (err) {
            logger.error("NvAbHubAPI: Initialization failed");
            NvAbHubAPI = undefined;
            ReportOptionalModuleLoadError(err);
        }));
    } catch (err) {
        logger.error("NvAbHubAPI: Module load failed");
        NvAbHubAPI = undefined;
        ReportOptionalModuleLoadError(err);
    }

	const nvSelfUpdateStatusFilePath = GetSelfUpdateStatusFilePath();
    // DownloaderAPI is already loaded, time to initialize it and modules that depend on it.
    promises.push(downloaderAPI.initialize()
        .then(function LoadAutoDownloadModules() {
            logger.info('Loading NvCommonTasks...');
            NvCommonTasks = require('./NvCommonTasks.js')();
            logger.info('NvCommonTasks loaded');

            logger.info('Loading NvAutoDriverDownload module...');
            NvAutoDriverDownload = require('./NvAutoDriverDownload.js')(NvCommonTasks, NvBackendAPI, downloaderAPI, logger);
            logger.info('NvAutoDriverDownload module loaded');

            logger.info('Loading NvAutoDownload module...');
            NvAutoGFEDownload = require('./NvAutoDownload.js');
            NvAutoGFEDownload.setAppDataPath(GetNvNodeAppdataDirectoryPath());
            logger.info('NvAutoDownload module loaded');
        })
        .then(function InitializeAutoDownloadModules() {
            return NvAutoGFEDownload.initialize(app, io, logger, nvUtil, NvCommonTasks, NvAccountAPI, NvBackendAPI, nvSelfUpdateStatusFilePath);
        }));

    // NvAccountAPI is already loaded, time to initialize it.
    promises.push(NvAccountAPI.initialize());

    try {
        logger.info('Loading NvGameStreamAPI module...');
        NvGameStreamAPI = require('./NvGameStreamAPI.js')(app, io, logger);
        logger.info('NvGameStreamAPI module loaded');
    } catch (err) {
        NvGameStreamAPI = undefined;
        ReportOptionalModuleLoadError(err);
    }

    if (NvGameStreamAPI) {
        promises.push(NvGameStreamAPI.initialize().catch(function (err) {
            NvGameStreamAPI = undefined;
            ReportOptionalModuleLoadError(err);
        }));
    }

    try {
        logger.info('Loading GalleryAPI module...');
        NvGalleryAPI = require('./NvGalleryAPI.js')(app, io, logger);
        logger.info('GalleryAPI module loaded');
    } catch (err) {
        NvGalleryAPI = undefined;
        ReportOptionalModuleLoadError(err);
    }

    let shadowplayDependencies = [];
    if (NvGalleryAPI) {
        let nvGalleryInitPromise = NvGalleryAPI.initialize().catch(function (err) {
            NvGalleryAPI = undefined;
            ReportOptionalModuleLoadError(err);
        });
        shadowplayDependencies.push(nvGalleryInitPromise);
        promises.push(nvGalleryInitPromise);
    }

    try {
        logger.info('Loading NvCameraAPI module...');
        NvCameraAPI = require('./NvCameraAPI.js')(app, io, logger);
        logger.info('NvCameraAPI module loaded');
    } catch (err) {
        NvCameraAPI = undefined;
        ReportOptionalModuleLoadError(err);
    }
    if (NvCameraAPI) {
        let nvCameraInitPromise = NvCameraAPI.initialize().catch(function (err) {
            NvCameraAPI = undefined;
            ReportOptionalModuleLoadError(err);
        });
        shadowplayDependencies.push(nvCameraInitPromise);
        promises.push(nvCameraInitPromise);
    }

    function LoadShadowPlay() {
        try {
            logger.info('Loading ShadowPlayAPI module...');
            ShadowPlayAPI = require('./NvShadowPlayAPI.js')(app, io, logger);
        } catch (err) {
            ShadowPlayAPI = undefined;
            ReportOptionalModuleLoadError(err);
        }
        if (ShadowPlayAPI) {
            logger.info('ShadowPlayAPI initalizing...');
            let spPromise = ShadowPlayAPI.initialize().catch(function (err) {
                logger.error("LoadShadowPlay initialize catch");
                ShadowPlayAPI = undefined;
                ReportOptionalModuleLoadError(err);
            });
            return spPromise;
        }
    }

    function LoadSDK() {
        if (!ShadowPlayAPI) {
            return;
        }

        var api;
        try {
            logger.info('Loading NvSDKAPI...');
            api = require('./NvSDKAPINode.node');
            NvSDKAPI = require('./NvSDKAPI.js')(app, io, logger, api);
            logger.info('NvSDKAPI initalizing...');
            let sdkPromise = NvSDKAPI.initialize().catch(function (err) {
                logger.error("LoadSDK initialize catch");
                NvSDKAPI = undefined;
                ReportOptionalModuleLoadError(err);
            });
            return sdkPromise;
        } catch (err) {
            NvSDKAPI = undefined;
            ReportOptionalModuleLoadError(err);
        }
    }

    promises.push(Promise.all(shadowplayDependencies).then(LoadShadowPlay).then(LoadSDK));

    return Promise.all(promises);
}

////////////////////////////////////////////////////////////////////////////////
// Common endpoints                                                           //
////////////////////////////////////////////////////////////////////////////////

app.get('/version', function (req, res) {
    var data = {};
    data.node = process.version;
    data.NvBackendAPI = NvBackendAPI.version();
    data.NvAccountAPI = NvAccountAPI.version();
    data.DriverInstallAPI = DriverInstallAPI.version();
    data.downloaderAPI = downloaderAPI.version();
    data.NvCommonTasks = NvCommonTasks.version();
    data.NvAutoDriverDownload = NvAutoDriverDownload.version();
    data.NvAutoGFEDownload = NvAutoGFEDownload.version();

    if (NvGameStreamAPI) {
        data.NvGameStreamAPI = NvGameStreamAPI.version();
    }
    if (ShadowPlayAPI) {
        data.ShadowPlayAPI = ShadowPlayAPI.version();
    }
    if (NvGalleryAPI) {
        data.NvGalleryAPI = NvGalleryAPI.version();
    }
    if (NvCameraAPI) {
        data.NvCameraAPI = NvCameraAPI.version();
    }
    if (NvSDKAPI) {
        data.NvSDKAPI = NvSDKAPI.version();
    }

    res.writeHead(200, { 'Content-Type': 'application/json' });

    if (NvAbHubAPI) {
        data.NvAbHubAPI = NvAbHubAPI.version();
    }
    res.end(JSON.stringify(data));
});

app.get('/beta', function (req, res) {

    let flag = false;
    try {
        flag = nvUtil.GetGFE3BetaFlagSync();
    }
    catch (err) {
        flag = false;
    }

    const data = { 'beta': flag };
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(data));

});

app.get('/Settings/v.1.0/Language', function (req, res) {
    var data = {};
    var languageValue = '';
    try {
        languageValue = nvUtil.GetLanguage();
        logger.info('Language:' + languageValue);
        if (languageValue !== undefined) {
            data.language = languageValue;
        }
        else {
            data.language = '';
        }
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify(data));
    }
    catch (err) {
        replyWithError(res, err);
    }
});

app.post('/Settings/v.1.0/Language', function (req, res) {
    var parsed = {};
    function onData(data) {
        logger.info('Data:' + data);
        try {
            parsed = JSON.parse(data);
        }
        catch (err) {
            replyWithError(res, err);
        }
    }

    function onEnd() {
        try {
            logger.info('Language:' + parsed.language);
            nvUtil.SaveLanguage(parsed.language);
            setImmediate(function () {
                io.emit('/Settings/v.1.0/Language', { language: parsed.language });
            });
            res.writeHead(200)
            res.end();

            NvBackendAPI.notifyUiLanguageChange(parsed.language);
        }
        catch (err) {
            replyWithError(res, err);
        }
    }

    req.on('data', onData);
    req.on('end', onEnd);
});

app.get('/threadpool', function (req, res) {
    var data = {};
    data.size = process.env.UV_THREADPOOL_SIZE;
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify(data));
});

//empty get, just to check if Node is up.
app.get('/up', function (req, res) {
    res.writeHead(200, { 'Content-Type': 'application/json', 'Cache-Control': 'no-cache' });
    res.end();
});

//! Formats the error and makes a reply with appropriate HTTP code.
//! @param res Response object provided by Express.
//! @param err Error object.
function replyWithError(res, err) {
    logger.error(err);
    if ('invalidArgument' in err) {
        res.writeHead(400, { 'Content-Type': 'text/html;charset=utf-8' });
    }
    else {
        res.writeHead(500, { 'Content-Type': 'text/html;charset=utf-8' });
    }

    var errorString = JSON.stringify(err);
    logger.error(errorString);
    res.end(errorString);
}

////////////////////////////////////////////////////////////////////////////////
// Starting up                                                                //
////////////////////////////////////////////////////////////////////////////////

function StartHTTPServer() {

    return new Promise(function StartHTTPServerPromise(resolve, reject) {

        logger.info('Starting HTTP server...');

        // Number of attempts to start listening before giving up.
        let listenAttemptCount = 5;

        function TryListening() {
            let host = '127.0.0.1';
            let portNumber = nvUtil.GetPortOverride();
            if (portNumber) {
                logger.info('Overriding port number with ' + portNumber);
            } else {
                portNumber = 0;
            }
            logger.info('Trying to listen http://%s:%s', host, portNumber);
            httpServer.listen(portNumber, host, OnListening);
            listenAttemptCount--;
        }

        function OnListening() {
            var host = httpServer.address().address;
            var port = httpServer.address().port;

            logger.info('Server is listening at http://%s:%s', host, port);
            //
            // Create file with security current port number and cookie.
            // See also http://jirasw.nvidia.com/browse/CRIMSON-978 and http://jirasw.nvidia.com/browse/CRIMSON-1205
            //
            let config = {};
            config.port = httpServer.address().port;
            config.secret = securityCookie;

            nvUtil.ConfirmInitialization(JSON.stringify(config));
            logger.info('Initialization complete.');
            resolve();
        }

        httpServer.on('error', function HandleHttpServerError(e) {
            logger.error('HTTP server error: ' + e);
            switch (e.syscall) {
                case 'listen':
                    //
                    // Listen failed. Try to listen again if possible.
                    //
                    if (listenAttemptCount > 0) {
                        setTimeout(TryListening, 1000);
                    }
                    else {
                        logger.error('Listen attempt count exceeded, giving up.');
                        reject(e);
                    }
                    break;
                default:
                    //
                    // It is unclear what to do with the rest, just re-throw.
                    //
                    throw e;
            }
        });

        TryListening();
    });
}

////////////////////////////////////////////////////////////////////////////////
// Shutdown logic                                                             //
////////////////////////////////////////////////////////////////////////////////

function Shutdown(exitCode) {
    logger.info('Shutting down.');

    //
    // TODO: call modules shutdown functions here.
    //

    if (NvSDKAPI) {
        NvSDKAPI.Cleanup();
    }

    if (NvCameraAPI) {
        NvCameraAPI.Cleanup();
    }

    if (ShadowPlayAPI) {
        ShadowPlayAPI.Cleanup();
    }

    if (NvAbHubAPI) {
        NvAbHubAPI.cleanup();
    }

    if (downloaderAPI) {
        downloaderAPI.cleanup();
    }

    if (NvAccountAPI) {
        NvAccountAPI.cleanup();
    }

    if (NvGameStreamAPI) {
        NvGameStreamAPI.cleanup();
    }

    if (NvPiplConfig) {
        NvPiplConfig.cleanup();
    }

    logger.infoSync('Stopping logging.');
    logger.destroyLogger();
    process.exit(exitCode);
};

function OnSIGTERM() {
    logger.info('Received SIGTERM.');
    Shutdown();
}

function OnSIGINT() {
    logger.info('Received SIGINT.');
    Shutdown();
}

function OnShutdownRequested() {
    logger.info('Shutdown requested.');
    Shutdown();
}

function RegisterShutdownCallbacks() {
    logger.debug('Registering shutdown callbacks...');
    process.on('SIGTERM', OnSIGTERM);
    process.on('SIGINT', OnSIGINT);
    nvUtil.SetExitCallback(OnShutdownRequested);
}
