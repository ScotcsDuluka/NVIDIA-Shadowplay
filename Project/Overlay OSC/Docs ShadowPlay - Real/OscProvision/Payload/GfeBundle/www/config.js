'use strict';
angular.module('main.config', [])
    .constant('GFECLIENT_CONFIG', {
        "userAgent": "NVIDIAGFEClient",
        "windowName": "gfeclient",
        "mode": "prod",
        "perfmonOCTool": true,
        "jarvis": {
            "server": "",
            "version": "1",
            "deviceId": "gfeclient",
            "clientId": "135333107684344109",
            "commonClientId": "102009640608333825",
            "cevoClientId": "165235150188183768",
            "gfnClientId": "156448804920361176",
            "nvidiaWebClientId": "94211547528691713",
            "clientDescription": "GFE {VERSION}",
            "redirectUrl": "https://rds-assets.nvidia.com/main/redirect/gfe-redirect.html#",
            "redirectUrlJarvisOauth": "http://localhost:{portNumber}",
            "portNumbers": [32425, 31415, 33435, 34415, 64571],
            "onCompleteRedirectUrl": "",
            "redirectParams": ["error"],
            "defaultTimeout": 15000,
            "defaultTimeBetweenRetries": 500,
            "clientCapabilities": ["OAuthPortal"]
        },
        "gfwsl": {
            "server": "",
            "defaultTimeout": 30000,
            "postMethodTimeout": 60000
        },
        "gfservices": {
            "server": "https://services.gfe.nvidia.com/GFE/",
            "version": "v1.0"
        },
        "imageFormatService": {
            "server": "https://img.nvidiagrid.net/"
        },
        "jsEvents": {
            "server": "",
            "version": "v1.0",
            "schemaVersion": "7.0",
            "defaultRetries": 2,
            "defaultTimeBetweenRetries": 1000,
            "defaultTimeout": 30000,
            "msBetweenSendRequest": 5000,
            "maxEventsPerRequest": 128
        },
        "cms": {
            "server": "https://static.nvidiagrid.net/",
            "defaultRetries": 2
        },
        "nodeService": {
            "reconnection": true,
            "reconnectionDelay": 1000,
            "reconnectionDelayMax": 1000,
            "reconnectionAttempts": 20
        },
        "rds": {
            "clientId": "b7e85bb1-afe6-4867-b11f-a32a58b7f797",
            "portalRealm": "831c3b05-4aee-4060-998f-e67cc375c35f"
        },
        "rollbackFeatureExperimental": false,
        "rollbackFeatureAllowExternalDriversExperimental": false,
        "anselFeature": true,
        "anselFeatureExperimental": false,
        "parllayConfig": {
            "env": "pro",
            "fhtToken": "AqJLydSIPI"
        },
        "survey": {
            "server": "https://api.gfe.nvidia.com/Survey/surveyengine/",
            "clientType": "gfe"
        },
        "cevo": {
            "server": "https://api.commune.ly/"
        },
        "aem": {
            "server": ""
        },
        "feedback": {
            "server": "https://api.gfe.nvidia.com/Survey/feedbackengine/",
            "surveyId": "4b57d5dc-923d-4287-9b71-244eee8e30de",
            "clientType": "gfe",
            "version": "v.0.1"
        },
        "vrs": {
            "server": ""
        },
        "geo": {
            "server": "https://geo-location-api.gx.nvidia.com/",
            "version": "v1",
            "defaultTimeout": 30000
        },
        "gfnCloudGames": {
            "server": "https://gfn.nvidia.com",
            "version": 1,
            "defaultTimeout": 30000
        },
        "gxtarget": {
            "server": "https://gx-target-experiments-frontend-api.gx.nvidia.com/",
            "version": "v2",
            "defaultTimeout": 30000,
            "gxTargetJarvisClientId": "135333107684344109",
            "platform": "Win"
        },
        "starfleet": {
            "featureEnabled": false,
            "platformType": "Browser",
            "url": "https://stg.login.nvidia.com",
            "keyValueServiceUrl": "https://stg.userstore.nvidia.com/v1",
            "settingsUrl": "https://accounts.nvgs.nvidia.com/api/1",
            "settingsClientId": "156448804920361176",
            "deviceId": "gfeclient",
            "clientId": "A2mXF2zAQa4Hkp7zLy4L_upSOzmQ_UPz01yHAjRD4ZE",
            "scope": "openid consent tk_client",
            "redirectUrl": "http://localhost:{portnumber}",
            "portNumbers": [2259, 6460, 7119, 8870, 9096, 4358, 8090, 6545, 3287, 5639],
            "onCompleteRedirectUrl": "https://static-login.nvidia.com/service/default/logout",
            "redirectParams": ["error"],
            "defaultTimeout": 30000,
            "pingTimeout": 10000,
            "defaultRetries": 2,
            "defaultTimeBetweenRetries": 10000,
            "httpServerDuration": 1800000,
            "clientTokenRefreshDuration": 2332800000,
            "clientTokenRefreshDurationInPercentage": 30,
            "useDeviceAuthLogin": false
        },
        "redirect": {
            "server": ""
        },
        "pipl": {
            "server": "PiplConfig",
            "version": "v.1.0",
            "defaultTimeout": 30000
        }
    })
    .constant('GFECLIENT_BUILD_INFO', {
        "gfePackageVersion": "3.28.0.412",
        "branch": "rel_03_28",
        "branchType": "rel",
        "gfeclientVersion": "0.1.0",
        "gitHash": "73c62bae62",
        "buildType": "prod"
    });