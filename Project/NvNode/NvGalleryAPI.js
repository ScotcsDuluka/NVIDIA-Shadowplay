/* Copyright (c) 2015-2016, NVIDIA CORPORATION.  All rights reserved.
 *
 * NVIDIA CORPORATION and its licensors retain all intellectual property
 * and proprietary rights in and to this software, related documentation
 * and any modifications thereto.  Any use, reproduction, disclosure or
 * distribution of this software and related documentation without an express
 * license agreement from NVIDIA CORPORATION is strictly prohibited.
 */

'use strict'

var api = require('./shims/NvGalleryAPINode.js');
var _logger;

//! Helper function that receives body of POST request and calls callback for that data.
//! @param req      Request object provided by Express.
//! @param callback Callback that is triggered on succesfully downloaded data.
function getPostDataAndDo(req, callback) {
    var content = ''

    function onData(data) {
        content += data;
    }

    function onEnd() {
        callback(content);
    }

    req.on('data', onData);
    req.on('end', onEnd);
}

//! Helper function converts the returned error string into a JSON object
//! @param errorType      String type of error
//! @param errorString    String of error message
//! @return               JSON object of error data
function BuildErrorObject(errorType, errorString) {
    var returnCode;
    var returnText;
    var returnMessage;
    var returnUserData;
    var parts = errorString.split("::");
    for (var i = 0; i < parts.length; i++) {
        var index = parts[i].search("Text:")
        if (index != -1) {
            returnText = parts[i].substring(index + 5);
        }
        index = parts[i].search("Code:")
        if (index != -1) {
            returnCode = parts[i].substring(index + 5);
        }
        index = parts[i].search("Message:")
        if (index != -1) {
            returnMessage = parts[i].substring(index + 8);
        }
        index = parts[i].search("UserData:")
        if (index != -1) {
            returnUserData = parts[i].substring(index + 9);
        }
    }
    var errorResult = { type: errorType, code: returnCode, codeText: returnText, message: returnMessage, userData: returnUserData };
    _logger.info(errorResult);
    return errorResult;
}

//! Helper function that receives JSON body of POST request and calls callback for the parsed data.
//! @param req      Request object provided by Express.
//! @param res      Response object provided by Express.
//! @param callback Callback that is triggered on succesfully parsed data.
function getJSONDataAndDo(req, res, callback) {
    function parseAndCallback(content) {
        var parsed = {};
        try {
            parsed = JSON.parse(content);
        }
        catch (e) {
            res.writeHead(400, { 'Content-Type': 'text/html' });
            res.end(e.name + ': ' + e.message);
            return;
        }
        callback(parsed);
    }
    getPostDataAndDo(req, parseAndCallback);
}

//! Formats the error and makes a reply with appropriate HTTP code.
//! @param res Response object provided by Express.
//! @param err Error object.
function replyWithError(res, err, httpCode) {
    if (httpCode) {
        res.writeHead(httpCode, { 'Content-Type': 'application/json' });
    }
    else if ('invalidArgument' in err) {
        res.writeHead(400, { 'Content-Type': 'application/json' });
    }
    else {
        res.writeHead(500, { 'Content-Type': 'application/json' });
    }
    var errorResult = BuildErrorObject(err.name, err.message);
    var errorString = JSON.stringify(errorResult);
    _logger.error(errorString);
    res.end(errorString);
}

//! Helper function for using promises with nodebacks.
//! @param resolve The promise resolve.
//! @param reject  The promise reject.
function CallbackToPromise(resolve, reject) {
    return function (error, data) {
        if (error) {
            reject(error);
        }
        else {
            resolve(data);
        }
    }
};

function NativeCallbackToPromise(resolve, reject) {
    return function (error, data) {
        //
        // HACK: for some reasons node may delay resolving promises and calling .then
        // until next tick when reject or resolve is called from native addon.
        // This setImmediate forces execution to be continued in next tick.
        //
        if (error) {
            setImmediate((function () { reject(error); }));
        }
        else {
            setImmediate((function () { resolve(data); }));
        }
    }
};

