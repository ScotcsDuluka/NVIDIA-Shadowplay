// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 129
// service runTimeConfigService | service hardwareConfigWrapper | service recordingPathConfigWrapper | service twitchIngestServerConfigWrapper | service facebookService | service googleService | service jarvisService | service imgurService | service shotWithGeForceService | service sinaService | service twitchService | service connectService | service uploadManagerService | service uploadService | service galleryService | service cacheService | provider ugcLib | provider connectEndpoints | provider facebookEndpoints | provider googlePhotosEndpoints | provider imgurEndpoints | provider shotWithGeForceEndpoints | provider twitchEndpoints | provider weiboEndpoints | provider youtubeEndpoints | provider galleryEndpoints | constant GALLERY_TYPES | constant GALLERY_AUDIOTYPES | constant GALLERY_SUBTYPES | constant GALLERY_EVENTS | constant GALLERY_FILEPROCESS | constant CONNECT_EVENTS | constant UGC_NOTIFICATION_EVENTS | constant GALLERY_STATE | constant DEFAULT_VIDEO_TAGS | defines angular.module("connectSdk")
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  (function(e) {
    "use strict";

    function t(e) {
      if (e && e.__esModule) return e;
      var t = {};
      if (null != e)
        for (var n in e) Object.prototype.hasOwnProperty.call(e, n) && (t[n] = e[n]);
      return t.default = e, t;
    }

    function r(e) {
      return e && e.__esModule ? e : {
        default: e
      };
    }
    var i = require(161),
      o = r(i),
      a = require(155),
      s = r(a),
      c = require(78),
      u = r(c),
      l = require(25),
      d = r(l),
      f = require(219),
      h = t(f),
      p = angular.module("ugc-lib", ["underscore", "nvAngularJarvisSdk", "connectSdk", "facebookSdk",
        "googlePhotosSdk", "imgurSdk", "shotWithGeForceSdk", "twitchSdk", "weiboSdk", "youtubeSdk",
        "gallerySdk"
      ]);
    p.provider("ugcLib", ["connectEndpointsProvider", "facebookEndpointsProvider",
      "googlePhotosEndpointsProvider", "imgurEndpointsProvider", "shotWithGeForceEndpointsProvider",
      "twitchEndpointsProvider", "weiboEndpointsProvider", "youtubeEndpointsProvider",
      "galleryEndpointsProvider",
      function(e, t, n, r, i, o, a, s, c) {
        return {
          setNodeConfig: function(u) {
            e.setConfig(u), t.setConfig(u), n.setConfig(u), r.setConfig(u), i.setConfig(u), o
              .setConfig(u), a.setConfig(u), s.setConfig(u), c.setNodeConfig(u);
          },
          setLocalConfig: function(e) {
            console.log(e.version), c.setLocalConfig(e);
          },
          $get: ["galleryEndpoints", function(e) {
            var t = function(t) {
              e.updateNodeInfo(t);
            };
            return {
              updateNodeInfo: t
            };
          }]
        };
      }
    ]).constant("GALLERY_TYPES", {
      VIDEO: "video",
      IMAGE: "image"
    }).constant("GALLERY_AUDIOTYPES", {
      UNKNOWN: "Unknown",
      SINGLE: "Single",
      SEPARATE: "Separate"
    }).constant("GALLERY_SUBTYPES", {
      NORMAL: "Normal",
      SUPER_RESOLUTION: "SuperResolution",
      SUPER_RESOLUTION_OVERSIZED: "SuperResolution_Oversized",
      MONO_360: "360Mono",
      STEREO: "Stereo",
      STEREO_360: "360Stereo",
      EXR: "Exr",
      LIVE: "Live",
      OFFLINE: "Offline",
      HIGHLIGHTS: "Highlight",
      MTA: "MTA",
      NORMAL_ANSEL: "Normal_Ansel",
      GIF: "AnimatedGif"
    }).constant("GALLERY_EVENTS", {
      UPLOAD_COMPLETE: "UPLOAD_COMPLETE"
    }).constant("GALLERY_FILEPROCESS", {
      TRIM: "trim",
      MOVE: "move",
      COPY: "copy"
    }).constant("CONNECT_EVENTS", {
      USER_LOGGED_IN: "USER_LOGGED_IN",
      USER_LOGGED_OUT: "USER_LOGGED_OUT",
      ACCESS_TOKEN_EXPIRED: "ACCESS_TOKEN_EXPIRED",
      FACEBOOK_REACTIONS: "FACEBOOK_REACTIONS",
      FACEBOOK_COMMENTS: "FACEBOOK_COMMENTS",
      BROADCAST_ERROR: "BROADCAST_ERROR",
      LOGIN_BLOCKED: "LOGIN_BLOCKED"
    }).constant("UGC_NOTIFICATION_EVENTS", {
      UGC_NOTIFICATION: "UGC_NOTIFICATION",
      BROADCAST_LOGIN: "UGC_LIB_BROADCAST_LOGIN",
      NO_YOUTUBE_CHANNEL: "UGC_LIB_NO_YOUTUBE_CHANNEL",
      UPLOAD_STARTED: "UGC_LIB_UPLOAD_STARTED",
      UPLOAD_SUCCESS: "UGC_LIB_UPLOAD_SUCCESS",
      UPLOAD_FAILED: "UGC_LIB_UPLOAD_FAILED"
    }).constant("GALLERY_STATE", {
      EMPTY: "EMPTY",
      FILES: "FILES",
      DIRECTORY: "DIRECTORY"
    }).constant("DEFAULT_VIDEO_TAGS", ["#GeForceGTX", "#ShotWithGeForce"]).service("runTimeConfigService",
      [function() {
        var e = this;
        e.init = function(t) {
          e.config = t;
        }, e.getConfig = function() {
          return e.config;
        };
      }]).service("hardwareConfigWrapper", [function() {
      var e = this;
      e.hardwareSystemInfoGetter = null, e.hardwareSystemDescriptionGetter = null, e.init = function(
        t, n) {
        e.hardwareSystemInfoGetter = t, e.hardwareSystemDescriptionGetter = n;
      };
    }]).service("recordingPathConfigWrapper", [function() {
      var e = this;
      e.recordingPathGetter = null, e.init = function(t) {
        e.recordingPathGetter = t;
      };
    }]).service("twitchIngestServerConfigWrapper", [function() {
      var e = this;
      e.twitchIngestServerGetter = null, e.init = function(t) {
        e.twitchIngestServerGetter = t;
      };
    }]);
    var m = angular.module("connectSdk", ["nvAngularHttpEndpoint"]);
    m.provider("connectEndpoints", [function() {
      var e, t, n;
      return {
        setConfig: function(r) {
          e = r.version, t = r.server + r.port + "/Connect/" + e, n = r.commonHeaders;
        },
        $get: ["NvEndpointFactory", "$http", "$log", function(e, r, i) {
          var o,
            a,
            s = new e();
          return s.setUrlGenerator(function(e, n) {
            return t + e.url;
          }), s.setHeaderGenerator(function(e, t) {
            return angular.merge({}, e.headers, n);
          }), o = s.createEndpoint({
            url: "",
            method: "GET"
          }), a = s.createEndpoint({
            url: "/Connect/LoggedIn",
            method: "GET"
          }), {
            getFullConnectUrl: s.generateFullUrl,
            get: o,
            isLoggedIntoJarvis: a
          };
        }]
      };
    }]);
    var v = angular.module("facebookSdk", ["nvAngularHttpEndpoint"]);
    v.provider("facebookEndpoints", [function() {
      var e;
      return {
        setConfig: function(t) {
          e = t.commonHeaders;
        },
        $get: ["$interpolate", "NvEndpointFactory", "$http", "jarvisService", function(t, n, r, i) {
          var o,
            a,
            s,
            c,
            u,
            l,
            f,
            h,
            p,
            m,
            v,
            g,
            y,
            b,
            E,
            $,
            w,
            T,
            C = new n(),
            x = "v9.0",
            S = "https://graph.facebook.com/" + x,
            A = 15e3,
            M = {
              Accept: "application/json, text/plain, */*",
              "Content-Type": "application/json;charset=utf-8"
            };
          return C.setDefaultTimeout(A), C.setUrlGenerator(function(e, t) {
            return S + e.url;
          }), C.setHeaderGenerator(function(n, r) {
            var i = (0, d.default)(n.headers),
              o = i ? t(i)(r) : void 0,
              a = o ? JSON.parse(o) : void 0;
            return angular.merge({}, a, M, e);
          }), s = C.createEndpoint({
            url: "/me",
            method: "GET",
            headers: {
              Authorization: "{{authorization}}"
            }
          }), c = C.createEndpoint({
            url: "/me/picture",
            method: "GET",
            headers: {
              Authorization: "{{authorization}}"
            },
            params: {
              redirect: !1,
              type: "small"
            }
          }), u = C.createEndpoint({
            url: "/me/accounts",
            method: "GET",
            headers: {
              Authorization: "{{authorization}}"
            }
          }), l = C.createEndpoint({
            url: "/me/groups",
            method: "GET",
            headers: {
              Authorization: "{{authorization}}"
            }
          }), a = function(e, t) {
            return i.makeProxyCall(e.name, x + "/oauth/access_token", {
              redirect_uri: e.redirectUri,
              code: encodeURIComponent(t)
            }, !0);
          }, f = function(e) {
            return C.createEndpoint({
              url: "/" + e.destination + "/live_videos",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                privacy: {
                  value: e.privacy
                },
                title: e.title,
                description: e.title
              }
            });
          }, h = function(e) {
            return C.createEndpoint({
              url: "/" + e.liveVideoId,
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                title: e.broadcastTitle,
                content_tags: void 0 !== e.contentId ? e.contentId : ""
              }
            });
          }, p = function(e, t) {
            return C.createEndpoint({
              url: "/" + e,
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                description: t
              }
            });
          }, m = function(e) {
            return C.createEndpoint({
              url: "/" + e,
              method: "GET",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                fields: "live_views"
              }
            });
          }, v = function(e, t) {
            return C.createEndpoint({
              url: "/" + e,
              method: "GET",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                fields: _.reduce(t, function(e, t) {
                  return e + "reactions.type(" + t +
                    ").limit(0).summary(total_count).as(" + t + "),";
                }, "").slice(0, -1)
              }
            });
          }, g = function(e, t) {
            return C.createEndpoint({
              url: "/" + e + "/comments",
              method: "GET",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                order: "reverse_chronological",
                summary: "total_count",
                limit: void 0 !== t ? t : 25
              }
            });
          }, T = function(e) {
            return C.createEndpoint({
              url: "/search",
              method: "GET",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                type: "adinterest",
                limit: 25,
                q: e
              }
            });
          }, y = function(e, t, n) {
            var i = new FormData();
            return i.append("source", e.file, "blob"), r({
              url: S + "/" + e.destination.id + "/photos",
              method: "POST",
              headers: {
                Authorization: t,
                "Content-Type": void 0
              },
              params: {
                allow_spherical_photo: !0,
                caption: e.name,
                privacy: {
                  value: n
                }
              },
              data: i,
              transformRequest: angular.identity
            });
          }, b = function(e, t) {
            return C.createEndpoint({
              url: "/" + t + "/videos",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                upload_phase: "start",
                file_size: e
              }
            });
          }, $ = function(e, t, n, i, o, a, s) {
            var c = new FormData(),
              u = t.file.slice(n, i);
            c.append("video_file_chunk", u, "blob"), a(n, i), r({
              url: S + "/" + t.destination.id + "/videos",
              method: "POST",
              headers: {
                Authorization: o,
                "Content-Type": void 0
              },
              params: {
                upload_phase: "transfer",
                upload_session_id: e,
                start_offset: n
              },
              data: c,
              transformRequest: angular.identity
            }).then(function(n) {
              n.data.start_offset !== n.data.end_offset ? $(e, t, n.data.start_offset, n
                .data.end_offset, o, a, s) : s(!0);
            }).catch(function(e) {
              s(!1, e);
            });
          }, E = function(e, t, n, r, i) {
            $(t.upload_session_id, e, t.start_offset, t.end_offset, n, r, i);
          }, w = function(e, t, n) {
            return C.createEndpoint({
              url: "/" + t.destination.id + "/videos",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                upload_phase: "finish",
                upload_session_id: e,
                content_category: "VIDEO_GAMING",
                title: t.title,
                description: t.title,
                embeddable: !0,
                privacy: {
                  value: n
                }
              }
            });
          }, {
            getFullFacebookUrl: C.generateFullUrl,
            get: o,
            getAccessToken: a,
            getUser: s,
            getProfilePicture: c,
            getPages: u,
            getGroups: l,
            createRtmpUrl: f,
            updateBroadcastTitle: h,
            updateBroadcastDescription: p,
            getViewerCount: m,
            getReactionsSummary: v,
            getComments: g,
            uploadImage: y,
            initializeVideoUpload: b,
            uploadVideo: E,
            finalizeVideoUpload: w,
            getContentTags: T
          };
        }]
      };
    }]);
    var g = angular.module("googlePhotosSdk", ["nvAngularHttpEndpoint"]);
    g.provider("googlePhotosEndpoints", [function() {
      var e;
      return {
        setConfig: function(t) {
          e = t.commonHeaders;
        },
        $get: ["$q", "$interpolate", "$http", "NvEndpointFactory", function(t, n, r, i) {
          var o,
            a,
            s,
            c,
            u,
            l = new i(),
            f = "https://photoslibrary.googleapis.com/v1/",
            h = 5e3,
            p = {};
          return l.setDefaultTimeout(h), l.setUrlGenerator(function(e, t) {
            return f + e.url;
          }), l.setHeaderGenerator(function(t, r) {
            var i = (0, d.default)(t.headers),
              o = i ? n(i)(r) : void 0,
              a = o ? JSON.parse(o) : void 0;
            return angular.merge({}, a, p, e);
          }), a = function(e, t, n) {
            return l.createEndpoint({
              url: "uploads",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}",
                "Content-Length": "0",
                "X-Goog-Upload-Command": "start",
                "X-Goog-Upload-Content-Type": n,
                "X-Goog-Upload-File-Name": t,
                "X-Goog-Upload-Protocol": "resumable",
                "X-Goog-Upload-Raw-Size": "" + e
              }
            });
          }, s = function(e, t, n, i, o, a, c, u) {
            var l = n.file.slice(i, o);
            c(i, o), r({
              url: e,
              method: "POST",
              headers: {
                Authorization: a,
                "Content-Type": void 0,
                "Content-Length": o - i,
                "X-Goog-Upload-Command": o - i === t ? "upload" : "upload, finalize",
                "X-Goog-Upload-Offset": i
              },
              data: l,
              transformRequest: angular.identity
            }).then(function(r) {
              if (o < n.file.size) {
                var i = o,
                  l = i + t <= n.file.size ? i + t : n.file.size;
                s(e, t, n, i, l, a, c, u);
              } else u(!0, r);
            }).catch(function(e) {
              u(!1, e);
            });
          }, o = function(e, t, n, r, i, o) {
            s(t, n, e, 0, n <= e.file.size ? n : e.file.size, r, i, o);
          }, c = function(e, t, n) {
            return r({
              url: f + "mediaItems:batchCreate",
              method: "POST",
              headers: {
                Authorization: n
              },
              data: {
                newMediaItems: [{
                  description: t.title,
                  simpleMediaItem: {
                    uploadToken: e
                  }
                }]
              }
            });
          }, u = function(e) {
            return r({
              method: "GET",
              url: "https://www.googleapis.com/userinfo/v2/me",
              headers: {
                Authorization: e.authorization,
                "Content-Type": "application/json"
              }
            }).then(function(e) {
              return {
                userName: e.data.name,
                avatarUri: e.data.picture
              };
            }, function(e) {
              return t.reject(e);
            });
          }, {
            getFullGooglePhotosUrl: l.generateFullUrl,
            uploadFile: o,
            initializeMediaUpload: a,
            finalizeMediaUpload: c,
            getUserInfo: u
          };
        }]
      };
    }]);
    var y = angular.module("imgurSdk", ["nvAngularHttpEndpoint"]);
    y.provider("imgurEndpoints", [function() {
      var e;
      return {
        setConfig: function(t) {
          e = t.commonHeaders;
        },
        $get: ["$interpolate", "NvEndpointFactory", function(t, n) {
          var r,
            i,
            o = new n(),
            a = "https://api.imgur.com/3/",
            s = 5e3,
            c = {
              Accept: "application/json"
            };
          return o.setDefaultTimeout(s), o.setUrlGenerator(function(e, t) {
            return a + e.url;
          }), o.setHeaderGenerator(function(n, r) {
            var i = (0, d.default)(n.headers),
              o = i ? t(i)(r) : void 0,
              a = o ? JSON.parse(o) : void 0;
            return angular.merge({}, a, c, e);
          }), r = o.createEndpoint({
            url: "",
            method: "GET"
          }), i = function(e, t) {
            return o.createEndpoint({
              url: "upload",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              data: {
                image: t,
                type: "base64",
                name: e,
                title: e,
                description: e
              },
              timeout: 2147483647
            });
          }, {
            getFullImgurUrl: o.generateFullUrl,
            get: r,
            uploadImage: i
          };
        }]
      };
    }]);
    var b = angular.module("shotWithGeForceSdk", ["nvAngularHttpEndpoint"]);
    b.provider("shotWithGeForceEndpoints", [function() {
      var e;
      return {
        setConfig: function(t) {
          e = t.commonHeaders;
        },
        $get: ["$q", "$http", "$log", "$interpolate", "NvEndpointFactory", "runTimeConfigService",
          "GALLERY_SUBTYPES",
          function(t, n, r, i, o, a, s) {
            function c() {
              E = a.getConfig().connect.swgf.baseUrl + "geforce/";
            }

            function u(e, r, i, o, a, s) {
              var c = i.file.slice(a * y, Math.min((a + 1) * y, i.file.size), i.file.type),
                l = new FormData();
              return l.append("qqfile", c, "blob"), n({
                url: E + "image/upload",
                method: "POST",
                headers: {
                  "Content-Type": void 0
                },
                params: {
                  userid: r,
                  session: e,
                  title: i.name,
                  qqpartindex: a,
                  qqtotalparts: s,
                  qquuid: o,
                  qqfilename: i.fileName,
                  done: a === s
                },
                data: l,
                transformRequest: angular.identity,
                timeout: 0
              }).then(function(t) {
                return a % 10 === 0 && v.info("Successfully uploaded chunk " + a + " of " + s),
                  a < s ? u(e, r, i, o, a + 1, s) : t;
              }, function(e) {
                return v.error("Upload failed on chunk " + a + " of " + s), t.reject(e);
              });
            }

            function l() {
              var e = Date.now(),
                t = "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(t) {
                  var n = (e + 16 * Math.random()) % 16 | 0;
                  return e = Math.floor(e / 16), ("x" == t ? n : 3 & n | 8).toString(16);
                });
              return t;
            }
            var f,
              h,
              p,
              m = new o(),
              v = r.getInstance("ugc-lib/shotWithGeForceSdk"),
              g = 5e3,
              y = 51352,
              b = {
                Accept: "application/json"
              },
              E = "";
            return m.setDefaultTimeout(g), m.setUrlGenerator(function(e, t) {
              return E + e.url;
            }), m.setHeaderGenerator(function(t, n) {
              var r = (0, d.default)(t.headers),
                o = r ? i(r)(n) : void 0,
                a = o ? JSON.parse(o) : void 0;
              return angular.merge({}, a, b, e);
            }), f = m.createEndpoint({
              url: "",
              method: "GET"
            }), h = function(e, t, n) {
              v.info("Uploading to SWGF");
              var r = Math.ceil(n.file.size / y) - 1;
              return u(e, t, n, l(), 0, r);
            }, p = function(e) {
              return m.createEndpoint({
                url: "user/auth",
                method: "POST",
                params: {
                  delegateToken: e
                },
                timeout: g
              })().then(function(e) {
                return e.data.success ? e.data : (v.error(
                  "ShotWithGeForce connection failed with: ", e), t.reject(e));
              });
            }, {
              getFullShotWithGeForceUrl: m.generateFullUrl,
              get: f,
              getSessionTokenAndUId: p,
              uploadImageData: h,
              init: c
            };
          }
        ]
      };
    }]);
    var E = angular.module("twitchSdk", ["nvAngularHttpEndpoint"]);
    E.provider("twitchEndpoints", [function() {
      var e;
      return {
        setConfig: function(t) {
          e = t.commonHeaders;
        },
        $get: ["$interpolate", "NvEndpointFactory", function(t, n) {
          var r,
            i,
            o,
            a,
            s,
            c,
            u,
            l = new n(),
            f = "https://api.twitch.tv/helix",
            h = 5e3,
            p = "https://ingest.twitch.tv",
            m = {};
          return l.setDefaultTimeout(h), l.setUrlGenerator(function(e, t) {
            return "/ingests" === e.url ? p + e.url : f + e.url;
          }), l.setHeaderGenerator(function(n, r) {
            var i = (0, d.default)(n.headers),
              o = i ? t(i)(r) : void 0,
              a = o ? JSON.parse(o) : void 0;
            return angular.merge({}, a, m, e);
          }), r = l.createEndpoint({
            url: "",
            method: "GET"
          }), i = function(e, t, n) {
            return l.createEndpoint({
              url: "/streams/key?broadcaster_id=" + e,
              method: "GET",
              headers: {
                Authorization: t,
                "Client-Id": n
              }
            });
          }, o = l.createEndpoint({
            url: "/ingests",
            method: "GET",
            headers: {}
          }), a = l.createEndpoint({
            url: "/users",
            method: "GET",
            headers: {
              Authorization: "{{authorization}}",
              "Client-Id": "{{clientId}}"
            }
          }), c = function(e, t, n, r) {
            var i = (0, d.default)({
              title: t.gameTitle
            });
            return l.createEndpoint({
              url: "/channels?broadcaster_id=" + e,
              method: "PATCH",
              headers: {
                Authorization: n,
                "Client-Id": r,
                "Content-Type": "application/json"
              },
              data: i
            });
          }, u = function(e, t, n) {
            return l.createEndpoint({
              url: "/channels?broadcaster_id=" + e,
              method: "GET",
              headers: {
                Authorization: t,
                "Client-Id": n,
                "Content-Type": "application/json"
              }
            });
          }, s = function(e, t, n) {
            return l.createEndpoint({
              url: "/streams?user_id=" + e,
              method: "GET",
              headers: {
                Authorization: t,
                "Client-Id": n
              }
            });
          }, {
            getFullTwitchUrl: l.generateFullUrl,
            get: r,
            getStreamKey: i,
            getIngestServers: o,
            getUser: a,
            getStream: s,
            updateChannelInfo: c,
            getChannelInfo: u
          };
        }]
      };
    }]);
    var $ = angular.module("weiboSdk", ["nvAngularHttpEndpoint"]);
    $.provider("weiboEndpoints", function() {
      var e;
      return {
        setConfig: function(t) {
          e = t.commonHeaders;
        },
        $get: ["$interpolate", "NvEndpointFactory", "$http", "$log", "$q", "jarvisService", function(t,
          n, r, i, o, a) {
          function s(e, t, n) {
            k || (k = new FileReader());
            var r = o.defer();
            k.onload = function() {
              r.resolve(k.result);
            }, k.onerror = function() {
              r.reject(k.error), k = null;
            };
            var i = e.slice(t, n);
            return k.readAsArrayBuffer(i), r.promise;
          }

          function c(e) {
            function t() {
              return s(e, i, a).then(function(e) {
                if (!e.byteLength) return c.reject("File read error, invalid chunk length"),
                  void(k = null);
                var o = h.lib.WordArray.create(e);
                if (r.update(o), e.byteLength < n) {
                  var s = r.finalize().toString();
                  return void c.resolve(s);
                }
                i += e.byteLength, a = i + n, t();
              }, function(e) {
                c.reject(e), k = null;
              });
            }
            var n = 5242880,
              r = h.algo.MD5.create(),
              i = 0,
              a = n,
              c = o.defer();
            return t(), c.promise;
          }

          function u(e) {
            var t = h.lib.WordArray.create(e);
            return h.MD5(t).toString();
          }

          function l(e, t, n, i, a, c, d, f) {
            var h = n + 1024 * i;
            d(n, h), o.all([r({
              url: T + "open_upload.json",
              method: "POST",
              headers: {
                "Content-Type": void 0
              },
              params: {
                access_token: c,
                filetoken: e,
                sectioncheck: a,
                startloc: n,
                client: "web"
              },
              data: t.file.slice(n, h)
            }), t.file.size < h ? o.when(!0) : s(t.file, h, h + 1024 * i)]).then(function(n) {
              if (n[0].data.fid) f(!0);
              else {
                var r = u(n[1]);
                l(e, t, h, i, r, c, d, f);
              }
            }).catch(function(e) {
              f(!1, e);
            });
          }
          var f,
            p,
            m,
            v,
            g,
            y,
            b,
            E,
            _ = new n(),
            $ = "https://api.weibo.com/",
            w = "https://api.weibo.com/",
            T = "https://multimedia.api.weibo.com/2/multimedia/",
            C = 5e3,
            x = " https://www.nvidia.com/zh-cn/geforce/geforce-experience/",
            S = (i.getInstance("ugc-lib/weiboEndpoints"), {
              Accept: "application/json"
            });
          _.setDefaultTimeout(C), _.setUrlGenerator(function(e, t) {
            return w + e.url;
          }), _.setHeaderGenerator(function(n, r) {
            var i = (0, d.default)(n.headers),
              o = i ? t(i)(r) : void 0,
              a = o ? JSON.parse(o) : void 0;
            return angular.merge({}, a, S, e);
          }), f = _.createEndpoint({
            url: "",
            method: "GET"
          }), p = function(e, t, n) {
            var i = new FormData();
            return i.append("access_token", t), i.append("status", e.name + x), i.append("pic", e
              .file, "blob"), r({
              url: $ + "2/statuses/share.json",
              method: "POST",
              headers: {
                "Content-Type": void 0
              },
              data: i,
              transformRequest: angular.identity
            });
          }, m = function(e, t) {
            return a.makeProxyCall(e.jarvisName, "oauth2/access_token", {
              grant_type: "authorization_code",
              redirect_uri: e.redirectUri,
              code: t
            }, !1, !0);
          };
          var A = _.createEndpoint({
              method: "POST",
              url: "oauth2/get_token_info"
            }),
            M = _.createEndpoint({
              method: "GET",
              url: "2/users/show.json"
            });
          v = function(e) {
            return A({
              access_token: e
            }).then(function(e) {
              return e.data.expire_in;
            });
          }, g = function(e) {
            return A({
              access_token: e
            }).then(function(t) {
              return M({
                uid: t.data.uid,
                access_token: e
              });
            });
          }, y = function(e, t) {
            return _.createEndpoint({
              method: "GET",
              url: e.logoutUrl + t
            });
          };
          var k = null;
          return b = function(e, t) {
            return c(e.file).then(function(n) {
              return r({
                url: T + "open_init.json",
                method: "POST",
                headers: {
                  "Content-Type": void 0
                },
                params: {
                  access_token: t,
                  length: e.file.size,
                  check: n,
                  name: e.title,
                  client: "web",
                  type: "video",
                  status_text: e.title + x,
                  status_visible: e.privacy.value
                }
              });
            });
          }, E = function(e, t, n, r, i) {
            var o, a;
            o = 0;
            var c = t.length;
            a = o + 1024 * c, s(e.file, o, a).then(function(a) {
              var s = u(a);
              l(t.fileToken, e, o, c, s, n, r, i);
            }, function(e) {
              i(!1, e);
            });
          }, {
            getFullWeiboUrl: _.generateFullUrl,
            get: f,
            uploadImage: p,
            getAccessToken: m,
            getTokenTimeLeft: v,
            getUserData: g,
            logout: y,
            initializeVideoUpload: b,
            uploadVideo: E
          };
        }]
      };
    });
    var w = angular.module("youtubeSdk", ["nvAngularHttpEndpoint"]);
    w.provider("youtubeEndpoints", [function() {
      var e;
      return {
        setConfig: function(t) {
          e = t.commonHeaders;
        },
        $get: ["$interpolate", "$q", "NvEndpointFactory", "jarvisService", function(t, n, r, i) {
          var o,
            a,
            s,
            c,
            u,
            l,
            f,
            h,
            p,
            m,
            v,
            g,
            y,
            b,
            E,
            _,
            $ = new r(),
            w = "https://www.googleapis.com/youtube/v3",
            T = 15e3,
            C = {
              "Content-Type": "application/json"
            };
          $.setDefaultTimeout(T), $.setUrlGenerator(function(e, t) {
            return w + e.url;
          }), $.setHeaderGenerator(function(n, r) {
            var i = (0, d.default)(n.headers),
              o = i ? t(i)(r) : void 0,
              a = o ? JSON.parse(o) : void 0;
            return angular.merge({}, a, C, e);
          }), o = $.createEndpoint({
            url: "",
            method: "POST"
          }), s = function(e) {
            return $.createEndpoint({
              url: "/liveBroadcasts",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                part: "snippet,status"
              },
              data: {
                snippet: {
                  title: e.title,
                  scheduledStartTime: e.startTime
                },
                status: {
                  privacyStatus: e.privacy
                }
              }
            });
          }, c = function(e) {
            return $.createEndpoint({
              url: "/liveStreams",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                part: "snippet,cdn"
              },
              data: {
                snippet: {
                  title: e.title
                },
                cdn: {
                  resolution: e.resolution,
                  frameRate: e.frameRate,
                  ingestionType: "rtmp"
                }
              }
            });
          }, u = function(e) {
            return $.createEndpoint({
              url: "/liveStreams",
              method: "GET",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                part: "id,status",
                id: e
              },
              data: {
                id: e
              }
            });
          }, l = function(e) {
            return $.createEndpoint({
              url: "/liveBroadcasts",
              method: "GET",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                part: "id,status",
                id: e
              },
              data: {
                id: e
              }
            });
          }, f = function(e, t) {
            return $.createEndpoint({
              url: "/liveBroadcasts/bind",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                part: "id",
                id: t,
                streamId: e
              },
              data: {
                id: t
              }
            });
          }, h = function(e) {
            return $.createEndpoint({
              url: "/liveBroadcasts/transition",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                broadcastStatus: "testing",
                id: e,
                part: "id,status"
              },
              data: {
                id: e
              }
            });
          }, p = function(e) {
            return $.createEndpoint({
              url: "/liveBroadcasts/transition",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                broadcastStatus: "live",
                id: e,
                part: "id,status"
              },
              data: {
                id: e
              }
            });
          }, m = function(e) {
            return $.createEndpoint({
              url: "/liveBroadcasts/transition",
              method: "POST",
              headers: {
                Authorization: "{{authorization}}"
              },
              params: {
                broadcastStatus: "complete",
                id: e,
                part: "id,status"
              },
              data: {
                id: e
              }
            });
          }, v = function(e, t) {
            return $.createEndpoint({
              url: "/liveBroadcasts",
              method: "PUT",
              params: {
                part: "id,snippet,status"
              },
              headers: {
                Authorization: "{{authorization}}"
              },
              data: {
                id: t,
                snippet: {
                  title: e.title,
                  scheduledStartTime: e.startTime
                },
                status: {
                  privacyStatus: e.privacy
                }
              }
            });
          };
          var x = $.createEndpoint({
            url: "/channels",
            method: "GET",
            params: {
              part: "snippet",
              mine: !0
            },
            headers: {
              Authorization: "{{authorization}}"
            },
            data: {
              mine: !0
            }
          });
          return b = function(e) {
            return x(e).then(function(e) {
              return {
                userName: e.data.items[0].snippet.title,
                avatarUri: e.data.items[0].snippet.thumbnails.default.url
              };
            }, function(e) {
              return n.reject(e);
            });
          }, E = function(e, t, n) {
            return i.makeProxyCall(e.name, "oauth2/v3/token", {
              grant_type: "authorization_code",
              redirect_uri: n || e.redirectUri,
              code: t
            }, !0);
          }, _ = function(e, t) {
            return i.makeProxyCall(e.name, "oauth2/v3/token", {
              grant_type: "refresh_token",
              redirect_uri: e.redirectUri,
              refresh_token: t.refreshToken
            }, !0).catch(function(e) {
              return console.log("refreshAccessToken error:", e), n.reject(e);
            });
          }, a = function(e) {
            return $.createEndpoint({
              url: "/videos",
              method: "GET",
              params: {
                part: "id,liveStreamingDetails",
                id: e
              },
              headers: {
                Authorization: "{{authorization}}"
              },
              data: {
                id: e
              }
            });
          }, g = function(e, t, n) {
            return $.createEndpoint({
              url: "/videos",
              method: "PUT",
              params: {
                part: "id,snippet"
              },
              headers: {
                Authorization: "{{authorization}}"
              },
              data: {
                id: e,
                snippet: {
                  description: t,
                  categoryId: 20,
                  title: n
                }
              }
            });
          }, y = function() {
            return $.createEndpoint({
              url: "/channels",
              method: "GET",
              params: {
                part: "status",
                mine: !0
              },
              headers: {
                Authorization: "{{authorization}}"
              }
            });
          }, {
            getFullYouTubeUrl: $.generateFullUrl,
            post: o,
            createLiveBroadcast: s,
            createLiveStream: c,
            listStreamRequest: u,
            listBroadcastRequest: l,
            broadcastToLivestream: f,
            transitionBroadcastToTesting: h,
            transitionBroadcastToLive: p,
            transitionBroadcastToComplete: m,
            updateBroadcastTitle: v,
            getUserInfo: b,
            getAccessToken: E,
            refreshAccessToken: _,
            listVideos: a,
            updateBroadcastDescription: g,
            listChannels: y
          };
        }]
      };
    }]);
    var T = angular.module("gallerySdk", ["nvAngularHttpEndpoint"]);
    T.provider("galleryEndpoints", [function() {
      var e,
        t,
        n,
        r,
        i,
        o,
        a = "v.1.2";
      return {
        setNodeConfig: function(s) {
          n = s.server, t = n + s.port + "/Gallery/", e && (r = t + e), i = t + a, o = s
            .commonHeaders;
        },
        setLocalConfig: function(n) {
          e = n.version, t && (r = t + e);
        },
        $get: ["NvEndpointFactory", function(s) {
          var c,
            u,
            l,
            d,
            f,
            h,
            p,
            m,
            v,
            g,
            y,
            b,
            E,
            _,
            $,
            w,
            T,
            C,
            x,
            S,
            A,
            M = new s(),
            k = new s();
          return M.setUrlGenerator(function(e, t) {
            return r + e.url;
          }), M.setHeaderGenerator(function(e, t) {
            return angular.merge({}, e.headers, o);
          }), k.setUrlGenerator(function(e, t) {
            return i + e.url;
          }), k.setHeaderGenerator(function(e, t) {
            return angular.merge({}, e.headers, o);
          }), c = k.createEndpoint({
            url: "/GetFolderListing",
            method: "POST",
            data: {
              directory: "",
              shouldWatch: "",
              shouldGetOnlyNv: "",
              excludeDirectoryType: "",
              shouldShowEXR: ""
            }
          }), u = M.createEndpoint({
            url: "/GetStats",
            method: "POST",
            data: {
              directory: "",
              width: "",
              height: "",
              shouldGetOnlyNv: "",
              quickCheck: "",
              shouldShowEXR: ""
            }
          }), $ = M.createEndpoint({
            url: "/GetFolderCRC",
            method: "POST",
            data: {
              directory: ""
            }
          }), l = M.createEndpoint({
            url: "/Recent/8",
            method: "GET"
          }), d = M.createEndpoint({
            url: "/Recent/Clear",
            method: "POST"
          }), f = M.createEndpoint({
            url: "/Upload/History",
            method: "GET"
          }), h = M.createEndpoint({
            url: "/Upload/History",
            method: "POST",
            data: {
              name: "",
              date: "",
              time: "",
              type: "",
              subtype: "",
              url: "",
              path: ""
            }
          }), p = M.createEndpoint({
            url: "Upload/History/Remove",
            method: "POST",
            data: {
              url: ""
            }
          }), m = M.createEndpoint({
            url: "/Remove",
            method: "POST",
            data: {
              file: "",
              forceDelete: !1
            }
          }), v = M.createEndpoint({
            url: "/GetFileMetaData",
            method: "POST",
            data: {
              file: "",
              width: "",
              height: ""
            }
          }), g = M.createEndpoint({
            url: "/GetFileMetaData",
            method: "POST",
            data: {
              file: ""
            }
          }), y = M.createEndpoint({
            url: "/GetImageFileDimensions",
            method: "POST",
            data: {
              file: ""
            }
          }), b = M.createEndpoint({
            url: "/GetThumbnail",
            method: "POST",
            data: {
              file: "",
              size: ""
            }
          }), E = M.createEndpoint({
            url: "/Upload/History/Clear",
            method: "POST"
          }), _ = M.createEndpoint({
            url: "/EnumerateDrives",
            method: "GET"
          }), w = M.createEndpoint({
            url: "/TranscodeMediaFile",
            method: "POST",
            data: {
              file: "",
              maxFileSizeMB: 0,
              targetPath: ""
            }
          }), T = M.createEndpoint({
            url: "/TranscodeVideoToGIF",
            method: "POST",
            data: {
              file: "",
              maxFileSizeMB: 0,
              quality: "",
              newHeight: 0,
              newFps: "",
              targetPath: "",
              newDuration: 0,
              userData: "",
              memeImage: ""
            }
          }), C = M.createEndpoint({
            url: "/CopyFile",
            method: "POST",
            data: {
              source: "",
              destination: ""
            }
          }), x = M.createEndpoint({
            url: "/isDirectoryWritable",
            method: "POST",
            data: {
              directory: ""
            }
          }), S = M.createEndpoint({
            url: "/WriteEncryptedBmp",
            method: "POST",
            data: {
              bitmapImage: ""
            }
          }), A = function(s) {
            o.X_LOCAL_SECURITY_COOKIE = s.secret, t = n + s.port + "/Gallery/", e && (r = t +
              e), i = t + a;
          }, {
            getFullGalleryUrl: M.generateFullUrl,
            getGalleryFolderListing: c,
            getGalleryStats: u,
            getGalleryRecent: l,
            clearRecentHistory: d,
            getGalleryUploadHistory: f,
            addGalleryUploadHistoryItem: h,
            removeGalleryUploadHistoryItem: p,
            removeGalleryItem: m,
            getGalleryFileMetaData: v,
            getGalleryFileMetaDataNoThumbnail: g,
            getGalleryImageFileDimensions: y,
            getGalleryThumbnail: b,
            clearUploadHistory: E,
            enumerateDrives: _,
            getGalleryFolderCRC: $,
            transcodeMediaFile: w,
            transcodeVideoToGIF: T,
            copyFile: C,
            isDirectoryWritable: x,
            writeEncryptedBmp: S,
            updateNodeInfo: A
          };
        }]
      };
    }]);
    var C = "https://www.googleapis.com/upload/drive/v2/files/",
      x = function() {
        this.interval = 1e3, this.maxInterval = 6e4;
      };
    x.prototype.retry = function(e) {
      setTimeout(e, this.interval), this.interval = this.nextInterval_();
    }, x.prototype.reset = function() {
      this.interval = 1e3;
    }, x.prototype.nextInterval_ = function() {
      var e = 2 * this.interval + this.getRandomInt_(0, 1e3);
      return Math.min(e, this.maxInterval);
    }, x.prototype.getRandomInt_ = function(e, t) {
      return Math.floor(Math.random() * (t - e + 1) + e);
    };
    var S = function(e) {
      var t = function() {};
      if (this.file = e.file, this.contentType = e.contentType || this.file.type ||
        "application/octet-stream", this.metadata = e.metadata || {
          title: this.file.name,
          mimeType: this.contentType
        }, this.token = e.token, this.onComplete = e.onComplete || t, this.onProgress = e.onProgress || t,
        this.onError = e.onError || t, this.offset = e.offset || 0, this.chunkSize = e.chunkSize || 0,
        this.retryHandler = new x(), this.url = e.url, !this.url) {
        var n = e.params || {};
        n.uploadType = "resumable", this.url = this.buildUrl_(e.fileId, n, e.baseUrl);
      }
      this.httpMethod = e.fileId ? "PUT" : "POST";
    };
    S.prototype.upload = function() {
      var e = new XMLHttpRequest();
      e.open(this.httpMethod, this.url, !0), e.setRequestHeader("Authorization", this.token), e
        .setRequestHeader("Content-Type", "application/json"), e.setRequestHeader(
          "X-Upload-Content-Length", this.file.size), e.setRequestHeader("X-Upload-Content-Type", this
          .contentType), e.onload = function(e) {
          if (e.target.status < 400) {
            var t = e.target.getResponseHeader("Location");
            this.url = t, this.sendFile_();
          } else this.onUploadError_(e);
        }.bind(this), e.onerror = this.onUploadError_.bind(this), e.send((0, d.default)(this.metadata));
    }, S.prototype.sendFile_ = function() {
      var e = this.file,
        t = this.file.size;
      (this.offset || this.chunkSize) && (this.chunkSize && (t = Math.min(this.offset + this.chunkSize,
        this.file.size)), e = e.slice(this.offset, t));
      var n = new XMLHttpRequest();
      n.open("PUT", this.url, !0), n.setRequestHeader("Content-Type", this.contentType), n
        .setRequestHeader("Content-Range", "bytes " + this.offset + "-" + (t - 1) + "/" + this.file.size),
        n.setRequestHeader("X-Upload-Content-Type", this.file.type), n.upload && n.upload
        .addEventListener("progress", this.onProgress), n.onload = this.onContentUploadSuccess_.bind(
        this), n.onerror = this.onContentUploadError_.bind(this), n.send(e);
    }, S.prototype.resume_ = function() {
      var e = new XMLHttpRequest();
      e.open("PUT", this.url, !0), e.setRequestHeader("Content-Range", "bytes */" + this.file.size), e
        .setRequestHeader("X-Upload-Content-Type", this.file.type), e.upload && e.upload.addEventListener(
          "progress", this.onProgress), e.onload = this.onContentUploadSuccess_.bind(this), e.onerror =
        this.onContentUploadError_.bind(this), e.send();
    }, S.prototype.extractRange_ = function(e) {
      var t = e.getResponseHeader("Range");
      t && (this.offset = parseInt(t.match(/\d+/g).pop(), 10) + 1);
    }, S.prototype.onContentUploadSuccess_ = function(e) {
      200 === e.target.status || 201 === e.target.status ? this.onComplete(e.target.response) : e.target
        .status >= 500 && e.target.status < 600 ? this.onError(e.target.response) : 308 === e.target
        .status && (this.extractRange_(e.target), this.retryHandler.reset(), this.sendFile_());
    }, S.prototype.onContentUploadError_ = function(e) {
      e.target.status && e.target.status < 500 ? this.onError(e.target.response) : this.retryHandler
        .retry(this.resume_.bind(this));
    }, S.prototype.onUploadError_ = function(e) {
      this.onError(e.target.response);
    }, S.prototype.buildQuery_ = function(e) {
      return e = e || {}, (0, u.default)(e).map(function(t) {
        return encodeURIComponent(t) + "=" + encodeURIComponent(e[t]);
      }).join("&");
    }, S.prototype.buildUrl_ = function(e, t, n) {
      var r = n || C;
      e && (r += e);
      var i = this.buildQuery_(t);
      return i && (r += "?" + i), r;
    };
    p.service("facebookService", ["$q", "$log", "$interval", "facebookEndpoints", "galleryService",
      "jarvisService", "eventAggregator", "runTimeConfigService", "GALLERY_TYPES", "GALLERY_SUBTYPES",
      "CONNECT_EVENTS", "UGC_NOTIFICATION_EVENTS",
      function(e, t, n, r, i, o, a, s, c, u, l, f) {
        function h(e) {
          var t = /^(.*?)\s\|/g,
            n = t.exec(e);
          return n ? n[1] : null;
        }
        var p = this,
          m = "OAuth ",
          v = null,
          g = null,
          y = void 0,
          b = void 0,
          E = void 0,
          $ = "v9.0",
          w = void 0,
          T = "ALL_FRIENDS",
          C = "",
          x = t.getInstance("ugc-lib/facebookService");
        p.name = "facebook", p.serviceNames = {
            FACEBOOK: "Facebook"
          }, p.providerInfo = {
            name: p.name,
            services: {},
            oauthUrl: "https://www.facebook.com/" + $ +
              "/dialog/oauth?client_id={{clientId}}&redirect_uri={{redirectUri}}&response_type=code&display=popup&scope={{scope}}",
            clientId: "",
            logoutUrl: "https://www.facebook.com/logout",
            cookies: {
              ".facebook.com": [""]
            }
          }, p.providerInfo.services[p.serviceNames.FACEBOOK] = {
            id: 3,
            name: p.serviceNames.FACEBOOK,
            type: 1,
            scope: "manage_pages publish_pages pages_show_list groups_show_list publish_to_groups publish_video"
          }, p.reactionTypes = ["NONE", "LIKE", "LOVE", "WOW", "HAHA", "SAD", "ANGRY", "THANKFUL"], p
          .logoutFunc = null;
        var S = function(t) {
          return void 0 === y || t ? null !== g && (g.expiresIn <= 0 || g.expiresOn > Date.now()) ? e
            .when(m + g.accessToken) : null !== g ? (g = null, p.logoutFunc && p.logoutFunc(p
              .serviceNames.FACEBOOK), e.reject("Invalid Login! Forced Logout")) : e.reject(
              "Not logged in!") : e.when(m + y);
        };
        p.setAuthToken = function(e) {
          g = e, void 0 !== e.code && void 0 === e.accessToken ? r.getAccessToken(p.providerInfo, e
            .code).then(function(e) {
            p.setAuthTokenCallback(p.serviceNames.FACEBOOK, e.data);
          }, function(e) {
            x.error("setAuthToken was unable to request an access token: ", e);
          }) : void 0 === e.accessToken && x.error(
            "setAuthToken called without valid accessToken: ", e);
        }, p.ensureValidToken = function() {
          return S(!0);
        }, p.getUser = function() {
          return S(!0).then(function(t) {
            var n = {
              user: r.getUser({
                authorization: t
              }),
              profilePicture: r.getProfilePicture({
                authorization: t
              })
            };
            return e.all(n).then(function(e) {
              return {
                userId: e.user.data.id,
                userName: e.user.data.name,
                avatarUri: e.profilePicture.data.data.url
              };
            }, function(t) {
              return x.error("getUser failed: ", t.statusText), e.reject(t);
            });
          });
        }, p.getRtmpUrl = function(e) {
          var t = {};
          return t.title = angular.isDefined(e.title) ? e.title : C, t.privacy = angular.isDefined(e
              .privacy) ? e.privacy.value : T, t.destination = angular.isDefined(e.destination) &&
            angular.isDefined(e.destination.id) ? e.destination.id : "me", y = angular.isDefined(e
              .destination) ? e.destination.access_token : void 0, S().then(function(e) {
              return r.createRtmpUrl(t)({
                authorization: e
              }).then(function(e) {
                v = e.data.id, x.info("Facebook video id: " + v);
                var t = "";
                return e.data.secure_stream_url ? (t = e.data.secure_stream_url, t.replace(
                  ":443", "")) : (t = e.data.stream_url, t.replace(":80", ""));
              }, function(e) {
                var t = "getRtmpUrl failed " + (0, d.default)(e);
                x.error(t), y = void 0;
              });
            });
        }, p.postRtmpProcess = function() {
          var t = p.providerInfo.services[p.serviceNames.FACEBOOK].commentsAndReactionsInterval;
          E = void 0, w = void 0;
          var r = function() {
            p.getReactionsCount(), p.getComments();
          };
          return r(), b = n(r, t), e.reject(!0);
        }, p.stopBroadcast = function() {
          x.info("Stop Broadcast"), y = void 0, void 0 !== b && n.cancel(b);
        }, p.setBroadcastDescription = function(t) {
          return x.info("Set Broadcast Description ", t), e.when(!0);
        }, p.setBroadcastTitle = function(t) {
          var n = {
            broadcastTitle: t,
            liveVideoId: v
          };
          return t ? S().then(function(e) {
            return p.getContentTags(t).then(function(i) {
              var o = _.filter(i, function(e) {
                return e.name === t;
              });
              if (0 === o.length && (o = _.filter(i, function(e) {
                  var n = e.name.toUpperCase().replace(/[:;,.\-']/g, ""),
                    r = t.toUpperCase().replace(/[:;,.\-']/g, "");
                  return n === r;
                })), o.length > 0) {
                var a = o[0];
                o.length > 1 && (a = _.findWhere(o, {
                  disambiguation_category: "Video Game"
                })), void 0 !== a && (n.contentId = a.id);
              }
              return x.info("Updating Broadcast Title for liveVideoId " + v + " to " + t +
                " and content ID: " + n.contentId), r.updateBroadcastTitle(n)({
                authorization: e
              }).then(function(e) {
                x.info("Broadcast Title updated");
              }, function(e) {
                x.error("Broadcast Title update failure " + (0, d.default)(e));
              });
            });
          }) : e.reject("Error, no broadcastTitle");
        }, p.getCurrentViewerCount = function() {
          var e = r.getViewerCount(v);
          return S().then(function(t) {
            return e({
              authorization: t
            }).then(function(e) {
              var t = 0;
              return null !== e.data.live_views && void 0 !== e.data.live_views && (t = e.data
                .live_views), x.info("Get Viewer Count success " + t), t;
            }, function(e) {
              x.error("Get Viewer Count error: " + (0, d.default)(e));
            });
          });
        }, p.getBroadcastQuality = function(e) {
          var t = p.providerInfo.services[p.serviceNames.FACEBOOK].qualityPrefix;
          return "Custom" === e && (e = "VeryGood"), t + e;
        }, p.getContentTags = function(t) {
          if (!t) return x.error("Get Content Tags error: no game title"), e.when([]);
          var n = r.getContentTags(t);
          return S().then(function(e) {
            return n({
              authorization: e
            }).then(function(e) {
              return x.info("Get Content Tags success, " + e.data.data.length +
                " matches found."), e.data.data;
            }, function(e) {
              return x.error("Get Content Tags error: " + (0, d.default)(e)), [];
            });
          });
        }, p.getPages = function() {
          return S(!0).then(function(t) {
            return r.getPages({
              authorization: t
            }).then(function(e) {
              return x.info("Get Pages success, " + e.data.data.length + " pages returned."),
                e.data.data;
            }, function(t) {
              return x.error("Get Pages error: " + (0, d.default)(t)), e.reject(void 0);
            });
          });
        }, p.getGroups = function() {
          return S(!0).then(function(t) {
            return r.getGroups({
              authorization: t
            }).then(function(e) {
              return x.info("Get Groups success, " + e.data.data.length +
                " groups returned."), e.data.data;
            }, function(t) {
              return x.error("Get Groups error: " + (0, d.default)(t)), e.reject(void 0);
            });
          });
        }, p.uploadImage = function(t, n) {
          return void 0 !== t.destination ? (y = t.destination.access_token, void 0 === t.destination
            .id && (t.destination.id = "me")) : (t.destination = {
            id: "me"
          }, y = void 0), S().then(function(a) {
            var l = void 0 !== t.privacy ? t.privacy.value : T;
            return r.uploadImage(t, a, l).then(function(e) {
              x.info("Upload to " + n.title + " completed: ", e.status), y = void 0;
              var r = new Date(),
                a = "https://www.facebook.com/" + e.data.id;
              if (s.getConfig().jarvisLinkedContent) {
                var l = {
                  source: h(t.name),
                  title: t.name,
                  uploadTime: r.toJSON(),
                  provider: p.name,
                  service: p.serviceNames.FACEBOOK,
                  type: c.IMAGE,
                  subType: u.NORMAL,
                  directUrl: a,
                  siteUrl: a,
                  path: t.filepath
                };
                o.uploadContent(l);
              }
              return i.addGalleryUploadHistoryItem(t.name, r.toLocaleDateString(), r
                .toLocaleTimeString(), c.IMAGE, t.subtype, a, t.originalSource).catch(
                function(e) {
                  x.error("Addition to upload history failed, error : ", e);
                });
            }, function(t) {
              return y = void 0, x.error("Failed to upload image to Facebook: ", t
                .statusText), t && 401 == t.status && p.logoutFunc && p.logoutFunc(p
                  .serviceNames.FACEBOOK), e.reject(t);
            });
          }, function(e) {
            y = void 0, x.info("Upload Access token error: ", e.statusText), a.trigger(f
              .UGC_NOTIFICATION, {
                eventName: f.BROADCAST_LOGIN,
                data: n.title
              });
          });
        }, p.uploadVideo = function(t, n) {
          return void 0 !== t.destination ? (y = t.destination.access_token, void 0 === t.destination
            .id && (t.destination.id = "me")) : (t.destination = {
            id: "me"
          }, y = void 0), S().then(function(n) {
            return x.info("Facebook video upload initialization: ", t.filepath, ", size: ", t.file
              .size), r.initializeVideoUpload(t.file.size, t.destination.id)({
              authorization: n
            }).then(function(a) {
              var l = e.defer(),
                d = a.data.upload_session_id,
                f = a.data.video_id;
              x.info("Facebook video upload transferring chunks. uploadSessionId = ", d,
                ", videoId = ", f);
              var m = function(e, t) {
                  x.info("Facebook video upload progress. uploadSessionId = ", d,
                    ", offsets = ", e, t);
                },
                v = function(e, a) {
                  if (!e) return a = a || "", x.info("Facebook upload failed, error : ", a), l
                    .reject(a), void(a && 401 == a.status && p.logoutFunc && p.logoutFunc(p
                      .serviceNames.FACEBOOK));
                  x.info("Facebook video upload complete. Posting... uploadSessionId = ", d);
                  var m = void 0 !== t.privacy ? t.privacy.value : T;
                  r.finalizeVideoUpload(d, t, m)({
                    authorization: n
                  }).then(function(e) {
                    x.info("Facebook video upload complete: ", e.data.success), y =
                    void 0;
                    var n = new Date(),
                      r = n.toLocaleDateString(),
                      a = n.toLocaleTimeString(),
                      d = "https://www.facebook.com/" + f;
                    if (s.getConfig().jarvisLinkedContent) {
                      var m = {
                        source: h(t.title),
                        title: t.title,
                        uploadTime: new Date().toJSON(),
                        provider: p.name,
                        service: p.serviceNames.FACEBOOK,
                        type: c.VIDEO,
                        subType: u.NORMAL,
                        directUri: d,
                        siteUri: d,
                        path: t.filepath
                      };
                      o.uploadContent(m);
                    }
                    var v = t.originalSource;
                    t.subtype === u.GIF && (v = t.filepath.replace(/\\\\/g, "\\")), x
                      .info("File for UploadHistory thumb: ", v), i
                      .addGalleryUploadHistoryItem(t.title, r, a, c.VIDEO, t.subtype, d,
                        v).then(function() {
                        l.resolve();
                      }, function(e) {
                        x.error("Addition to upload history failed, error : ", e), l
                          .resolve();
                      });
                  }, function(e) {
                    x.info("Facebook upload failed, error : ", e), l.reject(e), e &&
                      401 == e.status && p.logoutFunc && p.logoutFunc(p.serviceNames
                        .FACEBOOK);
                  });
                };
              return r.uploadVideo(t, a.data, n, m, v), l.promise;
            });
          }, function(t) {
            return y = void 0, x.info("Upload Access token error: ", t.statusText), a.trigger(f
              .UGC_NOTIFICATION, {
                eventName: f.BROADCAST_LOGIN,
                data: n.title
              }), e.reject(t);
          });
        }, p.getReactionsCount = function() {
          if (void 0 === v) return e.reject("No active live video broadcast!");
          var t = r.getReactionsSummary(v, p.reactionTypes);
          return S().then(function(e) {
            return t({
              authorization: e
            }).then(function(e) {
              var t = {},
                n = 0;
              return _.each(p.reactionTypes, function(r) {
                  null !== e.data[r] && void 0 !== e.data[r] && (t[r] = e.data[r].summary
                    .total_count, n += t[r]);
                }), n /= 2, x.info("Get Reactions Summary success " + n), void 0 !== w &&
                w === t || (a.trigger(l.FACEBOOK_REACTIONS, t), w = t), t;
            }, function(e) {
              x.error("Get Reaction Summary error: " + (0, d.default)(e));
            });
          });
        }, p.getComments = function(t) {
          if (void 0 === v) return e.reject("No active live video broadcast!");
          var n = r.getComments(v, void 0 !== t ? t : p.providerInfo.services[p.serviceNames.FACEBOOK]
            .maxNumberOfComments);
          return S().then(function(e) {
            return n({
              authorization: e
            }).then(function(e) {
              var t = e.data;
              if (t.data = _.filter(t.data, function(e) {
                  if (e && e.from) return !0;
                }), _.each(t.data, function(e) {
                  e.from.avatarUri = "http://graph.facebook.com/" + e.from.id +
                    "/picture?type=square";
                }), x.info("Get Comments success " + t.summary.total_count), void 0 !== t
                .data && null !== t.data && void 0 !== t.data[0]) {
                var n = t.data[0].created_time;
                void 0 !== E && E === n || (a.trigger(l.FACEBOOK_COMMENTS, t), E = n);
              }
              return t;
            }, function(e) {
              x.error("Get Comments error: " + (0, d.default)(e));
            });
          });
        }, p.init = function() {
          var e = s.getConfig().connect;
          e && e.facebook ? (p.providerInfo.clientId = e.facebook.clientId, p.providerInfo
              .useOauthProxy = e.facebook.useOauthProxy, e.facebook.config && (x.info(
                  "Facebook configuration found, merging with Facebook service config blob."), angular
                .merge(p.providerInfo.services[p.serviceNames.FACEBOOK], e.facebook.config))) : x
            .error("Facebook service initialization failed. No configuration found for Facebook!");
        };
      }
    ]), p.service("googleService", ["$q", "$log", "$interval", "eventAggregator", "youtubeEndpoints",
      "googlePhotosEndpoints", "jarvisService", "galleryService", "runTimeConfigService",
      "CONNECT_EVENTS", "UGC_NOTIFICATION_EVENTS", "GALLERY_TYPES", "GALLERY_SUBTYPES",
      function(e, t, n, r, i, o, a, s, c, l, f, h, p) {
        function m(e) {
          var t = /^(.*?)\s\|/g,
            n = t.exec(e);
          return n ? n[1] : null;
        }

        function v(e) {
          y.providerInfo.oauthUrl = R + "&hl=" + ("zh-CHT" === e ? "zh-TW" : e);
        }
        var g,
          y = this,
          b = 100,
          E = "Bearer ",
          $ = "Desktop",
          w = 100,
          T = "public",
          C = "private",
          x = null,
          A = null,
          M = {},
          k = 0,
          N = null,
          I = null,
          O = null,
          D = t.getInstance("ugc-lib/googleService");
        y.name = "google", y.serviceNames = {
          YOUTUBE: "YouTube",
          GOOGLE_PHOTOS: "GooglePhotos"
        };
        var R =
          "https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={{clientId}}&scope={{scope}}&redirect_uri={{redirectUri}}&prompt=consent&access_type=offline";
        y.providerInfo = {
          name: y.name,
          services: {},
          oauthUrl: R,
          clientId: "",
          clientSecret: "",
          logoutUrl: "https://accounts.google.com/Logout",
          cookies: {
            ".google.com": ["APISID", "HSID", "SAPISID", "SID", "SSID"]
          }
        }, y.providerInfo.services[y.serviceNames.YOUTUBE] = {
          id: 2,
          name: y.serviceNames.YOUTUBE,
          type: 5,
          endpoints: i,
          scope: "email profile https://www.googleapis.com/auth/youtube.upload https://www.googleapis.com/auth/youtube https://www.googleapis.com/auth/youtube.readonly ",
          default: 4
        }, y.providerInfo.services[y.serviceNames.GOOGLE_PHOTOS] = {
          name: y.serviceNames.GOOGLE_PHOTOS,
          title: "l10n.googlePhotos",
          type: 18,
          endpoints: o,
          scope: "profile https://www.googleapis.com/auth/photoslibrary.appendonly https://www.googleapis.com/auth/photoslibrary.readonly.appcreateddata",
          default: 2
        };
        var P = function(t) {
          if (void 0 === M[t]) return e.reject("No stored token found for service: " + t);
          if (t === y.serviceNames.GOOGLE_PHOTOS && 1 !== M[t].eFuse) return y.logoutFunc && y
            .logoutFunc(t), e.reject("Forced Google Photos logout due to new scopes.");
          var n = new Date(),
            o = new Date(M[t].expiresOn);
          return n < o ? e.when(E + M[t].accessToken) : void 0 !== M[t].refreshToken ? (r.trigger(l
            .ACCESS_TOKEN_EXPIRED, y.name), i.refreshAccessToken(y.providerInfo, M[t]).then(
            function(e) {
              return y.setAuthTokenCallback(t, e.data), E + M[t].accessToken;
            },
            function(n) {
              return M[t] = void 0, y.logoutFunc && y.logoutFunc(t), e.reject(
                "Google auth token refresh failed: " + n);
            })) : (M[t] = void 0, y.logoutFunc && y.logoutFunc(t), e.reject(
            "Google auth token expired since: " + o));
        };
        y.setAuthToken = function(e, t, n, r) {
          M[t] = e, void 0 !== e.code && void 0 === e.accessToken ? i.getAccessToken(y.providerInfo, e
            .code, r).then(function(e) {
            y.setAuthTokenCallback(t, e.data);
          }, function(e) {
            D.error("setAuthToken was unable to request an access token: ", e);
          }) : void 0 === e.accessToken ? D.error("setAuthToken called without valid accessToken: ",
            e) : t !== y.serviceNames.GOOGLE_PHOTOS || n || (M[t].eFuse = 1);
        }, y.ensureValidToken = function(e) {
          return P(e);
        }, y.getUser = function(t) {
          return P(t).then(function(e) {
            var n = y.providerInfo.services[t].endpoints;
            return n.getUserInfo({
              authorization: e
            });
          }).then(function(e) {
            return e;
          }, function(t) {
            return D.error("getUser failed: " + t.statusText), e.reject(t);
          });
        }, y.getRtmpUrl = function(t) {
          return P(y.serviceNames.YOUTUBE).then(function(n) {
            var o = {},
              a = new Date();
            O = a.toISOString(), angular.isDefined(t.title) ? (N = t.title, N.length > w && (N = N
                .substr(0, w))) : N = $, I = angular.isDefined(t.privacy) ? t.privacy.value : T, o
              .startTime = O, o.title = N, o.privacy = I;
            var s = i.createLiveBroadcast(o);
            return s({
              authorization: n
            }).then(function(a) {
              if (D.info("YTL LiveBroadcast id: " + a.data.id), x = a.data.id, o.resolution =
                t.resolution, o.frameRate = "30fps", 60 === t.framerate) {
                var s;
                s = y.providerInfo.services[y.serviceNames.YOUTUBE].supported_settings[t
                  .framerate];
                var c = _.findIndex(s, function(e) {
                  return t.resolution === e.split(" ")[0];
                });
                c !== -1 && (o.frameRate = "60fps");
              }
              var u = i.createLiveStream(o);
              return u({
                authorization: n
              }).then(function(t) {
                var o = t.data.cdn.ingestionInfo.ingestionAddress + "/" + t.data.cdn
                  .ingestionInfo.streamName;
                A = t.data.id, D.info("YTL RtmpUrl: " + o + " StreamId: " + A);
                var a = i.broadcastToLivestream(A, x);
                return a({
                  authorization: n
                }).then(function(e) {
                  return D.info("bindBroadcastToLivestream success"), o;
                }, function(t) {
                  var n = "bindBroadcastToLivestream failure: " + (0, d.default)(t);
                  return D.error(n), r.trigger(l.BROADCAST_ERROR, {
                    serviceName: y.name,
                    errorTxt: n
                  }), e.reject(t);
                });
              }, function(t) {
                var n = "createLiveStream failure: " + (0, d.default)(t);
                return D.error(n), r.trigger(l.BROADCAST_ERROR, {
                  serviceName: y.name,
                  errorTxt: n
                }), e.reject(t);
              });
            }, function(t) {
              var n = "createLiveBroadcast failure: " + (0, d.default)(t);
              return D.error(n), r.trigger(l.BROADCAST_ERROR, {
                serviceName: y.name,
                errorTxt: n
              }), e.reject(t);
            });
          });
        }, y.postRtmpProcess = function() {
          return g = n(function() {
            P(y.serviceNames.YOUTUBE).then(function(e) {
              var t = i.listStreamRequest(A);
              t({
                authorization: e
              }).then(function(t) {
                var o = t.data.items[0].status.streamStatus;
                if (D.info("listStream success " + o), 0 === o.localeCompare("active")) {
                  var a = i.listBroadcastRequest(x);
                  a({
                    authorization: e
                  }).then(function(t) {
                    var o = t.data.items[0].status.lifeCycleStatus;
                    if (D.info("listBroadcast success " + o), 0 === o.localeCompare(
                        "live") && n.cancel(g), 0 === o.localeCompare("ready")) {
                      var a = i.transitionBroadcastToTesting(x);
                      a({
                        authorization: e
                      }).then(function(e) {
                        D.info("transition to testing success " + e.data.status
                          .lifeCycleStatus);
                      }, function(e) {
                        var t = "transition to testing failure " + e.statusText;
                        r.trigger(l.BROADCAST_ERROR, {
                          serviceName: y.name,
                          errorTxt: t
                        });
                      });
                    }
                    if (0 === o.localeCompare("testing")) {
                      var s = i.transitionBroadcastToLive(x);
                      s({
                        authorization: e
                      }).then(function(e) {
                        D.info("transition to live success " + e.data.status
                          .lifeCycleStatus);
                      }, function(e) {
                        var t = "transition to live failure " + e.statusText;
                        D.error(t), r.trigger(l.BROADCAST_ERROR, {
                          serviceName: y.name,
                          errorTxt: t
                        });
                      });
                    }
                  }, function(e) {
                    var t = "listBroadcast failure " + e.statusText;
                    D.error(t), r.trigger(l.BROADCAST_ERROR, {
                      serviceName: y.name,
                      errorTxt: t
                    });
                  });
                }
              }, function(e) {
                var t = "listStream failure " + e.statusText;
                D.error(t), r.trigger(l.BROADCAST_ERROR, {
                  serviceName: y.name,
                  errorTxt: t
                });
              });
            });
          }, 1e3, b);
        }, y.stopBroadcast = function() {
          return angular.isDefined(g) && n.cancel(g), k = 0, P(y.serviceNames.YOUTUBE).then(function(
            e) {
            var t = i.transitionBroadcastToComplete(x);
            return t({
              authorization: e
            }).then(function(e) {
              D.info("transition to complete success ");
            }, function(e) {
              var t = "transition to complete failure " + e.statusText;
              D.error(t), r.trigger(l.BROADCAST_ERROR, {
                serviceName: y.name,
                errorTxt: t
              });
            });
          });
        }, y.setBroadcastTitle = function(t, n) {
          if (n) return e.when(!0);
          N = t, N.length > w && (N = N.substr(0, w));
          var r = {};
          return r.title = N, r.privacy = I, r.startTime = O, P(y.serviceNames.YOUTUBE).then(function(
            e) {
            var t = i.updateBroadcastTitle(r, x);
            return t({
              authorization: e
            }).then(function(e) {
              D.info("Broadcast Title updated");
            }, function(e) {
              D.error("Broadcast Title update failure " + e.statusText);
            });
          });
        }, y.setBroadcastDescription = function(e) {
          return P(y.serviceNames.YOUTUBE).then(function(t) {
            var n = i.updateBroadcastDescription(x, e, N);
            return n({
              authorization: t
            }).then(function(e) {
              D.info("updateBroadcastDescription success ");
            }, function(e) {
              D.error("updateBroadcastDescription error: " + e.statusText);
            });
          });
        }, y.getCurrentViewerCount = function() {
          return P(y.serviceNames.YOUTUBE).then(function(e) {
            var t = i.listVideos(x);
            return t({
              authorization: e
            }).then(function(e) {
              if (_.isUndefined(e.data.items[0]) || _.isNull(e.data.items[0])) return null;
              var t = e.data.items[0].liveStreamingDetails,
                n = t ? t.concurrentViewers : void 0;
              return D.info("Get Viewer Count success " + n), void 0 === n ? 0 : n;
            }, function(e) {
              D.error("Get Viewer Count error: " + e.statusText);
            });
          });
        }, y.userHasChannels = function() {
          return P(y.serviceNames.YOUTUBE).then(function(e) {
            var t = i.listChannels();
            return t({
              authorization: e
            }).then(function(e) {
              var t = !1;
              return _.find(e.data.items, function(e, n) {
                e.status.isLinked && (t = !0);
              }), t;
            }, function(e) {
              return D.error("Get Channel list error: " + e.statusText), !1;
            });
          });
        }, y.uploadImage = function(t, n) {
          return P(y.serviceNames.GOOGLE_PHOTOS).then(function(n) {
            return D.info("Google Photos media upload initialization: ", t.filepath, ", size: ", t
              .file.size), o.initializeMediaUpload(t.file.size, t.name, t.file.type)({
              authorization: n
            }).then(function(r) {
              var i = e.defer(),
                u = r.headers("X-Goog-Upload-URL"),
                l = Number(r.headers("X-Goog-Upload-Chunk-Granularity"));
              D.info("Google Photos media upload transferring chunks. uploadUrl = ", u,
                ", chunkSize = ", l);
              var d = function(e, t) {
                  D.info("Google Photos media upload progress. Offsets = ", e, t);
                },
                f = function(e, r) {
                  return e ? (D.info(
                      "Google Photos media upload complete. Adding to album..."), void o
                    .finalizeMediaUpload(r.data, t, n).then(function(e) {
                      if (!e.data.newMediaItemResults || !e.data.newMediaItemResults[0] ||
                        !e.data.newMediaItemResults[0].mediaItem) return void i.reject(
                        "Empty response received from finalizeMediaUpload call!");
                      var n = new Date(),
                        r = e.data.newMediaItemResults[0].mediaItem.productUrl;
                      if (D.info("Google Photos media upload complete (" + e.data
                          .newMediaItemResults[0].mediaItem.status + "): ", r), c
                        .getConfig().jarvisLinkedContent) {
                        var o = {
                          source: m(t.name),
                          title: t.name,
                          uploadTime: new Date().toJSON(),
                          provider: y.name,
                          service: y.serviceNames.GOOGLE_PHOTOS,
                          type: h.IMAGE,
                          subType: p.NORMAL,
                          directUri: r,
                          siteUri: r,
                          path: t.filepath
                        };
                        a.uploadContent(o);
                      }
                      var u = t.originalSource;
                      return t.subtype === p.GIF && (u = t.filepath.replace(/\\\\/g,
                          "\\")), D.info("File for UploadHistory thumb: ", u), s
                        .addGalleryUploadHistoryItem(t.name, n.toLocaleDateString(), n
                          .toLocaleTimeString(), h.IMAGE, t.subtype, r, u).then(
                      function() {
                          i.resolve();
                        }, function(e) {
                          D.error("Addition to upload history failed: ", e), i
                        .resolve();
                        });
                    }, function(e) {
                      D.info("Google Photos media upload failed: ", e), i.reject(e), e &&
                        401 == e.status && y.logoutFunc && y.logoutFunc(y.serviceNames
                          .GOOGLE_PHOTOS);
                    })) : (D.info("Google Photos media upload failed: ", r), i.reject(r),
                    void(r && 401 == r.status && y.logoutFunc && y.logoutFunc(y.serviceNames
                      .GOOGLE_PHOTOS)));
                };
              return o.uploadFile(t, u, l, n, d, f), i.promise;
            });
          }, function(t) {
            return D.info("Upload Access token error: ", t.statusText), r.trigger(f
              .UGC_NOTIFICATION, {
                eventName: f.BROADCAST_LOGIN,
                data: n.title
              }), e.reject(t);
          });
        }, y.uploadVideo = function(t, n) {
          return y.userHasChannels().then(function(i) {
            return i === !1 ? (r.trigger(f.UGC_NOTIFICATION, {
              eventName: f.NO_YOUTUBE_CHANNEL
            }), e.when(!1)) : P(y.serviceNames.YOUTUBE).then(function(n) {
              var r = {
                  snippet: {
                    title: t.title,
                    description: t.description,
                    tags: t.tags,
                    categoryId: t.categoryId
                  },
                  status: {
                    privacyStatus: angular.isDefined(t.privacy) ? t.privacy.value : C
                  }
                },
                i = e.defer(),
                o = new S({
                  baseUrl: "https://www.googleapis.com/upload/youtube/v3/videos",
                  file: t.file,
                  token: n,
                  metadata: r,
                  params: {
                    part: (0, u.default)(r).join(",")
                  },
                  onError: function(e) {
                    var t = e;
                    try {
                      var n = JSON.parse(e);
                      t = "Exception on upload: " + n.error.message;
                    } catch (e) {
                      t = "Exception on upload: " + t;
                    } finally {
                      D.info(t), i.reject(t);
                    }
                  },
                  onProgress: function(e) {
                    Date.now(), e.loaded, e.total;
                    D.info("Progress bytes uploaded: ", e.loaded);
                  },
                  onComplete: function(e) {
                    var n = JSON.parse(e);
                    D.info("Upload complete: ", n.id);
                    var r = new Date(),
                      o = r.toLocaleDateString(),
                      u = r.toLocaleTimeString();
                    if (c.getConfig().jarvisLinkedContent) {
                      var l = {
                        source: m(t.title),
                        title: t.title,
                        uploadTime: new Date().toJSON(),
                        provider: y.name,
                        service: y.serviceNames.YOUTUBE,
                        type: h.VIDEO,
                        subType: p.NORMAL,
                        directUri: "https://www.youtube.com/embed/" + n.id,
                        siteUri: "https://www.youtube.com/watch?v=" + n.id,
                        path: t.filepath
                      };
                      a.uploadContent(l);
                    }
                    return s.addGalleryUploadHistoryItem(t.title, o, u, h.VIDEO, t
                      .subtype, "https://www.youtube.com/watch?v=" + n.id, t
                      .originalSource).then(function() {
                      i.resolve();
                    }, function(e) {
                      D.error("Addition to upload history failed, error : ", e), i
                        .resolve();
                    });
                  }
                });
              return o.upload(), i.promise;
            }, function(e) {
              D.info("Upload Access token error: ", e.statusText), r.trigger(f
                .UGC_NOTIFICATION, {
                  eventName: f.BROADCAST_LOGIN,
                  data: n.title
                });
            });
          });
        }, y.getBroadcastQuality = function(e) {
          var t = y.providerInfo.services[y.serviceNames.YOUTUBE].qualityPrefix;
          return "Custom" === e && (e = "UltraGood"), t + e;
        }, y.init = function(e, t) {
          var n = c.getConfig().connect;
          n && n.google ? (y.providerInfo.clientId = n.google.clientId, y.providerInfo.clientSecret =
            n.google.clientSecret, y.providerInfo.useOauthProxy = n.google.useOauthProxy, y
            .providerInfo.embeddedRestricted = n.google.embeddedRestricted, y.providerInfo
            .redirectUriOverride = n.google.redirectUriOverride, n.google.config && n.google.config
            .youtube && (D.info(
                "YouTube configuration found, merging with Google service config blob."), angular
              .merge(y.providerInfo.services[y.serviceNames.YOUTUBE], n.google.config.youtube), w =
              y.providerInfo.services[y.serviceNames.YOUTUBE].broadcastTitleMaxLength), n.google
            .config && n.google.config.photos && (D.info(
                "Google Photos configuration found, merging with Google service config blob."),
              angular.merge(y.providerInfo.services[y.serviceNames.GOOGLE_PHOTOS], n.google.config
                .photos)), v(e), r.on(t, v)) : D.error(
            "Google service initialization failed. No configuration found for Google!");
        };
      }
    ]), p.service("jarvisService", ["$q", "$log", "jarvis", "JarvisSessionManager", "$http",
      "telemetryService", "nvAccountService", "runTimeConfigService", "hardwareConfigWrapper",
      function(t, n, r, i, a, c, u, l, d) {
        function f() {
          return d.hardwareSystemInfoGetter().catch(function() {
            return {};
          }).then(function(e) {
            return e.JarvisDeviceId = e.JarvisDeviceId || "deviceId", e.PCName = e.PCName ||
              "Unnamed PC", e;
          });
        }

        function h() {
          return t.when(O);
        }

        function p(e) {
          O = e;
        }

        function m(e) {
          return h().then(function(t) {
            return D.debug("storing session token"), t.sessionToken = e, p(t);
          });
        }

        function v(e, t) {
          c.setUserConsent({
            userId: e,
            consentSettings: t
          });
        }

        function g(e) {
          return void 0 === e || void 0 === e.userInfo || void 0 === e.userInfo.userId || null === e
            .userInfo.userId || "" === e.userInfo.userId;
        }

        function y(e) {
          return D.info(
            "Storing user token and information in accounts file and setting user consent."), f().then(
              function(t) {
                var n = {
                  userToken: I.userToken,
                  userId: S.userId,
                  deviceId: t.JarvisDeviceId,
                  dataTracking: e
                };
                v(S.userId, e), u.storeJarvisUserToken({
                  userToken: I.userToken,
                  userInfo: n
                });
              });
        }

        function b() {
          if (void 0 !== I && null !== I && null !== I.userToken && S) {
            var e = {
                trackFunctionalData: {
                  lastChangedOn: new Date().toISOString(),
                  level: c.getDefaultConsent().functional
                }
              },
              t = S.privacySettings && S.privacySettings.dataTracking ? angular.merge(S.privacySettings
                .dataTracking, e) : e;
            u.getJarvisUserToken().then(function(e) {
              g(e) || e.userInfo.userId !== S.userId ? y(t) : (D.info(
                "Valid accounts file already exists, no need to write it again. Setting user consent only."
                ), v(S.userId, t), u.setUserTelemetryConsent(S.userId, t));
            }).catch(function(e) {
              D.info("Unable to get info from accounts file, probably because it did not exist."),
                y(t);
            });
          }
        }

        function E(e, t) {
          return h().then(function(n) {
            return b(), D.debug("storing user token"), n.userToken = e, n.user = t, p(n);
          });
        }

        function $(e) {
          D.debug("Authenticated user and started session."), S = e, A && A.setItem(DB_NAMES
            .LAST_LOGGED_IN_USER, {
              email: S.core.primaryEmail,
              type: S.core.passwordLastChanged ? "nvidia" : "social"
            });
        }

        function w(e, t, n) {
          var i = {
            userToken: e,
            sessionToken: t,
            user: n
          };
          return i.domains = {}, r.settings.get({
            sessionToken: t
          }).then(function(e) {
            return i.domains = _.propertyOf(e.data)("domains"), i;
          }).catch(function(e) {
            return D.error("Could not retrieve domain list: ", e), i;
          });
        }

        function T(e) {
          var t;
          return f().then(function(n) {
            return r.session.login({
              userToken: e
            }, {
              deviceId: n.JarvisDeviceId
            }).then(function(e) {
              return t = e.data.sessionToken, r.profile.currentUser.get({
                sessionToken: t
              });
            }).then(function(n) {
              return w(e, t, n.data).then(function(e) {
                return N.cachedSessionObject = e, N.cachedSessionObject;
              });
            });
          });
        }

        function C(e, n, r) {
          return t.when(N.hasSession()).then(function(e) {
            if (!e) return t.reject({
              status: 401
            });
          }).then(function() {
            return I.callSessionApi(e, n, r);
          }).catch(function(e) {
            return M && e && 401 === e.status ? M(!0) : D.error(
              "Jarvis userToken expired, please login again", e), t.reject(e);
          });
        }

        function x(e) {
          return C(r.profile.get, {
            userId: e
          }).then(function(t) {
            var n = t.data;
            return C(r.status.get, {
              userId: e
            }).then(function(e) {
              return n.statuses = e.data.values, n;
            }).catch(function() {
              return n;
            });
          });
        }
        var S,
          A,
          M,
          k,
          N = this,
          I = null,
          O = {},
          D = n.getInstance("jarvisService");
        N.loginFromDatabase = function() {
          var e, n;
          return A ? h().then(function(e) {
            return e.userToken;
          }).then(function(n) {
            if (D.debug("GFE OnGot"), e = n, !e) return t.reject("Invalid user token");
          }).then(function() {
            return D.debug("GFE on got logic"), h().then(function(e) {
              return e.sessionToken;
            });
          }).then(function(i) {
            return n = i, r.profile.currentUser.get({
              sessionToken: n
            }).then(function(t) {
              return w(e, n, t.data);
            }).catch(function(r) {
              return 401 === r.status ? T(e) : r.status !== -1 || k && k.online ? t.reject() :
                h().then(function(t) {
                  return w(e, n, t.user);
                });
            });
          }) : u.getJarvisUserToken().then(function(e) {
            return e && e.userInfo && !_.isEmpty(e.userInfo) ? e.userInfo.userToken : t.reject(
              "Not logged into Jarvis");
          }, function(e) {
            return t.reject("Could not retrieve Jarvis user token: " + e);
          }).then(function(n) {
            if (e = n, !e) return t.reject("Invalid user token");
          }).then(function() {
            return T(e);
          });
        }, N.loginWithRegistrationToken = function(e, t) {
          return f().then(function(t) {
            var n = {
                accessToken: e
              },
              i = {
                deviceId: t.JarvisDeviceId,
                deviceDescription: t.PCName
              };
            return r.authentication.login(n, i).then(function(e) {
              return T(e.data.userToken);
            });
          });
        }, N.loginWithUserToken = function(e) {
          return T(e);
        }, N.startSession = function(e) {
          return I = new i(m), f().then(function(t) {
            return I.createSessionFromToken(e.sessionToken, {
              userToken: e.userToken,
              deviceId: t.JarvisDeviceId
            }), $(e.user), E(e.userToken, e.user), e.user;
          });
        }, N.hasSession = function() {
          return null != I;
        }, N.getSession = function() {
          return I;
        }, N.getLoggedInUser = function() {
          return S;
        }, N.logout = function() {
          S && (D.info("Clearing local user session info."), S = void 0, E(null, null), I
            .closeSession(), I = null);
        }, N.getOauthUrl = function(e) {
          return f().then(function(t) {
            return r.authentication.oauth.getQueryString({
              provider: e,
              data: {
                deviceId: t.JarvisDeviceId,
                deviceDescription: t.PCName
              }
            });
          });
        }, N.getLoginWithOauthUrl = function(e) {
          return r.authentication.loginWithOauth.getQueryString(e);
        }, N.getUserTokenFromCode = function(e) {
          return r.authentication.loginWithOauthResource({}, {
            code: e
          });
        }, N.getUser = x, N.getUserDatabyUserId = function(e, t, n) {
          return C(r.userData.sharedUser.get, {
            userId: e,
            clientId: t,
            blockKey: n
          }).then(function(e) {
            return e.data.data;
          });
        }, N.getUserData = function(e, t) {
          return C(r.userData.shared.get, {
            clientId: e,
            blockKey: t
          }).then(function(e) {
            return e.data.data;
          });
        }, N.setUserData = function(e, t, n, i) {
          var o,
            a = {
              clientId: e,
              blockKey: t
            },
            s = {
              key: t,
              data: n
            };
          return i ? (a.sessionToken = i.sessionToken, o = r.userData.shared.update.bind(this, a,
            s)) : o = C.bind(this, r.userData.shared.update, a, s), o().then(function(e) {
              return e.data.data;
            });
        }, N.modifyUserData = function(e, t, n, i) {
          var o,
            a = {
              clientId: e,
              blockKey: t
            },
            s = {
              operators: n
            };
          return i ? (a.sessionToken = i.sessionToken, o = r.userData.shared.modify.bind(this, a,
            s)) : o = C.bind(this, r.userData.shared.modify, a, s), o().then(function(e) {
              return e.data.data;
            });
        }, N.uploadContent = function(e) {
          var t = l.getConfig().jarvis.gfeClientId,
            n = l.getConfig().jarvis.blockKeys.upload;
          N.getUserData(t, n).then(function(r) {
            D.info("Jarvis getUserData"), D.info(r), (angular.equals(r, {}) || null === r ||
                void 0 === r) && (D.info("Jarivs empty"), r = {
                uploadedItems: []
              }), r.uploadedItems.push(e), N.setUserData(t, n, r), D.info("Jarvis setUserData"), D
              .info(r);
          });
        }, N.getDelegateToken = function(e) {
          return C(r.delegate.request, {}, {
            clientId: e
          }).then(function(e) {
            return e.data;
          });
        }, N.makeProxyCall = function(n, r, i, c, u) {
          if (I) {
            var d = l.getConfig().jarvis.server + "/api/1/proxy/" + n + "/" + r,
              f = (0, s.default)(i).map(function(e) {
                var t = (0, o.default)(e, 2),
                  n = t[0],
                  r = t[1];
                return n + "=" + r;
              }).join("&");
            return a({
              method: "POST",
              headers: {
                Authorization: "Basic " + e.from(I.sessionToken + ":").toString("base64")
              },
              url: u ? d + "?" + f : d,
              params: c || u ? void 0 : i,
              data: c && !u ? i : void 0
            });
          }
          return t.reject("Attempting to make Jarvis proxy calls without a valid jarvis session!");
        };
      }
    ]), p.service("imgurService", ["$q", "$log", "imgurEndpoints", "galleryService", "jarvisService",
      "runTimeConfigService", "UGC_NOTIFICATION_EVENTS", "GALLERY_TYPES", "GALLERY_SUBTYPES",
      function(e, t, n, r, i, o, a, s, c) {
        function u(e) {
          var t = /^(.*?)\s\|/g,
            n = t.exec(e);
          return n ? n[1] : null;
        }
        var l = this,
          d = null,
          f = "Bearer ",
          h = t.getInstance("ugc-lib/imgurService");
        l.name = "imgur", l.serviceNames = {
          IMGUR: "Imgur"
        }, l.providerInfo = {
          name: l.name,
          services: {},
          oauthUrl: "https://api.imgur.com/oauth2/authorize?response_type=token&client_id={{clientId}}",
          clientId: "",
          logoutUrl: "http://imgur.com/logout/logout?msid={{sessionId}}",
          cookies: {
            ".imgur.com": ["IMGURSESSION"],
            "imgur.com": ["authautologin"]
          }
        }, l.providerInfo.services[l.serviceNames.IMGUR] = {
          name: l.serviceNames.IMGUR,
          type: 18
        };
        var p = function() {
          return e.when(f + d.accessToken);
        };
        l.setAuthToken = function(e) {
          d = e;
        }, l.getUser = function() {
          return void 0 !== d && null !== d && void 0 !== d.accountUsername && null !== d
            .accountUsername ? e.when({
              userName: d.accountUsername
            }) : e.reject("Not logged into service " + l.serviceNames.IMGUR);
        }, l.uploadImage = function(t, d) {
          return p().then(function(a) {
            var d = new FileReader(),
              f = e.defer();
            return d.readAsDataURL(t.file), d.onloadend = function() {
              var e = d.result,
                p = e.split(",")[1];
              n.uploadImage(t.name, p)({
                authorization: a
              }).then(function(e) {
                h.info("Upload to Imgur completed: ", e.status);
                var n = new Date();
                if (o.getConfig().jarvisLinkedContent) {
                  var a = {
                    source: u(t.name),
                    title: t.name,
                    uploadTime: new Date().toJSON(),
                    provider: l.name,
                    service: l.serviceNames.IMGUR,
                    type: s.IMAGE,
                    subType: c.NORMAL,
                    directUri: e.data.data.link,
                    siteUri: e.data.data.link,
                    path: t.filepath
                  };
                  i.uploadContent(a);
                }
                var d = t.originalSource;
                return t.subtype === c.GIF && (d = t.filepath.replace(/\\\\/g, "\\")), h
                  .info("File for UploadHistory thumb: ", d), r.addGalleryUploadHistoryItem(
                    t.name, n.toLocaleDateString(), n.toLocaleTimeString(), s.IMAGE, t
                    .subtype, e.data.data.link, d).then(function() {
                    f.resolve();
                  }, function(e) {
                    h.error("Addition to upload history failed, error : ", e), f
                  .resolve();
                  });
              }, function(e) {
                h.error("Failed to upload image at Imgur: ", e.statusText), f.reject(e);
              });
            }, f.promise;
          }, function(e) {
            h.info("Upload Access token error: ", e.statusText), eventAggregator.trigger(a
              .UGC_NOTIFICATION, {
                eventName: a.BROADCAST_LOGIN,
                data: d.title
              });
          });
        }, l.init = function() {
          var e = o.getConfig().connect;
          e && e.imgur ? (l.providerInfo.clientId = e.imgur.clientId, e.imgur.config && (h.info(
              "Imgur configuration found, merging with Imgur service config blob."), angular
            .merge(l.providerInfo.services[l.serviceNames.IMGUR], e.imgur.config))) : h.error(
            "Imgur service initialization failed. No configuration found for Imgur!");
        };
      }
    ]), p.service("shotWithGeForceService", ["$q", "$log", "$window", "shotWithGeForceEndpoints",
      "galleryService", "hardwareService", "jarvisService", "eventAggregator", "nvAccountService",
      "runTimeConfigService", "GALLERY_TYPES", "CONNECT_EVENTS",
      function(e, t, n, r, i, o, a, s, c, u, l, f) {
        function h(e) {
          var t = /^(.*?)\s\|/g,
            n = t.exec(e);
          return n ? n[1] : null;
        }

        function p(e) {
          var t = /.*[\\\/](.*)/,
            n = t.exec(e);
          return n[1];
        }
        var m = this;
        m.storedToken = null, m.jarvisDeviceId = null;
        var v = t.getInstance("ugc-lib/shotWithGeForceService");
        m.name = "nvidia", m.serviceNames = {
          SHOT_WITH_GEFORCE: "ShotWithGeForce"
        }, m.providerInfo = {
          name: m.name,
          services: {},
          clientId: "",
          cookies: {}
        }, m.providerInfo.services[m.serviceNames.SHOT_WITH_GEFORCE] = {
          name: m.serviceNames.SHOT_WITH_GEFORCE,
          type: 10
        };
        var g = "oauth-" + m.serviceNames.SHOT_WITH_GEFORCE;
        m.startJarvisSession = function(t) {
          var i = e.when(0);
          return a.hasSession() || (i = a.startSession(t)), i.then(function() {
            return a.getDelegateToken(u.getConfig().connect.swgf.clientId).then(function(e) {
              return r.getSessionTokenAndUId(e.delegateToken).then(function(r) {
                return m.storedToken = {
                  accessToken: t.userToken,
                  sessionToken: r.result.session,
                  expiresIn: new Date(e.expiration).getTime() - Date.now(),
                  accountUsername: t.user.core.displayName,
                  accountId: r.result.userId,
                  connectedOn: Date.now(),
                  expiresOn: new Date(e.expiration).getTime()
                }, n.localStorage.setItem(g, (0, d.default)(m.storedToken)), s.trigger(f
                  .USER_LOGGED_IN, {
                    serviceName: m.serviceNames.SHOT_WITH_GEFORCE,
                    providerName: m.name
                  }), m.storedToken;
              });
            });
          });
        }, m.updateAccessToken = function() {
          return !m.storedToken || !m.storedToken.sessionToken || m.storedToken.expiresOn < Date
          .now() ? c.getJarvisUserToken().then(function(t) {
              return t && void 0 !== t.userInfo && void 0 !== t.userInfo.userToken ? a
                .loginWithUserToken(t.userInfo.userToken).then(function(e) {
                  return m.startJarvisSession(e);
                }, function(t) {
                  return t && t.data && "REPEAT_REQUEST" === t.data.error && a
                    .cachedSessionObject ? m.startJarvisSession(a.cachedSessionObject) : (v.info(
                        "updateAccessToken was unable to log into Jarvis with stored token: ", t),
                      n.localStorage.getItem(g) && (n.localStorage.removeItem(g), m.storedToken =
                        null, s.trigger(f.USER_LOGGED_OUT, m.serviceNames.SHOT_WITH_GEFORCE)), e
                      .reject(t));
                }) : e.reject(
                  "No stored Jarvis user token found! New login required to get access token.");
            }) : e.when(m.storedToken);
        }, m.setAuthToken = function(e) {
          m.storedToken = e, void 0 !== e.code && void 0 === e.loginToken ? a.getUserTokenFromCode(e
            .code).then(function(t) {
            e.expiration = t.data.expiration, a.loginWithRegistrationToken(t.data.loginToken)
              .then(function(e) {
                return m.startJarvisSession(e);
              }, function(e) {
                v.error(
                  "setAuthToken was unable to exchange the login token for a user token: ", e);
              });
          }, function(e) {
            v.error("setAuthToken was unable to exchange the access code for a login token: ", e);
          }) : void 0 === e.loginToken && v.error("setAuthToken called without valid accessToken: ",
            e);
        }, m.getUser = function() {
          return m.updateAccessToken().then(function() {
            return a.getUserData(u.getConfig().jarvis.gfeClientId, "avatar").then(function(t) {
              if (!t || !t.url) return e.reject("Failed to get user data from Jarvis!");
              var n = a.getLoggedInUser();
              return {
                userId: n.userId,
                userName: n.core.displayName,
                avatarUri: t.url.startsWith("http") ? t.url : void 0
              };
            });
          });
        }, m.uploadImage = function(t, n) {
          return m.updateAccessToken().then(function() {
            return t.fileName = p(t.filepath), r.uploadImageData(m.storedToken.sessionToken, m
              .storedToken.accountId, t).then(function(e) {
              v.info("Upload to ShotWithGeForce completed: ", e.status);
              var n = new Date();
              if (u.getConfig().jarvisLinkedContent) {
                var r = {
                  source: h(t.name),
                  title: t.name,
                  uploadTime: new Date().toJSON(),
                  provider: m.name,
                  service: m.serviceNames.SHOT_WITH_GEFORCE,
                  type: l.IMAGE,
                  subType: t.subtype,
                  directUri: e.data.result.url,
                  siteUri: e.data.result.url,
                  path: t.filepath
                };
                a.uploadContent(r);
              }
              return i.addGalleryUploadHistoryItem(t.name, n.toLocaleDateString(), n
                .toLocaleTimeString(), l.IMAGE, t.subtype, e.data.result.url, t
                .originalSource).catch(function(e) {
                v.error("Addition to upload history failed, error : ", e);
              });
            }, function(t) {
              return v.error("Failed to upload to ShotWithGeForce: " + t.status + ") " + t
                .statusText), e.reject(t);
            });
          }, function(e) {
            v.info("Update Access token error: ", e);
          });
        }, m.convertToJarvisLocale = function(e) {
          var t = e;
          return "zh-CHS" === t ? t = "zh-cn" : "zh-CHT" === t && (t = "zh-tw"), t;
        }, m.onLanguageChanged = function(e) {
          m.providerInfo.oauthUrl = a.getLoginWithOauthUrl({
            locale: m.convertToJarvisLocale(e)
          });
        }, m.init = function(e, t) {
          var n = u.getConfig();
          s.on(t, m.onLanguageChanged), n && n.jarvis && n.connect && n.connect.swgf ? (m.providerInfo
            .oauthUrl = a.getLoginWithOauthUrl({
              locale: m.convertToJarvisLocale(e)
            }), m.providerInfo.clientId = n.jarvis.clientId, n.connect.swgf.config && (v.info(
                "ShotWithGeForce configuration found, merging with NVIDIA service config blob."),
              angular.merge(m.providerInfo.services[m.serviceNames.SHOT_WITH_GEFORCE], n.connect
                .swgf.config)), r.init(), m.updateAccessToken(), o.getSystemInfo().then(function(
            e) {
              m.jarvisDeviceId = e.JarvisDeviceId;
            })) : v.error(
            "NVIDIA service initialization failed. No configuration found for ShotWithGeForce!");
        }, m.logout = function() {
          a.logout();
        };
      }
    ]), p.service("sinaService", ["$q", "$log", "weiboEndpoints", "galleryService",
      "shotWithGeForceService", "jarvisService", "eventAggregator", "runTimeConfigService",
      "UGC_NOTIFICATION_EVENTS", "GALLERY_TYPES", "GALLERY_SUBTYPES",
      function(e, t, n, r, i, o, a, s, c, u, l) {
        function d(e) {
          var t = /^(.*?)\s\|/g,
            n = t.exec(e);
          return n ? n[1] : null;
        }

        function f(e) {
          "en-US" === e || "en-GB" === e ? h.providerInfo.oauthUrl = g : h.providerInfo.oauthUrl = g;
        }
        var h = this,
          p = null,
          m = "",
          v = t.getInstance("ugc-lib/sinaService");
        h.name = "sina", h.serviceNames = {
          WEIBO: "Weibo"
        };
        var g =
          "https://api.weibo.com/oauth2/authorize?response_type=code&client_id={{clientId}}&redirect_uri={{redirectUri}}&forcelogin=true";
        h.providerInfo = {
          name: h.name,
          jarvisName: "weibo",
          services: {},
          oauthUrl: g,
          clientId: "",
          clientSecret: "",
          logoutUrl: "https://api.weibo.com/oauth2/revokeoauth2?access_token=",
          cookies: {
            ".weibo.com": [""]
          }
        }, h.providerInfo.services[h.serviceNames.WEIBO] = {
          name: h.serviceNames.WEIBO,
          type: 22,
          videoOptions: {
            maxVideoFileSizeMB: 2048,
            maxVideoDurationSeconds: 300
          }
        }, h.logoutFunc = null;
        var y = function() {
          var t = m + p.accessToken;
          return n.getTokenTimeLeft(t).then(function(n) {
            return n >= 0 ? e.when(t) : (h.logoutFunc && h.logoutFunc(h.serviceNames.WEIBO), e
              .reject("Weibo access token expired!"));
          }, function(n) {
            return 400 == n.status || 401 == n.status ? (h.logoutFunc && h.logoutFunc(h
                .serviceNames.WEIBO), e.reject("Webio rejected the stored access token")) : e
              .when(t);
          });
        };
        h.ensureValidToken = function() {
          return y();
        }, h.setAuthToken = function(e) {
          p = e, void 0 !== e.code && void 0 === e.accessToken ? n.getAccessToken(h.providerInfo, e
            .code).then(function(e) {
            h.setAuthTokenCallback(h.serviceNames.WEIBO, e.data);
          }, function(e) {
            v.error("setAuthToken was unable to request an access token: ", e);
          }) : void 0 === e.accessToken && v.error(
            "setAuthToken called without valid accessToken: ", e);
        }, h.getUser = function() {
          return void 0 !== p && null !== p ? n.getUserData(p.accessToken).then(function(e) {
            return {
              userId: e.data.id,
              userName: e.data.name,
              avatarUri: e.data.profile_image_url,
              profile_url: e.data.profile_url
            };
          }) : e.reject("Not logged into service " + h.serviceNames.WEIBO);
        }, h.uploadImage = function(t, i) {
          return y().then(function(i) {
            return n.uploadImage(t, i).then(function(e) {
              v.info("Upload to Weibo completed: ", e.status);
              var n = new Date();
              if (s.getConfig().jarvisLinkedContent) {
                var i = {
                  source: d(t.name),
                  title: t.name,
                  uploadTime: new Date().toJSON(),
                  provider: h.name,
                  service: h.serviceNames.WEIBO,
                  type: u.IMAGE,
                  subType: l.NORMAL,
                  directUri: e.data.original_pic,
                  siteUri: e.data.original_pic,
                  path: t.filepath
                };
                o.uploadContent(i);
              }
              var a = t.originalSource;
              return t.subtype === l.GIF && (a = t.filepath.replace(/\\\\/g, "\\")), v.info(
                "File for UploadHistory thumb: ", a), r.addGalleryUploadHistoryItem(t.name,
                n.toLocaleDateString(), n.toLocaleTimeString(), u.IMAGE, t.subtype, e.data
                .original_pic, a).catch(function(e) {
                v.error("Addition to upload history failed, error : ", e);
              });
            }, function(t) {
              return v.error("Failed to upload image to Weibo: ", t), e.reject(t);
            });
          }, function(t) {
            return v.info("Upload Access token error: ", t.statusText), a.trigger(c
              .UGC_NOTIFICATION, {
                eventName: c.BROADCAST_LOGIN,
                data: i.title
              }), e.reject(error);
          });
        }, h.uploadVideo = function(t, i) {
          return y().then(function(i) {
            return v.info("Weibo video upload initialization: ", t.filepath, ", size: ", t.file
              .size), n.initializeVideoUpload(t, i).then(function(a) {
              var c = e.defer(),
                f = a.data.fileToken;
              v.info("Weibo video upload transferring chunks. fileToken = ", f);
              var p = function(e, t) {
                  v.info("Weibo video upload progress. fileToken = ", f, ", offsets = ", e,
                  t);
                },
                m = function(e, n) {
                  if (!e) return n = n || "", v.error("Upload to Weibo failed, error : ", n),
                    c.reject(n), void(401 == n.status && h.logoutFunc && h.logoutFunc(h
                      .serviceNames.WEIBO));
                  v.info("Weibo video upload complete");
                  var i;
                  h.getUser().then(function(e) {
                    i = "https://www.weibo.com/" + e.profile_url;
                    var n = new Date();
                    if (s.getConfig().jarvisLinkedContent) {
                      var a = {
                        source: d(t.title),
                        title: t.title,
                        uploadTime: n.toJSON(),
                        provider: h.name,
                        service: h.serviceNames.WEIBO,
                        type: u.VIDEO,
                        subType: l.NORMAL,
                        directUri: i,
                        siteUri: i,
                        path: t.filepath
                      };
                      o.uploadContent(a);
                    }
                    var f = t.originalSource;
                    v.info("File for UploadHistory thumb: ", f), r
                      .addGalleryUploadHistoryItem(t.title, n.toLocaleDateString(), n
                        .toLocaleTimeString(), u.VIDEO, t.subtype, i, f).finally(
                      function() {
                        c.resolve();
                      });
                  }).catch(function(e) {
                    v.error("Addition to upload history failed, error : ", e), c
                  .resolve();
                  });
                };
              return n.uploadVideo(t, a.data, i, p, m), c.promise;
            }, function(t) {
              return t = t || "", v.error("Upload to Weibo failed, error: ", t), 401 == t
                .status && h.logoutFunc && h.logoutFunc(h.serviceNames.WEIBO), e.reject(t);
            });
          }, function(t) {
            return v.error("Upload Access token error: ", t), a.trigger(c.UGC_NOTIFICATION, {
              eventName: c.BROADCAST_LOGIN,
              data: i.title
            }), e.reject(t);
          });
        }, h.logout = function() {
          return y().then(function(e) {
            n.logout(h.providerInfo, e);
          });
        }, h.init = function(e, t) {
          var n = s.getConfig().connect;
          n && n.weibo ? (h.providerInfo.clientId = n.weibo.clientId, h.providerInfo.clientSecret = n
            .weibo.clientSecret, h.providerInfo.useOauthProxy = n.weibo.useOauthProxy, n.weibo
            .config && (v.info("Weibo configuration found, merging with Sina service config blob."),
              angular.merge(h.providerInfo.services[h.serviceNames.WEIBO], n.weibo.config)), f(e), a
            .on(t, f)) : v.error(
            "Sina service initialization failed. No configuration found for Sina!");
        };
      }
    ]), p.service("twitchService", ["$q", "$log", "twitchEndpoints", "shadowPlayEndpoints",
      "eventAggregator", "runTimeConfigService", "twitchIngestServerConfigWrapper", "CONNECT_EVENTS",
      function(e, t, n, r, i, o, a, s) {
        function c(e) {
          l.providerInfo.oauthUrl = v + "&lang=" + (g[e] || "en");
        }
        var u,
          l = this,
          f = "Bearer ",
          h = 140,
          p = null,
          m = t.getInstance("ugc-lib/twitchService");
        l.userId = null, l.name = "twitch", l.serviceNames = {
          TWITCH: "Twitch"
        };
        var v =
          "https://id.twitch.tv/oauth2/authorize?response_type=token&client_id={{clientId}}&scope={{scope}}&redirect_uri={{redirectUri}}&force_verify=true";
        l.providerInfo = {
          name: l.name,
          services: {},
          oauthUrl: v,
          clientId: "",
          logoutUrl: "https://www.twitch.tv/logout",
          cookies: {
            ".twitch.tv": ["_twitch_session_id"]
          }
        };
        var g = {
          "cs-CZ": "cs",
          "da-DK": "da",
          "de-DE": "de",
          "el-GR": "el",
          "en-US": "en",
          "en-GB": "en",
          "es-ES": "es",
          "es-MX": "es",
          "fi-FI": "fi",
          "fr-FR": "fr",
          "hu-HU": "hu",
          "it-IT": "it",
          "ja-JP": "ja",
          "ko-KR": "ko",
          "nl-NL": "nl",
          "nb-NO": "no",
          "pl-PL": "pl",
          "pt-PT": "pt",
          "pt-BR": "pt-br",
          "ru-RU": "ru",
          "sk-SK": "sk",
          "sl-SI": void 0,
          "sv-SE": "sv",
          "th-TH": "th",
          "tr-TR": "tr",
          "zh-CHS": "zh-cn",
          "zh-CHT": "zh-tw"
        };
        l.providerInfo.services[l.serviceNames.TWITCH] = {
          id: 1,
          name: l.serviceNames.TWITCH,
          type: 1,
          scope: "user:read:email channel:read:stream_key channel:manage:broadcast"
        }, l.setAuthToken = function(e) {
          p = e, u = f + e.accessToken;
        }, l.getUser = function() {
          return n.getUser({
            authorization: u,
            clientId: l.providerInfo.clientId
          }).then(function(e) {
            return l.userId = e.data.data[0].id, {
              userName: e.data.data[0].display_name,
              avatarUri: e.data.data[0].profile_image_url,
              userId: e.data.data[0].id
            };
          }, function(t) {
            return m.error("getUser failed" + +(0, d.default)(t)), e.reject(t);
          });
        }, l.getRtmpUrl = function(t) {
          var r = n.getStreamKey(l.userId, u, l.providerInfo.clientId);
          return e.all({
            streamKey: r({}),
            ingestServers: n.getIngestServers(),
            selectedIngestServerName: a.twitchIngestServerGetter()
          }).then(function(e) {
            var n = e.streamKey.data.data[0].stream_key,
              r = e.ingestServers.data.ingests;
            m.info("getRtmpUrl ingestServersList : ", r);
            var o = e.selectedIngestServerName,
              a = r.map(function(e) {
                return e.name;
              }),
              c = a.indexOf(o);
            if (c < 0 && (c = l.getClosestIngestServer(r)), c < 0 && (c = l.getDefaultServer(r)),
              m.info("selectedServerIndex : ", c), !r[c]) {
              var u = "getRtmpUrl failed to get ingest servers";
              return m.error(u), void i.trigger(s.BROADCAST_ERROR, {
                serviceName: l.name,
                errorTxt: u
              });
            }
            m.info("Selected Ingest server:", r[c].name);
            var d = r[c].url_template_secure;
            return angular.isDefined(t.title) && null !== t.title && (m.info(
                "sessionParams.title : ", t.title), l.setBroadcastTitle(t.title, null, !0)), d
              .replace("{stream_key}", n);
          }, function(e) {
            var t = "getRtmpUrl failed : " + (0, d.default)(e);
            m.error(t), i.trigger(s.BROADCAST_ERROR, {
              serviceName: l.name,
              errorTxt: t
            });
          });
        }, l.postRtmpProcess = function() {
          return e.reject(!0);
        }, l.stopBroadcast = function() {
          m.info("Stop Broadcast");
        }, l.setBroadcastDescription = function(e) {
          m.info("Set Broadcast Description ", e);
        }, l.getBroadcastTitle = function() {
          var e = n.getChannelInfo(l.userId, u, l.providerInfo.clientId);
          return e({}).then(function(e) {
            return m.info("twitchService.getBroadcastTitle response ", e), e.data.data[0].title;
          });
        }, l.setBroadcastTitle = function(t, r, i) {
          var o = {};
          if (t.length > h && (t = t.substr(0, h)), i) {
            m.info("Broadcast Title is " + t), o.gameTitle = t;
            var a = n.updateChannelInfo(l.userId, o, u, l.providerInfo.clientId);
            return a({}).then(function(e) {
              m.info("Broadcast channel info updated : ", e);
            }, function(e) {
              m.error("Broadcast channel info update failure " + (0, d.default)(e));
            });
          }
          return e.when(!0);
        }, l.getTwitchIngestServerList = function() {
          return n.getIngestServers({
            clientId: l.providerInfo.clientId
          }).then(function(e) {
            var t = e.data.ingests,
              n = t.filter(function(e) {
                return 1 === e.availability;
              });
            return n;
          });
        }, l.getClosestIngestServer = function(e) {
          var t = e;
          t.sort(function(e, t) {
            return parseFloat(e.priority) - parseFloat(t.priority);
          });
          for (var n = t.length, r = 0; r < n; r++)
            if (t[r].availability) return r;
          return 0;
        }, l.getDefaultServer = function(e) {
          for (var t = e.length, n = 0; n < t; n++)
            if (e[n].default && e[n].availability) return n;
          return 0;
        }, l.getCurrentViewerCount = function() {
          var e = n.getStream(l.userId, u, l.providerInfo.clientId);
          return e({}).then(function(e) {
            m.info("twitchService.getCurrentViewerCount response :", e);
            var t = 0;
            return null == e.data.data[0] ? m.info("Twitch: No Stream info") : t = e.data.data[0]
              .viewer_count, m.info("Get Viewer Count success " + t), t;
          }, function(e) {
            m.error("Get Viewer Count error: " + (0, d.default)(e));
          });
        }, l.getBroadcastQuality = function(e) {
          var t = l.providerInfo.services[l.serviceNames.TWITCH].qualityPrefix;
          return "Custom" === e && (e = "UltraGood"), t + e;
        }, l.init = function(e, t) {
          var n = o.getConfig().connect;
          n && n.twitch ? (l.providerInfo.clientId = n.twitch.clientId, n.twitch.config && (m.info(
              "Twitch configuration found, merging with Twitch service config blob."), angular
            .merge(l.providerInfo.services[l.serviceNames.TWITCH], n.twitch.config), h = l
            .providerInfo.services[l.serviceNames.TWITCH].broadcastTitleMaxLength), c(e), i.on(t,
            c)) : m.error(
            "Twitch service initialization failed. No configuration found for Twitch!");
        };
      }
    ]), p.service("connectService", ["$interpolate", "$window", "$q", "eventAggregator", "twitchService",
      "googleService", "facebookService", "imgurService", "sinaService", "shotWithGeForceService",
      "cefService", "nvAccountService", "runTimeConfigService", "CONNECT_EVENTS",
      function(e, t, n, r, i, o, a, s, c, u, l, f, h, p) {
        var m,
          v,
          g,
          y = this,
          b = "oauth-",
          E = "preferred-services",
          $ = "last-used-services",
          w = "last-uploaded-video-type",
          T = "params-",
          C = {};
        y.serviceTypes = {
            ALL: 0,
            STREAMING: 1,
            IMAGE_UPLOAD: 2,
            VIDEO_UPLOAD: 4,
            ANSEL_UPLOAD: 10,
            GIF_UPLOAD: 16
          }, y.providerNames = {
            FACEBOOK: a.name,
            GOOGLE: o.name,
            IMGUR: s.name,
            TWITCH: i.name,
            SINA: c.name,
            NVIDIA: u.name
          }, y.endpoints = {}, y.endpoints[y.providerNames.NVIDIA] = u, y.endpoints[y.providerNames
            .FACEBOOK] = a, y.endpoints[y.providerNames.GOOGLE] = o, y.endpoints[y.providerNames
          .IMGUR] = s, y.endpoints[y.providerNames.TWITCH] = i, y.endpoints[y.providerNames.SINA] = c, y
          .providers = {}, y.services = {}, y.initialize = function(e, n) {
            _.each(y.endpoints, function(t, r) {
              y.providers[r] = t.providerInfo, t.init && t.init(e, n);
            }), _.each(y.providers, function(e, t) {
              _.each(e.services, function(e) {
                e.providerName = t, y.services[e.name] = e;
              });
            });
            var i = t.localStorage.getItem(E);
            m = JSON.parse(i), null === m && (m = {});
            var o = t.localStorage.getItem($);
            v = JSON.parse(o), null === v && (v = {});
            var a = t.localStorage.getItem(w);
            g = JSON.parse(a) || y.serviceTypes.VIDEO_UPLOAD, _.each(y.services, function(e) {
              var t = y.endpoints[e.providerName];
              y.isLoggedIntoService(e.name) && void 0 !== t && t.setAuthToken(y.getAuthToken(e
                  .name), e.name, !0), t.setAuthTokenCallback = y.setAuthToken, t.logoutFunc = y
                .logout;
            }), r.on(p.USER_LOGGED_OUT, y.deleteServiceParameters);
          };
        var x = function(e) {
          return {
            accessToken: e.access_token,
            expiresIn: e.expires_in || 0,
            accountUsername: e.account_username,
            accountId: e.account_id,
            scope: e.scope,
            tokenType: e.token_type,
            refreshToken: e.refresh_token,
            code: e.code,
            idToken: e.id_token,
            connectedOn: new Date(),
            expiresOn: void 0 !== e.expires_in ? new Date(new Date().getTime() + 1e3 * e
              .expires_in) : void 0
          };
        };
        y.isLoggedIntoJarvis = function() {
          return y.getJarvisUserId().then(function(e) {
            return n.when(void 0 !== e);
          });
        }, y.getJarvisUserId = function() {
          return f.getJarvisUserToken().then(function(e) {
            return void 0 === e || null === e || void 0 === e.userInfo || _.isEmpty(e.userInfo) ?
              n.reject("Not logged into Jarvis") : n.when(e.userInfo.userId);
          }, function(e) {
            return n.reject("Could not retrieve Jarvis user token: " + e);
          });
        }, y.isLoggedIntoService = function(e) {
          var t = y.getAuthToken(e);
          return null !== t && void 0 !== t.accessToken;
        }, y.canLoginToService = function(e) {
          return e && !(y.services[e].providerName === y.providerNames.NVIDIA);
        }, y.getAuthToken = function(e) {
          if (angular.isUndefined(e)) return null;
          var n = t.localStorage.getItem(b + e);
          return JSON.parse(n);
        }, y.setAuthToken = function(e, n, i) {
          if (n && void 0 !== n) {
            var o = y.getAuthToken(e),
              a = x(n);
            if (void 0 !== o && null !== o)
              for (var s in o) void 0 === a[s] && (a[s] = o[s]);
            var c = y.endpoints[y.services[e].providerName];
            c.setAuthToken(a, e, void 0, i), t.localStorage.setItem(b + e, (0, d.default)(a)),
              void 0 !== a.accessToken && r.trigger(p.USER_LOGGED_IN, {
                serviceName: e,
                providerName: y.services[e].providerName
              });
          } else y.removeAuthToken(e);
        }, y.removeAuthToken = function(e) {
          t.localStorage.removeItem(b + e), r.trigger(p.USER_LOGGED_OUT, e);
        }, y.logout = function(e) {
          if (y.services[e]) {
            var t = y.endpoints[y.services[e].providerName],
              r = [];
            return _.each(t.providerInfo.cookies, function(e, t) {
              _.each(e, function(e) {
                r.push(l.deleteCookies(t, e));
              });
            }), C[e] = void 0, t.logout && t.logout(), y.removeAuthToken(e), n.all(r);
          }
        }, y.doubleCheckConnection = function(e) {
          e && y.endpoints[y.services[e].providerName].updateAccessToken && y.endpoints[y.services[e]
            .providerName].updateAccessToken();
        }, y.getServices = function(e) {
          if (void 0 === e || null === e || e === y.serviceTypes.ALL) return y.services;
          var t = {};
          return _.each(y.services, function(n) {
            0 !== (n.type & e) && (t[n.name] = n);
          }), t;
        }, y.getOAuthUrl = function(t) {
          var n = y.services[t],
            r = y.providers[n.providerName],
            i = h.getConfig().connect,
            o = i[n.providerName];
          r.redirectUri = o && o.redirectUriOverride ? o.redirectUriOverride : i.redirectUri, n = _
            .omit(n, "provider"), r = _.omit(r, "services");
          var a = angular.merge({}, r, n);
          return e(r.oauthUrl)(a);
        }, y.getLoginTimestamp = function(e) {
          var t = y.getAuthToken(e);
          return void 0 !== t && null !== t ? t.connectedOn : null;
        };
        var S = function(e) {
          if (!y.isLoggedIntoService(e)) return n.reject("Not logged into service " + e);
          var t = n.when(null);
          return y.endpoints[y.services[e].providerName].ensureValidToken && (t = y.endpoints[y
            .services[e].providerName].ensureValidToken(e)), t.then(function() {
            if (void 0 !== C[e] && null !== C[e]) return n.when(C[e]);
            var t = y.endpoints[y.services[e].providerName];
            return void 0 !== t && null !== t ? t.getUser(e).then(function(t) {
              return C[e] = t, t;
            }, function(t) {
              return n.reject("Error retrieving user information for " + e + ": " + t);
            }) : n.reject("Not logged into service " + e);
          });
        };
        y.getAvatarUri = function(e) {
          return S(e).then(function(e) {
            return e.avatarUri;
          }, function(e) {
            return n.reject(e);
          });
        }, y.getUserName = function(e) {
          return S(e).then(function(e) {
            return e.userName;
          }, function(e) {
            return n.reject(e);
          });
        }, y.isValidService = function(e) {
          var t = null;
          t = angular.isObject(e) ? e.name : e;
          var n = _.find(y.services, function(e) {
            return t === e.name;
          });
          return void 0 !== n;
        }, y.getPreferredService = function(e) {
          var t = m[e];
          if (y.isValidService(t) || (t = void 0), void 0 === t) {
            var n = y.getServices(e);
            1 === _.size(n) && (t = _.keys(n)[0]);
          }
          return void 0 === t || angular.isObject(t) ? t : y.services[t];
        }, y.setPreferredService = function(e, n) {
          m[e] = n, t.localStorage.setItem(E, (0, d.default)(m));
        }, y.getDefaultService = function(e) {
          var t = _.find(y.getServices(e), function(t) {
              if (0 !== (t.default & e)) return !0;
            }),
            n = _.find(y.getServices(e), function(e) {
              return !0;
            });
          return t || n;
        }, y.getPreferredServiceOrDefault = function(e) {
          var t = y.getPreferredService(e);
          return void 0 === t ? y.getDefaultService(e) : t;
        }, y.getPreferredServiceOrLastUsed = function(e) {
          var t = y.getPreferredService(e);
          return void 0 === t ? y.getLastUsedService(e) : t;
        }, y.getLastUsedService = function(e) {
          var t = void 0;
          return _.has(v, e) && (t = v[e], y.isValidService(t) || (e === y.serviceTypes
            .ANSEL_UPLOAD && (t = v[y.serviceTypes.IMAGE_UPLOAD]), y.isValidService(t) || (t =
              void 0))), void 0 === t || angular.isObject(t) ? t : y.services[t];
        }, y.setLastUsedService = function(e, n) {
          v[e] = n, v[y.serviceTypes.ALL] = n, t.localStorage.setItem($, (0, d.default)(v));
        }, y.getLastUsedServiceOrDefault = function(e, t) {
          var n = y.getLastUsedService(e);
          return void 0 === n ? y.getDefaultService(e) : n;
        }, y.setLastUploadedVideoType = function(e) {
          g = e || y.serviceTypes.VIDEO_UPLOAD, t.localStorage.setItem(w, (0, d.default)(e));
        }, y.getLastUploadedVideoTypeOrDefault = function() {
          return g;
        }, y.isUploadTargetServiceKnown = function(e) {
          var t = "image" === e ? y.getPreferredService(y.serviceTypes.IMAGE_UPLOAD) : y
            .getPreferredService(y.serviceTypes.VIDEO_UPLOAD);
          return void 0 !== t;
        }, y.setServiceParameters = function(e, n) {
          angular.isDefined(e.privacy) && (e.privacy = e.privacy.value), angular.isDefined(e
            .destinationType) && (e.destinationType = e.destinationType.name), angular.isDefined(e
            .destination) && (e.destination = e.destination.name), t.localStorage.setItem(T + n, (0,
            d.default)(e));
        }, y.deleteServiceParameters = function(e) {
          t.localStorage.removeItem(T + e);
        }, y.getServiceParameters = function(e) {
          var n = void 0,
            r = y.services[e];
          if (angular.isDefined(r)) {
            var i = t.localStorage.getItem(T + e);
            void 0 !== i && null !== i && (n = JSON.parse(i));
          }
          return angular.isUndefined(n) && (n = {}), angular.isDefined(n.privacy) && (n.privacy = _
            .findWhere(r.privacyOptions, {
              value: n.privacy
            })), angular.isUndefined(n.privacy) && (n.privacy = _.findWhere(r.privacyOptions, {
            default: !0
          })), angular.isDefined(n.destinationType) && (n.destinationType = _.findWhere(r
            .destinationOptions, {
              name: n.destinationType
            })), angular.isUndefined(n.destinationType) && (n.destinationType = _.find(r
            .destinationOptions,
            function() {
              return !0;
            })), n;
        };
      }
    ]), p.service("uploadManagerService", ["$log", function(e) {
      var t = this;
      t.uploadQueue = [], t.uploadInProgress = !1;
      var n = e.getInstance("ugc-lib/uploadManagerService");
      t.queueUpload = function(e) {
        t.uploadQueue.push(e), t.uploadInProgress || t.processUploadQueue();
      }, t.processUploadQueue = function() {
        n.info("Processing Upload Queue. Current queue size: ", t.uploadQueue.length);
        var e = t.uploadQueue.pop();
        e ? (n.info("Processing Upload Promise. Remaining queue size: ", t.uploadQueue.length), t
          .uploadInProgress = !0, e().then(function() {
            n.info("Processing remaining Upload Queue. Remaining queue size: ", t.uploadQueue
              .length), t.processUploadQueue();
          })) : (t.uploadInProgress = !1, n.info("Upload Queue empty. Done processing uploads."));
      }, t.init = function() {
        return n.info("Upload Manager Service initialized!"), !0;
      };
    }]), p.service("uploadService", ["$q", "$log", "$document", "cefService", "connectService",
      "eventAggregator", "uploadManagerService", "hardwareConfigWrapper", "recordingPathConfigWrapper",
      "galleryEndpoints", "GALLERY_EVENTS", "GALLERY_TYPES", "GALLERY_SUBTYPES", "DEFAULT_VIDEO_TAGS",
      "UGC_NOTIFICATION_EVENTS",
      function(e, t, n, r, i, o, a, s, c, u, l, d, f, h, p) {
        function m(e, t) {
          return n.on("dragover", function(e) {
            e.stopPropagation(), e.preventDefault(), e.dataTransfer.dropEffect = "copy";
          }), n.on("drop", function(r) {
            if (r.stopPropagation(), r.preventDefault(), !r.dataTransfer.files || !r.dataTransfer
              .files[0]) return void g.info("bad drop, no file list");
            var i = r.dataTransfer.files[0];
            i.name === e ? t(i) : g.info("bad drag file: " + e + " " + i.name), n.off("dragover"), n
              .off("drop");
          }), r.oscCreateDropUrl(e, 0, 0);
        }
        var v = this,
          g = t.getInstance("ugc-lib/uploadService");
        v.retryAttemptsDefault = 3, v.uploadContent = function(t, n) {
          g.info("UploadContent Entered");
          var r = t.uploadService,
            a = e.when(t.fileName),
            y = t.cleanupPromise || e.defer();
          return n = n || v.retryAttemptsDefault, t.type === d.IMAGE && t.fileSize > r.maxImageSize &&
            (a = c.recordingPathGetter().then(function(n) {
              return u.transcodeMediaFile({}, {
                file: t.fileName,
                maxFileSizeMB: r.maxImageSize,
                targetPath: n.tempFiles
              }).then(function(t) {
                return y.promise.then(function() {
                  u.removeGalleryItem({}, {
                    file: t.data.newFile
                  });
                }), e.when(t.data.newFile);
              });
            })), a.then(function(a) {
              return m(a, function(a) {
                var c = e.when(null);
                return t.type === d.VIDEO && (c = s.hardwareSystemDescriptionGetter()), c.then(
                  function(s) {
                    var c = t.type !== d.IMAGE && (t.subtype !== f.GIF || r.gifOptions && r
                        .gifOptions.shouldUploadGifAsVideo),
                      u = [];
                    u = h, u.push("#" + t.folder.replace(/\s+/g, ""));
                    var m = {
                      file: a,
                      filepath: t.fileName,
                      name: c ? void 0 : t.videoUploadTitle,
                      subtype: t.subtype,
                      title: c ? t.videoUploadTitle : void 0,
                      description: c ? s : void 0,
                      tags: c ? u : void 0,
                      categoryId: c ? "20" : void 0,
                      privacy: t.privacy,
                      destination: t.destination,
                      originalSource: t.fullFilename
                    };
                    t.subtype !== f.GIF && o.trigger(p.UGC_NOTIFICATION, {
                      eventName: p.UPLOAD_STARTED,
                      data: r.title
                    });
                    var g = c ? i.endpoints[r.providerName].uploadVideo : i.endpoints[r
                        .providerName].uploadImage,
                      b = {
                        newUpload: !0,
                        retryCount: n,
                        tryUploadPromise: e.defer()
                      };
                    v.tryUpload(m, r, g, b).then(function(e) {
                      "success" === e && v.notifyUploadResult(p.UPLOAD_SUCCESS, r.title);
                    }, function(e) {
                      v.notifyUploadResult(p.UPLOAD_FAILED, r.title), t.error = e;
                    }).then(function() {
                      t.retryCount = n - b.retryCount, o.trigger(l.UPLOAD_COMPLETE, t), y
                        .resolve();
                    });
                  });
              });
            });
        }, v.tryUpload = function(e, t, n, r) {
          var i = function() {
            return n(e, t).then(function() {
              g.info("Upload successful."), r.tryUploadPromise.resolve("success");
            }, function(i) {
              r.retryCount > 0 ? (g.info("Upload retry: attempts remaining - ", r.retryCount), r
                .retryCount--, v.tryUpload(e, t, n, r)) : (g.info("Upload failed."), r
                .tryUploadPromise.reject("failed"));
            });
          };
          if (a.queueUpload(i), r.newUpload) return r.newUpload = !1, r.tryUploadPromise.promise;
        }, v.notifyUploadResult = function(e, t) {
          o.trigger(p.UGC_NOTIFICATION, {
            eventName: e,
            data: t
          });
        };
      }
    ]), p.service("galleryService", ["$q", "$log", "$timeout", "$document", "galleryEndpoints",
      "cacheService", "eventAggregator", "GALLERY_STATE", "GALLERY_TYPES", "GALLERY_AUDIOTYPES",
      "GALLERY_SUBTYPES", "GALLERY_EVENTS",
      function(e, t, n, r, i, o, a, s, c, u, l, d) {
        var f = this,
          h = t.getInstance("ugc-lib/galleryService");
        f.populatedDirs = [], f.populatedDirsPost = [], f.populatedRecentFilesFolder = [], f
          .recentFiles = [], f.videoPath = "", f.tempPath = "", f.uploadHistory = [], f.wasCached = !1,
          f.thumbSizes = {
            width: 90,
            height: 90
          }, f.EXCLUDE_HIDDEN = "hidden", f.EXCLUDE_EMPTY = "empty", f.EXCLUDE_HIDDENOREMPTY =
          "hiddenOrEmpty", f.EXCLUDE_HIDDENANDEMPTY = "hiddenAndEmpty", o.initialize(), f
          .getPopulateFolderReference = function() {
            return o.getCurrentPopFolder();
          }, f.getFilesReference = function() {
            return o.files;
          }, f.checkCachedData = function(e) {
            return o.getCachedDataCRC(e);
          }, f.active = function(e) {
            o.galleryActive(e);
          }, f.getGalleryFolderListing = function(e, t, n, r) {
            return i.getGalleryFolderListing({}, {
              directory: e,
              shouldWatch: t,
              shouldGetOnlyNv: !!angular.isDefined(n) && n,
              excludeDirectoryType: angular.isDefined(r) ? r : f.EXCLUDE_HIDDENOREMPTY,
              shouldShowEXR: !0
            }).then(function(e) {
              return e.data;
            }, function(e) {
              return h.error("getGalleryFolderListing failed: ", e), 0;
            });
          }, f.enumerateDrives = function() {
            return i.enumerateDrives().then(function(e) {
              return e.data;
            }, function(e) {
              return h.error("enumerateDrives failed: ", e), 0;
            });
          }, f.getGalleryStats = function(e, t, n, r, o) {
            return i.getGalleryStats({}, {
              directory: e,
              width: t,
              height: n,
              shouldGetOnlyNv: !!angular.isDefined(r) && r,
              quickCheck: !!angular.isDefined(o) && o,
              shouldShowEXR: !0
            }).then(function(e) {
              return e.data;
            }, function(e) {
              return h.error("getGalleryStats failed: ", e), 0;
            });
          }, f.getGalleryRecent = function() {
            return i.getGalleryRecent().then(function(e) {
              return h.info("Gallery Recent files: ", e.data.files), e.data.files;
            }, function(e) {
              return h.error("getGalleryRecent failed: ", e), 0;
            });
          }, f.removeGalleryItem = function(t, n) {
            var n = n || !1;
            return i.removeGalleryItem({}, {
              file: t,
              forceDelete: n
            }).then(function(e) {
              return h.info("Gallery removeGalleryItem success"), e.data;
            }, function(t) {
              return h.error("removeGalleryItem failed: ", t), e.reject();
            });
          }, f.getGalleryFileMetaData = function(e, t, n) {
            return i.getGalleryFileMetaData({}, {
              file: e,
              width: t,
              height: n
            }).then(function(e) {
              return e.data;
            }, function(e) {
              return h.error("getGalleryFileMetaData failed: ", e), 0;
            });
          }, f.getGalleryFileMetaDataNoThumbnail = function(e) {
            return i.getGalleryFileMetaDataNoThumbnail({}, {
              file: e
            }).then(function(e) {
              return e.data;
            }, function(e) {
              return h.error("getGalleryFileMetaDataNoThumbnail failed: ", e), 0;
            });
          }, f.getGalleryImageFileDimensions = function(e) {
            return i.getGalleryImageFileDimensions({}, {
              file: e
            }).then(function(e) {
              return e.data;
            });
          }, f.getGalleryThumbnail = function(e, t) {
            return void 0 === t && (t = f.thumbSizes), i.getGalleryThumbnail({}, {
              file: e,
              width: t.width,
              height: t.height
            }).then(function(e) {
              return e.data;
            }, function(e) {
              return h.error("getGalleryThumbnail failed: ", e), 0;
            });
          }, f.getGalleryThumbnailUploadHistory = function(e, t, n) {
            return i.getGalleryThumbnail({}, {
              file: e.replace(/\//g, "\\"),
              width: t.width,
              height: t.height
            }).then(function(e) {
              return f.uploadHistory[n].thumbnail = e.data.thumbnail, e.data.thumbnail;
            }, function(e) {
              var t = null;
              return f.uploadHistory[n].thumbnail = t, h.error(
                "GalleryThumbnailUploadHistory failed: ", e), t;
            });
          }, f.getGalleryUploadHistory = function() {
            return f.uploadHistory.length = 0, i.getGalleryUploadHistory().then(function(e) {
              return void 0 !== e.data.items && (f.uploadHistory = e.data.items), f.uploadHistory;
            }, function(e) {
              return h.error("getGalleryUploadHistory failed: ", e), 0;
            });
          }, f.appendProperThumbs = function() {
            function t() {
              r.resolve(!0);
            }
            var n = f.thumbSizes;
            if (0 === f.uploadHistory.length) return !0;
            var r = e.defer(),
              i = [];
            return _.each(f.uploadHistory, function(e, t) {
              var r = void 0 === e.thumbnail ? e.path : e.thumbnail;
              i.push(f.getGalleryThumbnailUploadHistory(r, n, t));
            }), e.all(i).then(t), r.promise;
          }, f.getUploadHistory = function() {
            return f.getGalleryUploadHistory().then(function(e) {
              h.info("Upload history: ", e);
            }).then(function() {
              return f.appendProperThumbs();
            }).then(function() {
              return f.uploadHistory;
            });
          }, f.addGalleryUploadHistoryItem = function(e, t, n, r, o, a, s) {
            return i.addGalleryUploadHistoryItem({}, {
              name: e,
              date: t,
              time: n,
              type: r,
              subtype: o,
              url: a,
              path: s
            }).then(function(e) {
              return h.info("Gallery addUploadHistoryItem success"), e.data;
            }, function(e) {
              return h.error("addGalleryUploadHistoryItem failed: ", e), 0;
            });
          }, f.removeGalleryUploadHistoryItem = function(e) {
            return i.removeGalleryUploadHistoryItem({}, {
              url: e
            }).then(function(e) {
              return h.info("Gallery removeUploadHistoryItem success"), e.data;
            }, function(e) {
              return h.error("removeGalleryUploadHistoryItem failed: ", e), 0;
            });
          }, f.clearUploadHistory = function() {
            return i.clearUploadHistory().then(function(e) {
              return h.info("Clear Upload History success"), e;
            }, function(e) {
              return h.error("clearUploadHistory failed: ", e), 0;
            });
          }, f.setRecordingPaths = function(e) {
            if ("" === f.videoPath) f.videoPath = e.videos, f.tempPath = e.tempFiles, o
              .setRecordingPaths(f.videoPath);
            else if (e.videos !== f.videoPath) return h.info("Recording path changed: ", e), i
              .clearRecentHistory().then(function() {
                f.videoPath = e.videos, f.tempPath = e.tempFiles, o.setRecordingPaths(f.videoPath);
              }, function(e) {
                return h.error("setRecordingPaths failed: ", e), 0;
              });
          }, f.getData = function(e, t) {
            var n = !angular.isDefined(t) || t;
            return f.getGalleryFolderListing(e, !1, n).then(function(e) {
              o.files = e.files, o.directories = e.directories;
            });
          }, f.getStats = function(e, t, n) {
            var r = f.thumbSizes,
              i = o.directories[t],
              a = e + "\\" + i,
              s = n ? r.width : 0,
              c = n ? r.height : 0;
            return f.getGalleryStats(a, s, c, !0, n).then(function(r) {
              if (0 !== r.videos || 0 !== r.screenshots) {
                var o = void 0 === r.thumbnail ? null : r.thumbnail,
                  a = n ? f.populatedDirs : f.populatedDirsPost;
                a.push({
                  path: e,
                  folder: i,
                  index: t,
                  videos: n ? "" : r.videos,
                  screenshots: n ? "" : r.screenshots,
                  data: o
                });
              }
            });
          }, f.getFolderStats = function(e) {
            var t = f.thumbSizes;
            return f.getGalleryStats(e, t.width, t.height, !0, !1).then(function(e) {
              return e;
            });
          }, f.saveMetaData = function(e, t, n, r, i, a, s, d) {
            var f,
              h = void 0 === a.duration ? "No Data" : a.duration,
              p = void 0 === a.date ? "05/13/1959 12:00:00 AM" : a.date,
              m = void 0 === a.fileSizeMB ? "200" : a.fileSizeMB,
              v = void 0 === a.audiotype ? u.UNKNOWN : a.audiotype,
              g = void 0 === a.hevc ? "0" : a.hevc;
            f = !o.files || void 0 === o.files[r] || s ? {
              name: n,
              type: a.fileType,
              subtype: a.fileSubType,
              source: a.fileSource
            } : o.files[r], f.type === c.VIDEO && f.subtype === l.NORMAL && v === u.SEPARATE && (f
              .subtype = l.MTA), f.DRSName = a.DRSName, f.DRSProfileName = a.DRSProfileName;
            var y = void 0 === a.thumbnail ? null : a.thumbnail;
            void 0 === d ? i[r] = {
              fullFilename: e,
              file: f,
              folder: t,
              index: r,
              duration: h,
              date: p,
              fileSize: m,
              audiotype: v,
              hevc: g,
              data: y
            } : i.push({
              fullFilename: e,
              file: f,
              folder: t,
              index: r,
              duration: h,
              date: p,
              fileSize: m,
              audiotype: v,
              data: y,
              hlIndex: d
            });
          }, f.getMetaData = function(e, t, n, r, i, o, a, s) {
            var c = f.thumbSizes;
            return o === !0 ? f.getGalleryFileMetaDataNoThumbnail(e).then(function(o) {
              0 !== o && f.saveMetaData(e, t, n, r, i, o, a, s);
            }) : f.getGalleryFileMetaData(e, c.width, c.height).then(function(o) {
              0 !== o && f.saveMetaData(e, t, n, r, i, o, a, s);
            });
          }, f.getThumbnail = function(e, t, n, r, i) {
            var o = f.thumbSizes;
            return f.getGalleryThumbnail(e, o).then(function(o) {
              var a = void 0 === o.thumbnail ? null : o.thumbnail;
              i[r] = {
                fullFilename: e,
                file: n,
                folder: t,
                index: r,
                data: a
              };
            });
          }, f.getUsableData = function(e) {
            h.info("GetUsableData");
            var t = f.videoPath;
            return o.useCacheData(t) ? (h.info("GetUsableData - Using cached data"), f.wasCached = !0,
              0 !== f.populatedDirs.length) : (f.wasCached = !1, e ? (f.populatedDirs.length = 0, f
              .getData(t).then(function() {
                return f.getDirectoryData(t, e);
              })) : (f.populatedDirsPost.length = 0, f.getDirectoryData(t, e)));
          }, f.sortData = function(e) {
            return e = _.sortBy(e, function(e) {
              return e.folder.toLowerCase();
            }), _.each(e, function(e, t) {
              e.index = t;
            }), e;
          }, f.getDirectoryData = function(t, n) {
            function r() {
              n ? f.populatedDirs = f.sortData(f.populatedDirs) : (f.populatedDirsPost = f.sortData(f
                .populatedDirsPost), _.each(f.populatedDirs, function(e, t) {
                e.videos = f.populatedDirsPost[t] ? f.populatedDirsPost[t].videos : 0, e
                  .screenshots = f.populatedDirsPost[t] ? f.populatedDirsPost[t].screenshots : 0;
              }), f.populatedDirsPost.length = 0, o.setCacheNotDirty(t)), i.resolve(0 !== f
                .populatedDirs.length), h.info("GetUsableData - Fetched data");
            }
            var i = e.defer(),
              a = [];
            return _.each(o.directories, function(e, r) {
              a.push(f.getStats(t, r, n));
            }), e.all(a).then(r), i.promise;
          }, f.getRawFolderData = function(t, n) {
            h.info("getRawFolderData");
            var r = f.videoPath + "\\" + t.folder,
              i = !angular.isDefined(n) || n;
            return o.useCacheData(r, !0) ? (f.wasCached = !0, h.info(
              "getRawFolderData - Using cached data"), e.when(!0)) : (f.wasCached = !1, o
              .resetCurrentPopFolder(), f.getGalleryFolderListing(r, !1, i).then(function(e) {
                void 0 !== e && (o.files = e.files, o.directories = e.directories, h.info(
                  "Folder data received"), o.setCacheLoadedPartial(r));
              }));
          }, f.getFolderData = function(t, n, r, i) {
            function a() {
              o.files.length === l.length && (h.info("getFolderData folder loaded!!"), o.setCacheLoaded(
                c)), d.resolve(!0);
            }
            var s = t.folder,
              c = f.videoPath + "\\" + s,
              u = Math.min(o.files.length, n + r);
            if (o.useCacheData(c, !1)) return f.wasCached = !0, e.when(!0);
            f.wasCached = !1;
            for (var l = o.getCurrentPopFolder(), d = e.defer(), p = [], m = n; m < u; m++) {
              var v = o.files[m],
                g = c + "\\" + v.name;
              p.push(f.getMetaData(g, s, v, m, l, i, !1));
            }
            return e.all(p).then(a), d.promise;
          }, f.getRecentFiles = function() {
            return f.getGalleryRecent().then(function(e) {
              if (f.recentFiles = e, h.info("recentFiles: ", f.recentFiles), f
                .populatedRecentFilesFolder.length = 0, 0 !== f.recentFiles.length) return f
                .getRecentData();
            });
          }, f.extractGameName = function(e) {
            var t = e.split("\\");
            return t.length < 2 ? (h.error("extractGameName: Cannot parse!"), "Unknown Game") : t[t
              .length - 2];
          }, f.getRecentData = function() {
            function t() {
              f.populatedRecentFilesFolder = _.compact(f.populatedRecentFilesFolder), f
                .populatedRecentFilesFolder = _.map(f.populatedRecentFilesFolder, function(e, t) {
                  var n = e;
                  return n.index = t, n;
                }), _.each(f.populatedRecentFilesFolder, function(e, t) {
                  h.info(e.index + " " + e.file);
                }), n.resolve(!0);
            }
            var n = e.defer(),
              r = [];
            return _.each(f.recentFiles, function(e, t) {
              var n = e,
                i = f.extractGameName(n);
              r.push(f.getMetaData(n, i, n, t, f.populatedRecentFilesFolder, !1, !0));
            }), e.all(r).then(t), n.promise;
          }, f.copyFile = function(e, t) {
            return i.copyFile({}, {
              source: e,
              destination: t
            }).then(function(e) {
              return h.info("copyFile success"), !0;
            }, function(e) {
              return h.error("copyFile failed: ", e), !1;
            });
          }, f.isDirectoryWritable = function(e) {
            return h.info("isDirectoryWritable Folder:", e), i.isDirectoryWritable({}, {
              directory: e
            }).then(function(e) {
              return h.info("isDirectoryWritable: ", e.data.writable), e.data.writable;
            }, function(e) {
              return h.error("isDirectoryWritable failed: ", e), !1;
            });
          }, f.writeEncryptedBmp = function(e) {
            return i.writeEncryptedBmp({}, {
              bitmapImage: e
            }).then(function(e) {
              return h.info("writeEncryptedBmp passed!"), !0;
            }, function(e) {
              return h.error("writeEncryptedBmp failed!"), !1;
            });
          };
      }
    ]), p.service("cacheService", ["$log", "$interval", "galleryEndpoints", function(e, t, n) {
      var r = this,
        i = e.getInstance("ugc-lib/cacheService");
      r.videoPath = "", r.files = [], r.directories = [], r.populatedFolder = [
          [],
          [],
          [],
          [],
          []
        ], r.folderData = [], r.fileData = [], r.mainCache = [], r.folderCache = [], r.cacheEntries =
        5, r.initialized = !1, r.ageCounter = 0, r.dataIndex = 0, r.freeInterval = 3e5, r
        .minEntryAge = 12e5, r.active = !1;
      var o = {
        EMPTY: 0,
        PARTIAL: 1,
        FULL: 2
      };
      r.resetCacheEntry = function(e) {
        r.folderCache[e] = {
          dirty: !0,
          crc: 0,
          path: "",
          age: 0,
          time: 0,
          loaded: o.EMPTY
        };
      }, r.initialize = function() {
        if (i.info("cacheService initialize"), !r.initialized) {
          r.mainCache[0] = {
            dirty: !0,
            crc: 0,
            path: ""
          };
          for (var e = 0; e < r.cacheEntries; e++) r.resetCacheEntry(e);
          r.cacheTimerOn(), r.initialized = !0, i.info("cacheService initialize complete");
        }
      }, r.flush = function() {
        i.info("cacheService flush"), r.initialized = !1, r.initialize();
      }, r.galleryActive = function(e) {
        r.active = e;
      }, r.cacheTimerOn = function() {
        angular.isDefined(r.cacheTimer) || (r.cacheTimer = t(function() {
          if (!r.active) {
            for (var e = 2147483647, t = -1, n = new Date(), i = n.getTime(), a = 0; a < r
              .cacheEntries; a++) {
              var s = r.folderCache[a].time,
                c = i - s;
              0 !== r.folderCache[a].age && r.folderCache[a].age < e && c > r.minEntryAge && (
                t = a, e = r.folderCache[a].age);
            }
            0 !== e && t !== -1 && (r.folderData[t].directories.length = 0, r.folderData[t]
              .files.length = 0, r.folderCache[t].loaded === o.FULL && (r.fileData[t]
                .popFolder.length = 0), r.resetCacheEntry(t));
          }
        }, r.freeInterval));
      }, r.cacheTimerOff = function() {
        angular.isDefined(r.cacheTimer) && (t.cancel(r.cacheTimer), r.cacheTimer = void 0);
      }, r.setRecordingPaths = function(e) {
        r.videoPath = e;
      }, r.getCurrentPopFolder = function() {
        return r.populatedFolder[r.dataIndex];
      }, r.resetCurrentPopFolder = function() {
        r.populatedFolder[r.dataIndex].length = 0;
      }, r.useCacheData = function(e, t) {
        var n = !1;
        if (e !== r.videoPath) {
          var i = _.findIndex(r.folderCache, {
            path: e
          });
          if (i === -1) return !1;
          n = !r.folderCache[i].dirty && r.folderCache[i].path === e;
          var a = n && r.folderCache[i].loaded !== o.NONE,
            s = n && r.folderCache[i].loaded === o.FULL;
          r.dataIndex = i;
          var c = new Date(),
            u = c.getTime();
          return t ? (a && (r.files = r.folderData[i].files, r.directories = r.folderData[i]
            .directories, r.folderCache[i].time = u), a) : (s && (r.populatedFolder[i] = r
            .fileData[i].popFolder, r.folderCache[i].time = u), s);
        }
        return n = !r.mainCache[0].dirty && r.mainCache[0].path === e;
      }, r.setCacheNotDirty = function(e) {
        if (e === r.videoPath) r.mainCache[0].dirty = !1;
        else {
          var t = _.findIndex(r.folderCache, {
            path: e
          });
          t !== -1 && (r.folderCache[t].dirty = !1);
        }
      }, r.setCacheLoadedPartial = function(e) {
        var t = _.findIndex(r.folderCache, {
          path: e
        });
        t !== -1 && (r.folderCache[t].loaded = o.PARTIAL, r.folderData[t] = {
          files: r.files,
          directories: r.directories
        });
      }, r.setCacheLoaded = function(e) {
        var t = _.findIndex(r.folderCache, {
          path: e
        });
        t !== -1 && (r.folderCache[t].loaded = o.FULL, r.fileData[t] = {
          popFolder: r.populatedFolder[t]
        });
      }, r.getCachedDataCRC = function(e) {
        void 0 === e && (e = r.videoPath);
        var t = -1;
        if (e === r.videoPath) return r.getGalleryFolderCRC(e).then(function(e) {
          var t = r.mainCache[0].crc !== e.crc;
          r.mainCache[0] = {
            dirty: t,
            crc: e.crc,
            path: r.videoPath
          };
        });
        if (t = _.findIndex(r.folderCache, {
            path: e
          }), t === -1 && (t = _.findIndex(r.folderCache, {
            crc: 0
          })), t === -1)
          for (var n = 2147483647, a = 0; a < r.cacheEntries; a++) r.folderCache[a].dirty && r
            .folderCache[a].age < n && (t = a, n = r.folderCache[a].age);
        if (t === -1)
          for (var n = 2147483647, a = 0; a < r.cacheEntries; a++) r.folderCache[a].age < n && (t =
            a, n = r.folderCache[a].age);
        return t >= r.cacheEntries && i.error("folder cache has bad indexing!"), r
          .getGalleryFolderCRC(e).then(function(n) {
            var i = r.folderCache[t].crc !== n.crc,
              a = r.folderCache[t].loaded === o.EMPTY,
              s = i || a;
            r.ageCounter++;
            var c = new Date(),
              u = c.getTime();
            r.folderCache[t] = {
              dirty: s,
              crc: n.crc,
              path: e,
              age: r.ageCounter,
              time: u,
              loaded: i ? o.EMPTY : r.folderCache[t].loaded
            };
          });
      }, r.getGalleryFolderCRC = function(e) {
        return n.getGalleryFolderCRC({}, {
          directory: e
        }).then(function(e) {
          return i.info("Gallery folder CRC: ", e.data.crc), e.data;
        }, function(e) {
          return i.error("getGalleryFolderCRC failed: ", e), 0;
        });
      };
    }]);
  }).call(exports, require(165).Buffer);
}
