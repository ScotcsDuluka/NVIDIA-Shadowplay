// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 123
// factory JarvisSessionManager | provider jarvis | constant JARVIS_CONSTANTS | defines angular.module("nvAngularJarvisSdk")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";
  angular.module("nvAngularJarvisSdk", ["nvAngularHttpEndpoint", "nvAngularAccountSdk", "base64"]), angular
    .module("nvAngularJarvisSdk").provider("jarvis", [function() {
      var e,
        t,
        n,
        r,
        i,
        o,
        a = 1,
        s = "https://partner-auth.nvgs.nvidia.com",
        c = "anything",
        u = "94211403124047873",
        l = 0,
        d = 0;
      return {
        setConfig: function(f) {
          a = f.version, s = f.server, e = f.redirectUrl, t = f.redirectUrlJarvisOauth, i = {
              "Accept-Language": f.locale ? f.locale : "en-US"
            }, f.magic && (i.MagicHeader = f.magic), c = f.deviceId, n = f.deviceDescription, u = f
            .clientId, r = f.clientDescription, o = f.defaultTimeout, l = f.defaultRetries || l, d = f
            .defaultTimeBetweenRetries || d;
        },
        $get: ["NvEndpointFactory", "$window", "JARVIS_CONSTANTS", function(f, h, p) {
          function m(e) {
            return h.btoa(h.unescape(h.encodeURIComponent(e)));
          }

          function v(e) {
            return e = e || "", {
              getAll: y.createEndpoint({
                url: "/datastore/user/shared/:clientId/client",
                method: "GET",
                authType: "sessionToken",
                params: {
                  clientId: e
                }
              }),
              get: y.createEndpoint({
                url: "/datastore/user/shared/:clientId/client/:blockKey",
                method: "GET",
                authType: "sessionToken",
                params: {
                  clientId: e,
                  blockKey: ""
                }
              }),
              search: y.createEndpoint({
                url: "/datastore/user/shared/:clientId/client/list/:blockKey/search?q=:query",
                method: "GET",
                authType: "sessionToken",
                params: {
                  clientId: e,
                  blockKey: "",
                  query: ""
                }
              }),
              updateMultiple: y.createEndpoint({
                url: "/datastore/user/shared/:clientId/client",
                method: "PUT",
                authType: "sessionToken",
                params: {
                  clientId: e
                }
              }),
              update: y.createEndpoint({
                url: "/datastore/user/shared/:clientId/client/:blockKey",
                method: "PUT",
                authType: "sessionToken",
                params: {
                  clientId: e,
                  blockKey: ""
                }
              }),
              modify: y.createEndpoint({
                url: "/datastore/user/shared/:clientId/client/:blockKey/modify",
                method: "POST",
                authType: "sessionToken",
                params: {
                  clientId: e,
                  blockKey: ""
                },
                data: {
                  operators: []
                }
              })
            };
          }

          function g(e) {
            s = e;
          }
          var y = new f();
          y.setUrlGenerator(function(e, t) {
            return e.url.startsWith("/dev/") || e.url.startsWith("/ping") ? s + e.url : s +
              "/api/" + a + e.url;
          }), y.setHeaderGenerator(function(e, t) {
            var n = angular.merge({}, e.headers, i),
              r = "";
            return e.authType && ("userCredentials" === e.authType ? t.accessToken ? (r = m(t
              .accessToken + ":"), delete t.accessToken) : t.username && t.password && (r = m(
              t.username + ":" + t.password), delete t.username, delete t.password) : (r = m(
              t[e.authType] + ":"), delete t[e.authType]), n.Authorization = "Basic " + r), n;
          }), y.setDefaultTimeout(o), y.setDefaultRetries(l), y.setDefaultTimeBetweenRetries(d);
          var b = {
              get: y.createEndpoint({
                url: "/profile/user/:userId",
                method: "GET",
                authType: "sessionToken",
                params: {
                  userId: ""
                }
              }),
              create: y.createEndpoint({
                url: "/profile/user",
                method: "POST",
                data: {
                  email: "",
                  birthdate: "",
                  displayName: ""
                }
              }),
              currentUser: {
                get: y.createEndpoint({
                  url: "/profile/user",
                  method: "GET",
                  authType: "sessionToken"
                }),
                update: y.createEndpoint({
                  url: "/profile/user",
                  method: "PUT",
                  authType: "sessionToken"
                }),
                getPrivacy: y.createEndpoint({
                  url: "/profile/user/privacy",
                  method: "GET",
                  authType: "sessionToken"
                })
              },
              email: {
                requestVerify: y.createEndpoint({
                  url: "/profile/user/email/requestverify",
                  method: "POST",
                  data: {
                    email: ""
                  }
                }),
                verify: y.createEndpoint({
                  url: "/profile/user/email/verify",
                  method: "POST",
                  data: {
                    token: ""
                  }
                }),
                change: y.createEndpoint({
                  url: "/profile/user/email",
                  method: "POST",
                  authType: "sessionToken",
                  data: {
                    password: "",
                    email: "new_email_address"
                  }
                })
              },
              password: {
                resetRequest: y.createEndpoint({
                  url: "/profile/user/password/reset",
                  method: "POST",
                  data: {
                    email: ""
                  }
                }),
                update: y.createEndpoint({
                  url: "/profile/user/password",
                  method: "POST",
                  authType: "sessionToken",
                  data: {
                    token: "",
                    currentPassword: "",
                    newPassword: ""
                  }
                })
              },
              authenticators: {
                get: y.createEndpoint({
                  url: "/profile/user/authenticators",
                  method: "GET",
                  authType: "sessionToken"
                }),
                add: y.createEndpoint({
                  url: "/profile/user/authenticators",
                  method: "POST",
                  authType: "sessionToken",
                  data: {
                    type: "",
                    value: ""
                  }
                }),
                delete: y.createEndpoint({
                  url: "/profile/user/authenticators/:authenticatorId",
                  method: "DELETE",
                  authType: "sessionToken",
                  params: {
                    authenticatorId: ""
                  }
                }),
                requestVerify: y.createEndpoint({
                  url: "/profile/user/authenticators/requestVerification/:authenticatorId",
                  method: "POST",
                  authType: "sessionToken",
                  params: {
                    authenticatorId: ""
                  }
                }),
                verify: y.createEndpoint({
                  url: "/profile/user/authenticators/verify",
                  method: "POST",
                  authType: "sessionToken",
                  data: {
                    challengeId: "",
                    type: "",
                    value: ""
                  }
                })
              }
            },
            E = {
              login: y.createEndpoint({
                url: "/authentication/user/login",
                method: "POST",
                authType: "userCredentials",
                data: {
                  clientId: u,
                  clientDescription: r,
                  deviceId: c,
                  deviceDescription: n
                }
              }),
              logout: y.createEndpoint({
                url: "/authentication/user/logout",
                method: "POST",
                authType: "userToken"
              }),
              oauth: y.createEndpoint({
                url: "/authentication/external/:provider",
                method: "GET",
                params: {
                  data: {
                    redirect_url: e,
                    clientId: u,
                    clientDescription: r,
                    deviceId: c,
                    deviceDescription: n,
                    state: void 0,
                    prompt: "Select"
                  }
                }
              }),
              loginWithOauth: y.createEndpoint({
                url: "/oauth/authorize",
                method: "GET",
                params: {
                  response_type: "code",
                  scope: "user_token",
                  client_id: u,
                  redirect_uri: t,
                  prompt: "login"
                }
              }),
              loginWithOauthResource: y.createEndpoint({
                url: "/oauth/resource",
                method: "POST",
                data: {
                  grant_type: "authorization_code",
                  scope: "Any",
                  code: "",
                  redirect_uri: t,
                  client_id: u
                }
              }),
              devices: y.createEndpoint({
                url: "/authentication/user/devices",
                method: "GET",
                authType: "sessionToken"
              }),
              authenticators: y.createEndpoint({
                url: "/authentication/user/authenticators",
                method: "GET",
                authType: "stepUpToken"
              }),
              challenge: y.createEndpoint({
                url: "/authentication/user/authenticators/requestChallenge/:authenticatorId",
                method: "POST",
                authType: "stepUpToken"
              }),
              stepUp: y.createEndpoint({
                url: "/authentication/user/stepUp",
                method: "POST",
                authType: "sessionToken"
              })
            },
            _ = {
              login: y.createEndpoint({
                url: 1 == a ? "/authentication/client/login" : "/authentication/session/login",
                method: "POST",
                authType: "userToken",
                data: {
                  clientId: u,
                  clientDescription: r,
                  deviceId: c
                }
              }),
              logout: y.createEndpoint({
                url: 1 == a ? "/authentication/client/logout" : "/authentication/session/logout",
                method: "POST",
                authType: "sessionToken"
              }),
              chain: y.createEndpoint({
                url: 1 == a ? "/authentication/client/chain" : "/authentication/session/chain",
                method: "POST",
                authType: "sessionToken",
                data: {
                  clientId: u,
                  deviceId: c
                }
              })
            },
            $ = {
              currentUser: {
                following: y.createEndpoint({
                  url: "/social/following",
                  method: "GET",
                  authType: "sessionToken"
                }),
                unfollow: y.createEndpoint({
                  url: "/social/following/:userId",
                  method: "DELETE",
                  authType: "sessionToken",
                  params: {
                    userId: ""
                  }
                }),
                follow: y.createEndpoint({
                  url: "/social/following/:userId",
                  method: "PUT",
                  authType: "sessionToken",
                  params: {
                    userId: ""
                  }
                }),
                followedBy: y.createEndpoint({
                  url: "/social/followedby",
                  method: "GET",
                  authType: "sessionToken"
                })
              },
              following: y.createEndpoint({
                url: "/social/:userId/following",
                method: "GET",
                authType: "sessionToken",
                params: {
                  userId: ""
                }
              }),
              followedBy: y.createEndpoint({
                url: "/social/:userId/followedby",
                method: "GET",
                authType: "sessionToken",
                params: {
                  userId: ""
                }
              })
            },
            w = {
              get: y.createEndpoint({
                url: "/status/user/:userId",
                method: "GET",
                authType: "sessionToken",
                params: {
                  userId: ""
                }
              })
            },
            T = {
              getAll: y.createEndpoint({
                url: "/datastore/user/client",
                method: "GET",
                authType: "sessionToken"
              }),
              get: y.createEndpoint({
                url: "/datastore/user/client/:blockKey",
                method: "GET",
                authType: "sessionToken",
                params: {
                  blockKey: ""
                }
              }),
              updateMultiple: y.createEndpoint({
                url: "/datastore/user/client",
                method: "PUT",
                authType: "sessionToken"
              }),
              update: y.createEndpoint({
                url: "/datastore/user/client/:blockKey",
                method: "PUT",
                authType: "sessionToken",
                params: {
                  blockKey: ""
                }
              }),
              shared: v(),
              sharedCommon: v(p.NV_COMMON_CLIENT_ID),
              sharedOwn: v(u),
              sharedUser: {
                getAll: y.createEndpoint({
                  url: "/datastore/user/:userId/shared/:clientId/client/",
                  method: "GET",
                  authType: "sessionToken"
                }),
                get: y.createEndpoint({
                  url: "/datastore/user/:userId/shared/:clientId/client/:blockKey",
                  method: "GET",
                  authType: "sessionToken",
                  params: {
                    blockKey: ""
                  }
                })
              }
            },
            C = {
              get: y.createEndpoint({
                url: "/validation/clientsdk",
                method: "GET"
              }),
              validate: y.createEndpoint({
                url: "/validation",
                method: "GET"
              })
            },
            x = {
              get: y.createEndpoint({
                url: "/stringtable/client",
                method: "GET",
                authType: "sessionToken"
              })
            },
            S = {
              request: y.createEndpoint({
                url: "/authentication/delegate/request",
                method: "POST",
                authType: "sessionToken",
                data: {
                  clientId: ""
                }
              }),
              redeem: y.createEndpoint({
                url: "/authentication/delegate/redeem",
                method: "POST",
                authType: "delegateToken",
                data: {
                  clientId: "",
                  deviceId: ""
                }
              })
            },
            A = {
              user: y.createEndpoint({
                url: "/search/user",
                method: "GET",
                params: {
                  q: {}
                }
              })
            },
            M = {
              user: y.createEndpoint({
                url: "/search/user",
                method: "GET",
                authType: "sessionToken",
                params: {
                  q: {}
                }
              })
            },
            k = {
              get: y.createEndpoint({
                url: "/settings/client",
                method: "GET",
                authType: "sessionToken"
              })
            },
            N = {
              following: {
                getUserProfiles: y.createEndpoint({
                  url: "/batch/foreach/list_of_follows_out/get_user_profile",
                  method: "GET",
                  authType: "sessionToken"
                }),
                getUserStatuses: y.createEndpoint({
                  url: "/batch/foreach/list_of_follows_out/get_user_status",
                  method: "GET",
                  authType: "sessionToken"
                })
              }
            },
            I = {
              testaccount: {
                get: y.createEndpoint({
                  url: "/dev/testaccount/:userId",
                  method: "GET",
                  authType: "userCredentials",
                  params: {
                    userId: "",
                    username: "nv_email_address"
                  }
                }),
                list: y.createEndpoint({
                  url: "/dev/testaccount",
                  method: "GET",
                  authType: "userCredentials",
                  params: {
                    username: "nv_email_address"
                  }
                }),
                convert: y.createEndpoint({
                  url: "/dev/testaccount/convert/:userId",
                  method: "PUT",
                  authType: "userCredentials",
                  params: {
                    userId: "",
                    username: "nv_email_address"
                  },
                  data: {
                    password: "accountPassword"
                  }
                }),
                delete: y.createEndpoint({
                  url: "/dev/testaccount/:userId",
                  method: "DELETE",
                  authType: "userCredentials",
                  params: {
                    userId: "",
                    username: "nv_email_address"
                  }
                })
              }
            },
            O = {
              get: y.createEndpoint({
                url: "/ping",
                method: "GET"
              })
            };
          return {
            getFullUrl: y.generateFullUrl,
            setServer: g,
            constants: p,
            profile: b,
            authentication: E,
            session: _,
            social: $,
            status: w,
            userData: T,
            validation: C,
            stringTable: x,
            delegate: S,
            search: A,
            searchWithSession: M,
            settings: k,
            batch: N,
            dev: I,
            ping: O
          };
        }]
      };
    }]), angular.module("nvAngularJarvisSdk").constant("JARVIS_CONSTANTS", {
      NV_COMMON_CLIENT_ID: "102009640608333825"
    }), angular.module("nvAngularJarvisSdk").factory("JarvisSessionManager", ["$q", "jarvis",
      "nvAccountService",
      function(e, t, n) {
        function r(e) {
          this.onNewTokenCallback = e, this.sessionToken = null, this.tokenListeners = [];
        }
        return r.prototype = function() {
          var i = function(e) {
              var n = this;
              return t.session.login(e).then(function(t) {
                n.sessionToken = t.data.sessionToken, n.onNewTokenCallback(n.sessionToken), n
                  .userToken = e.userToken, n.sessionParams = e;
              });
            },
            o = function(e, t) {
              this.userToken = t.userToken, this.sessionParams = t, this.sessionToken = e, this
                .onNewTokenCallback(e);
            },
            a = function(e) {
              this.userToken = null, this.sessionToken = null, this.sessionParams = null, null !== e &&
                void 0 !== e || n.deleteJarvisUserToken(), this.onNewTokenCallback(this.sessionToken);
            },
            s = function(e) {
              var n = this,
                r = angular.extend({}, n.sessionParams);
              return delete r.userToken, delete r.clientDescription, r.sessionToken = e, t.session.chain(
                r).then(function(e) {
                n.sessionToken = e.data.sessionToken, n.onNewTokenCallback(n.sessionToken);
              });
            },
            c = function(e, t, n) {
              return e(angular.extend({}, t, {
                sessionToken: this.sessionToken
              }), n);
            },
            u = function(t, n, r) {
              var i = e.defer();
              return this.tokenListeners.push({
                api: t,
                params: n,
                data: r,
                deferred: i
              }), i.promise;
            },
            l = function(t) {
              var n = this;
              if (angular.forEach(n.tokenListeners, function(e) {
                  t ? e.deferred.reject("Could not get a valid session token") : e.deferred.resolve(c
                    .call(n, e.api, e.params, e.data));
                }), t) return e.reject();
            },
            d = function() {
              var e = this;
              return i.call(e, e.sessionParams).then(function() {
                return l.call(e);
              }, function() {
                return l.call(e, !0);
              });
            },
            f = function(e) {
              var t = this;
              return s.call(t, e).then(function() {
                return l.call(t);
              }, function() {
                return d.call(t);
              });
            },
            h = function(t, n, r, i, o) {
              var a = this;
              return f.call(a, n).then(function(e) {
                return c.call(a, r, i, o);
              }, function() {
                return e.reject(t);
              });
            },
            p = function(t, n, r) {
              var i = this,
                o = i.sessionToken;
              return c.call(i, t, n, r).then(function(e) {
                return e;
              }, function(a) {
                return 401 === a.status ? "INVALID" === i.sessionToken ? u.call(i, t, n, r) : i
                  .sessionToken !== o ? c.call(i, t, n, r) : (i.sessionToken = "INVALID", h.call(i, a,
                    o, t, n, r)) : e.reject(a);
              });
            };
          return {
            constructor: r,
            createSession: i,
            createSessionFromToken: o,
            closeSession: a,
            callSessionApi: p
          };
        }(), r;
      }
    ]);
}
