// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 162
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  function n(e) {
    e = e || {}, this.ms = e.min || 100, this.max = e.max || 1e4, this.factor = e.factor || 2, this.jitter = e
      .jitter > 0 && e.jitter <= 1 ? e.jitter : 0, this.attempts = 0;
  }
  module.exports = n, n.prototype.duration = function() {
    var e = this.ms * Math.pow(this.factor, this.attempts++);
    if (this.jitter) {
      var t = Math.random(),
        n = Math.floor(t * this.jitter * e);
      e = 0 == (1 & Math.floor(10 * t)) ? e - n : e + n;
    }
    return 0 | Math.min(e, this.max);
  }, n.prototype.reset = function() {
    this.attempts = 0;
  }, n.prototype.setMin = function(e) {
    this.ms = e;
  }, n.prototype.setMax = function(e) {
    this.max = e;
  }, n.prototype.setJitter = function(e) {
    this.jitter = e;
  };
}
