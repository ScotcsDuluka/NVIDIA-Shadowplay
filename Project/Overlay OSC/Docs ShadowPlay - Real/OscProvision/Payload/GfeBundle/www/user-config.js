'use strict';
angular.module('main.userConfig', [])
    .constant('GFECLIENT_USER_CONFIG', {
        "verboseLoggingEnabled": true,
        "eventLoggingEnabled": false,
        "perfLoggingEnabled": true,
        "imageLoadPerfEnabled": false
    });