/*!
 * Copyright (c) 2015-2018, NVIDIA CORPORATION.  All rights reserved.       
 *                                                                           
 * NVIDIA CORPORATION and its licensors retain all intellectual property     
 * and proprietary rights in and to this software, related documentation     
 * and any modifications thereto.  Any use, reproduction, disclosure or      
 * distribution of this software and related documentation without an express
 * license agreement from NVIDIA CORPORATION is strictly prohibited.
 */
webpackJsonp([6],[function(e,t,o){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),t.ngRedirectModule=void 0,o(60),o(61);var n=angular.module("redirect",["ui.router"]);n.config(["$stateProvider",function(e){e.state("redirect",{url:"?code&state",onEnter:["$window","$stateParams",function(e,t){e.open("","gfeclient").postMessage({code:t.code,state:t.state,time:new Date},"*")}]})}]),t.ngRedirectModule=n}]);
//# sourceMappingURL=redirect.js.map