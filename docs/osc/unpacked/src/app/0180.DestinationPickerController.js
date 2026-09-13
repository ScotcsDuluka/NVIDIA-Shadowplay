// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 180
// controller DestinationPickerController
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function i(e) {
    if (e && e.__esModule) return e;
    var t = {};
    if (null != e)
      for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
    return t.default = e, t;
  }

  function o(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.DestinationPickerController = void 0;
  var r = require(8),
    a = o(r),
    l = require(64),
    s = o(l),
    d = require(3),
    c = (i(d), require(1) /* app/1 — main (module) */);
  require(11), require(90) /* app/90 — nvFallbackSrc (directive) */;
  var u = c.ngMainModule.controller("DestinationPickerController", ["$scope", "$log", "$filter", "$timeout",
    "$window", "$q", "eventAggregator", "_", "connectService", "oscGalleryService", "ugcService",
    "CONNECT_EVENTS", "GALLERY_TYPES", "GALLERY_SUBTYPES",
    function(e, t, n, i, o, r, l, d, c, u, f, m, g, p) {
      function h(e) {
        return e === p.STEREO || e === p.STEREO_360 || e === p.MONO_360 || e === p.SUPER_RESOLUTION ||
          e === p.NORMAL_ANSEL || e === p.EXR;
      }

      function b() {
        v.shouldShowBrowserIcon = f.checkIfBrowserLogin(v.useUploadService.name), c.canLoginToService(v
          .useUploadService.name) ? v.shouldShowBrowserIcon ? v.loginString = n("translate")(
          "l10n.loginBrowserRequired", {
            arg1: n("translate")(v.useUploadService.title)
          }) : v.loginString = n("translate")("l10n.connectNotLoggedIn") : v.loginString = n(
          "translate")("l10n.toottipUploadScreenshotsToSWGF");
      }
      var x,
        v = this,
        y = t.getInstance("osc/DestinationPickerController");
      v.selected = "", v.selectedPrivacy = void 0, v.selectedLocationType = void 0, v
        .selectedLocation = {}, v.locations = {}, v.avatarValid = !1, v.useServiceType = "", v
        .useUploadService = "", v.serviceType = "", v.fileToUpload = "", v.uploadData = void 0, v
        .broadcast = !1, v.title = void 0, v.showMoments = !1, v.checkPattern = !1, v
        .shouldShowWarningNotice = !1, v.youTube = n("translate")("l10n.youTube"), v.titleMessageLabel =
        "", v.savedProviderTitles = {}, v.nvChangePicker = "", v.isGifableSource = !1, v.outputTypes = (
          x = {}, (0, s.default)(x, p.NORMAL, {
            type: g.VIDEO,
            subtype: p.NORMAL,
            displayName: n("translate")("l10n.videoRecording"),
            default: !0
          }), (0, s.default)(x, p.GIF, {
            type: g.VIDEO,
            subtype: p.GIF,
            displayName: n("translate")("l10n.animatedGIF")
          }), x), v.selectedOutputType = v.outputTypes[p.NORMAL], v.selectedDestination = null, v
        .isFormatBoxDisabled = !1, v.validConnection = !1, e.textDestination = "l10n.destination", e
        .textPostAs = "l10n.postAs", e.textTitle = "l10n.title", e.textLocation = "l10n.location", e
        .textAudience = "l10n.audience", e.textPage = "l10n.page", e.textGroup = "l10n.group", e
        .textFormat = "l10n.format", e.leftWidth = "", e.rightWidth = "", e.lastItemWidth1 = "", e
        .lastItemWidth2 = "", e.lastItemWidth3 = "";
      var w = 100,
        S = 120;
      v.titleMaxLength = w;
      var E = "last-logged-services",
        k = c.serviceTypes.STREAMING,
        _ = c.serviceTypes.IMAGE_UPLOAD,
        T = c.serviceTypes.VIDEO_UPLOAD,
        C = c.serviceTypes.ANSEL_UPLOAD,
        O = c.serviceTypes.GIF_UPLOAD;
      v.shouldShowLoginButton = function() {
        return !!c.canLoginToService(v.useUploadService.name);
      }, v.setOutputType = function() {
        v.selectedOutputType.subtype === p.GIF ? y.info("Switching to GIF UI") : y.info(
          "Switching to normal UI"), u.setOutputFormat(v.selectedOutputType.subtype), v.refresh();
      }, v.setUploadData = function() {
        y.info("setUploadData Entered");
        var t = v.selectedOutputType;
        v.fileToUpload && v.fileToUpload.file && v.fileToUpload.file.type !== g.VIDEO && (t = void 0), v
          .uploadData = {
            isLoggedIn: v.isLoggedIn,
            uploadTitle: v.title,
            selectedPrivacy: v.selectedPrivacy,
            selectedLocationType: v.selectedLocationType,
            selectedLocation: v.selectedLocation && v.selectedLocationType ? v.selectedLocation[v
              .selectedLocationType.param] : void 0,
            serviceType: v.serviceType,
            uploadService: v.uploadService,
            selectedOutputType: t,
            validConnection: v.validConnection
          }, e.onUploadDataChanged({
            data: v.uploadData
          });
      }, v.setTargetService = function(e) {
        if (e || (e = d.find(v.services, function(e) {
            if (e.service.name === v.selectedDestination) return e;
          })), e && v.selected !== e) {
          y.info("setTargetService: ", e.service.name);
          var t = v.selected.service;
          v.selected = e, v.checkPattern = v.selected.service.name === v.youTube, v.useUploadService = e
            .service, v.broadcast === !1 && v.fileToUpload ? v.useServiceType = v.fileToUpload.file
            .type === g.VIDEO ? T | O : _ : v.useServiceType = c.serviceTypes.STREAMING, v.processTitle(
              t), v.refresh(), u.setOutputService(v.useUploadService);
        }
      }, v.isActive = function(e) {
        return v.selected === e;
      }, v.login = function() {
        e.onLoginViaParent({
          uploadService: v.uploadService
        }), o.localStorage.setItem(E, (0, a.default)(v.uploadService.name));
      }, v.pickerDisabled = function() {
        return void 0 === v.isLoggedIn || !v.isLoggedIn;
      }, v.findService = function() {
        var e = v.useUploadService;
        void 0 !== e && (v.selected = d.find(v.services, function(t) {
          if (t.service === e) return t;
        }));
      }, v.checkUploadService = function() {
        if (v.broadcast !== !0 && v.fileToUpload) {
          var e = !1;
          if (void 0 !== v.useUploadService) {
            var t = u.getDPSettings();
            if (v.fileToUpload.file.type === g.VIDEO ? t ? (e = !0, v.useServiceType = t.outputType ===
                p.GIF ? O : T) : 0 === (v.useUploadService.type & (T | O)) && (e = !0, v
                .useServiceType = T | O) : v.fileToUpload.file.type === g.IMAGE && (h(v.fileToUpload
                  .file.subtype) && 0 === (v.useUploadService.type & C) ? (e = !0, v.useServiceType =
                C) : 0 === (v.useUploadService.type & _) && (e = !0, v.useServiceType = _)), e) {
              var n = t.service || c.getLastUsedServiceOrDefault(v.useServiceType),
                i = c.getLastUploadedVideoTypeOrDefault();
              v.selectedOutputType = t ? v.outputTypes[t.outputType] : i & 0 === _ ? v.outputTypes[p
                .NORMAL] : v.outputTypes[p.GIF], v.initializeService(n), v.findService(), y.info(
                "Changing upload service to: ", v.useUploadService.name);
            }
            A();
          }
        }
      };
      var A = function() {
        if (v.broadcast !== !0 && v.fileToUpload) {
          var e = v.useUploadService.type;
          v.selectedOutputType.subtype === p.NORMAL && 0 === (e & T) ? v.selectedOutputType = v
            .outputTypes[p.GIF] : v.selectedOutputType.subtype === p.GIF && 0 === (e & O) && (v
              .selectedOutputType = v.outputTypes[p.NORMAL]);
          var t = 0;
          (v.useUploadService.type & O) === O && t++, (v.useUploadService.type & T) === T && t++, t <
            2 ? v.isFormatBoxDisabled = !0 : v.isFormatBoxDisabled = !1, v.fileToUpload.file.type === g
            .VIDEO && (u.setOutputFormat(v.selectedOutputType.subtype), u.setOutputService(v
              .useUploadService));
        }
      };
      v.updateUploadService = function() {
        void 0 !== v.useUploadService && (v.isLoggedIn = c.isLoggedIntoService(v.useUploadService.name),
          v.isLoggedIn ? (c.getUserName(v.useUploadService.name).then(function(e) {
            v.loginString = e;
          }, function(e) {
            y.info("undateUploadService Error: ", e), v.isLoggedIn = !1, b();
          }), c.getAvatarUri(v.useUploadService.name).then(function(e) {
            v.userAvatarUri = e, v.avatarValid = void 0 !== v.userAvatarUri && null !== v
              .userAvatarUri;
          }, function(e) {
            y.info("updateUploadService Error: ", e), v.isLoggedIn = !1, b();
          })) : b());
      }, v.getFacebookLocationInfo = function() {
        v.hasLocations = !1;
        var e = {
          page: c.endpoints[v.useUploadService.providerName].getPages(),
          group: c.endpoints[v.useUploadService.providerName].getGroups()
        };
        return r.all(e).then(function(e) {
          return d.each(e, function(e, t) {
            v.locations[t] = e, v.locations[t].length > 0 && (v.hasLocations |= !0, void 0 !== v
              .savedParams.destination && (v.selectedLocation[t] = d.findWhere(v.locations[
              t], {
                name: v.savedParams.destination
              })), void 0 === v.selectedLocation[t] && (v.selectedLocation[t] = d.find(v
                .locations[t],
                function() {
                  return !0;
                })));
          }), r.when(!0);
        }, function(e) {
          return v.locations = {}, r.when(!1);
        });
      }, v.getTwitchTitleInfo = function() {
        if (v.isLoggedIn === !0) {
          var e = c.endpoints[v.useUploadService.providerName];
          return e.getUser().then(function(t) {
            return e.getBroadcastTitle().then(function(e) {
              return v.useUploadService.providerName === c.providerNames.TWITCH && (v.title =
                e), r.when(!0);
            });
          });
        }
        return v.title = void 0, r.when(!1);
      }, v.processTitle = function(e) {
        v.broadcast === !1 && (e && (v.savedProviderTitles[e.name] = v.title), v.title = v
          .savedProviderTitles[v.useUploadService.name]), v.placeholderTitle = n("translate")(
          "l10n.placeholderTitle"), v.bracketsValidationText = n("translate")(
          "l10n.bracketsValidationText");
      }, v.preprocessTitles = function(e) {
        v.broadcast === !1 && v.fileToUpload && d.each(e, function(e, t) {
          v.showMoments === !1 ? v.savedProviderTitles[t] = n("translate")(
            "l10n.uploadVideoMarketingSuffixNew", {
              arg1: v.fileToUpload.folder
            }) : v.savedProviderTitles[t] = n("translate")(
            "l10n.uploadVideoMarketingSuffixMoments", {
              arg1: v.fileToUpload.folder,
              arg2: v.fileToUpload.moment.highlightName
            });
        });
      }, v.cleanupTitleForm = function() {
        v.title = void 0, v.titleMaxLength = 1e3;
        var t;
        t = v.useUploadService.maxTitleChars ? v.useUploadService.maxTitleChars : v.fileToUpload && v
          .fileToUpload.file.type === g.IMAGE ? S : w;
        var n = i(function() {
            v.titleMaxLength = t, i.cancel(n);
          }),
          o = v.checkPattern === !1 ? e.titleForm1 : e.titleForm2;
        o.$setPristine(), o.$setValidity(), o.$setUntouched();
      }, v.showData = function() {
        v.cleanupTitleForm(), v.processTitle(), v.updateUploadService(), v.availablePrivacyOptions = d
          .filter(v.useUploadService.privacyOptions, function(e) {
            if (!e.scope) return !0;
            var t = v.useServiceType;
            return v.fileToUpload.file.type === g.VIDEO && (t = v.selectedOutputType.subtype === p
              .GIF ? O : T), (e.scope & t) === t;
          }), v.savedParams = c.getServiceParameters(v.useUploadService.name), v.selectedPrivacy = v
          .savedParams.privacy;
        var e = !1;
        if (v.selectedPrivacy) {
          var t = d.indexOf(v.availablePrivacyOptions, v.selectedPrivacy);
          e = t < 0;
        } else e = !0;
        return e && (v.selectedPrivacy = d.find(v.availablePrivacyOptions, function() {
            return !0;
          })), v.selectedLocationType = v.savedParams.destinationType, void 0 === v
          .selectedLocationType && (v.selectedLocationType = d.find(v.destinationOptions, function() {
            return !0;
          })), v.useUploadService.providerName === c.providerNames.FACEBOOK ? v
          .getFacebookLocationInfo() : v.useUploadService.providerName === c.providerNames.TWITCH ? v
          .getTwitchTitleInfo() : r.when(!0);
      }, v.cleanupDestinationOptions = function() {
        v.destinationOptions = d.reject(v.uploadService.destinationOptions, function(e) {
          return void 0 !== v.locations[e.param] && 0 === v.locations[e.param].length;
        });
      }, v.initializeService = function(e) {
        v.useUploadService = e, "" !== v.useServiceType && "" !== v.useUploadService ? (v.serviceType =
          v.useServiceType, v.uploadService = v.useUploadService, !v.broadcast && v.fileToUpload && v
          .fileToUpload.file.type === g.VIDEO ? v.isGifableSource = !0 : v.isGifableSource = !1, y
          .info("InitializeService uploadServiceType: ", v.serviceType), y.info(
            "InitializeService uploadService: ", v.uploadService.name)) : (y.error("Wrong Type: ", v
          .useServiceType), y.error("Wrong Service: ", v.useUploadService)), c.doubleCheckConnection(e
          .name);
      }, v.initialize = function() {
        y.info("Initialize Destination Picker");
        var e = d.sortBy(c.services, "name");
        v.filteredServices = c.getServices(v.useServiceType), v.preprocessTitles(v.filteredServices);
        var t = v.useServiceType === k,
          n = (v.useServiceType & _) === _ || (v.useServiceType & T) === T || (v.useServiceType & O) ===
          O,
          i = !1,
          r = !1,
          a = !1;
        void 0 !== v.fileToUpload && (i = v.fileToUpload.file.type === g.VIDEO, r = v.fileToUpload.file
          .type === g.IMAGE, a = !(v.fileToUpload.file.type !== g.IMAGE || !h(v.fileToUpload.file
            .subtype))), v.services = d.map(e, function(e) {
          var o = {};
          o.service = e, o.enabled = !1, o.isLoggedIn = c.isLoggedIntoService(e.name);
          var l = (e.type & k) === k && t,
            s = (e.type & C) === _ && n && r,
            u = (e.type & C) === C && n && a,
            f = (e.type & T) === T && n && i,
            m = (e.type & O) === O && n && i;
          return o.showItem = l || s || f || u || m, d.each(v.filteredServices, function(t) {
            t === e && 0 !== (e.type & t.type) && (o.enabled = !0);
          }), o;
        });
        var s = o.localStorage.getItem(E),
          u = JSON.parse(s);
        if (null !== u) {
          y.info("Last Logged service: ", u);
          var f = d.find(v.services, function(e) {
            if (e.service.name === u) return e;
          });
          v.useUploadService = f.service, o.localStorage.removeItem(E);
        } else v.broadcast === !0 ? v.useUploadService = c.getLastUsedServiceOrDefault(c.serviceTypes
          .STREAMING) : v.useUploadService = c.getLastUsedServiceOrDefault(r ? c.serviceTypes
          .IMAGE_UPLOAD : i ? c.serviceTypes.VIDEO_UPLOAD : c.serviceTypes.ALL);
        if (v.findService(), v.selected.enabled === !1 || v.selected.showItem === !1 || !v.selected
          .service) {
          y.info("Selected service is not visible or enabled: ", v.useUploadService.name);
          var f = d.find(v.services, function(e) {
            if (e.enabled === !0 && e.showItem === !0) return e;
          });
          v.useUploadService = f.service, v.findService(), y.info(
            "Now using the following service instead: ", v.useUploadService.name);
        }
        v.checkPattern = v.selected.service.name === v.youTube, v.selectedDestination = v
          .useUploadService.name, y.info("UploadService: ", v.useUploadService), y.info("selected: ", v
            .selected), v.checkUploadService(), v.refresh(), l.on(m.USER_LOGGED_IN, v.refresh), l.on(m
            .USER_LOGGED_OUT, v.refresh);
      }, v.refresh = function() {
        v.initializeService(v.selected.service), v.validConnection = !1, v.setUploadData(), v.showData()
          .then(function(e) {
            v.validConnection = e, v.cleanupDestinationOptions(), A(), v.setUploadData();
          });
      }, e.$watch("nvChangePicker", function() {
        if (v.nvChangePicker !== e.nvChangePicker) {
          if (v.warningNoticeText !== e.nvChangePicker.warningNoticeText) return v.warningNoticeText =
            e.nvChangePicker.warningNoticeText, void(v.shouldShowWarningNotice = !!v
              .warningNoticeText);
          void 0 !== e.nvChangePicker.serviceType && void 0 !== e.nvChangePicker.fileToUpload ? (v
              .useServiceType = e.nvChangePicker.serviceType, v.fileToUpload = e.nvChangePicker
              .fileToUpload, y.info("Picker upload"), y.info("ServiceType: ", v.useServiceType), y
              .info("FileToUpload: ", v.fileToUpload.file.name), "" !== v.fileToUpload && v
              .initialize()) : e.nvChangePicker.serviceType === c.serviceTypes.STREAMING && void 0 ===
            e.nvChangePicker.fileToUpload && (y.info("Picker broadcast"), v.useServiceType = e
              .nvChangePicker.serviceType, v.fileToUpload = e.nvChangePicker.fileToUpload, v
              .broadcast = !0, v.initialize()), void 0 !== e.nvChangePicker.showMoments ? (v
              .showMoments = e.nvChangePicker.showMoments, v.preprocessTitles(v.filteredServices), v
              .processTitle()) : v.showMoments = !1;
        }
      }, !0), e.$on("$destroy", function() {
        l.off(m.USER_LOGGED_IN, v.refresh), l.off(m.USER_LOGGED_OUT, v.refresh);
      });
    }
  ]);
  exports.DestinationPickerController = u;
}