function RegisterExpressEndpoints(app, logger) {
    //! Gets all files in a directory. 
    app.post('/Gallery/v.1.2/GetFolderListing', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                logger.info(JSON.stringify(data));
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }
        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryGetFolderListing12(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    // Gets the count of the number of screenshots/videos in a directory
    app.post('/Gallery/v.1.0/GetStats', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }
        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryGetStats(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    //! Gets "count" most recent items in the gallery.
    app.get('/Gallery/v.1.0/Recent/:count', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        try {
            api.GalleryGetRecent(doReply, parseInt(req.params.count));
        }
        catch (err) {
            replyWithError(res, err);
            console.error(err);
        }
    });

    //! Clears the recent list.
    app.post('/Gallery/v.1.0/Recent/Clear', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200);
                res.end(JSON.stringify(data));
            }
        }

        try {
            api.GalleryClearUploadHistory(doReply, false);
        }
        catch (err) {
            replyWithError(res, err);
            console.error(err);
        }
    });

    //! Gets upload history.
    app.get('/Gallery/v.1.0/Upload/History', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        try {
            api.GalleryGetUploadHistory(doReply);
        }
        catch (err) {
            replyWithError(res, err);
            console.error(err);
        }
    });

    //! Adds new upload history item.
    app.post('/Gallery/v.1.0/Upload/History', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200);
                res.end(JSON.stringify(data));
            }
        }
        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryAddUploadHistoryItem(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    //! Removes an item from the upload history.
    app.post('/Gallery/v.1.0/Upload/History/Remove', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200);
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryRemoveUploadHistory(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    //! Clears the entire upload history.
    app.post('/Gallery/v.1.0/Upload/History/Clear', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200);
                res.end(JSON.stringify(data));
            }
        }

        try {
            api.GalleryClearUploadHistory(doReply, true);
        }
        catch (err) {
            replyWithError(res, err);
            console.error(err);
        }
    });

    //! Removes videos from the fs/gallery.
    app.post('/Gallery/v.1.0/Remove', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200);
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryRemove(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    //! Gets metadata about a screenshot/video.
    app.post('/Gallery/v.1.0/GetFileMetaData', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryGetMetaData(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    //! Gets a thumbnail for a file.
    app.post('/Gallery/v.1.0/GetThumbnail', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryGetThumbnail(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    app.post('/Gallery/v.1.0/GetImageFileDimensions', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryGetImageDimensions(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    app.post('/Gallery/v.1.0/TranscodeMediaFile', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryTranscodeMediaFile(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    app.post('/Gallery/v.1.0/TranscodeVideoToGIF', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryTranscodeMediaFile(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    //! Gets system drive information.
    app.get('/Gallery/v.1.0/EnumerateDrives', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        try {
            api.GalleryEnumerateDrives(doReply);
        }
        catch (err) {
            replyWithError(res, err);
            console.error(err);
        }
    });

    //! Gets the CRC for a folder and all of its subfolders.
    app.post('/Gallery/v.1.0/GetFolderCRC', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryGetFolderCRC(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    //! Copies an NV file to another location
    app.post('/Gallery/v.1.0/CopyFile', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryCopyFile(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    // Check if a directory is writable
    app.post('/Gallery/v.1.0/IsDirectoryWritable', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200, { 'Content-Type': 'application/json' });
                res.end(JSON.stringify(data));
            }
        }
        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryIsDirectoryWritable(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
                console.error(err);
            }
        })
    });

    //! Decrypts and writes an encrypted bmp file to temp folder.
    app.post('/Gallery/v.1.0/WriteEncryptedBmp', function (req, res) {
        function doReply(err, data) {
            if (err) {
                replyWithError(res, err);
            }
            else {
                res.writeHead(200)
                res.end(JSON.stringify(data));
            }
        }

        getJSONDataAndDo(req, res, function (content) {
            try {
                api.GalleryWriteEncryptedBmp(doReply, content);
            }
            catch (err) {
                replyWithError(res, err);
            }
        })
    });

}


module.exports = function (app, io, logger) {
    if (app === undefined || io === undefined || logger == undefined) {
        throw 'You need to provide express app, socket io and logging';
    }

    //! Allows global access to logging module
    _logger = logger;

    //! This must be the last declaration in the file. Anything below it will be masked.
    return {
        version: function () {
            return api.GetVersion();
        },
        initialize: function () {
            return new Promise(function CreateInitializationPromise(resolve, reject) {
                api.Initialize(NativeCallbackToPromise(resolve, reject));
            }).then(function OnNativeAPIInitialized() {
                RegisterExpressEndpoints(app, logger);
                logger.info('NvGalleryAPI module initialized');
            });
        }
    };
};
